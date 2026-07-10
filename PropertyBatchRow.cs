using System.Collections.Generic;

namespace DataImportTool
{
    /// <summary>
    /// 批量属性值导入（物品维度）Excel 行模型。
    /// 每行 = 一个物品，PropertyValues / PropertyNumbers 的 Key 与分类下属性名一一对应。
    /// </summary>
    public class PropertyBatchRow
    {
        /// <summary>Excel 中的行号（从1开始）</summary>
        public int RowNumber { get; set; }

        /// <summary>品名 / 物品名称（第1列）</summary>
        public string ItemName { get; set; }

        /// <summary>
        /// 属性名 → 属性值
        /// </summary>
        public Dictionary<string, string> PropertyValues { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// 属性名 → PROPERTY_NUMBER
        /// </summary>
        public Dictionary<string, string> PropertyNumbers { get; set; } = new Dictionary<string, string>();
    }
}
