using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace DataImportTool
{
    /// <summary>
    /// 匹配规则「默认排除字段」持久化助手。
    /// 排除列表以 JSON 保存于操作系统临时目录（%TEMP%/DataImportTool），跨程序退出保留，下次启动自动加载。
    /// 内部比较统一使用大小写不敏感，调用方可直接传入属性 internal_name。
    /// </summary>
    public static class MatchRuleExclusions
    {
        private const string FolderName = "DataImportTool";
        private const string FileName = "match_rule_exclusions.json";
        private static readonly object _lock = new object();
        private static HashSet<string> _cache;

        private static string FilePath
        {
            get
            {
                string dir = Path.Combine(Path.GetTempPath(), FolderName);
                return Path.Combine(dir, FileName);
            }
        }

        /// <summary>获取当前排除字段集合（大小写不敏感，返回副本避免外部篡改缓存）。</summary>
        public static HashSet<string> GetExcluded()
        {
            lock (_lock)
            {
                if (_cache == null) Load();
                return new HashSet<string>(_cache, StringComparer.OrdinalIgnoreCase);
            }
        }

        private static void Load()
        {
            _cache = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            try
            {
                string path = FilePath;
                if (!File.Exists(path)) return;
                string txt = File.ReadAllText(path);
                if (string.IsNullOrWhiteSpace(txt)) return;
                var obj = JsonConvert.DeserializeObject<ExclusionFile>(txt);
                if (obj != null && obj.ExcludedFields != null)
                {
                    foreach (var f in obj.ExcludedFields)
                        if (!string.IsNullOrWhiteSpace(f))
                            _cache.Add(f);
                }
            }
            catch
            {
                // 读取失败不阻断主流程，视为无排除配置
            }
        }

        /// <summary>保存排除字段集合（覆盖式）：同时写入文件与内存缓存，供本次会话及下次启动使用。</summary>
        public static void Save(IEnumerable<string> excluded)
        {
            if (excluded == null) excluded = new List<string>();
            lock (_lock)
            {
                _cache = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                foreach (var f in excluded)
                    if (!string.IsNullOrWhiteSpace(f))
                        _cache.Add(f);
                try
                {
                    string path = FilePath;
                    string dir = Path.GetDirectoryName(path);
                    if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
                    var obj = new ExclusionFile { ExcludedFields = new List<string>(_cache) };
                    File.WriteAllText(path, JsonConvert.SerializeObject(obj, Formatting.Indented));
                }
                catch
                {
                    // 写入失败不阻断主流程
                }
            }
        }

        private class ExclusionFile
        {
            public List<string> ExcludedFields { get; set; } = new List<string>();
        }
    }
}
