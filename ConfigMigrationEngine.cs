using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;

namespace DataImportTool
{
    /// <summary>
    /// 配置迁移引擎：将源库（测试）数据迁移到目标库（正式），保留所有 OBJECT_ID，
    /// 迁移后重设序列。支持 ID 重叠检测、迁移前自动备份、一键恢复。
    ///
    /// 涉及两类数据库（同服务器）：
    ///   主 PLM 库（连接串默认库）：t_object_classification 等 7 张序列表。
    ///   配置库 plm_config_v2：meta_element / config_item / 表单三表。
    /// 配置库连接由「主连接替换 Database=plm_config_v2」派生（见 ConfigDbName）。
    /// </summary>
    public class ConfigMigrationEngine
    {
        /// <summary>配置库名（与主库同服务器）。若用户环境不同，请修改此常量。</summary>
        public const string ConfigDbName = "plm_config_v2";

        /// <summary>迁移范围（可组合）。</summary>
        [Flags]
        public enum Scope
        {
            None = 0,
            Classification = 1,   // #1~#7 分类/分类组/关系（7 张序列表）
            MetaElement = 2,     // #8 meta_element（全 meta_type）
            FormTemplates = 4,   // #9~#11 分类表单三表
            ConfigItem = 8,      // #12 config_item（仅最新版本 + 全字段核对）
            All = Classification | MetaElement | FormTemplates | ConfigItem
        }

        // 主库 7 张序列表（保留 ID）；顺序即依赖先后（父先于子），供复制与序列重设
        private static readonly string[] MainTables = new[]
        {
            "t_object_classification_group",
            "t_object_classification_group_to_child",
            "t_object_classification",
            "t_object_classification_group_to_classification",
            "t_property",
            "t_property_value",
            "t_property_to_property_value"
        };

        // 配置库 5 张表
        private static readonly string[] ConfigTables = new[]
        {
            "meta_element",
            "form_meta_template_definition",
            "form_meta_template_ui",
            "form_meta_template_setting",
            "config_item"
        };

        // 依赖顺序（父→子）：供 Restore 两阶段使用（删按反向、插按正向），避免外键约束冲突
        private static readonly string[] RestoreOrder = new[]
        {
            "t_object_classification_group",
            "t_object_classification_group_to_child",
            "t_object_classification",
            "t_object_classification_group_to_classification",
            "t_property",
            "t_property_value",
            "t_property_to_property_value",
            "meta_element",
            "form_meta_template_definition",
            "form_meta_template_ui",
            "form_meta_template_setting",
            "config_item"
        };

        // 7 张序列表对应的序列名（DbHelper.GetSequenceName 保持一致）
        private static readonly Dictionary<string, string> SeqOf = new Dictionary<string, string>
        {
            { "t_object_classification_group", "SEQ_ID_OBJECT_CLASSIFICATION_GROUP" },
            { "t_object_classification_group_to_child", "SEQ_ID_OBJECT_CLASSIFICATION_GROUP_TO_CHILD" },
            { "t_object_classification", "SEQ_ID_OBJECT_CLASSIFICATION" },
            { "t_object_classification_group_to_classification", "SEQ_ID_OBJECT_CLASSIFICATION_GROUP_TO_CLASSIFICATION" },
            { "t_property", "SEQ_ID_PROPERTY" },
            { "t_property_value", "SEQ_ID_PROPERTY_VALUE" },
            { "t_property_to_property_value", "SEQ_ID_PROPERTY_TO_PROPERTY_VALUE" }
        };

        // 备份影子表前缀
        private const string BakPrefix = "z_mig_bak_";

        // config_item 比较时跳过的字段（content 本身是要更新的；时间戳/主键为系统或定位字段）
        private static readonly HashSet<string> CfgSkipCompare = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "content", "release_datetime", "lastmodify_datetime", "creation_datetime", "object_id"
        };

        // ============================================================
        // 连接串辅助
        // ============================================================
        /// <summary>把连接串的 Database 替换为指定库名（同服务器跨库）。</summary>
        private static string SwapDb(string conn, string newDb)
        {
            if (string.IsNullOrEmpty(conn)) return conn;
            int idx = conn.IndexOf("Database=", StringComparison.OrdinalIgnoreCase);
            if (idx < 0) return conn;
            int start = idx + "Database=".Length;
            int end = conn.IndexOf(';', start);
            if (end < 0) return conn.Substring(0, start) + newDb;
            return conn.Substring(0, start) + newDb + conn.Substring(end);
        }

        // ============================================================
        // 连接测试
        // ============================================================
        public bool TestConnection(string connStr, out string serverVersion)
        {
            serverVersion = null;
            try
            {
                using (var conn = new SqlConnection(connStr))
                {
                    conn.Open();
                    serverVersion = conn.ServerVersion;
                }
                return true;
            }
            catch (Exception ex)
            {
                serverVersion = ex.Message;
                return false;
            }
        }

        // ============================================================
        // ID 重叠检测（需求 R1：逻辑写进程序）
        // ============================================================
        public class DetectRow
        {
            public string Database;     // 主库 / 配置库
            public string Table;
            public long SrcMax;
            public long SrcMin;
            public long SrcRows;
            public long TgtMax;
            public long TgtMin;
            public long TgtRows;
        }

        public class DetectResult
        {
            public List<DetectRow> Rows = new List<DetectRow>();
            public string Verdict;      // "A" 干净 / "B" 冲突(拦截) / "C" 合并(警告)
            public string Message;
            public string SrcVersion;
            public string TgtVersion;
        }

        public DetectResult Detect(string srcMain, string tgtMain)
        {
            string srcCfg = SwapDb(srcMain, ConfigDbName);
            string tgtCfg = SwapDb(tgtMain, ConfigDbName);
            var result = new DetectResult();

            try { result.SrcVersion = ReadPlmVersion(srcMain); }
            catch (Exception ex) { result.SrcVersion = "检测失败:" + ex.Message; }
            try { result.TgtVersion = ReadPlmVersion(tgtMain); }
            catch (Exception ex) { result.TgtVersion = "检测失败:" + ex.Message; }

            foreach (var t in MainTables)
                AddDetectRow(result, "主库", t, srcMain, tgtMain);
            foreach (var t in ConfigTables)
                AddDetectRow(result, "配置库", t, srcCfg, tgtCfg);

            // 判定（主要依据 7 张序列表的 ID 区间；配置表仅作信息展示，不阻断）
            bool conflict = false, hasTargetData = false;
            foreach (var r in result.Rows)
            {
                if (r.TgtRows > 0) hasTargetData = true;
                bool isMain = Array.IndexOf(MainTables, r.Table) >= 0;
                if (isMain && r.SrcRows > 0 && r.TgtRows > 0)
                {
                    // 区间相交即冲突
                    if (!(r.TgtMax < r.SrcMin || r.SrcMax < r.TgtMin))
                        conflict = true;
                }
            }
            if (conflict) { result.Verdict = "B"; result.Message = "目标库存在 ID 重叠，禁止直接执行（请先清空目标相关表或改方案）。"; }
            else if (hasTargetData) { result.Verdict = "C"; result.Message = "目标库已有数据但 ID 不重叠（合并场景），执行将覆盖/插入，请确认。"; }
            else { result.Verdict = "A"; result.Message = "目标库相关表为空，可安全执行「保留旧 ID」迁移。"; }

            // 版本一致性
            if (!string.Equals(result.SrcVersion, result.TgtVersion, StringComparison.OrdinalIgnoreCase)
                && !result.SrcVersion.StartsWith("检测失败") && !result.TgtVersion.StartsWith("检测失败"))
            {
                result.Message += " ⚠ 源/目标 PLM 版本不一致（源=" + result.SrcVersion + "，目标=" + result.TgtVersion + "），仅支持同版本迁移。";
                result.Verdict = "B";
            }
            return result;
        }

        private void AddDetectRow(DetectResult result, string db, string table, string srcConn, string tgtConn)
        {
            var row = new DetectRow { Database = db, Table = table };
            row.SrcMax = MaxId(srcConn, table, out row.SrcMin, out row.SrcRows);
            row.TgtMax = MaxId(tgtConn, table, out row.TgtMin, out row.TgtRows);
            result.Rows.Add(row);
        }

        private long MaxId(string conn, string table, out long min, out long rows)
        {
            long max = 0;
            min = 0; rows = 0;
            using (var c = new SqlConnection(conn))
            {
                c.Open();
                // 部分配置库表（GUID 主键 / 自然键）没有 OBJECT_ID 列，
                // 这类表不参与 ID 区间判定，只统计行数，避免“列名无效”。
                if (ColumnExists(conn, table, "OBJECT_ID"))
                {
                    using (var cmd = new SqlCommand(
                        "SELECT ISNULL(MAX(OBJECT_ID),0), ISNULL(MIN(OBJECT_ID),0), COUNT(*) FROM [dbo].[" + table + "]", c))
                    {
                        using (var r = cmd.ExecuteReader())
                        {
                            if (r.Read())
                            {
                                rows = Convert.ToInt64(r[2]);
                                if (rows > 0) { max = Convert.ToInt64(r[0]); min = Convert.ToInt64(r[1]); }
                            }
                        }
                    }
                }
                else
                {
                    using (var cmd = new SqlCommand("SELECT COUNT(*) FROM [dbo].[" + table + "]", c))
                        rows = Convert.ToInt64(cmd.ExecuteScalar());
                    max = -1; min = -1; // -1 表示无数值主键，不参与 ID 重叠判定
                }
            }
            return max;
        }

        /// <summary>判断指定表是否含某列（用于兼容无 OBJECT_ID 的配置库表）。</summary>
        private bool ColumnExists(string conn, string table, string column)
        {
            using (var c = new SqlConnection(conn))
            {
                c.Open();
                using (var cmd = new SqlCommand(
                    "SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA='dbo' AND TABLE_NAME=@t AND COLUMN_NAME=@col", c))
                {
                    cmd.Parameters.AddWithValue("@t", table);
                    cmd.Parameters.AddWithValue("@col", column);
                    using (var r = cmd.ExecuteReader())
                        return r.Read();
                }
            }
        }

        private string ReadPlmVersion(string mainConn)
        {
            var db = new DbHelper(mainConn);
            var v = db.DetectPlmVersion();
            return v == DbHelper.PlmVersion.V2026 ? "2026"
                 : v == DbHelper.PlmVersion.V2027 ? "2027"
                 : "未知";
        }

        // ============================================================
        // 备份 / 恢复（需求 R3 + Q9）
        // ============================================================
        /// <summary>迁移前自动备份：若目标某范围表有数据，复制到同库影子表。返回备份标签（空=无需备份）。</summary>
        public string Backup(string srcMain, string tgtMain, Scope scope, Action<string> log)
        {
            string tgtCfg = SwapDb(tgtMain, ConfigDbName);
            bool need = false;
            // 先用检测判断目标是否非空
            foreach (var t in MainTables)
                if (InScope(scope, Scope.Classification) && CountRows(tgtMain, t) > 0) { need = true; break; }
            if (!need)
            {
                foreach (var t in ConfigTables)
                {
                    bool inScope = (t == "meta_element" && InScope(scope, Scope.MetaElement))
                                || (t.StartsWith("form_meta_template") && InScope(scope, Scope.FormTemplates))
                                || (t == "config_item" && InScope(scope, Scope.ConfigItem));
                    if (inScope && CountRows(tgtCfg, t) > 0) { need = true; break; }
                }
            }
            if (!need) { log("目标库相关表均为空，跳过备份。"); return string.Empty; }

            string tag = DateTime.Now.ToString("yyyyMMddHHmmss");
            var manifest = new BackupManifest { Tag = tag, Time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") };
            manifest.Items = new List<BackupItem>();

            foreach (var t in MainTables)
            {
                if (!InScope(scope, Scope.Classification)) continue;
                if (CountRows(tgtMain, t) > 0)
                    manifest.Items.Add(BackupTable(tgtMain, t, tag, log));
            }
            foreach (var t in ConfigTables)
            {
                bool inScope = (t == "meta_element" && InScope(scope, Scope.MetaElement))
                            || (t.StartsWith("form_meta_template") && InScope(scope, Scope.FormTemplates))
                            || (t == "config_item" && InScope(scope, Scope.ConfigItem));
                if (!inScope) continue;
                if (CountRows(tgtCfg, t) > 0)
                    manifest.Items.Add(BackupTable(tgtCfg, t, tag, log));
            }

            SaveBackupManifest(manifest);
            log("已生成备份，标签：" + tag + "（共 " + manifest.Items.Count + " 张表）");
            return tag;
        }

        private BackupItem BackupTable(string conn, string table, string tag, Action<string> log)
        {
            string shadow = BakPrefix + tag + "_" + table;
            using (var c = new SqlConnection(conn))
            {
                c.Open();
                using (var cmd = new SqlCommand(
                    "IF OBJECT_ID('[dbo].[" + shadow + "]') IS NOT NULL DROP TABLE [dbo].[" + shadow + "];" +
                    "SELECT * INTO [dbo].[" + shadow + "] FROM [dbo].[" + table + "];", c))
                {
                    cmd.ExecuteNonQuery();
                }
            }
            log("  备份 " + table + " → " + shadow);
            return new BackupItem { Table = table, Shadow = shadow, Conn = conn };
        }

        /// <summary>恢复备份：两阶段（先按依赖反向清空，再按依赖正向从影子表回填），随后删除影子表。tag 为空则恢复最近一次。</summary>
        public void Restore(string tgtMain, Action<string> log, string tag = null)
        {
            var manifest = string.IsNullOrEmpty(tag) ? LoadLatestBackupManifest() : LoadBackupManifest(tag);
            if (manifest == null || manifest.Items == null || manifest.Items.Count == 0)
            { log("未找到可用备份，无法恢复。"); return; }

            string tgtCfg = SwapDb(tgtMain, ConfigDbName);
            log("开始恢复，回滚目标：" + manifest.Time + "（备份标签 " + manifest.Tag + "，共 " + manifest.Items.Count + " 张表）");

            // 仅保留本清单涉及、且在依赖顺序表中的表，按父→子排序
            var involved = RestoreOrder
                .Where(t => manifest.Items.Any(i => string.Equals(i.Table, t, StringComparison.OrdinalIgnoreCase)))
                .ToList();

            // 阶段一：按依赖反向（子→父）清空，避开外键约束
            log("阶段一：清空当前数据（按依赖反向）");
            foreach (var t in involved.AsEnumerable().Reverse())
            {
                string conn = ConnForTable(t, tgtMain, tgtCfg);
                TruncateTable(conn, t, log);
            }

            // 阶段二：按依赖正向（父→子）从影子表回填
            log("阶段二：从备份回填（按依赖正向）");
            foreach (var t in involved)
            {
                var it = manifest.Items.First(i => string.Equals(i.Table, t, StringComparison.OrdinalIgnoreCase));
                string conn = ConnForTable(t, tgtMain, tgtCfg);
                RestoreInsert(conn, t, it.Shadow, log);
            }

            // 删除影子表
            foreach (var it in manifest.Items)
            {
                string conn = ConnForTable(it.Table, tgtMain, tgtCfg);
                using (var c = new SqlConnection(conn))
                {
                    c.Open();
                    using (var cmd = new SqlCommand("IF OBJECT_ID('[dbo].[" + it.Shadow + "]') IS NOT NULL DROP TABLE [dbo].[" + it.Shadow + "];", c))
                        cmd.ExecuteNonQuery();
                }
            }
            DeleteBackupManifest(manifest.Tag);
            log("恢复完成（已回滚到备份 " + manifest.Tag + "，影子表已清理）。");
        }

        /// <summary>按表名推导其所属库的连接串：主库 7 表用主连接，其余（配置库）用配置连接。</summary>
        private static string ConnForTable(string table, string mainConn, string cfgConn)
        {
            return Array.IndexOf(MainTables, table) >= 0 ? mainConn : cfgConn;
        }

        private void TruncateTable(string conn, string table, Action<string> log)
        {
            using (var c = new SqlConnection(conn))
            {
                c.Open();
                using (var cmd = new SqlCommand("DELETE FROM [dbo].[" + table + "];", c))
                    cmd.ExecuteNonQuery();
            }
            log("  清空 " + table);
        }

        private void RestoreInsert(string conn, string table, string shadow, Action<string> log)
        {
            var cols = GetWritableColumns(conn, shadow);
            using (var c = new SqlConnection(conn))
            {
                c.Open();
                string colList = string.Join(",", cols.Select(x => "[" + x.Name + "]"));
                using (var ins = new SqlCommand(
                    "INSERT INTO [dbo].[" + table + "] (" + colList + ") SELECT " + colList + " FROM [dbo].[" + shadow + "];", c))
                {
                    ins.ExecuteNonQuery();
                }
            }
            log("  恢复 " + table + " ← " + shadow);
        }

        // ============================================================
        // 迁移主流程
        // ============================================================
        public void Migrate(string srcMain, string tgtMain, Scope scope, Action<string> log, Func<bool> shouldCancel)
        {
            string srcCfg = SwapDb(srcMain, ConfigDbName);
            string tgtCfg = SwapDb(tgtMain, ConfigDbName);

            // 前置：连接测试
            string sv;
            if (!TestConnection(srcMain, out sv)) throw new Exception("源库连接失败：" + sv);
            if (!TestConnection(tgtMain, out sv)) throw new Exception("目标库连接失败：" + sv);

            // 前置：检测（B 冲突则拦截）
            var det = Detect(srcMain, tgtMain);
            if (det.Verdict == "B")
                throw new Exception("ID 重叠/版本不一致，已拦截执行：" + det.Message);

            // 前置：自动备份（目标非空时）
            Backup(srcMain, tgtMain, scope, log);
            if (shouldCancel()) return;

            // ① 主库 7 张序列表（按依赖顺序复制，保留 ID）
            if (InScope(scope, Scope.Classification))
            {
                log("【阶段1】复制主库分类/组/关系（保留 OBJECT_ID）");
                foreach (var t in MainTables)
                {
                    if (shouldCancel()) { log("已取消。"); return; }
                    log("  复制 " + t);
                    CopyTable(srcMain, tgtMain, t);
                }
                if (shouldCancel()) { log("已取消。"); return; }
                ResetSequences(tgtMain, log);
            }

            // ② 配置库
            if (InScope(scope, Scope.MetaElement))
            {
                if (shouldCancel()) return;
                log("【阶段2】元模型字段 meta_element（全 meta_type，覆盖策略）");
                UpsertMetaElement(srcCfg, tgtCfg, log, shouldCancel);
            }
            if (InScope(scope, Scope.FormTemplates))
            {
                if (shouldCancel()) return;
                log("【阶段3】分类表单三表（保留 GUID）");
                foreach (var t in new[] { "form_meta_template_definition", "form_meta_template_ui", "form_meta_template_setting" })
                {
                    if (shouldCancel()) return;
                    log("  复制 " + t);
                    CopyTable(srcCfg, tgtCfg, t);
                }
            }
            if (InScope(scope, Scope.ConfigItem))
            {
                if (shouldCancel()) return;
                log("【阶段4】配置 config_item（仅最新版本 + 全字段核对后更新 content）");
                MigrateConfigItem(srcCfg, tgtCfg, log, shouldCancel);
            }

            log("迁移完成。请运行「校验」核对行数与外键完整性。");
        }

        // ============================================================
        // 通用表复制（保留 ID）
        // ============================================================
        private void CopyTable(string srcConn, string tgtConn, string table)
        {
            var srcCols = GetWritableColumns(srcConn, table);
            var tgtCols = GetWritableColumns(tgtConn, table);
            var use = srcCols.Where(c => tgtCols.Contains(c)).ToList();
            if (use.Count == 0) throw new Exception("源/目标表 " + table + " 无公共可写列，无法复制。");

            using (var sconn = new SqlConnection(srcConn))
            using (var tconn = new SqlConnection(tgtConn))
            {
                sconn.Open(); tconn.Open();
                string colList = string.Join("],[", use.Select(c => c.Name));
                using (var cmd = new SqlCommand("SELECT [" + colList + "] FROM [dbo].[" + table + "]", sconn))
                using (var rdr = cmd.ExecuteReader())
                using (var bcp = new SqlBulkCopy(tconn, SqlBulkCopyOptions.KeepIdentity, null))
                {
                    bcp.DestinationTableName = "[dbo].[" + table + "]";
                    bcp.BatchSize = 2000;
                    bcp.BulkCopyTimeout = 600;
                    foreach (var c in use) bcp.ColumnMappings.Add(c.Name, c.Name);
                    bcp.WriteToServer(rdr);
                }
            }
        }

        private List<ColumnInfo> GetWritableColumns(string conn, string table)
        {
            var list = new List<ColumnInfo>();
            using (var c = new SqlConnection(conn))
            {
                c.Open();
                const string sql = @"
                    SELECT c.COLUMN_NAME,
                        ISNULL(COLUMNPROPERTY(OBJECT_ID(QUOTENAME(c.TABLE_SCHEMA)+'.'+QUOTENAME(c.TABLE_NAME)), c.COLUMN_NAME, 'IsComputed'),0) AS IsComputed,
                        ISNULL(COLUMNPROPERTY(OBJECT_ID(QUOTENAME(c.TABLE_SCHEMA)+'.'+QUOTENAME(c.TABLE_NAME)), c.COLUMN_NAME, 'IsRowVersion'),0) AS IsRowVersion
                    FROM INFORMATION_SCHEMA.COLUMNS c
                    WHERE c.TABLE_SCHEMA='dbo' AND c.TABLE_NAME=@t
                    ORDER BY c.ORDINAL_POSITION";
                using (var cmd = new SqlCommand(sql, c))
                {
                    cmd.Parameters.AddWithValue("@t", table);
                    using (var r = cmd.ExecuteReader())
                        while (r.Read())
                        {
                            if ((int)r["IsComputed"] == 1) continue;
                            if ((int)r["IsRowVersion"] == 1) continue;
                            list.Add(new ColumnInfo { Name = (string)r["COLUMN_NAME"] });
                        }
                }
            }
            return list;
        }

        private class ColumnInfo { public string Name; }

        // ============================================================
        // 序列重设
        // ============================================================
        private void ResetSequences(string tgtMain, Action<string> log)
        {
            using (var c = new SqlConnection(tgtMain))
            {
                c.Open();
                foreach (var kv in SeqOf)
                {
                    // 每张表各自取其 MAX(OBJECT_ID)+1，重设对应序列，
                    // 避免全局统一值造成不同表 ID 跳号/取值错乱
                    long m;
                    using (var sel = new SqlCommand("SELECT ISNULL(MAX(OBJECT_ID),0) FROM [dbo].[" + kv.Key + "]", c))
                        m = Convert.ToInt64(sel.ExecuteScalar());
                    long next = m + 1;
                    try
                    {
                        using (var cmd = new SqlCommand("ALTER SEQUENCE [dbo].[" + kv.Value + "] RESTART WITH @next", c))
                        {
                            cmd.Parameters.AddWithValue("@next", next);
                            cmd.ExecuteNonQuery();
                        }
                        log("  序列 " + kv.Value + " 重设至 " + next + "（表 " + kv.Key + " 当前最大 " + m + "）");
                    }
                    catch (Exception ex)
                    {
                        log("  ⚠ 序列 " + kv.Value + " 重设失败：" + ex.Message);
                    }
                }
            }
        }

        // ============================================================
        // meta_element 覆盖式 upsert（Q3 全部 / Q5 覆盖）
        // ============================================================
        private void UpsertMetaElement(string srcCfg, string tgtCfg, Action<string> log, Func<bool> shouldCancel)
        {
            var rows = ReadAllRows(srcCfg, "meta_element");
            foreach (var row in rows)
            {
                if (shouldCancel()) return;
                string type = GetStr(row, "meta_type");
                string name = GetStr(row, "internal_name");
                if (ExistsMetaElement(tgtCfg, type, name))
                    UpdateMetaElement(tgtCfg, row, log);
                else
                    InsertMetaElement(tgtCfg, row, log);
            }
            log("  meta_element 处理 " + rows.Count + " 行。");
        }

        private bool ExistsMetaElement(string conn, string type, string name)
        {
            using (var c = new SqlConnection(conn))
            {
                c.Open();
                using (var cmd = new SqlCommand(
                    "SELECT COUNT(*) FROM [dbo].[meta_element] WHERE meta_type=@t AND internal_name=@n", c))
                {
                    cmd.Parameters.AddWithValue("@t", type);
                    cmd.Parameters.AddWithValue("@n", name);
                    return (int)cmd.ExecuteScalar() > 0;
                }
            }
        }

        private void UpdateMetaElement(string conn, Dictionary<string, object> row, Action<string> log)
        {
            using (var c = new SqlConnection(conn))
            {
                c.Open();
                using (var cmd = new SqlCommand(
                    "UPDATE [dbo].[meta_element] SET meta=@meta, state=@state, owner_type=@ot, owner_internal_name=@oin, lastmodify_datetime=GETDATE() " +
                    "WHERE meta_type=@t AND internal_name=@n", c))
                {
                    cmd.Parameters.AddWithValue("@meta", GetObj(row, "meta"));
                    cmd.Parameters.AddWithValue("@state", GetObj(row, "state"));
                    cmd.Parameters.AddWithValue("@ot", GetObj(row, "owner_type"));
                    cmd.Parameters.AddWithValue("@oin", GetObj(row, "owner_internal_name"));
                    cmd.Parameters.AddWithValue("@t", GetObj(row, "meta_type"));
                    cmd.Parameters.AddWithValue("@n", GetObj(row, "internal_name"));
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private void InsertMetaElement(string conn, Dictionary<string, object> row, Action<string> log)
        {
            using (var c = new SqlConnection(conn))
            {
                c.Open();
                using (var cmd = new SqlCommand(
                    "INSERT INTO [dbo].[meta_element] (meta_type, internal_name, meta, state, owner_type, owner_internal_name, creator, creation_datetime) " +
                    "VALUES (@t, @n, @meta, @state, @ot, @oin, @cr, GETDATE())", c))
                {
                    cmd.Parameters.AddWithValue("@t", GetObj(row, "meta_type"));
                    cmd.Parameters.AddWithValue("@n", GetObj(row, "internal_name"));
                    cmd.Parameters.AddWithValue("@meta", GetObj(row, "meta"));
                    cmd.Parameters.AddWithValue("@state", GetObj(row, "state"));
                    cmd.Parameters.AddWithValue("@ot", GetObj(row, "owner_type"));
                    cmd.Parameters.AddWithValue("@oin", GetObj(row, "owner_internal_name"));
                    cmd.Parameters.AddWithValue("@cr", GetObj(row, "creator"));
                    cmd.ExecuteNonQuery();
                }
            }
        }

        // ============================================================
        // config_item 迁移（仅最新版本 + 全字段核对后更新 content，需求 R2）
        // ============================================================
        private void MigrateConfigItem(string srcCfg, string tgtCfg, Action<string> log, Func<bool> shouldCancel)
        {
            var srcRows = ReadLatestConfigItems(srcCfg);
            int updated = 0, inserted = 0, conflict = 0;
            foreach (var r in srcRows)
            {
                if (shouldCancel()) return;
                var tgt = FindConfigItem(tgtCfg, r);
                if (tgt == null)
                {
                    InsertConfigItem(tgtCfg, r, log);
                    inserted++;
                }
                else if (AllMetaEqual(r, tgt))
                {
                    UpdateConfigItemContent(tgtCfg, r, log);
                    updated++;
                }
                else
                {
                    conflict++;
                    log("  ⚠ 冲突跳过：config_item(" + NaturalKey(r) + ") 元数据字段与目标不一致，未覆盖 content。");
                }
            }
            log(string.Format("  config_item 处理：更新 {0} / 新增 {1} / 冲突跳过 {2}。", updated, inserted, conflict));
        }

        private List<Dictionary<string, object>> ReadLatestConfigItems(string conn)
        {
            var rows = new List<Dictionary<string, object>>();
            using (var c = new SqlConnection(conn))
            {
                c.Open();
                const string sql = @"
                    WITH ranked AS (
                        SELECT *, ROW_NUMBER() OVER (
                            PARTITION BY top_level_catalog, second_level_catalog, third_level_catalog,
                                         config_directory, config_name, owner_type, owner_object_id
                            ORDER BY ISNULL(release_datetime, '1900-01-01'), ISNULL(lastmodify_datetime, '1900-01-01') DESC) AS rn
                        FROM [dbo].[config_item]
                    )
                    SELECT * FROM ranked WHERE rn = 1";
                using (var cmd = new SqlCommand(sql, c))
                using (var r = cmd.ExecuteReader())
                {
                    while (r.Read())
                    {
                        var dict = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
                        for (int i = 0; i < r.FieldCount; i++)
                            dict[r.GetName(i)] = r.IsDBNull(i) ? null : r.GetValue(i);
                        rows.Add(dict);
                    }
                }
            }
            return rows;
        }

        private Dictionary<string, object> FindConfigItem(string conn, Dictionary<string, object> src)
        {
            using (var c = new SqlConnection(conn))
            {
                c.Open();
                const string sql = @"
                    SELECT * FROM [dbo].[config_item]
                    WHERE top_level_catalog=@a AND second_level_catalog=@b AND third_level_catalog=@c
                      AND config_directory=@d AND config_name=@n AND owner_type=@ot AND owner_object_id=@oid AND version=@v";
                using (var cmd = new SqlCommand(sql, c))
                {
                    cmd.Parameters.AddWithValue("@a", GetObj(src, "top_level_catalog"));
                    cmd.Parameters.AddWithValue("@b", GetObj(src, "second_level_catalog"));
                    cmd.Parameters.AddWithValue("@c", GetObj(src, "third_level_catalog"));
                    cmd.Parameters.AddWithValue("@d", GetObj(src, "config_directory"));
                    cmd.Parameters.AddWithValue("@n", GetObj(src, "config_name"));
                    cmd.Parameters.AddWithValue("@ot", GetObj(src, "owner_type"));
                    cmd.Parameters.AddWithValue("@oid", GetObj(src, "owner_object_id"));
                    cmd.Parameters.AddWithValue("@v", GetObj(src, "version"));
                    using (var r = cmd.ExecuteReader())
                    {
                        if (r.Read())
                        {
                            var dict = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
                            for (int i = 0; i < r.FieldCount; i++)
                                dict[r.GetName(i)] = r.IsDBNull(i) ? null : r.GetValue(i);
                            return dict;
                        }
                    }
                }
            }
            return null;
        }

        private bool AllMetaEqual(Dictionary<string, object> src, Dictionary<string, object> tgt)
        {
            foreach (var kv in src)
            {
                if (CfgSkipCompare.Contains(kv.Key)) continue;
                object tv;
                if (!tgt.TryGetValue(kv.Key, out tv)) return false;
                if (!ValuesEqual(kv.Value, tv)) return false;
            }
            return true;
        }

        private bool ValuesEqual(object a, object b)
        {
            if (a == null && b == null) return true;
            if (a == null || b == null) return false;
            // 统一按字符串比较，规避 decimal/int/long 等类型差异
            return string.Equals(Convert.ToString(a), Convert.ToString(b), StringComparison.OrdinalIgnoreCase);
        }

        private void UpdateConfigItemContent(string conn, Dictionary<string, object> src, Action<string> log)
        {
            using (var c = new SqlConnection(conn))
            {
                c.Open();
                const string sql = @"
                    UPDATE [dbo].[config_item]
                    SET content=@content, release_datetime=GETDATE(), lastmodify_datetime=GETDATE()
                    WHERE top_level_catalog=@a AND second_level_catalog=@b AND third_level_catalog=@c
                      AND config_directory=@d AND config_name=@n AND owner_type=@ot AND owner_object_id=@oid AND version=@v";
                using (var cmd = new SqlCommand(sql, c))
                {
                    cmd.Parameters.AddWithValue("@content", GetObj(src, "content"));
                    cmd.Parameters.AddWithValue("@a", GetObj(src, "top_level_catalog"));
                    cmd.Parameters.AddWithValue("@b", GetObj(src, "second_level_catalog"));
                    cmd.Parameters.AddWithValue("@c", GetObj(src, "third_level_catalog"));
                    cmd.Parameters.AddWithValue("@d", GetObj(src, "config_directory"));
                    cmd.Parameters.AddWithValue("@n", GetObj(src, "config_name"));
                    cmd.Parameters.AddWithValue("@ot", GetObj(src, "owner_type"));
                    cmd.Parameters.AddWithValue("@oid", GetObj(src, "owner_object_id"));
                    cmd.Parameters.AddWithValue("@v", GetObj(src, "version"));
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private void InsertConfigItem(string conn, Dictionary<string, object> src, Action<string> log)
        {
            var tgtCols = GetWritableColumns(conn, "config_item").Select(x => x.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);
            var cols = src.Keys.Where(k => tgtCols.Contains(k)).ToList();
            string colList = string.Join(",", cols.Select(k => "[" + k + "]"));
            string paramList = string.Join(",", cols.Select(k => "@" + k));
            // 仅当 config_item 确含 IDENTITY 列时才显式开关，避免无 IDENTITY 时语句报错
            bool hasIdentity = TableHasIdentity(conn, "config_item");
            string pre = hasIdentity ? "SET IDENTITY_INSERT [dbo].[config_item] ON; " : string.Empty;
            string post = hasIdentity ? "; SET IDENTITY_INSERT [dbo].[config_item] OFF" : string.Empty;
            using (var c = new SqlConnection(conn))
            {
                c.Open();
                using (var cmd = new SqlCommand(pre + "INSERT INTO [dbo].[config_item] (" + colList + ") VALUES (" + paramList + ")" + post, c))
                {
                    foreach (var k in cols)
                        cmd.Parameters.AddWithValue("@" + k, GetObj(src, k));
                    cmd.ExecuteNonQuery();
                }
            }
        }

        /// <summary>判断表是否含 IDENTITY 列（决定能否 SET IDENTITY_INSERT）。</summary>
        private bool TableHasIdentity(string conn, string table)
        {
            using (var c = new SqlConnection(conn))
            {
                c.Open();
                using (var cmd = new SqlCommand(
                    "SELECT COUNT(*) FROM sys.identity_columns WHERE object_id = OBJECT_ID(QUOTENAME(@t))", c))
                {
                    cmd.Parameters.AddWithValue("@t", table);
                    return Convert.ToInt64(cmd.ExecuteScalar()) > 0;
                }
            }
        }

        // ============================================================
        // 小工具
        // ============================================================
        private static bool InScope(Scope s, Scope f) => (s & f) == f;

        private long CountRows(string conn, string table)
        {
            using (var c = new SqlConnection(conn))
            {
                c.Open();
                using (var cmd = new SqlCommand("SELECT COUNT(*) FROM [dbo].[" + table + "]", c))
                    return Convert.ToInt64(cmd.ExecuteScalar());
            }
        }

        private List<Dictionary<string, object>> ReadAllRows(string conn, string table)
        {
            var rows = new List<Dictionary<string, object>>();
            using (var c = new SqlConnection(conn))
            {
                c.Open();
                using (var cmd = new SqlCommand("SELECT * FROM [dbo].[" + table + "]", c))
                using (var r = cmd.ExecuteReader())
                {
                    while (r.Read())
                    {
                        var dict = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
                        for (int i = 0; i < r.FieldCount; i++)
                            dict[r.GetName(i)] = r.IsDBNull(i) ? null : r.GetValue(i);
                        rows.Add(dict);
                    }
                }
            }
            return rows;
        }

        private static string GetStr(Dictionary<string, object> row, string key)
        {
            object v; return row.TryGetValue(key, out v) && v != null ? Convert.ToString(v) : string.Empty;
        }
        private static object GetObj(Dictionary<string, object> row, string key)
        {
            object v; return row.TryGetValue(key, out v) ? (v ?? DBNull.Value) : DBNull.Value;
        }
        private static string NaturalKey(Dictionary<string, object> r)
        {
            return string.Format("{0}/{1}/{2}/{3}/{4}",
                GetStr(r, "top_level_catalog"), GetStr(r, "second_level_catalog"),
                GetStr(r, "config_directory"), GetStr(r, "config_name"), GetStr(r, "owner_object_id"));
        }

        // ============================================================
        // 备份清单持久化（本地 JSON）
        // ============================================================
        private class BackupItem { public string Table; public string Shadow; public string Conn; }
        private class BackupManifest
        {
            public string Tag;
            public string Time;
            public List<BackupItem> Items;
        }

        private static string BackupManifestPath(string tag)
        {
            string dir = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) ?? ".", "migration_backup");
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
            return Path.Combine(dir, "bak_" + tag + ".json");
        }
        private static string BackupIndexDir()
        {
            return Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) ?? ".", "migration_backup");
        }

        private void SaveBackupManifest(BackupManifest m)
        {
            string path = BackupManifestPath(m.Tag);
            // Conn 含敏感信息，仅持久化表名与影子表名（恢复时用当前目标连接串，忽略存储的 Conn）
            var slim = new { m.Tag, m.Time, Items = m.Items.Select(i => new { i.Table, i.Shadow }).ToList() };
            File.WriteAllText(path, Newtonsoft.Json.JsonConvert.SerializeObject(slim, Newtonsoft.Json.Formatting.Indented), Encoding.UTF8);
        }
        private void DeleteBackupManifest(string tag)
        {
            string path = BackupManifestPath(tag);
            if (File.Exists(path)) File.Delete(path);
        }
        private BackupManifest LoadLatestBackupManifest()
        {
            string dir = BackupIndexDir();
            if (!Directory.Exists(dir)) return null;
            var files = Directory.GetFiles(dir, "bak_*.json").OrderByDescending(f => f).ToArray();
            if (files.Length == 0) return null;
            string tag = Path.GetFileNameWithoutExtension(files[0]).Substring(4); // "bak_<tag>" -> "<tag>"
            return LoadBackupManifest(tag);
        }

        private BackupManifest LoadBackupManifest(string tag)
        {
            string path = BackupManifestPath(tag);
            if (!File.Exists(path)) return null;
            var jobj = Newtonsoft.Json.Linq.JObject.Parse(File.ReadAllText(path, Encoding.UTF8));
            var m = new BackupManifest
            {
                Tag = (string)jobj["Tag"],
                Time = (string)jobj["Time"],
                Items = new List<BackupItem>()
            };
            var items = jobj["Items"] as Newtonsoft.Json.Linq.JArray;
            if (items != null)
                foreach (var it in items)
                    m.Items.Add(new BackupItem { Table = (string)it["Table"], Shadow = (string)it["Shadow"], Conn = null });
            return m;
        }

        /// <summary>备份点信息（供 UI 展示）。</summary>
        public class BackupInfo
        {
            public string Tag;
            public string Time;
            public int TableCount;
            public List<string> Tables = new List<string>();
            public override string ToString()
            {
                return string.Format("{0}（{1}，{2} 张表）", Time, Tag, TableCount);
            }
        }

        /// <summary>列出本地所有可用备份点（按时间倒序，最新在前）。</summary>
        public List<BackupInfo> ListBackups()
        {
            var list = new List<BackupInfo>();
            string dir = BackupIndexDir();
            if (!Directory.Exists(dir)) return list;
            foreach (var f in Directory.GetFiles(dir, "bak_*.json").OrderByDescending(x => x))
            {
                try
                {
                    var jobj = Newtonsoft.Json.Linq.JObject.Parse(File.ReadAllText(f, Encoding.UTF8));
                    var arr = jobj["Items"] as Newtonsoft.Json.Linq.JArray;
                    var info = new BackupInfo { Tag = (string)jobj["Tag"], Time = (string)jobj["Time"] };
                    if (arr != null)
                        foreach (var it in arr)
                        {
                            string t = (string)it["Table"];
                            if (!string.IsNullOrEmpty(t)) info.Tables.Add(t);
                        }
                    info.TableCount = info.Tables.Count;
                    list.Add(info);
                }
                catch { /* 忽略损坏的清单文件 */ }
            }
            return list;
        }

        // ============================================================
        // 校验（迁移后行数对账 + 版本）
        // ============================================================
        public class VerifyRow
        {
            public string Database;
            public string Table;
            public long SrcRows;
            public long TgtRows;
            public bool Match;
        }
        public class VerifyResult
        {
            public List<VerifyRow> Rows = new List<VerifyRow>();
            public string Summary;
            public string SrcVersion;
            public string TgtVersion;
        }

        public VerifyResult Verify(string srcMain, string tgtMain, Scope scope)
        {
            string srcCfg = SwapDb(srcMain, ConfigDbName);
            string tgtCfg = SwapDb(tgtMain, ConfigDbName);
            var result = new VerifyResult();
            try { result.SrcVersion = ReadPlmVersion(srcMain); }
            catch (Exception ex) { result.SrcVersion = "检测失败:" + ex.Message; }
            try { result.TgtVersion = ReadPlmVersion(tgtMain); }
            catch (Exception ex) { result.TgtVersion = "检测失败:" + ex.Message; }

            if (InScope(scope, Scope.Classification))
                foreach (var t in MainTables)
                    AddVerifyRow(result, "主库", t, srcMain, tgtMain);
            if (InScope(scope, Scope.MetaElement))
                AddVerifyRow(result, "配置库", "meta_element", srcCfg, tgtCfg);
            if (InScope(scope, Scope.FormTemplates))
                foreach (var t in new[] { "form_meta_template_definition", "form_meta_template_ui", "form_meta_template_setting" })
                    AddVerifyRow(result, "配置库", t, srcCfg, tgtCfg);
            if (InScope(scope, Scope.ConfigItem))
                AddVerifyRow(result, "配置库", "config_item", srcCfg, tgtCfg);

            int mismatch = 0, total = 0;
            foreach (var r in result.Rows)
            {
                total++;
                if (!r.Match) mismatch++;
            }
            if (mismatch == 0)
                result.Summary = string.Format("全部 {0} 张表行数一致，迁移数据完整。", total);
            else
                result.Summary = string.Format("{0}/{1} 张表行数不一致，请检查迁移结果。", mismatch, total);
            return result;
        }

        private void AddVerifyRow(VerifyResult result, string db, string table, string srcConn, string tgtConn)
        {
            long sm, tm;
            sm = CountRows(srcConn, table);
            tm = CountRows(tgtConn, table);
            result.Rows.Add(new VerifyRow
            {
                Database = db,
                Table = table,
                SrcRows = sm,
                TgtRows = tm,
                Match = sm == tm
            });
        }
    }
}
