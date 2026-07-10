using System;

namespace DataImportTool
{
    /// <summary>
    /// 从 plm_config_v2.dbo.meta_element 解析出的属性元信息
    /// internal_name = 'ljzdysx' 的记录，其 meta JSON 中 properties 数组
    /// 每一项再查一次 meta_element，取 internal_name 和 display_name.zh-cn
    /// </summary>
    public class PropertyMetaInfo
    {
        /// <summary>属性的内部名称（来自 meta_element.internal_name）</summary>
        public string InternalName { get; set; }

        /// <summary>属性的中文显示名称（来自 meta_element.meta → display_name.zh-cn）</summary>
        public string DisplayName { get; set; }

        public override string ToString() => DisplayName;
    }
}
