using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Windows.Forms;
using Newtonsoft.Json.Linq;

namespace DataImportTool
{
    /// <summary>
    /// 自动更新管理器：从 Gitee 公开仓库读取 version.json，检测并应用新版本。
    /// 复刻「京威工具」的自动更新逻辑（Gitee raw + version.json + update.bat 自替换）。
    /// </summary>
    public static class UpdateManager
    {
        // 默认更新服务器（公开仓库 raw 路径），可被 App.config 的 UpdateServerUrl 覆盖
        private const string DefaultUpdateServerUrl =
            "https://gitee.com/zmwhelloword/dataimporttool-release/raw/master/";

        static UpdateManager()
        {
            // Gitee raw 走 https，确保 TLS1.2
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
        }

        /// <summary>更新服务器根地址（以 / 结尾）。</summary>
        public static string UpdateServerUrl
        {
            get
            {
                string cfg = string.Empty;
                try { cfg = ConfigurationManager.AppSettings["UpdateServerUrl"]; }
                catch { }
                if (string.IsNullOrWhiteSpace(cfg))
                    return DefaultUpdateServerUrl;
                if (!cfg.EndsWith("/"))
                    cfg = cfg + "/";
                return cfg;
            }
        }

        /// <summary>当前程序版本（取自 AssemblyVersion）。</summary>
        public static Version CurrentVersion
        {
            get
            {
                try
                {
                    return System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
                }
                catch
                {
                    return new Version(1, 0, 0, 0);
                }
            }
        }

        public static string CurrentVersionString
        {
            get { return CurrentVersion.ToString(); }
        }

        // ====== 检测 ======
        public static UpdateCheckResult CheckForUpdate()
        {
            var result = new UpdateCheckResult();
            try
            {
                string url = UpdateServerUrl + "version.json";
                string json;
                using (var wc = new WebClient())
                {
                    wc.Encoding = Encoding.UTF8;
                    wc.Headers.Add("User-Agent", "DataImportTool-Updater/1.0");
                    json = wc.DownloadString(url);
                }

                if (string.IsNullOrWhiteSpace(json) || !json.TrimStart().StartsWith("{"))
                {
                    result.Success = false;
                    result.ErrorMessage = "服务器返回的版本信息格式不正确。";
                    return result;
                }

                // 用 Newtonsoft 解析（避免手工 IndexOf 解析 JSON 的致命坑）
                var jo = JObject.Parse(json);
                var info = new UpdateInfo
                {
                    Version = (string)jo["version"],
                    DownloadUrl = (string)jo["downloadUrl"],
                    ReleaseNotes = (string)jo["releaseNotes"],
                    ReleaseDate = (string)jo["releaseDate"],
                    IsMandatory = (bool?)jo["isMandatory"] ?? false,
                    FileSize = (long?)jo["fileSize"] ?? 0,
                    FileHash = (string)jo["fileHash"]
                };

                if (string.IsNullOrWhiteSpace(info.Version))
                {
                    result.Success = false;
                    result.ErrorMessage = "版本信息缺少 version 字段。";
                    return result;
                }

                result.Success = true;
                result.Info = info;
                result.UpdateAvailable = IsNewer(info.Version, CurrentVersion);
            }
            catch (WebException ex)
            {
                result.Success = false;
                result.ErrorMessage = "无法连接更新服务器：" + ex.Message;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.ErrorMessage = "检查更新失败：" + ex.Message;
            }
            return result;
        }

        /// <summary>serverVersion 是否比 current 新。</summary>
        public static bool IsNewer(string serverVersion, Version current)
        {
            if (!TryParseVersion(serverVersion, out Version srv))
                return false;
            return srv > current;
        }

        private static bool TryParseVersion(string s, out Version v)
        {
            v = null;
            if (string.IsNullOrWhiteSpace(s))
                return false;
            // 去掉可能的 "v" 前缀
            s = s.Trim().TrimStart('v', 'V');
            var parts = s.Split('.');
            int major = 0, minor = 0, build = 0, revision = 0;
            if (parts.Length > 0 && int.TryParse(parts[0], out major)) { }
            else return false;
            if (parts.Length > 1) int.TryParse(parts[1], out minor);
            if (parts.Length > 2) int.TryParse(parts[2], out build);
            if (parts.Length > 3) int.TryParse(parts[3], out revision);
            v = new Version(major, minor, build, revision);
            return true;
        }

        // ====== 跳过版本机制（持久化到 LocalAppData）======
        private static string SkipConfigPath
        {
            get
            {
                var dir = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "DataImportTool");
                return Path.Combine(dir, "updateconfig.json");
            }
        }

        public static string GetSkipVersion()
        {
            try
            {
                if (!File.Exists(SkipConfigPath))
                    return string.Empty;
                var txt = File.ReadAllText(SkipConfigPath, Encoding.UTF8);
                var jo = JObject.Parse(txt);
                return (string)jo["skipVersion"] ?? string.Empty;
            }
            catch { return string.Empty; }
        }

        public static void SetSkipVersion(string version)
        {
            try
            {
                var dir = Path.GetDirectoryName(SkipConfigPath);
                Directory.CreateDirectory(dir);
                var jo = new JObject { ["skipVersion"] = version ?? string.Empty };
                File.WriteAllText(SkipConfigPath, jo.ToString(), Encoding.UTF8);
            }
            catch { }
        }

        // ====== 下载 + 应用 ======
        /// <summary>将文件下载到 destPath，progress 回调 (percent, received, total)。</summary>
        public static void DownloadFile(string url, string destPath, Action<int, long, long> progress)
        {
            using (var wc = new WebClient())
            {
                wc.Encoding = Encoding.UTF8;
                wc.Headers.Add("User-Agent", "DataImportTool-Updater/1.0");
                if (progress != null)
                {
                    wc.DownloadProgressChanged += (s, e) =>
                    {
                        progress(e.ProgressPercentage, e.BytesReceived, e.TotalBytesToReceive);
                    };
                }
                wc.DownloadFile(url, destPath);
            }
        }

        /// <summary>可选：校验下载文件的 MD5 是否与 info.FileHash 一致（为空则跳过）。</summary>
        public static bool VerifyHash(string filePath, UpdateInfo info)
        {
            if (info == null || string.IsNullOrWhiteSpace(info.FileHash))
                return true;
            try
            {
                using (var md5 = System.Security.Cryptography.MD5.Create())
                using (var fs = File.OpenRead(filePath))
                {
                    var hash = md5.ComputeHash(fs);
                    var sb = new StringBuilder();
                    foreach (var b in hash)
                        sb.Append(b.ToString("x2"));
                    return string.Equals(sb.ToString(), info.FileHash, StringComparison.OrdinalIgnoreCase);
                }
            }
            catch { return false; }
        }

        /// <summary>写入 update.bat 并启动它，随后退出当前程序（由 bat 完成替换与重启）。</summary>
        public static void ApplyUpdate(string downloadedExePath, UpdateInfo info)
        {
            string appPath = Application.ExecutablePath;
            string batPath = Path.Combine(Path.GetTempPath(), "DataImportTool_update.bat");

            var sb = new StringBuilder();
            // 注意：不要写 chcp 65001，否则中文路径会乱码（cmd 默认 936 与 Encoding.Default 一致）
            sb.AppendLine("@echo off");
            sb.AppendLine("timeout /t 2 >nul");
            sb.AppendLine(string.Format("copy /y \"{0}\" \"{1}\"", downloadedExePath, appPath));
            sb.AppendLine(string.Format("start \"\" \"{0}\"", appPath));
            sb.AppendLine(string.Format("del /f /q \"{0}\"", downloadedExePath));
            sb.AppendLine("exit");

            File.WriteAllText(batPath, sb.ToString(), Encoding.Default);

            var psi = new ProcessStartInfo(batPath)
            {
                UseShellExecute = true,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden
            };
            Process.Start(psi);

            // 退出当前程序，释放 exe 文件锁，让 bat 完成替换
            Application.Exit();
        }
    }

    /// <summary>服务器版本信息。</summary>
    public class UpdateInfo
    {
        public string Version;
        public string DownloadUrl;
        public string ReleaseNotes;
        public string ReleaseDate;
        public bool IsMandatory;
        public long FileSize;
        /// <summary>下载文件的 MD5（小写 32 位）；为空则不校验。</summary>
        public string FileHash;
    }

    /// <summary>检测更新结果。</summary>
    public class UpdateCheckResult
    {
        public bool Success;
        public string ErrorMessage;
        public UpdateInfo Info;
        public bool UpdateAvailable;
    }
}
