using System;
using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace DataImportTool
{
    /// <summary>
    /// 配置管理器：读写程序目录下的 config.json
    /// </summary>
    public static class ConfigManager
    {
        private static readonly string ConfigFilePath =
            GetConfigPath();

        private static string GetConfigPath()
        {
            var dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "DataImportTool");
            Directory.CreateDirectory(dir);
            return Path.Combine(dir, "config.json");
        }

        /// <summary>
        /// 从程序目录下的 config.json 读取并反序列化。
        /// 若文件不存在或反序列化失败，返回全新的 AppConfig（IsConfigured 为 false）。
        /// </summary>
        public static AppConfig Load()
        {
            try
            {
                if (!File.Exists(ConfigFilePath))
                    return new AppConfig();

                string json = File.ReadAllText(ConfigFilePath, Encoding.UTF8);
                if (string.IsNullOrWhiteSpace(json))
                    return new AppConfig();

                var cfg = JsonConvert.DeserializeObject<AppConfig>(json) ?? new AppConfig();

                // 解密密码（若曾被加密）
                cfg.Password = AppConfig.DecryptPassword(cfg.Password);

                if (string.IsNullOrEmpty(cfg.NameColumn))
                    cfg.NameColumn = "Name";

                return cfg;
            }
            catch
            {
                return new AppConfig();
            }
        }

        /// <summary>
        /// 将配置对象序列化为格式化的 JSON 并写入 config.json。
        /// 密码以简单加密形式存储，避免明文落地。
        /// </summary>
        public static void Save(AppConfig config)
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));

            // 构造用于序列化的副本，仅对密码字段加密
            var toSave = new
            {
                Server = config.Server,
                Database = config.Database,
                UseWindowsAuth = config.UseWindowsAuth,
                UserId = config.UserId,
                Password = AppConfig.EncryptPassword(config.Password),
                NameColumn = config.NameColumn
            };

            string json = JsonConvert.SerializeObject(toSave, Formatting.Indented);
            File.WriteAllText(ConfigFilePath, json, Encoding.UTF8);
        }
    }
}
