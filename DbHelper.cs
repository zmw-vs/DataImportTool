using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace DataImportTool
{
    /// <summary>
    /// 分类（含可选表单）插入结果
    /// </summary>
    public class ClassificationInsertResult
    {
        public long ClsId { get; set; }
        /// <summary>是否成功插入了表单模板</summary>
        public bool FormGenerated { get; set; }
        /// <summary>是否因该分类已存在表单而跳过插入</summary>
        public bool FormSkipped { get; set; }
        /// <summary>跳过时的提示信息</summary>
        public string FormMessage { get; set; }
    }

    public class DbHelper
    {
        private readonly string _connStr;

        /// <summary>2027 新增字段（2026 表中不存在），插入分类时按是否存在该列自动决定是否写入</summary>
        public const string ColSourceAbstractClsId = "SOURCE_ABSTRACT_CLS_ID";
        public const string TableClassification = "t_object_classification";
        public const string TableClassificationGroup = "t_object_classification_group";

        // 表列存在性缓存（key = "表名.列名"），避免每次插入都查元数据
        private readonly Dictionary<string, bool> _columnCache = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
        private readonly object _columnCacheLock = new object();
        private bool _plmVersionDetected = false;
        private PlmVersion _plmVersionCache = PlmVersion.Unknown;

        public DbHelper(string connectionString)
        {
            _connStr = connectionString;
        }

        public string TestConnection()
        {
            using (var conn = new SqlConnection(_connStr))
            {
                conn.Open();
                return conn.ServerVersion;
            }
        }

        /// <summary>
        /// PLM 版本检测结果
        /// </summary>
        public enum PlmVersion
        {
            Unknown = 0,
            V2026 = 2026,
            V2027 = 2027
        }

        /// <summary>
        /// 检测 PLM 版本：2027 的 T_OBJECT_CLASSIFICATION 表含 SOURCE_ABSTRACT_CLS_ID 列；2026 不含。
        /// 结果带缓存，整个 DbHelper 生命周期内只检测一次。
        /// </summary>
        public PlmVersion DetectPlmVersion()
        {
            if (_plmVersionDetected) return _plmVersionCache;
            _plmVersionCache = ClassificationHasSourceAbstractClsId() ? PlmVersion.V2027 : PlmVersion.V2026;
            _plmVersionDetected = true;
            return _plmVersionCache;
        }

        /// <summary>
        /// 判断指定表是否包含某列（结果缓存，每个组合只查一次）。
        /// 用于兼容 2026 / 2027 表结构差异：有该列才插入，没有则自动跳过。
        /// </summary>
        public bool TableHasColumn(string tableName, string columnName)
        {
            string tbl = (tableName ?? string.Empty).Trim();
            string col = (columnName ?? string.Empty).Trim();
            string key = tbl + "." + col;

            lock (_columnCacheLock)
            {
                bool cached;
                if (_columnCache.TryGetValue(key, out cached)) return cached;
            }

            bool exists = false;
            using (var conn = new SqlConnection(_connStr))
            {
                conn.Open();
                const string sql = @"SELECT COUNT(1) FROM INFORMATION_SCHEMA.COLUMNS
                                     WHERE TABLE_NAME = @t AND COLUMN_NAME = @c";
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@t", tbl);
                    cmd.Parameters.AddWithValue("@c", col);
                    exists = Convert.ToInt32(cmd.ExecuteScalar()) > 0;
                }
            }

            lock (_columnCacheLock) { _columnCache[key] = exists; }
            return exists;
        }

        /// <summary>分类表 t_object_classification 是否含 SOURCE_ABSTRACT_CLS_ID（2027 有，2026 没有）</summary>
        public bool ClassificationHasSourceAbstractClsId()
        {
            return TableHasColumn(TableClassification, ColSourceAbstractClsId);
        }

        /// <summary>分类组表 t_object_classification_group 是否含 SOURCE_ABSTRACT_CLS_ID</summary>
        public bool ClassificationGroupHasSourceAbstractClsId()
        {
            return TableHasColumn(TableClassificationGroup, ColSourceAbstractClsId);
        }

        private string GetSequenceName(string tableName)
        {
            switch (tableName.ToLower())
            {
                case "t_object_classification_group": return "SEQ_ID_OBJECT_CLASSIFICATION_GROUP";
                case "t_object_classification": return "SEQ_ID_OBJECT_CLASSIFICATION";
                case "t_object_classification_group_to_child": return "SEQ_ID_OBJECT_CLASSIFICATION_GROUP_TO_CHILD";
                case "t_object_classification_group_to_classification": return "SEQ_ID_OBJECT_CLASSIFICATION_GROUP_TO_CLASSIFICATION";
                case "t_property": return "SEQ_ID_PROPERTY";
                case "t_property_value": return "SEQ_ID_PROPERTY_VALUE";
                case "t_property_to_property_value": return "SEQ_ID_PROPERTY_TO_PROPERTY_VALUE";
                default: return null;
            }
        }

        private long GetNextIdFromSequence(SqlConnection conn, SqlTransaction tran, string tableName)
        {
            string sequenceName = GetSequenceName(tableName);
            if (string.IsNullOrEmpty(sequenceName))
                throw new InvalidOperationException($"表 {tableName} 没有对应的序列");
            using (var cmd = new SqlCommand($"SELECT NEXT VALUE FOR [dbo].[{sequenceName}]", conn, tran))
            {
                return Convert.ToInt64(cmd.ExecuteScalar());
            }
        }

        // ========== 分类组操作 ==========

        public long InsertClassificationGroup(string name)
        {
            return InsertClassificationNode(name, "分类组", 0, null);
        }

        public long InsertSubClassificationGroup(string name, long parentGroupId)
        {
            return InsertClassificationNode(name, "分类组", parentGroupId, null);
        }

        // ========== 分类操作（所有分类入口都经过此方法，自动插入 meta_element）==========

        public long InsertClassification(string name, long? groupId)
        {
            return InsertClassificationInternal(name, groupId, false, null);
        }

        public ClassificationInsertResult InsertClassificationWithForm(string name, long? groupId, bool generateForm, string templateUiContent)
        {
            var result = new ClassificationInsertResult();
            long clsId = InsertClassificationInternal(name, groupId, false, null);
            result.ClsId = clsId;
            if (generateForm && !string.IsNullOrEmpty(templateUiContent))
            {
                bool inserted = InsertFormTemplateForClassification(clsId, name, templateUiContent);
                if (inserted)
                    result.FormGenerated = true;
                else
                {
                    result.FormSkipped = true;
                    result.FormMessage = "该分类已存在对应表单，跳过表单插入";
                }
            }
            return result;
        }

        // ========== 通用分类节点插入（类型：分类组/分类）==========

        public long InsertClassificationNode(string name, string type, long parentId, string parentType)
        {
            if (string.Equals(type, "分类组", StringComparison.OrdinalIgnoreCase))
            {
                using (var conn = new SqlConnection(_connStr))
                {
                    conn.Open();
                    using (var tran = conn.BeginTransaction())
                    {
                        try
                        {
                            long newId = InsertClassificationGroupInternal(conn, tran, name, parentId);
                            tran.Commit();
                            return newId;
                        }
                        catch { tran.Rollback(); throw; }
                    }
                }
            }
            else
            {
                long? groupId = parentId > 0 ? (long?)parentId : null;
                return InsertClassificationInternal(name, groupId, false, null);
            }
        }

        // ========== 查询操作 ==========

        public List<ClassificationGroup> GetAllClassificationGroups()
        {
            var list = new List<ClassificationGroup>();
            const string sql = "SELECT OBJECT_ID, NAME FROM [dbo].[t_object_classification_group] ORDER BY OBJECT_ID";
            using (var conn = new SqlConnection(_connStr))
            using (var cmd = new SqlCommand(sql, conn))
            {
                conn.Open();
                using (var reader = cmd.ExecuteReader())
                    while (reader.Read())
                        list.Add(new ClassificationGroup { ObjectId = reader.GetInt64(0), Name = reader.IsDBNull(1) ? string.Empty : reader.GetString(1) });
            }
            return list;
        }

        public List<ClassificationGroup> GetAllClassifications()
        {
            var list = new List<ClassificationGroup>();
            // PARENT_ID 是分类自引用父子层级列；不同 PLM 版本可能缺失，用 TableHasColumn 兼容，避免写死版本号。
            bool hasParent = TableHasColumn("t_object_classification", "PARENT_ID");
            string sql = hasParent
                ? "SELECT OBJECT_ID, NAME, PARENT_ID FROM t_object_classification"
                : "SELECT OBJECT_ID, NAME FROM t_object_classification";
            using (var conn = new SqlConnection(_connStr))
            using (var cmd = new SqlCommand(sql, conn))
            {
                conn.Open();
                using (var reader = cmd.ExecuteReader())
                    while (reader.Read())
                    {
                        var g = new ClassificationGroup
                        {
                            ObjectId = reader.GetInt64(0),
                            Name = reader.IsDBNull(1) ? string.Empty : reader.GetString(1)
                        };
                        if (hasParent)
                            g.ParentId = reader.IsDBNull(2) ? 0 : reader.GetInt64(2);
                        list.Add(g);
                    }
            }
            return list;
        }

        public string GetClassificationNameById(long clsId)
        {
            const string sql = "SELECT NAME FROM [dbo].[t_object_classification] WHERE OBJECT_ID = @id";
            using (var conn = new SqlConnection(_connStr))
            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@id", clsId);
                conn.Open();
                var val = cmd.ExecuteScalar();
                return val == null || val == DBNull.Value ? string.Empty : val.ToString();
            }
        }

        /// <summary>
        /// 获取所有可用的属性内部名称（从 meta_element 中 meta_type='property'）
        /// </summary>
        public List<string> GetPropertyNamesFromMetaElement()
        {
            var names = new List<string>();
            const string sql = @"
        SELECT internal_name 
        FROM [plm_config_v2].[dbo].[meta_element] 
        WHERE meta_type = 'property'";

            using (var conn = new SqlConnection(_connStr))
            using (var cmd = new SqlCommand(sql, conn))
            {
                conn.Open();
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                        names.Add(reader.GetString(0));
                }
            }
            return names;
        }

        // ========== 匹配规则（match_rule）配置读写 ==========
        // 配置存放于 plm_config_v2.dbo.config_item，该表含【版本链】：同一 config_name 可有多条记录，
        // 旧版本为「还原点」（enable_rollback_flag=1），仅最新版本（release_datetime 最新）生效。
        // 本工具只读/写「最新版本」那条的 content 字段，不动 version / owner_type / owner_object_id，不删任何记录。
        private const string MatchRuleTopCatalog = "plm";
        private const string MatchRuleSecondCatalog = "service_config";
        private const string MatchRuleThirdCatalog = "persistence";
        private const string MatchRuleDirectory = "match_rule";

        /// <summary>读取指定 config_name 的匹配规则记录（含定位 owner_type/owner_object_id/version）。
        /// 定位「最新版本」：按 release_datetime 最新（未发布的 0001/空值排后），lastmodify_datetime 次之兜底。无记录返回 null。</summary>
        public MatchRuleRecord GetMatchRuleRecord(string configName)
        {
            const string sql = @"
                SELECT TOP 1 content, owner_type, owner_object_id, version
                FROM [plm_config_v2].[dbo].[config_item]
                WHERE top_level_catalog = @top AND second_level_catalog = @second
                  AND third_level_catalog = @third AND config_directory = @dir
                  AND config_name = @name
                ORDER BY release_datetime DESC, lastmodify_datetime DESC";
            using (var conn = new SqlConnection(_connStr))
            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@top", MatchRuleTopCatalog);
                cmd.Parameters.AddWithValue("@second", MatchRuleSecondCatalog);
                cmd.Parameters.AddWithValue("@third", MatchRuleThirdCatalog);
                cmd.Parameters.AddWithValue("@dir", MatchRuleDirectory);
                cmd.Parameters.AddWithValue("@name", configName);
                conn.Open();
                using (var rdr = cmd.ExecuteReader())
                {
                    if (!rdr.Read()) return null;
                    var rec = new MatchRuleRecord();
                    rec.Content = rdr.IsDBNull(0) ? null : Convert.ToString(rdr[0]);
                    rec.OwnerType = rdr.IsDBNull(1) ? 0 : Convert.ToInt32(rdr[1]);
                    rec.OwnerObjectId = rdr.IsDBNull(2) ? 0L : Convert.ToInt64(rdr[2]);
                    rec.Version = rdr.IsDBNull(3) ? null : Convert.ToString(rdr[3]);
                    return rec;
                }
            }
        }

        /// <summary>便捷方法：仅返回最新版本 content（无记录返回 null）。</summary>
        public string GetMatchRuleContent(string configName)
        {
            var rec = GetMatchRuleRecord(configName);
            return rec == null ? null : rec.Content;
        }

        /// <summary>
        /// 写回指定 config_name 的匹配规则 content（严格只写 content 字段）。
        /// 精确定位「最新版本」那一行（ownerType + ownerObjectId + version），
        /// 不动 version / owner_type / owner_object_id，不删除任何记录。
        /// 仅当该定位下无任何记录（首次无配置）才 INSERT 一行（沿用传入定位，version 兜底 v1.0.0），不覆盖既有记录。
        /// </summary>
        public void SaveMatchRuleContent(string configName, string content, int ownerType, long ownerObjectId, string version)
        {
            using (var conn = new SqlConnection(_connStr))
            {
                conn.Open();
                using (var tran = conn.BeginTransaction())
                {
                    try
                    {
                        // version 可能为 NULL（系统官方记录常见），空串无法匹配 NULL，故用「NULL 安全」谓词定位
                        string verParam = string.IsNullOrEmpty(version) ? string.Empty : version;

                        // 先判断目标行是否存在（定位元组 + NULL 安全 version），避免「UPDATE 0 行就 INSERT」导致重复行雪崩
                        const string existsSql = @"
                            SELECT COUNT(*) FROM [plm_config_v2].[dbo].[config_item]
                            WHERE top_level_catalog = @top AND second_level_catalog = @second
                              AND third_level_catalog = @third AND config_directory = @dir
                              AND config_name = @name AND (owner_type = @ot OR (owner_type IS NULL AND @ot = 0)) AND (owner_object_id = @oid OR (owner_object_id IS NULL AND @oid = 0))
                              AND (version = @ver OR (version IS NULL AND @ver = ''))";
                        int cnt;
                        using (var cmd = new SqlCommand(existsSql, conn, tran))
                        {
                            cmd.Parameters.AddWithValue("@top", MatchRuleTopCatalog);
                            cmd.Parameters.AddWithValue("@second", MatchRuleSecondCatalog);
                            cmd.Parameters.AddWithValue("@third", MatchRuleThirdCatalog);
                            cmd.Parameters.AddWithValue("@dir", MatchRuleDirectory);
                            cmd.Parameters.AddWithValue("@name", configName);
                            cmd.Parameters.AddWithValue("@ot", ownerType);
                            cmd.Parameters.AddWithValue("@oid", ownerObjectId);
                            cmd.Parameters.AddWithValue("@ver", verParam);
                            cnt = (int)cmd.ExecuteScalar();
                        }

                        if (cnt > 0)
                        {
                            // 已存在：只更新 content，并刷新 release_datetime 使自己保持「最新版本」（不碰 version/owner）
                            const string updateSql = @"
                                UPDATE [plm_config_v2].[dbo].[config_item]
                                SET content = @content, release_datetime = GETDATE(), lastmodify_datetime = GETDATE()
                                WHERE top_level_catalog = @top AND second_level_catalog = @second
                                  AND third_level_catalog = @third AND config_directory = @dir
                                  AND config_name = @name AND (owner_type = @ot OR (owner_type IS NULL AND @ot = 0)) AND (owner_object_id = @oid OR (owner_object_id IS NULL AND @oid = 0))
                                  AND (version = @ver OR (version IS NULL AND @ver = ''))";
                            using (var cmd = new SqlCommand(updateSql, conn, tran))
                            {
                                cmd.Parameters.AddWithValue("@content", content);
                                cmd.Parameters.AddWithValue("@top", MatchRuleTopCatalog);
                                cmd.Parameters.AddWithValue("@second", MatchRuleSecondCatalog);
                                cmd.Parameters.AddWithValue("@third", MatchRuleThirdCatalog);
                                cmd.Parameters.AddWithValue("@dir", MatchRuleDirectory);
                                cmd.Parameters.AddWithValue("@name", configName);
                                cmd.Parameters.AddWithValue("@ot", ownerType);
                                cmd.Parameters.AddWithValue("@oid", ownerObjectId);
                                cmd.Parameters.AddWithValue("@ver", verParam);
                                cmd.ExecuteNonQuery();
                            }
                        }
                        else
                        {
                            // 首次无配置：INSERT 一行。必须写入 release_datetime，否则读端 ORDER BY release_datetime DESC
                            // 会把它排到最后，导致每次保存都读不到它、又 INSERT 一条，形成「多余行」雪崩。
                            const string insertSql = @"
                                INSERT INTO [plm_config_v2].[dbo].[config_item]
                                    (owner_type, owner_object_id, owner_internal_name,
                                     top_level_catalog, second_level_catalog, third_level_catalog,
                                     config_directory, config_name, content, version, state, creator,
                                     creation_datetime, release_datetime, lastmodify_datetime)
                                VALUES (@ot, @oid, NULL,
                                     @top, @second, @third,
                                     @dir, @name, @content, @ver, 1, '系统', @now, @now, @now)";
                            using (var insert = new SqlCommand(insertSql, conn, tran))
                            {
                                insert.Parameters.AddWithValue("@ot", ownerType);
                                insert.Parameters.AddWithValue("@oid", ownerObjectId);
                                insert.Parameters.AddWithValue("@top", MatchRuleTopCatalog);
                                insert.Parameters.AddWithValue("@second", MatchRuleSecondCatalog);
                                insert.Parameters.AddWithValue("@third", MatchRuleThirdCatalog);
                                insert.Parameters.AddWithValue("@dir", MatchRuleDirectory);
                                insert.Parameters.AddWithValue("@name", configName);
                                insert.Parameters.AddWithValue("@content", content);
                                insert.Parameters.AddWithValue("@ver", verParam);
                                insert.Parameters.AddWithValue("@now", DateTime.Now);
                                insert.ExecuteNonQuery();
                            }
                        }

                        tran.Commit();
                    }
                    catch
                    {
                        tran.Rollback();
                        throw;
                    }
                }
            }
        }

        // ========== 领码设置（class_link_code_setting）配置读写 ==========
        // 与匹配规则一样，配置存放于 plm_config_v2.dbo.config_item 同一张版本链表。
        // 定位：third_level_catalog='code_management' AND config_directory='class_link_code_setting' AND config_name='part'
        // 读/写「最新版本」那条的 content 字段，不动 version / owner_type / owner_object_id，不删任何记录。
        private const string CodeLinkThirdCatalog = "code_management";
        private const string CodeLinkDirectory = "class_link_code_setting";
        private const string CodeLinkConfigName = "part";

        /// <summary>读取领码设置（class_link_code_setting/part）最新版本记录（含定位信息）。无记录返回 null。</summary>
        public MatchRuleRecord GetCodeLinkSettingRecord()
        {
            const string sql = @"
                SELECT TOP 1 content, owner_type, owner_object_id, version
                FROM [plm_config_v2].[dbo].[config_item]
                WHERE top_level_catalog = @top AND second_level_catalog = @second
                  AND third_level_catalog = @third AND config_directory = @dir
                  AND config_name = @name
                ORDER BY release_datetime DESC, lastmodify_datetime DESC";
            using (var conn = new SqlConnection(_connStr))
            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@top", "plm");
                cmd.Parameters.AddWithValue("@second", "service_config");
                cmd.Parameters.AddWithValue("@third", CodeLinkThirdCatalog);
                cmd.Parameters.AddWithValue("@dir", CodeLinkDirectory);
                cmd.Parameters.AddWithValue("@name", CodeLinkConfigName);
                conn.Open();
                using (var rdr = cmd.ExecuteReader())
                {
                    if (!rdr.Read()) return null;
                    var rec = new MatchRuleRecord();
                    rec.Content = rdr.IsDBNull(0) ? null : Convert.ToString(rdr[0]);
                    rec.OwnerType = rdr.IsDBNull(1) ? 0 : Convert.ToInt32(rdr[1]);
                    rec.OwnerObjectId = rdr.IsDBNull(2) ? 0L : Convert.ToInt64(rdr[2]);
                    rec.Version = rdr.IsDBNull(3) ? null : Convert.ToString(rdr[3]);
                    return rec;
                }
            }
        }

        /// <summary>
        /// 写回领码设置 content（严格只写 content 字段）。
        /// 精确定位「最新版本」那一行（ownerType + ownerObjectId + version），
        /// 不动 version / owner_type / owner_object_id，不删除任何记录。
        /// 仅当该定位下无任何记录（首次无配置）才 INSERT 一行（沿用传入定位，version 兜底 v1.0.0）。
        /// </summary>
        public void SaveCodeLinkSettingContent(string content, int ownerType, long ownerObjectId, string version)
        {
            using (var conn = new SqlConnection(_connStr))
            {
                conn.Open();
                using (var tran = conn.BeginTransaction())
                {
                    try
                    {
                        // version 可能为 NULL（系统官方记录常见），空串无法匹配 NULL，故用「NULL 安全」谓词定位
                        string verParam = string.IsNullOrEmpty(version) ? string.Empty : version;

                        // 先判断目标行是否存在（定位元组 + NULL 安全 version），避免「UPDATE 0 行就 INSERT」导致重复行雪崩
                        const string existsSql = @"
                            SELECT COUNT(*) FROM [plm_config_v2].[dbo].[config_item]
                            WHERE top_level_catalog = @top AND second_level_catalog = @second
                              AND third_level_catalog = @third AND config_directory = @dir
                              AND config_name = @name AND (owner_type = @ot OR (owner_type IS NULL AND @ot = 0)) AND (owner_object_id = @oid OR (owner_object_id IS NULL AND @oid = 0))
                              AND (version = @ver OR (version IS NULL AND @ver = ''))";
                        int cnt;
                        using (var cmd = new SqlCommand(existsSql, conn, tran))
                        {
                            cmd.Parameters.AddWithValue("@top", "plm");
                            cmd.Parameters.AddWithValue("@second", "service_config");
                            cmd.Parameters.AddWithValue("@third", CodeLinkThirdCatalog);
                            cmd.Parameters.AddWithValue("@dir", CodeLinkDirectory);
                            cmd.Parameters.AddWithValue("@name", CodeLinkConfigName);
                            cmd.Parameters.AddWithValue("@ot", ownerType);
                            cmd.Parameters.AddWithValue("@oid", ownerObjectId);
                            cmd.Parameters.AddWithValue("@ver", verParam);
                            cnt = (int)cmd.ExecuteScalar();
                        }

                        if (cnt > 0)
                        {
                            // 已存在：只更新 content，并刷新 release_datetime 使自己保持「最新版本」（不碰 version/owner）
                            const string updateSql = @"
                                UPDATE [plm_config_v2].[dbo].[config_item]
                                SET content = @content, release_datetime = GETDATE(), lastmodify_datetime = GETDATE()
                                WHERE top_level_catalog = @top AND second_level_catalog = @second
                                  AND third_level_catalog = @third AND config_directory = @dir
                                  AND config_name = @name AND (owner_type = @ot OR (owner_type IS NULL AND @ot = 0)) AND (owner_object_id = @oid OR (owner_object_id IS NULL AND @oid = 0))
                                  AND (version = @ver OR (version IS NULL AND @ver = ''))";
                            using (var cmd = new SqlCommand(updateSql, conn, tran))
                            {
                                cmd.Parameters.AddWithValue("@content", content);
                                cmd.Parameters.AddWithValue("@top", "plm");
                                cmd.Parameters.AddWithValue("@second", "service_config");
                                cmd.Parameters.AddWithValue("@third", CodeLinkThirdCatalog);
                                cmd.Parameters.AddWithValue("@dir", CodeLinkDirectory);
                                cmd.Parameters.AddWithValue("@name", CodeLinkConfigName);
                                cmd.Parameters.AddWithValue("@ot", ownerType);
                                cmd.Parameters.AddWithValue("@oid", ownerObjectId);
                                cmd.Parameters.AddWithValue("@ver", verParam);
                                cmd.ExecuteNonQuery();
                            }
                        }
                        else
                        {
                            // 首次无配置：INSERT 一行。必须写入 release_datetime，否则读端 ORDER BY release_datetime DESC
                            // 会把它排到最后，导致每次保存都读不到它、又 INSERT 一条，形成「多余行」雪崩。
                            const string insertSql = @"
                                INSERT INTO [plm_config_v2].[dbo].[config_item]
                                    (owner_type, owner_object_id, owner_internal_name,
                                     top_level_catalog, second_level_catalog, third_level_catalog,
                                     config_directory, config_name, content, version, state, creator,
                                     creation_datetime, release_datetime, lastmodify_datetime)
                                VALUES (@ot, @oid, NULL,
                                     @top, @second, @third,
                                     @dir, @name, @content, @ver, 1, '系统', @now, @now, @now)";
                            using (var insert = new SqlCommand(insertSql, conn, tran))
                            {
                                insert.Parameters.AddWithValue("@ot", ownerType);
                                insert.Parameters.AddWithValue("@oid", ownerObjectId);
                                insert.Parameters.AddWithValue("@top", "plm");
                                insert.Parameters.AddWithValue("@second", "service_config");
                                insert.Parameters.AddWithValue("@third", CodeLinkThirdCatalog);
                                insert.Parameters.AddWithValue("@dir", CodeLinkDirectory);
                                insert.Parameters.AddWithValue("@name", CodeLinkConfigName);
                                insert.Parameters.AddWithValue("@content", content);
                                insert.Parameters.AddWithValue("@ver", verParam);
                                insert.Parameters.AddWithValue("@now", DateTime.Now);
                                insert.ExecuteNonQuery();
                            }
                        }

                        tran.Commit();
                    }
                    catch
                    {
                        tran.Rollback();
                        throw;
                    }
                }
            }
        }

        /// <summary>获取元模型属性候选（internal_name + 中文名），去重，用于 match_rule 字段勾选</summary>
        public List<MetaPropertyOption> GetMetaPropertyOptions()
        {
            var list = new List<MetaPropertyOption>();
            const string sql = @"
                SELECT internal_name, MIN(meta) AS meta
                FROM [plm_config_v2].[dbo].[meta_element]
                WHERE meta_type = 'property' AND internal_name IS NOT NULL AND internal_name <> ''
                GROUP BY internal_name
                ORDER BY internal_name";
            using (var conn = new SqlConnection(_connStr))
            using (var cmd = new SqlCommand(sql, conn))
            {
                conn.Open();
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string internalName = reader.IsDBNull(0) ? string.Empty : reader.GetString(0);
                        string metaStr = reader.IsDBNull(1) ? null : reader.GetString(1);
                        list.Add(new MetaPropertyOption
                        {
                            InternalName = internalName,
                            DisplayName = ExtractZhCnFromMeta(metaStr, internalName),
                            IsSystemDefault = !ContainsChinese(ExtractEnUsFromMeta(metaStr))
                        });
                    }
                }
            }
            return list;
        }

        /// <summary>
        /// 获取所有可用的属性集（meta_element 中 meta_type='property_set' AND owner_type=1）
        /// DisplayName 优先取 meta.display_name.zh-cn，无则取 internal_name
        /// </summary>
        public List<MetaPropertySetItem> GetPropertySets()
        {
            var list = new List<MetaPropertySetItem>();
            const string sql = @"
        SELECT internal_name, meta
        FROM [plm_config_v2].[dbo].[meta_element]
        WHERE meta_type = 'property_set' AND owner_type = 1
        ORDER BY internal_name";

            using (var conn = new SqlConnection(_connStr))
            using (var cmd = new SqlCommand(sql, conn))
            {
                conn.Open();
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string internalName = reader.GetString(0);
                        string metaStr = reader.IsDBNull(1) ? null : reader.GetString(1);
                        string displayName = ExtractZhCnFromMeta(metaStr, internalName);
                        list.Add(new MetaPropertySetItem
                        {
                            InternalName = internalName,
                            DisplayName = displayName
                        });
                    }
                }
            }
            return list;
        }

        private static string ExtractZhCnFromMeta(string metaStr, string fallback)
        {
            if (string.IsNullOrEmpty(metaStr)) return fallback;
            try
            {
                var jo = JObject.Parse(metaStr);
                var dn = jo["display_name"];
                if (dn != null && dn["zh-cn"] != null)
                {
                    var zh = dn["zh-cn"].ToString();
                    if (!string.IsNullOrEmpty(zh)) return zh;
                }
            }
            catch { }
            return fallback;
        }

        /// <summary>从 meta JSON 提取 display_name.en-us（无则返回空串）</summary>
        private static string ExtractEnUsFromMeta(string metaStr)
        {
            if (string.IsNullOrEmpty(metaStr)) return string.Empty;
            try
            {
                var jo = JObject.Parse(metaStr);
                var dn = jo["display_name"] as JObject;
                if (dn != null && dn["en-us"] != null)
                    return dn["en-us"].ToString();
            }
            catch { }
            return string.Empty;
        }

        /// <summary>获取「生产类型」枚举项（part_source_type_lookup 的 items）</summary>
        public List<SourceTypeOption> GetSourceTypes()
        {
            var list = new List<SourceTypeOption>();
            const string sql = @"SELECT meta FROM [plm_config_v2].[dbo].[meta_element] WHERE meta_type='lookup' AND internal_name='part_source_type_lookup'";
            using (var conn = new SqlConnection(_connStr))
            using (var cmd = new SqlCommand(sql, conn))
            {
                conn.Open();
                var val = cmd.ExecuteScalar();
                if (val == null || val == DBNull.Value) return list;
                string metaStr = val.ToString();
                if (string.IsNullOrEmpty(metaStr)) return list;
                try
                {
                    var jo = JObject.Parse(metaStr);
                    var items = jo["items"] as JArray;
                    if (items == null) return list;
                    foreach (var item in items)
                    {
                        string key = item["internal_name"] != null ? item["internal_name"].ToString() : string.Empty;
                        if (string.IsNullOrEmpty(key)) continue;
                        string zh = string.Empty;
                        var dn = item["display_name"] as JObject;
                        if (dn != null && dn["zh-cn"] != null)
                            zh = dn["zh-cn"].ToString();
                        list.Add(new SourceTypeOption
                        {
                            InternalName = key,
                            DisplayName = string.IsNullOrEmpty(zh) ? key : zh
                        });
                    }
                }
                catch { }
            }
            return list;
        }

        /// <summary>
        /// 元模型属性导入（单事务）
        /// 1) 锁定所选属性集行
        /// 2) 逐行：若 meta_element (meta_type='property', internal_name=A) 不存在则 INSERT；已存在则跳过
        /// 3) 将本次去重后的 internal_name 追加到属性集 meta.properties 数组（已存在则跳过）
        /// 4) UPDATE 属性集行
        /// </summary>
        public MetaImportResult ImportMetaPropertySet(
            string propertySetInternalName,
            List<MetaPropertyRow> rows,
            System.Threading.CancellationToken token,
            Action<string> log,
            HashSet<string> skipUpdates = null)
        {
            var result = new MetaImportResult();
            if (string.IsNullOrEmpty(propertySetInternalName))
                throw new ArgumentException("propertySetInternalName 不能为空");
            if (rows == null || rows.Count == 0)
                return result;

            using (var conn = new SqlConnection(_connStr))
            {
                conn.Open();
                using (var tran = conn.BeginTransaction())
                {
                    try
                    {
                        token.ThrowIfCancellationRequested();
                        var r = ImportMetaPropertiesInTran(conn, tran, propertySetInternalName, rows, log, token, skipUpdates);
                        result.Inserted = r.Inserted;
                        result.SkippedExist = r.SkippedExist;
                        result.SkippedExistNames = r.SkippedExistNames;
                        result.Updated = r.Updated;
                        result.UpdatedNames = r.UpdatedNames;
                        result.PropertySetSkipped = r.PropertySetSkipped;
                        result.PropertySetSkippedNames = r.PropertySetSkippedNames;
                        tran.Commit();
                        return result;
                    }
                    catch
                    {
                        try { tran.Rollback(); } catch { }
                        throw;
                    }
                }
            }
        }

        /// <summary>
        /// 事务内导入元模型属性（供超级批量导入复用）。
        /// propertySetInternalName 为空时仅插入/更新 meta_element 属性，不写入属性集。
        /// </summary>
        internal MetaImportResult ImportMetaPropertiesInTran(
            SqlConnection conn,
            SqlTransaction tran,
            string propertySetInternalName,
            List<MetaPropertyRow> rows,
            Action<string> log,
            System.Threading.CancellationToken token = default,
            HashSet<string> skipUpdates = null)
        {
            var result = new MetaImportResult();
            if (rows == null || rows.Count == 0) return result;

            // 统一处理：propertySetInternalName 去空格 + 转小写
            if (!string.IsNullOrEmpty(propertySetInternalName))
                propertySetInternalName = propertySetInternalName.Trim().ToLower();

            bool hasSet = !string.IsNullOrEmpty(propertySetInternalName);

            // 1) 锁定属性集行 + 读取其 meta
            JObject propSetMeta = null;
            JArray propertiesArr = null;
            var existingInSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            if (hasSet)
            {
                const string selectSql = @"SELECT meta FROM [plm_config_v2].[dbo].[meta_element] WITH (UPDLOCK, ROWLOCK) WHERE meta_type = 'property_set' AND internal_name = @name";
                string metaStr;
                using (var cmd = new SqlCommand(selectSql, conn, tran))
                {
                    cmd.Parameters.AddWithValue("@name", propertySetInternalName);
                    var v = cmd.ExecuteScalar();
                    if (v == null || v == DBNull.Value)
                        throw new Exception("属性集 [" + propertySetInternalName + "] 不存在");
                    metaStr = v.ToString();
                }
                if (log != null) log("已锁定属性集 [" + propertySetInternalName + "]");

                try { propSetMeta = string.IsNullOrEmpty(metaStr) ? new JObject() : JObject.Parse(metaStr); }
                catch { propSetMeta = new JObject(); }

                propertiesArr = propSetMeta["properties"] as JArray;
                if (propertiesArr == null)
                {
                    propertiesArr = new JArray();
                    propSetMeta["properties"] = propertiesArr;
                }
                foreach (var p in propertiesArr)
                {
                    if (p != null && p.Type == JTokenType.String && !string.IsNullOrEmpty((string)p))
                        existingInSet.Add((string)p);
                }
            }

            // 2) 逐行处理
            const string checkSql = @"SELECT COUNT(1) FROM [plm_config_v2].[dbo].[meta_element] WHERE meta_type = 'property' AND internal_name = @name";
            const string insertSql = @"
                INSERT INTO [plm_config_v2].[dbo].[meta_element]
                    (internal_name, meta_type, state, owner_type, creator, creation_datetime, meta)
                VALUES
                    (@meName, 'property', 'release', 1, '试用员', @meDt, @meMeta)";

            var allUniqueNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var row in rows)
            {
                token.ThrowIfCancellationRequested();
                string internalName = (row.InternalName ?? string.Empty).Trim().ToLower();
                if (string.IsNullOrEmpty(internalName)) continue;
                if (!allUniqueNames.Add(internalName)) continue; // 同表内去重

                bool exists;
                using (var cmd = new SqlCommand(checkSql, conn, tran))
                {
                    cmd.Parameters.AddWithValue("@name", internalName);
                    exists = ((int)cmd.ExecuteScalar()) > 0;
                }

                if (exists)
                {
                    // 如果用户选择忽略此属性，则不更新 BCD，仅加到属性集（如果适用）
                    bool userSkipped = skipUpdates != null && skipUpdates.Contains(internalName);

                    if (userSkipped)
                    {
                        result.SkippedExist++;
                        result.SkippedExistNames.Add(internalName);
                        if (log != null) log("  [忽略] 属性 [" + internalName + "] 已存在，用户选择忽略，保留现有值");
                    }
                    else
                    {
                        string oldMetaStr;
                        const string selMetaSql = @"SELECT meta FROM [plm_config_v2].[dbo].[meta_element] WHERE meta_type = 'property' AND internal_name = @name";
                        using (var cmd = new SqlCommand(selMetaSql, conn, tran))
                        {
                            cmd.Parameters.AddWithValue("@name", internalName);
                            var m = cmd.ExecuteScalar();
                            oldMetaStr = (m == null || m == DBNull.Value) ? null : m.ToString();
                        }

                        JObject oldMeta;
                        try { oldMeta = string.IsNullOrEmpty(oldMetaStr) ? new JObject() : JObject.Parse(oldMetaStr); }
                        catch { oldMeta = new JObject(); }

                        string oldZhCn = GetMetaLang(oldMeta, "display_name", "zh-cn");
                        string oldEnUs = GetMetaLang(oldMeta, "display_name", "en-us");
                        string oldZhTw = GetMetaLang(oldMeta, "display_name", "zh-tw");

                        string newZhCn = (row.DisplayZhCn ?? string.Empty).Trim();
                        string newEnUs = (row.DisplayEnUs ?? string.Empty).Trim();
                        string newZhTw = (row.DisplayZhTw ?? string.Empty).Trim();

                        var dn = oldMeta["display_name"] as JObject ?? new JObject();
                        dn["zh-cn"] = newZhCn;
                        dn["en-us"] = newEnUs;
                        dn["zh-tw"] = newZhTw;
                        oldMeta["display_name"] = dn;

                        var ds = oldMeta["description"] as JObject ?? new JObject();
                        ds["zh-cn"] = newZhCn;
                        ds["en-us"] = newEnUs;
                        ds["zh-tw"] = newZhTw;
                        oldMeta["description"] = ds;

                        const string updMetaSql = @"UPDATE [plm_config_v2].[dbo].[meta_element] SET meta = @meta WHERE meta_type = 'property' AND internal_name = @name";
                        using (var cmd = new SqlCommand(updMetaSql, conn, tran))
                        {
                            cmd.Parameters.AddWithValue("@meta", oldMeta.ToString(Newtonsoft.Json.Formatting.None));
                            cmd.Parameters.AddWithValue("@name", internalName);
                            cmd.ExecuteNonQuery();
                        }

                        var changes = new List<string>();
                        if (oldZhCn != newZhCn) changes.Add(string.Format("zh-cn: [{0}]→[{1}]", oldZhCn, newZhCn));
                        if (oldEnUs != newEnUs) changes.Add(string.Format("en-us: [{0}]→[{1}]", oldEnUs, newEnUs));
                        if (oldZhTw != newZhTw) changes.Add(string.Format("zh-tw: [{0}]→[{1}]", oldZhTw, newZhTw));

                        result.SkippedExist++;
                        result.SkippedExistNames.Add(internalName);
                        if (changes.Count > 0)
                        {
                            result.Updated++;
                            result.UpdatedNames.Add(internalName);
                            if (log != null) log("  [更新] 属性 [" + internalName + "] 已存在，已更新名称 → " + string.Join("; ", changes));
                        }
                        else
                        {
                            if (log != null) log("  [跳过] 属性 [" + internalName + "] 已存在，BCD 与现有值一致，无需更新");
                        }
                    }
                }
                else
                {
                    string zhCn = (row.DisplayZhCn ?? string.Empty).Trim();
                    string enUs = (row.DisplayEnUs ?? string.Empty).Trim();
                    string zhTw = (row.DisplayZhTw ?? string.Empty).Trim();
                    string dtStr = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fffffff");

                    JObject propMeta = new JObject();
                    propMeta["meta_type"] = "property";
                    propMeta["internal_name"] = internalName;
                    propMeta["field"] = internalName;
                    var dn = new JObject();
                    dn["en-us"] = enUs;
                    dn["zh-cn"] = zhCn;
                    dn["zh-tw"] = zhTw;
                    propMeta["display_name"] = dn;
                    var ds = new JObject();
                    ds["en-us"] = enUs;
                    ds["zh-cn"] = zhCn;
                    ds["zh-tw"] = zhTw;
                    propMeta["description"] = ds;
                    propMeta["visibility"] = "public";
                    propMeta["data_type"] = "string";
                    propMeta["length"] = 256;
                    propMeta["ref_lookup"] = "";
                    propMeta["default_val"] = "";
                    propMeta["enum_values"] = JValue.CreateNull();
                    propMeta["min"] = "";
                    propMeta["max"] = "";
                    propMeta["precision"] = "";
                    propMeta["array_element_type"] = "";
                    propMeta["use_type"] = "";

                    using (var cmd = new SqlCommand(insertSql, conn, tran))
                    {
                        cmd.Parameters.AddWithValue("@meName", internalName);
                        cmd.Parameters.AddWithValue("@meDt", dtStr);
                        cmd.Parameters.AddWithValue("@meMeta", propMeta.ToString(Newtonsoft.Json.Formatting.None));
                        cmd.ExecuteNonQuery();
                    }
                    result.Inserted++;
                    if (log != null) log("  [插入] 属性 [" + internalName + "]");
                }

                // 追加到属性集（已存在则去重跳过）
                if (hasSet)
                {
                    if (!existingInSet.Contains(internalName))
                    {
                        propertiesArr.Add(internalName);
                        existingInSet.Add(internalName);
                    }
                    else
                    {
                        result.PropertySetSkipped++;
                        result.PropertySetSkippedNames.Add(internalName);
                    }
                }
            }

            // 3) UPDATE 属性集
            if (hasSet)
            {
                const string updateSql = @"UPDATE [plm_config_v2].[dbo].[meta_element] SET meta = @meta WHERE meta_type = 'property_set' AND internal_name = @name";
                using (var cmd = new SqlCommand(updateSql, conn, tran))
                {
                    cmd.Parameters.AddWithValue("@meta", propSetMeta.ToString(Newtonsoft.Json.Formatting.None));
                    cmd.Parameters.AddWithValue("@name", propertySetInternalName);
                    cmd.ExecuteNonQuery();
                }
                if (log != null) log("已更新属性集 [" + propertySetInternalName + "] meta.properties");
            }

            return result;
        }
        /// <summary>
        /// 批量检查属性内部名称是否在 meta_element 中存在（meta_type='property''）
        /// 返回其中不存在的名称集合
        /// </summary>
        public HashSet<string> GetInvalidPropertyNames(IEnumerable<string> internalNames)
        {
            var invalid = new HashSet<string>(internalNames, StringComparer.OrdinalIgnoreCase);
            if (invalid.Count == 0) return invalid;

            var namesList = invalid.ToList();
            var parameters = new List<string>();
            for (int i = 0; i < namesList.Count; i++)
                parameters.Add($"@p{i}");

            string sql = $@"
        SELECT internal_name 
        FROM [plm_config_v2].[dbo].[meta_element] 
        WHERE meta_type = 'property' 
          AND internal_name IN ({string.Join(",", parameters)})";

            using (var conn = new SqlConnection(_connStr))
            using (var cmd = new SqlCommand(sql, conn))
            {
                for (int i = 0; i < namesList.Count; i++)
                    cmd.Parameters.AddWithValue($"@p{i}", namesList[i]);

                conn.Open();
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string name = reader.GetString(0);
                        invalid.Remove(name);   // 存在的从无效集合中移除
                    }
                }
            }
            return invalid;
        }

        /// <summary>
        /// 根据属性内部名称列表，获取每个属性的显示名称（zh-cn 优先）
        /// </summary>
        public List<PropertyMetaInfo> GetPropertyMetaItems(List<string> propertyNames)
        {
            var result = new List<PropertyMetaInfo>();
            if (propertyNames == null || propertyNames.Count == 0) return result;

            var placeholders = new List<string>();
            for (int i = 0; i < propertyNames.Count; i++)
                placeholders.Add($"@p{i}");

            string sql = $@"
        SELECT internal_name, meta 
        FROM [plm_config_v2].[dbo].[meta_element] 
        WHERE internal_name IN ({string.Join(",", placeholders)})";

            using (var conn = new SqlConnection(_connStr))
            using (var cmd = new SqlCommand(sql, conn))
            {
                for (int i = 0; i < propertyNames.Count; i++)
                    cmd.Parameters.AddWithValue($"@p{i}", propertyNames[i]);

                conn.Open();
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string internalName = reader.GetString(0);
                        string metaRaw = reader.IsDBNull(1) ? string.Empty : reader.GetString(1);
                        string displayName = internalName;
                        if (!string.IsNullOrEmpty(metaRaw))
                        {
                            try
                            {
                                var obj = JObject.Parse(metaRaw);
                                displayName = (obj["display_name"]?["zh-cn"]
                                            ?? obj["display_name"]
                                            ?? obj["name"])?.ToString()
                                            ?? internalName;
                            }
                            catch { }
                        }
                        result.Add(new PropertyMetaInfo
                        {
                            InternalName = internalName,
                            DisplayName = displayName
                        });
                    }
                }
            }
            return result;
        }

        // ========== 属性操作 ==========

        public List<PropertyItem> GetRootProperties()
        {
            var list = new List<PropertyItem>();
            const string sql = @"SELECT OBJECT_ID, NAME, INTERNAL_NAME, CLS_ID FROM [dbo].[t_property] WHERE CLS_INTERNAL_NAME IS NULL AND CLS_ID = 0 ORDER BY OBJECT_ID";
            using (var conn = new SqlConnection(_connStr))
            using (var cmd = new SqlCommand(sql, conn))
            {
                conn.Open();
                using (var reader = cmd.ExecuteReader())
                    while (reader.Read()) list.Add(ReadPropertyItem(reader));
            }
            return list;
        }

        public List<PropertyItem> GetNonRootProperties()
        {
            var list = new List<PropertyItem>();
            const string sql = @"SELECT OBJECT_ID, NAME, INTERNAL_NAME, CLS_ID FROM [dbo].[t_property] WHERE CLS_INTERNAL_NAME IS NOT NULL AND CLS_ID <> 0 ORDER BY OBJECT_ID";
            using (var conn = new SqlConnection(_connStr))
            using (var cmd = new SqlCommand(sql, conn))
            {
                conn.Open();
                using (var reader = cmd.ExecuteReader())
                    while (reader.Read()) list.Add(ReadPropertyItem(reader));
            }
            return list;
        }

        public List<PropertyItem> GetPropertiesByClassificationId(long clsId)
        {
            var list = new List<PropertyItem>();
            const string sql = @"SELECT OBJECT_ID, NAME, INTERNAL_NAME, CLS_ID FROM [dbo].[t_property] WHERE CLS_INTERNAL_NAME IS NOT NULL AND CLS_ID = @clsId ORDER BY OBJECT_ID";
            using (var conn = new SqlConnection(_connStr))
            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@clsId", clsId);
                conn.Open();
                using (var reader = cmd.ExecuteReader())
                    while (reader.Read()) list.Add(ReadPropertyItem(reader));
            }
            return list;
        }

        /// <summary>
        /// 按 t_property 的 CLS_ID 枚举"实际有属性的分类"：取 CLS_ID 非空且非 0 的去重值，关联 t_object_classification 取分类名。
        /// （属性管理里判断一个分类有没有属性，就是看有没有带这个 cls_id 的属性）
        /// </summary>
        public List<ClassificationGroup> GetClsIdsHavingProperties()
        {
            var list = new List<ClassificationGroup>();
            const string sql = @"
                SELECT DISTINCT p.CLS_ID, ISNULL(c.NAME, '分类#' + CAST(p.CLS_ID AS VARCHAR(20)))
                FROM [dbo].[t_property] p
                LEFT JOIN [dbo].[t_object_classification] c ON p.CLS_ID = c.OBJECT_ID
                WHERE p.CLS_ID IS NOT NULL AND p.CLS_ID <> 0
                ORDER BY p.CLS_ID";
            using (var conn = new SqlConnection(_connStr))
            using (var cmd = new SqlCommand(sql, conn))
            {
                conn.Open();
                using (var reader = cmd.ExecuteReader())
                    while (reader.Read())
                        list.Add(new ClassificationGroup
                        {
                            ObjectId = reader.GetInt64(0),
                            Name = reader.IsDBNull(1) ? string.Empty : reader.GetString(1)
                        });
            }
            return list;
        }

        /// <summary>
        /// 按 CLS_ID 取分类下的属性。这里不加 CLS_INTERNAL_NAME 非空的条件，
        /// 否则会漏掉"CLS_ID 有值但 CLS_INTERNAL_NAME 为空"的数据（和 GetClsIdsHavingProperties 的判定保持一致）。
        /// </summary>
        public List<PropertyItem> GetPropertiesByClsId(long clsId)
        {
            var list = new List<PropertyItem>();
            const string sql = @"SELECT OBJECT_ID, NAME, INTERNAL_NAME, CLS_ID FROM [dbo].[t_property] WHERE CLS_ID = @clsId ORDER BY OBJECT_ID";
            using (var conn = new SqlConnection(_connStr))
            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@clsId", clsId);
                conn.Open();
                using (var reader = cmd.ExecuteReader())
                    while (reader.Read()) list.Add(ReadPropertyItem(reader));
            }
            return list;
        }

        private PropertyItem ReadPropertyItem(SqlDataReader reader)
        {
            return new PropertyItem
            {
                ObjectId = reader.GetInt64(0),
                Name = reader.IsDBNull(1) ? string.Empty : reader.GetString(1),
                InternalName = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
                ClsId = reader.IsDBNull(3) ? (long?)null : reader.GetInt64(3)
            };
        }

        public long InsertProperty(string propertyInternalName, string propertyDisplayName, long? clsId)
        {
            // 统一标准化：去空格 + 转小写
            propertyInternalName = (propertyInternalName ?? string.Empty).Trim().ToLower();
            if (propertyInternalName.Contains(" "))
                throw new ArgumentException(string.Format("属性内部名 [{0}] 包含空格，不允许", propertyInternalName));

            using (var conn = new SqlConnection(_connStr))
            {
                conn.Open();
                using (var tran = conn.BeginTransaction())
                {
                    try
                    {
                        var now = DateTime.Now;
                        long newId = GetNextIdFromSequence(conn, tran, "t_property");
                        const string sql = @"
                            INSERT INTO [dbo].[t_property]
                                (OBJECT_ID, INTERNAL_NAME, CLASS_INTERNAL_NAME, CLS_INTERNAL_NAME, CLS_ID,
                                 PROPERTY_INTERNAL_NAME, SUPPORT_MANUAL_FLAG, SELECTION_PROPERTIES,
                                 NAME, CREATOR, CREATOR_OBJECT_ID, CREATOR_INTERNAL_NAME, CREATION_DATETIME)
                            VALUES
                                (@oid, 'property', 'part',
                                 @clsInternalName, @clsId,
                                 @propInternalName, 0, '{}',
                                 @name, '试用员', 9007199254740990, 'person', @dt)";
                        using (var cmd = new SqlCommand(sql, conn, tran))
                        {
                            cmd.Parameters.AddWithValue("@oid", newId);
                            cmd.Parameters.AddWithValue("@clsInternalName", clsId.HasValue && clsId.Value != 0 ? "object_classification" : (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@clsId", clsId ?? (object)0);
                            cmd.Parameters.AddWithValue("@propInternalName", propertyInternalName);
                            cmd.Parameters.AddWithValue("@name", propertyDisplayName);
                            cmd.Parameters.AddWithValue("@dt", now);
                            cmd.ExecuteNonQuery();
                        }
                        tran.Commit();
                        return newId;
                    }
                    catch { tran.Rollback(); throw; }
                }
            }
        }

        public void DeleteProperty(long objectId)
        {
            const string sql = "DELETE FROM [dbo].[t_property] WHERE OBJECT_ID = @objectId";
            using (var conn = new SqlConnection(_connStr))
            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@objectId", objectId);
                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        // ========== 属性值操作 ==========

        public long InsertPropertyValue(string propertyNumber, string propertyValue)
        {
            using (var conn = new SqlConnection(_connStr))
            {
                conn.Open();
                using (var tran = conn.BeginTransaction())
                {
                    try
                    {
                        var now = DateTime.Now;
                        long newId = GetNextIdFromSequence(conn, tran, "t_property_value");
                        const string sql = @"
                            INSERT INTO [dbo].[t_property_value]
                                (OBJECT_ID, INTERNAL_NAME, PROPERTY_VALUE, PROPERTY_NUMBER,
                                 PROPERTY_VALUE_NUMBER, DEFAULT_VALUE_FLAG,
                                 CREATOR, CREATOR_OBJECT_ID, CREATOR_INTERNAL_NAME, CREATION_DATETIME)
                            VALUES
                                (@oid, 'property_value', @propValue, @propNum,
                                 2, 0,
                                 '试用员', 9007199254740990, 'person', @dt)";
                        using (var cmd = new SqlCommand(sql, conn, tran))
                        {
                            cmd.Parameters.AddWithValue("@oid", newId);
                            cmd.Parameters.AddWithValue("@propValue", propertyValue ?? string.Empty);
                            cmd.Parameters.AddWithValue("@propNum", propertyNumber ?? string.Empty);
                            cmd.Parameters.AddWithValue("@dt", now);
                            cmd.ExecuteNonQuery();
                        }
                        tran.Commit();
                        return newId;
                    }
                    catch { tran.Rollback(); throw; }
                }
            }
        }

        public void InsertPropertyToPropertyValue(long propertyObjectId, long propertyValueObjectId)
        {
            using (var conn = new SqlConnection(_connStr))
            {
                conn.Open();
                using (var tran = conn.BeginTransaction())
                {
                    try
                    {
                        var now = DateTime.Now;
                        long newId = GetNextIdFromSequence(conn, tran, "t_property_to_property_value");
                        const string sql = @"
                            INSERT INTO [dbo].[t_property_to_property_value]
                                (OBJECT_ID, INTERNAL_NAME, ROLEA_INTERNAL_NAME, ROLEA_OBJECT_ID,
                                 ROLEB_INTERNAL_NAME, ROLEB_OBJECT_ID,
                                 CREATOR, CREATOR_OBJECT_ID, CREATOR_INTERNAL_NAME, CREATION_DATETIME)
                            VALUES
                                (@oid, 'property_to_property_value', 'property', @propId,
                                 'property_value', @propValId,
                                 '试用员', 9007199254740990, 'person', @dt)";
                        using (var cmd = new SqlCommand(sql, conn, tran))
                        {
                            cmd.Parameters.AddWithValue("@oid", newId);
                            cmd.Parameters.AddWithValue("@propId", propertyObjectId);
                            cmd.Parameters.AddWithValue("@propValId", propertyValueObjectId);
                            cmd.Parameters.AddWithValue("@dt", now);
                            cmd.ExecuteNonQuery();
                        }
                        tran.Commit();
                    }
                    catch { tran.Rollback(); throw; }
                }
            }
        }

        /// <summary>
        /// 按属性值文本查找已存在的属性值 OBJECT_ID。用于跨属性复用同一值，避免 t_property_value 堆重复行。
        /// 找不到返回 0。
        /// </summary>
        public long FindPropertyValueByText(string propertyValue)
        {
            const string sql = "SELECT TOP 1 OBJECT_ID FROM [dbo].[t_property_value] WHERE PROPERTY_VALUE = @val ORDER BY OBJECT_ID";
            using (var conn = new SqlConnection(_connStr))
            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@val", propertyValue ?? string.Empty);
                conn.Open();
                var obj = cmd.ExecuteScalar();
                return (obj == null || obj == DBNull.Value) ? 0 : Convert.ToInt64(obj);
            }
        }

        /// <summary>
        /// 判断某属性是否已关联指定文本的属性值（关联表 join 值表文本判断）。用于预览"已存在跳过"与执行去重。
        /// </summary>
        public bool PropertyValueTextLinkedToProperty(long propertyObjectId, string propertyValue)
        {
            const string sql = @"
                SELECT COUNT(1)
                FROM [dbo].[t_property_to_property_value] t
                INNER JOIN [dbo].[t_property_value] v ON t.ROLEB_OBJECT_ID = v.OBJECT_ID
                WHERE t.ROLEA_OBJECT_ID = @propId AND v.PROPERTY_VALUE = @propVal";
            using (var conn = new SqlConnection(_connStr))
            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@propId", propertyObjectId);
                cmd.Parameters.AddWithValue("@propVal", propertyValue ?? string.Empty);
                conn.Open();
                var cnt = Convert.ToInt64(cmd.ExecuteScalar());
                return cnt > 0;
            }
        }

        // ========== 表单模板 ==========

        public List<FormTemplateInfo> GetFormTemplateList()
        {
            var list = new List<FormTemplateInfo>();
            const string sql = @"
                SELECT id, display_name
                FROM [plm_config_v2].[dbo].[form_meta_template_definition]
                WHERE class_internal_name = 'part'
                ORDER BY creation_datetime DESC";
            using (var conn = new SqlConnection(_connStr))
            using (var cmd = new SqlCommand(sql, conn))
            {
                conn.Open();
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string id = reader.GetString(0);
                        string displayNameRaw = reader.IsDBNull(1) ? string.Empty : reader.GetString(1);
                        string displayName = id;
                        if (!string.IsNullOrEmpty(displayNameRaw))
                        {
                            try
                            {
                                var obj = JObject.Parse(displayNameRaw);
                                displayName = (obj["zh-cn"] ?? obj["en-us"] ?? obj["display_name"])?.ToString() ?? id;
                            }
                            catch { displayName = displayNameRaw; }
                        }
                        list.Add(new FormTemplateInfo { Id = id, DisplayName = displayName });
                    }
                }
            }
            return list;
        }

        public string GetFormTemplateUiContent(string templateId)
        {
            const string sql = @"SELECT TOP 1 content FROM [plm_config_v2].[dbo].[form_meta_template_ui] WHERE template_id = @tid";
            using (var conn = new SqlConnection(_connStr))
            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@tid", templateId);
                conn.Open();
                var val = cmd.ExecuteScalar();
                return val == null || val == DBNull.Value ? string.Empty : val.ToString();
            }
        }

        // ========== 分类表单模板字段（用于 match_rule 预选） ==========

        /// <summary>
        /// 获取指定分类表单模板中的字段列表（区分用户自定义/系统默认）。
        /// 关联：form_meta_template_definition(classification_object_id) -> form_meta_template_ui(template_id=definition.id)。
        /// 系统默认字段判定：display_name.en-us 为纯英文（不含中文字符）；
        /// 用户自定义（拼音）字段的 en-us 通常混入中文，据此区分。
        /// </summary>
        public List<FormTemplateField> GetFormTemplateFieldsByClassification(long clsId)
        {
            var result = new List<FormTemplateField>();

            string templateId = null;
            const string defSql = @"
                SELECT TOP 1 id
                FROM [plm_config_v2].[dbo].[form_meta_template_definition]
                WHERE classification_object_id = @cid AND state = 1
                ORDER BY owner_type DESC";
            using (var conn = new SqlConnection(_connStr))
            using (var cmd = new SqlCommand(defSql, conn))
            {
                cmd.Parameters.AddWithValue("@cid", clsId);
                conn.Open();
                var v = cmd.ExecuteScalar();
                if (v != null && v != DBNull.Value) templateId = v.ToString();
            }

            if (string.IsNullOrEmpty(templateId)) return result;

            string content = null;
            const string uiSql = @"SELECT TOP 1 content FROM [plm_config_v2].[dbo].[form_meta_template_ui] WHERE template_id = @tid";
            using (var conn = new SqlConnection(_connStr))
            using (var cmd = new SqlCommand(uiSql, conn))
            {
                cmd.Parameters.AddWithValue("@tid", templateId);
                conn.Open();
                var v = cmd.ExecuteScalar();
                if (v != null && v != DBNull.Value) content = v.ToString();
            }

            if (!string.IsNullOrWhiteSpace(content))
            {
                try
                {
                    var root = JObject.Parse(content);
                    var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                    CollectFormTemplateFields(root, result, seen);
                }
                catch { }
            }
            return result;
        }

        private static void CollectFormTemplateFields(JToken node, List<FormTemplateField> list, HashSet<string> seen)
        {
            var obj = node as JObject;
            if (obj != null)
            {
                var prop = obj["property"];
                if (prop != null && prop.Type == JTokenType.String)
                {
                    string p = prop.ToString();
                    if (!string.IsNullOrEmpty(p) && seen.Add(p))
                    {
                        string en = string.Empty;
                        var dn = obj["display_name"] as JObject;
                        if (dn != null && dn["en-us"] != null)
                            en = dn["en-us"].ToString();
                        list.Add(new FormTemplateField { Property = p, IsSystemDefault = !ContainsChinese(en) });
                    }
                }
                foreach (var v in obj.Values())
                    CollectFormTemplateFields(v, list, seen);
            }
            else
            {
                var arr = node as JArray;
                if (arr != null)
                    foreach (var v in arr)
                        CollectFormTemplateFields(v, list, seen);
            }
        }

        private static bool ContainsChinese(string text)
        {
            if (string.IsNullOrEmpty(text)) return false;
            foreach (char c in text)
                if (c >= '\u4e00' && c <= '\u9fff')
                    return true;
            return false;
        }

        public Dictionary<string, long> GetPropertyDictionaryByClassification(long clsId, bool isRoot)
        {
            var dict = new Dictionary<string, long>(StringComparer.OrdinalIgnoreCase);
            string sql = isRoot
                ? @"SELECT OBJECT_ID, NAME FROM [dbo].[t_property] WHERE CLS_INTERNAL_NAME IS NULL AND CLS_ID = 0 ORDER BY NAME"
                : @"SELECT OBJECT_ID, NAME FROM [dbo].[t_property] WHERE CLS_INTERNAL_NAME IS NOT NULL AND CLS_ID = @clsId ORDER BY NAME";
            using (var conn = new SqlConnection(_connStr))
            using (var cmd = new SqlCommand(sql, conn))
            {
                if (!isRoot) cmd.Parameters.AddWithValue("@clsId", clsId);
                conn.Open();
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        long oid = reader.GetInt64(0);
                        string nm = reader.IsDBNull(1) ? string.Empty : reader.GetString(1);
                        if (!string.IsNullOrEmpty(nm)) dict[nm] = oid;
                    }
                }
            }
            return dict;
        }

        // ================== 私有内部方法 ==================

        private long InsertClassificationGroupInternal(SqlConnection conn, SqlTransaction tran, string name, long parentId)
        {
            var now = DateTime.Now;
            long newId = GetNextIdFromSequence(conn, tran, "t_object_classification_group");

            // 2027 起分类组表可能带 SOURCE_ABSTRACT_CLS_ID；不存在该列则完全不写（默认 0）
            bool hasSourceClsId = ClassificationGroupHasSourceAbstractClsId();
            // 注意：SQL 中若含 '{}' 等花括号字面量，不能用 string.Format（会被当占位符），故统一用拼接
            string sourceCol = hasSourceClsId ? ", " + ColSourceAbstractClsId : string.Empty;
            string sourceVal = hasSourceClsId ? ", 0" : string.Empty;
            string insertGroup = @"
                INSERT INTO [dbo].[t_object_classification_group]
                    (OBJECT_ID, INTERNAL_NAME, ABSTRACT_FLAG, OWNER_INTERNAL_NAME, DESCRIPTION,
                     CREATOR, CREATOR_OBJECT_ID, CREATOR_INTERNAL_NAME, CREATION_DATETIME,
                     LASTMODIFIER, LASTMODIFIER_OBJECT_ID, LASTMODIFIER_INTERNAL_NAME, LASTMODIFY_DATETIME,
                     NAME" + sourceCol + @")
                VALUES
                    (@oid, 'object_classification_group', 0, 'part', NULL,
                     '试用员', 9007199254740990, 'person', @dt,
                     NULL, NULL, NULL, NULL,
                     @name" + sourceVal + @")";
            using (var cmd = new SqlCommand(insertGroup, conn, tran))
            {
                cmd.Parameters.AddWithValue("@oid", newId);
                cmd.Parameters.AddWithValue("@dt", now);
                cmd.Parameters.AddWithValue("@name", name);
                cmd.ExecuteNonQuery();
            }

            long relId = GetNextIdFromSequence(conn, tran, "t_object_classification_group_to_child");
            const string insertRelation = @"
                INSERT INTO [dbo].[t_object_classification_group_to_child]
                    (OBJECT_ID, INTERNAL_NAME, ROLEA_INTERNAL_NAME, ROLEA_OBJECT_ID,
                     ROLEB_INTERNAL_NAME, ROLEB_OBJECT_ID, NAME, DESCRIPTION,
                     CREATOR, CREATOR_OBJECT_ID, CREATOR_INTERNAL_NAME, CREATION_DATETIME,
                     LASTMODIFIER, LASTMODIFIER_OBJECT_ID, LASTMODIFIER_INTERNAL_NAME)
                VALUES
                    (@oid, 'object_classification_group_to_child', 'object_classification_group', @parentId,
                     'object_classification_group', @childId, NULL, NULL,
                     '试用员', 9007199254740990, 'person', @dt,
                     NULL, NULL, NULL)";
            using (var cmd = new SqlCommand(insertRelation, conn, tran))
            {
                cmd.Parameters.AddWithValue("@oid", relId);
                cmd.Parameters.AddWithValue("@parentId", parentId);
                cmd.Parameters.AddWithValue("@childId", newId);
                cmd.Parameters.AddWithValue("@dt", now);
                cmd.ExecuteNonQuery();
            }
            return newId;
        }

        private long InsertClassificationInternal(string name, long? groupId, bool generateForm, string templateUiContent)
        {
            using (var conn = new SqlConnection(_connStr))
            {
                conn.Open();
                using (var tran = conn.BeginTransaction())
                {
                    try
                    {
                        var now = DateTime.Now;
                        long newId = GetNextIdFromSequence(conn, tran, "t_object_classification");

                        // 2027 的 SOURCE_ABSTRACT_CLS_ID 默认写 0；2026 无此列，整列跳过
                        bool hasSourceClsId = ClassificationHasSourceAbstractClsId();
                        string sourceCol = hasSourceClsId ? ", " + ColSourceAbstractClsId : string.Empty;
                        string sourceVal = hasSourceClsId ? ", 0" : string.Empty;
                        // 注意：SQL 中含 '{}' 字面量，不能用 string.Format（花括号会被当占位符），故用拼接
                        string insertCls = @"
                            INSERT INTO [dbo].[t_object_classification]
                                (OBJECT_ID, INTERNAL_NAME, ABSTRACT_FLAG, OWNER_INTERNAL_NAME, RULE_EXTEND_INFO,
                                 CLS_META_PROPERTY_SET, LAST_QUERYABLE_PROPERTIES, DESCRIPTION,
                                 CREATOR, CREATOR_OBJECT_ID, CREATOR_INTERNAL_NAME, CREATION_DATETIME,
                                 LASTMODIFIER, LASTMODIFIER_OBJECT_ID, LASTMODIFIER_INTERNAL_NAME,
                                 LASTMODIFY_DATETIME" + sourceCol + @", NAME)
                            VALUES
                                (@oid, 'object_classification', 0, 'part', NULL,
                                 'object_classification_' + CAST(@oid AS VARCHAR(20)) + '_property_set', '{}', NULL,
                                 '试用员', 9007199254740990, 'person', @dt,
                                 NULL, NULL, NULL,
                                 '1899-12-30'" + sourceVal + @", @name)";
                        using (var cmd = new SqlCommand(insertCls, conn, tran))
                        {
                            cmd.Parameters.AddWithValue("@oid", newId);
                            cmd.Parameters.AddWithValue("@dt", now);
                            cmd.Parameters.AddWithValue("@name", name);
                            cmd.ExecuteNonQuery();
                        }

                        long linkId = GetNextIdFromSequence(conn, tran, "t_object_classification_group_to_classification");
                        const string insertLink = @"
                            INSERT INTO [dbo].[t_object_classification_group_to_classification]
                                (OBJECT_ID, INTERNAL_NAME, ROLEA_INTERNAL_NAME, ROLEA_OBJECT_ID,
                                 ROLEB_INTERNAL_NAME, ROLEB_OBJECT_ID, NAME, DESCRIPTION,
                                 CREATOR, CREATOR_OBJECT_ID, CREATOR_INTERNAL_NAME, CREATION_DATETIME,
                                 LASTMODIFIER, LASTMODIFIER_OBJECT_ID, LASTMODIFIER_INTERNAL_NAME)
                            VALUES
                                (@oid, 'object_classification_group_to_classification', 'object_classification_group', @groupId,
                                 'object_classification', @clsId, NULL, NULL,
                                 '试用员', 9007199254740990, 'person', @dt,
                                 NULL, NULL, NULL)";
                        using (var cmd = new SqlCommand(insertLink, conn, tran))
                        {
                            cmd.Parameters.AddWithValue("@oid", linkId);
                            cmd.Parameters.AddWithValue("@groupId", groupId ?? 0);
                            cmd.Parameters.AddWithValue("@clsId", newId);
                            cmd.Parameters.AddWithValue("@dt", now);
                            cmd.ExecuteNonQuery();
                        }

                        // 写入 property_set 的 meta_element
                        InsertPropertySetMetaElement(conn, tran, newId);

                        tran.Commit();
                        return newId;
                    }
                    catch { tran.Rollback(); throw; }
                }
            }
        }

        private void InsertPropertySetMetaElement(SqlConnection conn, SqlTransaction tran, long clsId)
        {
            string propSetMe = "object_classification_" + clsId.ToString() + "_property_set";
            var metaObj = new JObject
            {
                ["meta_type"] = "property_set",
                ["internal_name"] = propSetMe,
                ["display_name"] = new JObject
                {
                    ["en-us"] = propSetMe,
                    ["zh-cn"] = propSetMe,
                    ["zh-tw"] = propSetMe
                },
                ["description"] = new JObject
                {
                    ["en-us"] = propSetMe + ".",
                    ["zh-cn"] = propSetMe + "。",
                    ["zh-tw"] = propSetMe + "。"
                },
                ["visibility"] = "public",
                ["properties"] = null,
                ["queryable_properties"] = null,
                ["property_details"] = null,
                ["property_lookups"] = null
            };
            string metaJson = metaObj.ToString(Newtonsoft.Json.Formatting.None);

            const string insertMeta = @"
                INSERT INTO [plm_config_v2].[dbo].[meta_element]
                    (internal_name, meta_type, state, owner_type, creator, creation_datetime, meta)
                VALUES
                    (@meName, @meType, @meState, 1, @meCreator, @meDt, @meMeta)";
            using (var cmd = new SqlCommand(insertMeta, conn, tran))
            {
                cmd.Parameters.AddWithValue("@meName", propSetMe);
                cmd.Parameters.AddWithValue("@meType", "property_set");
                cmd.Parameters.AddWithValue("@meState", "release");
                cmd.Parameters.AddWithValue("@meCreator", "试用员");
                cmd.Parameters.AddWithValue("@meDt", DateTime.Now);
                cmd.Parameters.AddWithValue("@meMeta", metaJson);
                cmd.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// 从 meta JSON 读取指定 section / lang 的语言值；缺失或空返回空串
        /// </summary>
        private static string GetMetaLang(JObject meta, string section, string lang)
        {
            if (meta == null) return string.Empty;
            var sec = meta[section] as JObject;
            if (sec == null) return string.Empty;
            var v = sec[lang];
            return v == null || v.Type == JTokenType.Null ? string.Empty : v.ToString();
        }

        private bool InsertFormTemplateForClassification(long clsId, string name, string templateUiContent)
        {
            using (var conn = new SqlConnection(_connStr))
            {
                conn.Open();
                using (var tran = conn.BeginTransaction())
                {
                    try
                    {
                        // 需求3：若该分类（同名）已存在对应表单，则跳过插入
                        const string existSql = @"
                            SELECT COUNT(1)
                            FROM [plm_config_v2].[dbo].[form_meta_template_definition] f
                            JOIN [dbo].[t_object_classification] c ON f.classification_object_id = c.OBJECT_ID
                            WHERE f.class_internal_name = 'part' AND c.NAME = @name";
                        using (var chk = new SqlCommand(existSql, conn, tran))
                        {
                            chk.Parameters.AddWithValue("@name", name);
                            int cnt = Convert.ToInt32(chk.ExecuteScalar());
                            if (cnt > 0) return false;
                        }

                        var now = DateTime.Now;
                        string dtStr = now.ToString("yyyy-MM-dd HH:mm:ss.fffffff");
                        string defId = Guid.NewGuid().ToString("D");
                        string dn = new JObject
                        {
                            ["en-us"] = name,
                            ["zh-cn"] = name,
                            ["zh-tw"] = ""
                        }.ToString(Newtonsoft.Json.Formatting.None);

                        const string insertDef = @"
                            INSERT INTO [plm_config_v2].[dbo].[form_meta_template_definition]
                                (id, internal_name, class_internal_name, classification_object_id,
                                 property_condition, display_name, description,
                                 creator, creation_datetime, version, owner_type, owner_internal_name, state)
                            VALUES
                                (@id, @intName, 'part', @clsObjId,
                                 @propCond, @dn, @desc,
                                 '试用员', @dt, 'v1.0.0', 1, 'person', 0)";
                        using (var cmd = new SqlCommand(insertDef, conn, tran))
                        {
                            cmd.Parameters.AddWithValue("@id", defId);
                            cmd.Parameters.AddWithValue("@intName", "part-classification" + clsId.ToString());
                            cmd.Parameters.AddWithValue("@clsObjId", clsId);
                            cmd.Parameters.AddWithValue("@propCond", "{\"property_internal_name\":\"\",\"property_value\":\"\"}");
                            cmd.Parameters.AddWithValue("@dn", dn);
                            cmd.Parameters.AddWithValue("@desc", dn);
                            cmd.Parameters.AddWithValue("@dt", dtStr);
                            cmd.ExecuteNonQuery();
                        }

                        string settingId = Guid.NewGuid().ToString("D");
                        const string insertSetting = @"
                            INSERT INTO [plm_config_v2].[dbo].[form_meta_template_setting]
                                (id, template_id, setting_type, content, creator, creation_datetime)
                            VALUES
                                (@id, @tplId, 'export', '{}', '试用员', @dt)";
                        using (var cmd = new SqlCommand(insertSetting, conn, tran))
                        {
                            cmd.Parameters.AddWithValue("@id", settingId);
                            cmd.Parameters.AddWithValue("@tplId", defId);
                            cmd.Parameters.AddWithValue("@dt", dtStr);
                            cmd.ExecuteNonQuery();
                        }

                        string uiId = Guid.NewGuid().ToString("D");
                        const string insertUi = @"
                            INSERT INTO [plm_config_v2].[dbo].[form_meta_template_ui]
                                (id, template_id, content, creator, creation_datetime)
                            VALUES
                                (@id, @tplId, @content, '试用员', @dt)";
                        using (var cmd = new SqlCommand(insertUi, conn, tran))
                        {
                            cmd.Parameters.AddWithValue("@id", uiId);
                            cmd.Parameters.AddWithValue("@tplId", defId);
                            cmd.Parameters.AddWithValue("@content", templateUiContent ?? string.Empty);
                            cmd.Parameters.AddWithValue("@dt", dtStr);
                            cmd.ExecuteNonQuery();
                        }

                        tran.Commit();
                        return true;
                    }
                    catch { tran.Rollback(); throw; }
                }
            }
        }

        /// <summary>
        /// 预览元模型属性导入（只读，不写入数据库）
        /// </summary>
        public MetaPropertyPreviewResult PreviewMetaPropertyImport(
            System.Collections.Generic.List<MetaPropertyRow> rows)
        {
            var result = new MetaPropertyPreviewResult();
            if (rows == null || rows.Count == 0) return result;
            result.TotalRows = rows.Count;

            // 同表内去重（同 ImportMetaPropertySet 逻辑）
            var allUniqueNames = new System.Collections.Generic.HashSet<string>(
                System.StringComparer.OrdinalIgnoreCase);

            using (var conn = new SqlConnection(_connStr))
            {
                conn.Open();

                // ── Step 1: 批量查询已存在的 internal_name ───────────────────
                var uniqueNames = new System.Collections.Generic.List<string>();
                foreach (var row in rows)
                {
                    string n = (row.InternalName ?? string.Empty).Trim().ToLower();
                    if (string.IsNullOrEmpty(n)) continue;
                    if (allUniqueNames.Add(n)) uniqueNames.Add(n);
                }

                var existingNames = new System.Collections.Generic.HashSet<string>(
                    System.StringComparer.OrdinalIgnoreCase);
                var nameToMeta = new System.Collections.Generic.Dictionary<string, string>(
                    System.StringComparer.OrdinalIgnoreCase);

                for (int i = 0; i < uniqueNames.Count; i += 2000)
                {
                    var batch = uniqueNames.GetRange(i, System.Math.Min(2000, uniqueNames.Count - i));
                    var namesStr = string.Join("','", batch);
                    string sql = "SELECT internal_name, meta FROM [plm_config_v2].[dbo].[meta_element] "
                               + "WHERE meta_type = 'property' AND internal_name IN ('" + namesStr + "')";
                    using (var cmd = new SqlCommand(sql, conn))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string nm = reader.GetString(0);
                            string metaStr = reader.IsDBNull(1) ? null : reader.GetString(1);
                            existingNames.Add(nm);
                            nameToMeta[nm] = metaStr;
                        }
                    }
                }

                // ── Step 2: 构建预览行 ───────────────────────────────────────
                allUniqueNames.Clear(); // reset for row iteration
                foreach (var row in rows)
                {
                    string internalName = (row.InternalName ?? string.Empty).Trim().ToLower();
                    if (string.IsNullOrEmpty(internalName)) continue;
                    if (!allUniqueNames.Add(internalName)) continue; // 同表去重

                    var pr = new MetaPropertyPreviewRow
                    {
                        RowNumber = row.RowNumber,
                        InternalName = internalName,
                        NewZhCn = (row.DisplayZhCn ?? string.Empty).Trim(),
                        NewEnUs = (row.DisplayEnUs ?? string.Empty).Trim(),
                        NewZhTw = (row.DisplayZhTw ?? string.Empty).Trim(),
                    };

                    if (!existingNames.Contains(internalName))
                    {
                        pr.Action = "新增";
                        pr.Changes = "数据库中不存在，将新增该属性";
                        result.WillInsert++;
                    }
                    else
                    {
                        // 已有：读取现有 meta，比较 BCD
                        string metaStr;
                        nameToMeta.TryGetValue(internalName, out metaStr);
                        JObject oldMeta;
                        try
                        {
                            oldMeta = string.IsNullOrEmpty(metaStr)
                                ? new JObject()
                                : JObject.Parse(metaStr);
                        }
                        catch { oldMeta = new JObject(); }

                        string oldZhCn = GetMetaLang(oldMeta, "display_name", "zh-cn");
                        string oldEnUs = GetMetaLang(oldMeta, "display_name", "en-us");
                        string oldZhTw = GetMetaLang(oldMeta, "display_name", "zh-tw");

                        pr.CurrentZhCn = oldZhCn;
                        pr.CurrentEnUs = oldEnUs;
                        pr.CurrentZhTw = oldZhTw;

                        var changes = new System.Collections.Generic.List<string>();
                        if (oldZhCn != pr.NewZhCn)
                            changes.Add(string.Format("zh-cn: {0} → {1}",
                                string.IsNullOrEmpty(oldZhCn) ? "(空)" : oldZhCn,
                                string.IsNullOrEmpty(pr.NewZhCn) ? "(空)" : pr.NewZhCn));
                        if (oldEnUs != pr.NewEnUs)
                            changes.Add(string.Format("en-us: {0} → {1}",
                                string.IsNullOrEmpty(oldEnUs) ? "(空)" : oldEnUs,
                                string.IsNullOrEmpty(pr.NewEnUs) ? "(空)" : pr.NewEnUs));
                        if (oldZhTw != pr.NewZhTw)
                            changes.Add(string.Format("zh-tw: {0} → {1}",
                                string.IsNullOrEmpty(oldZhTw) ? "(空)" : oldZhTw,
                                string.IsNullOrEmpty(pr.NewZhTw) ? "(空)" : pr.NewZhTw));

                        if (changes.Count > 0)
                        {
                            pr.Action = "更新";
                            pr.Changes = string.Join("；", changes);
                            result.WillUpdate++;
                        }
                        else
                        {
                            pr.Action = "跳过";
                            pr.Changes = "BCD 值与现有值一致，无需更新";
                            result.WillSkip++;
                        }
                    }

                    result.Rows.Add(pr);
                }
            }
            return result;
        }

}
}