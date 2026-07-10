using System;
using System.Collections.Generic;

namespace DataImportTool
{
    /// <summary>分类匹配模式（超级批量导入 UI 开关）</summary>
    public enum ClassificationMatchMode
    {
        /// <summary>作为全新分类导入（默认行为）：无论库中是否已存在同名分类，都新建</summary>
        CreateNew = 0,
        /// <summary>按分类名匹配：库中已存在同名分类则识别为同一分类并合并导入，不存在则新建</summary>
        MatchByName = 1
    }

    // ====== 分类结构（工作表1：分类结构）======
    public class SuperBatchClassificationNode
    {
        public int RowNumber { get; set; }
        /// <summary>节点名称</summary>
        public string Name { get; set; }
        /// <summary>节点类型：分类组 或 分类</summary>
        public string NodeType { get; set; }
        /// <summary>父节点名称（可选，空=根节点）</summary>
        public string ParentName { get; set; }
        /// <summary>是否生成表单（是/否，默认否）</summary>
        public bool GenerateForm { get; set; }
        /// <summary>表单名称（可选，为空则使用节点名称）</summary>
        public string FormName { get; set; }
        public bool IsClassification => NodeType.Equals("分类", StringComparison.OrdinalIgnoreCase);
        public bool IsGroup => NodeType.Equals("分类组", StringComparison.OrdinalIgnoreCase);
    }

    // ====== 分类属性值（工作表2：分类属性值）======
    public class SuperBatchPropertyDef
    {
        public int RowNumber { get; set; }
        /// <summary>分类名称（A）</summary>
        public string ClassificationName { get; set; }
        /// <summary>属性显示名称（B）</summary>
        public string DisplayName { get; set; }
        /// <summary>属性内部名称（C）</summary>
        public string InternalName { get; set; }
        /// <summary>属性编号 / PROPERTY_NUMBER（D，可选）</summary>
        public string Number { get; set; }
        /// <summary>属性值 / PROPERTY_VALUE（E，可选）</summary>
        public string Value { get; set; }
        /// <summary>归属属性集内部名（F，可选）</summary>
        public string PropertySetInternalName { get; set; }
    }

    // ====== 元模型属性（工作表3：属性（元模型））======
    public class SuperBatchMetaProperty
    {
        public int RowNumber { get; set; }
        /// <summary>属性内部名（A，必填，不含中文）</summary>
        public string InternalName { get; set; }
        /// <summary>中文名称（B）</summary>
        public string DisplayZhCn { get; set; }
        /// <summary>英文名称（C）</summary>
        public string DisplayEnUs { get; set; }
        /// <summary>繁体名称（D）</summary>
        public string DisplayZhTw { get; set; }
        /// <summary>归属属性集内部名（E，可选；为空则仅插入属性不写入属性集）</summary>
        public string PropertySetInternalName { get; set; }
    }

    // ====== 完整导入数据 ======
    public class SuperBatchData
    {
        public List<SuperBatchClassificationNode> ClassificationNodes { get; set; } = new List<SuperBatchClassificationNode>();
        public List<SuperBatchPropertyDef> PropertyDefinitions { get; set; } = new List<SuperBatchPropertyDef>();
        public List<SuperBatchMetaProperty> MetaProperties { get; set; } = new List<SuperBatchMetaProperty>();
        public string TemplateId { get; set; }
        /// <summary>是否生成分类表单（超级批量导入 UI 开关，默认是）</summary>
        public bool GenerateForms { get; set; } = true;
        /// <summary>分类匹配模式（超级批量导入 UI 开关，默认 CreateNew 以保留原有"总是新建"行为）</summary>
        public ClassificationMatchMode ClassificationMatchMode { get; set; } = ClassificationMatchMode.CreateNew;
    }

    // ====== 预览模型 ======
    public class PreviewNode
    {
        public string Name { get; set; }
        public string NodeType { get; set; }
        public bool GenerateForm { get; set; }
        public string FormName { get; set; }
        public List<PreviewNode> Children { get; set; } = new List<PreviewNode>();
        public List<PreviewPropertyInfo> Properties { get; set; } = new List<PreviewPropertyInfo>();

        public bool IsClassification => NodeType == "分类";
    }

    public class PreviewPropertyInfo
    {
        public string DisplayName { get; set; }
        public string InternalName { get; set; }
        public List<PreviewPropertyValueInfo> Values { get; set; } = new List<PreviewPropertyValueInfo>();
    }

    public class PreviewPropertyValueInfo
    {
        public string Number { get; set; }
        public string Value { get; set; }
    }

    // ====== 校验结果 ======
    public class SuperBatchValidationResult
    {
        public bool IsValid { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
        public List<string> Warnings { get; set; } = new List<string>();
    }

    // ====== 导入统计 ======
    public class SuperBatchStats
    {
        public int Groups { get; set; }
        public int Classifications { get; set; }
        public int Properties { get; set; }
        public int PropertyValues { get { return Properties; } set { } }
        public int Forms { get; set; }
        public int MetaProperties { get; set; }
    }

    /// <summary>模板 content 属性冲突信息</summary>
    public class PropertyConflict
    {
        public string InternalName { get; set; }
        public string OldZhCn { get; set; }
        public string NewZhCn { get; set; }
    }
}
