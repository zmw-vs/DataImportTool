using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace DataImportTool
{
    /// <summary>
    /// 匹配规则「预保存（待落库）」暂存持久化助手。
    /// 与「默认排除字段」(MatchRuleExclusions) 共用临时目录 %TEMP%/DataImportTool，跨程序退出保留，下次启动自动恢复。
    /// 仅持久化 IsNew=true 的暂存项（即预保存后尚未保存到数据库的数据）；成功落库后由调用方清除。
    /// 按生产类型(source type)的 internal_name 分键存储，避免不同类型的暂存规则互相覆盖。
    /// </summary>
    public static class MatchRuleStaging
    {
        private const string FolderName = "DataImportTool";
        private const string FileName = "match_rule_staged.json";
        private static readonly object _lock = new object();
        private static Dictionary<string, List<MatchRuleClassification>> _cache;

        private static string FilePath
        {
            get
            {
                string dir = Path.Combine(Path.GetTempPath(), FolderName);
                return Path.Combine(dir, FileName);
            }
        }

        private static void LoadAll()
        {
            _cache = new Dictionary<string, List<MatchRuleClassification>>(StringComparer.OrdinalIgnoreCase);
            try
            {
                string path = FilePath;
                if (!File.Exists(path)) return;
                string txt = File.ReadAllText(path);
                if (string.IsNullOrWhiteSpace(txt)) return;
                var obj = JsonConvert.DeserializeObject<StagedFile>(txt);
                if (obj != null && obj.Staged != null)
                {
                    foreach (var kv in obj.Staged)
                    {
                        if (kv.Value == null) continue;
                        var list = new List<MatchRuleClassification>();
                        foreach (var r in kv.Value)
                        {
                            list.Add(new MatchRuleClassification
                            {
                                ObjectId = r.ObjectId,
                                StrictMatchFlag = r.StrictMatchFlag,
                                Fields = new List<string>(r.Fields ?? new List<string>())
                            });
                        }
                        _cache[kv.Key] = list;
                    }
                }
            }
            catch
            {
                // 读取失败不阻断主流程，视为无暂存
            }
        }

        /// <summary>载入指定生产类型的暂存（预保存未落库）规则，统一标记 IsNew=true 供界面高亮与还原。</summary>
        public static List<MatchRuleClassification> LoadStaged(string key)
        {
            lock (_lock)
            {
                if (_cache == null) LoadAll();
                if (_cache.TryGetValue(key ?? string.Empty, out var list) && list != null)
                {
                    var result = new List<MatchRuleClassification>();
                    foreach (var r in list)
                    {
                        result.Add(new MatchRuleClassification
                        {
                            ObjectId = r.ObjectId,
                            StrictMatchFlag = r.StrictMatchFlag,
                            Fields = new List<string>(r.Fields ?? new List<string>()),
                            IsNew = true
                        });
                    }
                    return result;
                }
                return new List<MatchRuleClassification>();
            }
        }

        /// <summary>保存指定生产类型的暂存规则：仅持久化 IsNew=true 的项；若无暂存项则删除该 key 记录。</summary>
        public static void SaveStaged(string key, List<MatchRuleClassification> rules)
        {
            if (rules == null) rules = new List<MatchRuleClassification>();
            lock (_lock)
            {
                if (_cache == null) LoadAll();
                key = key ?? string.Empty;
                var onlyNew = new List<MatchRuleClassification>();
                foreach (var r in rules)
                {
                    if (r.IsNew)
                    {
                        onlyNew.Add(new MatchRuleClassification
                        {
                            ObjectId = r.ObjectId,
                            StrictMatchFlag = r.StrictMatchFlag,
                            Fields = new List<string>(r.Fields ?? new List<string>())
                        });
                    }
                }
                if (onlyNew.Count == 0)
                    _cache.Remove(key);
                else
                    _cache[key] = onlyNew;
                WriteFile();
            }
        }

        /// <summary>清除指定生产类型的暂存记录（如某类型成功落库后调用）。</summary>
        public static void ClearStaged(string key)
        {
            lock (_lock)
            {
                if (_cache == null) LoadAll();
                _cache.Remove(key ?? string.Empty);
                WriteFile();
            }
        }

        /// <summary>清除全部暂存记录（保存到数据库后调用：所有类型均已落库，临时暂存不再需要）。</summary>
        public static void ClearAllStaged()
        {
            lock (_lock)
            {
                _cache = new Dictionary<string, List<MatchRuleClassification>>(StringComparer.OrdinalIgnoreCase);
                WriteFile();
            }
        }

        private static void WriteFile()
        {
            try
            {
                string path = FilePath;
                string dir = Path.GetDirectoryName(path);
                if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
                var obj = new StagedFile { Staged = _cache };
                File.WriteAllText(path, JsonConvert.SerializeObject(obj, Formatting.Indented));
            }
            catch
            {
                // 写入失败不阻断主流程
            }
        }

        private class StagedFile
        {
            public Dictionary<string, List<MatchRuleClassification>> Staged
                = new Dictionary<string, List<MatchRuleClassification>>(StringComparer.OrdinalIgnoreCase);
        }
    }
}
