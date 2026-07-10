using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace DataImportTool
{
    // ====== 导入结果 ======
    public class ImportResult
    {
        public int SuccessCount { get; set; }
        public int FailCount { get; set; }
        /// <summary>本次操作中被删除的属性条数</summary>
        public int DeleteCount { get; set; }
        /// <summary>用户是否终止了导入</summary>
        public bool Aborted { get; set; }
        public System.Collections.Generic.List<ImportError> Errors { get; set; }
        /// <summary>每条变更记录（新增/更新/删除/跳过）</summary>
        public System.Collections.Generic.List<string> Changes { get; set; }

        public ImportResult()
        {
            Errors = new System.Collections.Generic.List<ImportError>();
            Changes = new System.Collections.Generic.List<string>();
        }

        public string Summary
        {
            get
            {
                var s = string.Format("成功：{0} 条，失败：{1} 条", SuccessCount, FailCount);
                if (DeleteCount > 0) s += string.Format("，删除：{0} 条", DeleteCount);
                return s;
            }
        }
    }

    public class ImportError
    {
        public int RowNumber { get; set; }
        public string Name { get; set; }
        public string Message { get; set; }

        public override string ToString()
        {
            return string.Format("行 {0} [{1}]：{2}", RowNumber, Name ?? "(空)", Message);
        }
    }

    // ====== 分类组 ======
    /// <summary>
    /// PRD v2.1: 批量属性值导入 Excel 行数据
    /// </summary>
    public class PropertyValueBatchRow
    {
        public int RowNumber { get; set; }
        public string PropertyName { get; set; }
        public string PropertyNumber { get; set; }
        public string PropertyValue { get; set; }
    }

    public class ClassificationGroup
    {
        public long ObjectId { get; set; }
        public string Name { get; set; }
        /// <summary>父分类 OBJECT_ID（t_object_classification.PARENT_ID 自引用）；0 表示根分类。</summary>
        public long ParentId { get; set; }
    }

    // ====== 分类组/分类混合导入（PRD v2.4） ======
    /// <summary>
    /// 分类组/分类导入的 Excel 行：节点名称 + 节点类型 + 父节点名称
    /// </summary>
    public class ClassificationNodeRow
    {
        public int RowNumber { get; set; }
        /// <summary>节点名称（分类组或分类的名称）</summary>
        public string Name { get; set; }
        /// <summary>节点类型："分类组" 或 "分类"</summary>
        public string NodeType { get; set; }
        /// <summary>父节点名称；空字符串表示根节点</summary>
        public string ParentName { get; set; }
    }

    // ====== 表单模板（分类表单生成用，PRD v2.2） ======
    public class FormTemplateInfo
    {
        /// <summary>form_meta_template_definition.id（GUID 字符串）</summary>
        public string Id { get; set; }
        /// <summary>display_name 中的 zh-cn 值，用于下拉展示</summary>
        public string DisplayName { get; set; }

        public override string ToString() => DisplayName ?? Id;
    }

    // ====== t_property 表的记录模型（供下拉选择用） ======
    public class PropertyItem
    {
        public long ObjectId { get; set; }
        public string Name { get; set; }
        public string InternalName { get; set; }
        /// <summary>所属分类 ID（null 表示根目录）</summary>
        public long? ClsId { get; set; }
        /// <summary>是否标记为删除（本次操作待删除）</summary>
        public bool MarkDeleted { get; set; }

        public override string ToString() => Name;
    }

    // ====== 连接配置实体（可经 Newtonsoft.Json 序列化） ======
    public class AppConfig
    {
        /// <summary>服务器地址</summary>
        public string Server { get; set; } = string.Empty;

        /// <summary>数据库名</summary>
        public string Database { get; set; } = string.Empty;

        /// <summary>是否使用 Windows 集成验证</summary>
        public bool UseWindowsAuth { get; set; }

        /// <summary>用户名（SQL 验证时使用）</summary>
        public string UserId { get; set; } = string.Empty;

        /// <summary>密码（SQL 验证时使用，明文保存于内存）</summary>
        public string Password { get; set; } = string.Empty;

        /// <summary>Excel 中代表名称的列名（默认 "Name"）</summary>
        public string NameColumn { get; set; } = "Name";

        /// <summary>只读：判断连接核心参数是否已填写（服务器与数据库均非空）</summary>
        public bool IsConfigured
        {
            get
            {
                return !string.IsNullOrWhiteSpace(Server)
                    && !string.IsNullOrWhiteSpace(Database);
            }
        }

        /// <summary>
        /// 根据当前参数生成 SqlConnection 可用的连接字符串
        /// </summary>
        public string BuildConnectionString()
        {
            if (UseWindowsAuth)
            {
                return string.Format(
                    "Server={0};Database={1};Integrated Security=True;",
                    Server, Database);
            }
            return string.Format(
                "Server={0};Database={1};User Id={2};Password={3};",
                Server, Database, UserId, Password);
        }

        /// <summary>返回主界面展示用的连接信息文本</summary>
        public string GetDisplayInfo()
        {
            if (!IsConfigured) return string.Empty;
            string auth = UseWindowsAuth ? "Windows认证" : "SQL认证";
            return string.Format("{0} / {1}（{2}）", Server, Database, auth);
        }

        // ====== 密码简单加密（Base64，仅用于避免明文落地） ======
        private const string EncPrefix = "enc::";

        /// <summary>加密密码用于写入 config.json（非安全，仅混淆）</summary>
        public static string EncryptPassword(string plain)
        {
            if (string.IsNullOrEmpty(plain)) return plain;
            return EncPrefix + Convert.ToBase64String(Encoding.UTF8.GetBytes(plain));
        }

        /// <summary>从 config.json 读取后解密密码；非加密内容原样返回</summary>
        public static string DecryptPassword(string stored)
        {
            if (string.IsNullOrEmpty(stored) || !stored.StartsWith(EncPrefix))
                return stored;
            try
            {
                return Encoding.UTF8.GetString(
                    Convert.FromBase64String(stored.Substring(EncPrefix.Length)));
            }
            catch
            {
                return stored;
            }
        }
    }


    /// <summary>
    /// 元模型属性导入行（PRD 新功能）
    /// </summary>
    public class MetaPropertyRow
    {
        public int RowNumber { get; set; }
        public string InternalName { get; set; }
        public string DisplayZhCn { get; set; }
        public string DisplayEnUs { get; set; }
        public string DisplayZhTw { get; set; }
    }



    /// <summary>
    /// 属性集项（用于下拉框显示）
    /// </summary>
    public class MetaPropertySetItem
    {
        public string InternalName { get; set; }
        public string DisplayName { get; set; }  // 从 meta JSON 提取的 display_name.zh-cn
    }



    /// <summary>
    /// 元模型导入结果
    /// </summary>
    public class MetaImportResult
    {
        public int Inserted { get; set; }
        public int SkippedExist { get; set; }
        public List<string> SkippedExistNames { get; set; } = new List<string>();
        /// <summary>已存在但用 BCD 列更新了 display_name / description 的属性数量</summary>
        public int Updated { get; set; }
        public List<string> UpdatedNames { get; set; } = new List<string>();
        public int PropertySetSkipped { get; set; }
        public List<string> PropertySetSkippedNames { get; set; } = new List<string>();
    }



    /// <summary>
    /// 单行预览结果
    /// </summary>
    public class MetaPropertyPreviewRow
    {
        public int RowNumber { get; set; }
        public string InternalName { get; set; }
        /// <summary>新增 / 更新 / 跳过</summary>
        public string Action { get; set; }
        /// <summary>现有中文名；不存在时 null</summary>
        public string CurrentZhCn { get; set; }
        /// <summary>现有英文名；不存在时 null</summary>
        public string CurrentEnUs { get; set; }
        /// <summary>现有繁体名；不存在时 null</summary>
        public string CurrentZhTw { get; set; }
        /// <summary>BCD 新值：中文名</summary>
        public string NewZhCn { get; set; }
        /// <summary>BCD 新值：英文名</summary>
        public string NewEnUs { get; set; }
        /// <summary>BCD 新值：繁体名</summary>
        public string NewZhTw { get; set; }
        /// <summary>变更描述（供预览用）</summary>
        public string Changes { get; set; }
        /// <summary>用户选择：True=更新（默认），False=忽略（保留现有）</summary>
        public bool ShouldUpdate { get; set; } = true;
    }

    /// <summary>
    /// 预览结果汇总
    /// </summary>
    public class MetaPropertyPreviewResult
    {
        public int TotalRows { get; set; }
        public int WillInsert { get; set; }
        public int WillUpdate { get; set; }
        public int WillSkip { get; set; }
        public System.Collections.Generic.List<MetaPropertyPreviewRow> Rows { get; set; }
            = new System.Collections.Generic.List<MetaPropertyPreviewRow>();
    }


    // ====== 匹配规则（match_rule）配置 ======
    /// <summary>制造件下一个分类的匹配规则（object_classifications 数组元素）</summary>
    public class MatchRuleClassification
    {
        public long ObjectId { get; set; }
        public bool StrictMatchFlag { get; set; }
        public List<string> Fields { get; set; } = new List<string>();
        /// <summary>是否经「预保存」暂存（仅界面高亮用，不参与 JSON 持久化）。</summary>
        [JsonIgnore]
        public bool IsNew { get; set; } = false;
    }

    /// <summary>匹配规则记录在 config_item 中的定位与内容。本工具只读写 content 字段，
    /// 保存时不改动 owner_type / owner_object_id / version，也不删除任何记录。
    /// OwnerType / OwnerObjectId / Version 用于精确定位「最新版本」那一行（config_item 含版本链，旧版本为还原点）。</summary>
    public class MatchRuleRecord
    {
        public string Content { get; set; }
        public int OwnerType { get; set; }
        public long OwnerObjectId { get; set; }
        public string Version { get; set; }
    }

    /// <summary>元模型属性候选（match_rule 字段勾选用）</summary>
    public class MetaPropertyOption
    {
        public string InternalName { get; set; }
        public string DisplayName { get; set; }
        /// <summary>true=系统默认字段（en-us 纯英文）；false=用户自定义字段（拼音）</summary>
        public bool IsSystemDefault { get; set; }
    }

    /// <summary>分类表单模板中的一个字段（用于预选判断）</summary>
    public class FormTemplateField
    {
        public string Property { get; set; }
        /// <summary>true=系统默认字段（en-us 纯英文）；false=用户自定义字段（拼音，en-us 混入中文）</summary>
        public bool IsSystemDefault { get; set; }
    }

    /// <summary>生产类型（part_source_type 枚举项，如 manufacture=自制件）</summary>
    public class SourceTypeOption
    {
        public string InternalName { get; set; }
        public string DisplayName { get; set; }
    }




}
