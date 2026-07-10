using System;

namespace DataImportTool
{
    /// <summary>
    /// 功能六 Excel 行模型。
    /// 固定读取第1列=PROPERTY_NUMBER，第2列=PROPERTY_VALUE，
    /// 数据从第2行开始（第1行为标题）。
    /// </summary>
    public class PropertyValueRow
    {
        /// <summary>行号（Excel 中的行号，从1开始）</summary>
        public int RowNumber { get; set; }

        /// <summary>属性编号（Excel 第1列）</summary>
        public string PropertyNumber { get; set; }

        /// <summary>属性值（Excel 第2列）</summary>
        public string PropertyValue { get; set; }
    }
}
