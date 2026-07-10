using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace DataImportTool
{
    /// <summary>
    /// 操作日志条目
    /// </summary>
    public class LogEntry
    {
        /// <summary>时间戳</summary>
        public DateTime Time { get; set; }

        /// <summary>功能模块名称</summary>
        public string Module { get; set; }

        /// <summary>消息类型：Info / Success / Error / Import</summary>
        public string Type { get; set; }

        /// <summary>日志消息</summary>
        public string Message { get; set; }

        /// <summary>导入详情（仅 Type=Import 时有值）</summary>
        public ImportDetail Detail { get; set; }

        public LogEntry() { }

        public LogEntry(DateTime time, string module, string type, string message)
        {
            Time = time;
            Module = module ?? string.Empty;
            Type = type ?? "Info";
            Message = message ?? string.Empty;
        }
    }

    /// <summary>
    /// 导入操作详情（记录导入数据和结果）
    /// </summary>
    public class ImportDetail
    {
        /// <summary>导入文件路径</summary>
        public string FilePath { get; set; }

        /// <summary>成功数</summary>
        public int SuccessCount { get; set; }

        /// <summary>失败数</summary>
        public int FailCount { get; set; }

        /// <summary>总行数</summary>
        public int TotalCount { get; set; }

        /// <summary>是否已取消</summary>
        public bool Cancelled { get; set; }

        /// <summary>错误详情列表（每条一行）</summary>
        public List<string> Errors { get; set; }

        /// <summary>附加参数（如分类名、属性集名等）</summary>
        public string Parameters { get; set; }

        /// <summary>每条变更记录（新增/更新/删除/跳过）</summary>
        public List<string> Changes { get; set; }

        public ImportDetail()
        {
            Errors = new List<string>();
            Changes = new List<string>();
        }
    }

    /// <summary>
    /// 操作日志管理器（线程安全单例）
    /// 日志存储在 %LocalAppData%\DataImportTool\operation_logs.json
    /// </summary>
    public sealed class Logger
    {
        private static readonly Lazy<Logger> _instance =
            new Lazy<Logger>(() => new Logger());

        private readonly object _lock = new object();
        private readonly string _filePath;
        private List<LogEntry> _logs;

        public static Logger Instance => _instance.Value;

        private Logger()
        {
            var dir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "DataImportTool");
            Directory.CreateDirectory(dir);
            _filePath = Path.Combine(dir, "operation_logs.json");
            Load();
        }

        /// <summary>
        /// 记录一条日志
        /// </summary>
        public void Log(string module, string message, string type = "Info")
        {
            var entry = new LogEntry(DateTime.Now, module, type, message);
            lock (_lock)
            {
                _logs.Add(entry);
                Save();
            }
        }

        /// <summary>
        /// 记录一条成功日志
        /// </summary>
        public void LogSuccess(string module, string message)
        {
            Log(module, message, "Success");
        }

        /// <summary>
        /// 记录一条错误日志
        /// </summary>
        public void LogError(string module, string message)
        {
            Log(module, message, "Error");
        }

        /// <summary>
        /// 记录一条导入日志（包含导入数据和结果详情）
        /// </summary>
        public void LogImport(string module, string message, ImportDetail detail)
        {
            var entry = new LogEntry(DateTime.Now, module, "Import", message);
            entry.Detail = detail;
            lock (_lock)
            {
                _logs.Add(entry);
                Save();
            }
        }

        /// <summary>
        /// 获取所有导入日志（按时间降序）
        /// </summary>
        public List<LogEntry> GetImportLogs()
        {
            lock (_lock)
            {
                return _logs
                    .Where(e => e.Type == "Import")
                    .OrderByDescending(e => e.Time)
                    .ToList();
            }
        }

        /// <summary>
        /// 获取所有日志（按时间降序）
        /// </summary>
        public List<LogEntry> GetAllLogs()
        {
            lock (_lock)
            {
                return _logs
                    .OrderByDescending(e => e.Time)
                    .ToList();
            }
        }

        /// <summary>
        /// 按模块和关键字搜索日志
        /// </summary>
        public List<LogEntry> Search(string keyword, string moduleFilter = null)
        {
            lock (_lock)
            {
                var query = _logs.AsEnumerable();

                if (!string.IsNullOrWhiteSpace(keyword))
                {
                    var kw = keyword.Trim();
                    query = query.Where(e =>
                        e.Message.IndexOf(kw, StringComparison.OrdinalIgnoreCase) >= 0 ||
                        e.Module.IndexOf(kw, StringComparison.OrdinalIgnoreCase) >= 0);
                }

                if (!string.IsNullOrWhiteSpace(moduleFilter))
                {
                    query = query.Where(e =>
                        string.Equals(e.Module, moduleFilter, StringComparison.OrdinalIgnoreCase));
                }

                return query.OrderByDescending(e => e.Time).ToList();
            }
        }

        /// <summary>
        /// 获取所有不同的模块名称
        /// </summary>
        public List<string> GetAllModules()
        {
            lock (_lock)
            {
                return _logs
                    .Select(e => e.Module)
                    .Distinct()
                    .OrderBy(m => m)
                    .ToList();
            }
        }

        /// <summary>
        /// 清空所有日志
        /// </summary>
        public void Clear()
        {
            lock (_lock)
            {
                _logs.Clear();
                Save();
            }
        }

        /// <summary>
        /// 获取日志文件路径
        /// </summary>
        public string GetFilePath()
        {
            return _filePath;
        }

        // ====== 内部方法 ======

        private void Load()
        {
            try
            {
                if (!File.Exists(_filePath))
                {
                    _logs = new List<LogEntry>();
                    return;
                }

                var json = File.ReadAllText(_filePath, Encoding.UTF8);
                if (string.IsNullOrWhiteSpace(json))
                {
                    _logs = new List<LogEntry>();
                    return;
                }

                _logs = JsonConvert.DeserializeObject<List<LogEntry>>(json) ?? new List<LogEntry>();
            }
            catch
            {
                _logs = new List<LogEntry>();
            }
        }

        private void Save()
        {
            try
            {
                var json = JsonConvert.SerializeObject(_logs, Formatting.Indented);
                File.WriteAllText(_filePath, json, Encoding.UTF8);
            }
            catch
            {
                // 静默忽略保存失败
            }
        }
    }
}
