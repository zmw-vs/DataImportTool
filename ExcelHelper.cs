﻿using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;

namespace DataImportTool
{
    public static class ExcelHelper
    {
        // ====== 获取列头列表 ======
        public static List<string> GetColumnNames(string filePath)
        {
            EnsureXlsx(filePath);

            using (var package = new ExcelPackage(new FileInfo(filePath)))
            {
                var sheet = GetFirstSheet(package);
                var headers = new List<string>();

                int lastCol = sheet.Dimension?.End.Column ?? 0;
                for (int col = 1; col <= lastCol; col++)
                {
                    var cell = sheet.Cells[1, col];
                    headers.Add(cell.Text.Trim());
                }
                return headers;
            }
        }

        // ====== 读取 Name 列数据（行号从1开始，数据从第2行起）======
        public static List<KeyValuePair<int, string>> ReadNameColumn(string filePath, string nameColumn)
        {
            EnsureXlsx(filePath);
            var result = new List<KeyValuePair<int, string>>();

            using (var package = new ExcelPackage(new FileInfo(filePath)))
            {
                var sheet = GetFirstSheet(package);

                // 查找目标列索引
                int colIndex = FindColumnIndex(sheet, nameColumn);
                if (colIndex == -1)
                    throw new Exception($"未找到列名名{nameColumn}");

                int lastRow = sheet.Dimension?.End.Row ?? 1;
                // 从第2行开始读（第1行是标题）
                for (int row = 2; row <= lastRow; row++)
                {
                    string name = sheet.Cells[row, colIndex].Text.Trim();
                    // 即使为空也加入，由调用方处理空值
                    result.Add(new KeyValuePair<int, string>(row, name));
                }
            }

            return result;
        }

        // ====== 私有辅助 ======
        private static void EnsureXlsx(string filePath)
        {
            string ext = Path.GetExtension(filePath).ToLowerInvariant();
            if (ext != ".xlsx")
                throw new NotSupportedException("当前版本仅支持 .xlsx 格式，请将 .xls 文件另存为 .xlsx 后再导入。文件：" + filePath);
        }

        private static ExcelWorksheet GetFirstSheet(ExcelPackage package)
        {
            var sheet = package.Workbook.Worksheets.FirstOrDefault();
            if (sheet == null)
                throw new Exception("Excel 文件中没有工作表");
            return sheet;
        }

        private static int FindColumnIndex(ExcelWorksheet sheet, string columnName)
        {
            if (string.IsNullOrEmpty(columnName))
                throw new ArgumentException("列名不能为空", nameof(columnName));

            int lastCol = sheet.Dimension?.End.Column ?? 0;
            for (int col = 1; col <= lastCol; col++)
            {
                string header = sheet.Cells[1, col].Text.Trim();
                if (header.Equals(columnName, StringComparison.OrdinalIgnoreCase))
                    return col;
            }
            return -1;
        }

        // ====== 功能六：读取属性值 Excel（固定第1列=PROPERTY_NUMBER，第2列=PROPERTY_VALUE）======
        public static List<PropertyValueRow> ReadPropertyValueColumns(string filePath)
        {
            EnsureXlsx(filePath);
            var result = new List<PropertyValueRow>();

            using (var package = new ExcelPackage(new FileInfo(filePath)))
            {
                var sheet = GetFirstSheet(package);
                int lastRow = sheet.Dimension?.End.Row ?? 1;
                // 从第2行开始读（第1行是标题）
                for (int row = 2; row <= lastRow; row++)
                {
                    string propNum = sheet.Cells[row, 1].Text.Trim();
                    string propVal = sheet.Cells[row, 2].Text.Trim();
                    result.Add(new PropertyValueRow
                    {
                        RowNumber = row,
                        PropertyNumber = propNum,
                        PropertyValue = propVal
                    });
                }
            }
            return result;
        }

        // PRD v2.1 新增：批量读取属性值导入Excel（按列标题定位）
        public static List<PropertyValueBatchRow> ReadPropertyValueBatchColumns(string filePath)
        {
            var rows = new List<PropertyValueBatchRow>();
            using (var package = new ExcelPackage(new FileInfo(filePath)))
            {
                var ws = package.Workbook.Worksheets[1];
                int maxRow = ws.Dimension.End.Row;
                int propNameCol = -1, propNumCol = -1, propValCol = -1;

                // 第1行定位列
                for (int col = 1; col <= ws.Dimension.End.Column; col++)
                {
                    string header = ws.Cells[1, col].Text?.Trim();
                    if (header == "属性名称") propNameCol = col;
                    else if (header == "PROPERTY_NUMBER") propNumCol = col;
                    else if (header == "PROPERTY_VALUE") propValCol = col;
                }

                if (propNameCol < 0 || propNumCol < 0 || propValCol < 0)
                    throw new Exception("Excel 中未找到所需的列标题「属性名称」「PROPERTY_NUMBER」「PROPERTY_VALUE」，请检查第1行。");

                // 第2行起读取数据
                for (int row = 2; row <= maxRow; row++)
                {
                    string pName = (ws.Cells[row, propNameCol].Text ?? string.Empty).Trim();
                    string pNum = (ws.Cells[row, propNumCol].Text ?? string.Empty).Trim();
                    string pVal = (ws.Cells[row, propValCol].Text ?? string.Empty).Trim();

                    if (string.IsNullOrEmpty(pName) && string.IsNullOrEmpty(pNum) && string.IsNullOrEmpty(pVal))
                        continue; // 跳过空行

                    rows.Add(new PropertyValueBatchRow
                    {
                        RowNumber = row,
                        PropertyName = pName,
                        PropertyNumber = pNum,
                        PropertyValue = pVal
                    });
                }
            }
            return rows;
        }

        // ====== 新功能：批量导入属性值 ======
        // Excel 格式：第1列=Name，第2列起每属性2列（PropertyName_VALUE / PropertyName_NUMBER）
        // 第1行为标题行，数据从第2行起
        public static List<PropertyBatchRow> ReadBatchPropertyImport(string filePath, List<string> propertyNames)
        {
            EnsureXlsx(filePath);
            var result = new List<PropertyBatchRow>();

            using (var package = new ExcelPackage(new FileInfo(filePath)))
            {
                var sheet = GetFirstSheet(package);
                int lastRow = sheet.Dimension?.End.Row ?? 1;
                int lastCol = sheet.Dimension?.End.Column ?? 1;

                // 第1步：扫描第1行，建立属性名到列索引的映射
                // 格式：A1=属性名1, B1=(空), C1=属性名2, D1=(空)...
                // 数据：A列=NUMBER, B列=VALUE, C列=NUMBER, D列=VALUE...
                var propColMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase); // 属性名 -> 起始列索引（NUMBER列）
                for (int col = 1; col <= lastCol; col += 2)
                {
                    string header = (sheet.Cells[1, col].Text ?? string.Empty).Trim();
                    if (string.IsNullOrEmpty(header)) continue;
                    
                    // 检查该属性名是否在预期列表中
                    if (propertyNames.Contains(header, StringComparer.OrdinalIgnoreCase))
                    {
                        propColMap[header] = col; // col = NUMBER列, col+1 = VALUE列
                    }
                }

                // 第2步：按属性分别读取，每个属性独立计算有效行数
                // 数据从第3行开始（第1行=属性名，第2行=列说明）
                const int dataStartRow = 3;
                
                // 先找出所有属性中最大的有效行数
                int maxRow = dataStartRow - 1;
                foreach (var propName in propertyNames)
                {
                    if (!propColMap.ContainsKey(propName)) continue;
                    
                    int numberCol = propColMap[propName];
                    for (int row = lastRow; row >= dataStartRow; row--)
                    {
                        string num = (sheet.Cells[row, numberCol].Text ?? string.Empty).Trim();
                        string val = numberCol + 1 <= lastCol 
                            ? (sheet.Cells[row, numberCol + 1].Text ?? string.Empty).Trim() 
                            : string.Empty;
                        
                        // 只要NUMBER或VALUE有值，就算有效行
                        if (!string.IsNullOrEmpty(num) || !string.IsNullOrEmpty(val))
                        {
                            if (row > maxRow) maxRow = row;
                            break;
                        }
                    }
                }

                if (maxRow < dataStartRow) return result; // 没有数据

                // 第3步：逐行读取，但每个属性只取自己有效范围内的值
                for (int row = dataStartRow; row <= maxRow; row++)
                {
                    var batchRow = new PropertyBatchRow
                    {
                        RowNumber = row,
                        ItemName = string.Empty
                    };

                    bool rowHasAnyData = false;
                    foreach (var propName in propertyNames)
                    {
                        if (propColMap.ContainsKey(propName))
                        {
                            int numberCol = propColMap[propName];
                            int valueCol = numberCol + 1;
                            
                            string num = (sheet.Cells[row, numberCol].Text ?? string.Empty).Trim();
                            string val = valueCol <= lastCol 
                                ? (sheet.Cells[row, valueCol].Text ?? string.Empty).Trim() 
                                : string.Empty;

                            // 只有该属性在此行有数据时才存入
                            if (!string.IsNullOrEmpty(num) || !string.IsNullOrEmpty(val))
                            {
                                batchRow.PropertyNumbers[propName] = num;
                                batchRow.PropertyValues[propName] = val;
                                rowHasAnyData = true;
                            }
                            // 否则不加入字典，避免插入空值
                        }
                    }

                    // 只有当该行至少有一个属性有数据时才添加
                    if (rowHasAnyData)
                    {
                        result.Add(batchRow);
                    }
                }
            }

            return result;
        }

        // ====== 生成批量属性值导入 Excel 模板 ======
        // 列结构：A1=属性名1, B1=(空), C1=属性名2, D1=(空)...
        // 数据行：A列=PROPERTY_NUMBER, B列=PROPERTY_VALUE, C列=PROPERTY_NUMBER...
        public static void GenerateBatchPropertyImportTemplate(string filePath, List<string> propertyNames)
        {
            EnsureXlsx(filePath);

            using (var package = new ExcelPackage())
            {
                var sheet = package.Workbook.Worksheets.Add("批量属性值");

                // === 第1行：属性名（每属性占2列，奇数列是属性名，偶数列留空）===
                int col = 1;
                var nameColor = Color.FromArgb(0, 120, 215); // 蓝色 - 属性名
                foreach (var propName in propertyNames)
                {
                    // 奇数列：属性名
                    sheet.Cells[1, col].Value = propName;
                    sheet.Cells[1, col].Style.Font.Bold = true;
                    sheet.Cells[1, col].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    sheet.Cells[1, col].Style.Fill.BackgroundColor.SetColor(nameColor);
                    sheet.Cells[1, col].Style.Font.Color.SetColor(Color.White);
                    sheet.Cells[1, col].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;

                    // 偶数列：留空（合并单元格视觉效果）
                    sheet.Cells[1, col + 1].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    sheet.Cells[1, col + 1].Style.Fill.BackgroundColor.SetColor(nameColor);

                    col += 2;
                }

                // === 第2行：列说明（A=编号，B=值）===
                col = 1;
                var numberColor = Color.FromArgb(108, 117, 125); // 灰色 - 编号列
                var valueColor = Color.FromArgb(40, 167, 69);    // 绿色 - 值列
                foreach (var propName in propertyNames)
                {
                    // A列（奇数列）：PROPERTY_NUMBER
                    sheet.Cells[2, col].Value = "属性编号";
                    sheet.Cells[2, col].Style.Font.Bold = true;
                    sheet.Cells[2, col].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    sheet.Cells[2, col].Style.Fill.BackgroundColor.SetColor(numberColor);
                    sheet.Cells[2, col].Style.Font.Color.SetColor(Color.White);
                    sheet.Cells[2, col].Style.Font.Size = 9;

                    // B列（偶数列）：PROPERTY_VALUE
                    sheet.Cells[2, col + 1].Value = "属性值";
                    sheet.Cells[2, col + 1].Style.Font.Bold = true;
                    sheet.Cells[2, col + 1].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    sheet.Cells[2, col + 1].Style.Fill.BackgroundColor.SetColor(valueColor);
                    sheet.Cells[2, col + 1].Style.Font.Color.SetColor(Color.White);
                    sheet.Cells[2, col + 1].Style.Font.Size = 9;

                    col += 2;
                }

                // === 第3行：示例数据 ===
                col = 1;
                for (int i = 0; i < propertyNames.Count; i++)
                {
                    sheet.Cells[3, col].Value = "编号" + (i + 1);      // NUMBER
                    sheet.Cells[3, col + 1].Value = "值" + (i + 1);    // VALUE
                    col += 2;
                }

                // === 第4行：提示行 ===
                sheet.Cells[4, 1].Value = "（请在此行下方填写实际数据，A列=属性编号，B列=属性值）";
                sheet.Cells[4, 1].Style.Font.Italic = true;
                sheet.Cells[4, 1].Style.Font.Color.SetColor(Color.Gray);

                // === 冻结前两行 ===
                sheet.View.FreezePanes(3, 1);

                // === 列宽设置 ===
                col = 1;
                foreach (var propName in propertyNames)
                {
                    sheet.Column(col).Width = 15;     // NUMBER列
                    sheet.Column(col + 1).Width = 20; // VALUE列
                    col += 2;
                }

                package.SaveAs(new FileInfo(filePath));
            }
        }


        // ====== 读取分类组/分类混合导入 Excel（PRD v2.4）======
        // 列标题（第1行，大小写不敏感）：节点名称 / 节点类型 / 父节点名称
        public static List<ClassificationNodeRow> ReadClassificationNodes(string filePath)
        {
            EnsureXlsx(filePath);
            var result = new List<ClassificationNodeRow>();

            using (var package = new ExcelPackage(new FileInfo(filePath)))
            {
                var sheet = GetFirstSheet(package);
                int lastRow = sheet.Dimension?.End.Row ?? 1;
                int lastCol = sheet.Dimension?.End.Column ?? 1;

                int nameCol = -1, typeCol = -1, parentCol = -1;
                for (int col = 1; col <= lastCol; col++)
                {
                    string header = (sheet.Cells[1, col].Text ?? string.Empty).Trim();
                    if (header.Equals("节点名称", StringComparison.OrdinalIgnoreCase)) nameCol = col;
                    else if (header.Equals("节点类型", StringComparison.OrdinalIgnoreCase)) typeCol = col;
                    else if (header.Equals("父节点名称", StringComparison.OrdinalIgnoreCase)) parentCol = col;
                }

                if (nameCol < 0 || typeCol < 0 || parentCol < 0)
                    throw new Exception("Excel 第1行未找到所需列标题「节点名称」「节点类型」「父节点名称」，请使用最新模板。");

                for (int row = 2; row <= lastRow; row++)
                {
                    string name = (sheet.Cells[row, nameCol].Text ?? string.Empty).Trim();
                    string type = (sheet.Cells[row, typeCol].Text ?? string.Empty).Trim();
                    string parent = (sheet.Cells[row, parentCol].Text ?? string.Empty).Trim();

                    if (string.IsNullOrEmpty(name) && string.IsNullOrEmpty(type) && string.IsNullOrEmpty(parent))
                        continue;

                    result.Add(new ClassificationNodeRow
                    {
                        RowNumber = row,
                        Name = name,
                        NodeType = type,
                        ParentName = parent
                    });
                }
            }
            return result;
        }


        /// <summary>
        /// 创建分类组/分类混合导入模板（PRD v2.4）：节点名称 / 节点类型 / 父节点名称
        /// </summary>
        public static void CreateGroupImportTemplate(string filePath)
        {
            using (var package = new ExcelPackage())
            {
                var ws = package.Workbook.Worksheets.Add("分类结构");
                ws.Cells[1, 1].Value = "节点名称";
                ws.Cells[1, 2].Value = "节点类型";
                ws.Cells[1, 3].Value = "父节点名称";
                ws.Cells[1, 1].Style.Font.Bold = true;
                ws.Cells[1, 2].Style.Font.Bold = true;
                ws.Cells[1, 3].Style.Font.Bold = true;

                string[,] sample = new string[,]
                {
                    { "根分类组", "分类组", "" },
                    { "机械零件", "分类组", "根分类组" },
                    { "电气设备", "分类组", "根分类组" },
                    { "螺栓", "分类", "机械零件" },
                    { "螺母", "分类", "机械零件" },
                    { "开关", "分类", "电气设备" }
                };
                for (int r = 0; r < sample.GetLength(0); r++)
                {
                    ws.Cells[r + 2, 1].Value = sample[r, 0];
                    ws.Cells[r + 2, 2].Value = sample[r, 1];
                    ws.Cells[r + 2, 3].Value = sample[r, 2];
                }

                ws.Column(1).Width = 20;
                ws.Column(2).Width = 12;
                ws.Column(3).Width = 20;
                package.SaveAs(new FileInfo(filePath));
            }
        }

        // ====== 元模型属性导入 Excel 支持（PRD 新功能）======
        /// <summary>
        /// 读取元模型属性导入 Excel，第1行表头，第2行起为数据
        /// </summary>
        public static List<MetaPropertyRow> ReadMetaPropertyImport(string filePath)
        {
            EnsureXlsx(filePath);
            var result = new List<MetaPropertyRow>();

            using (var package = new ExcelPackage(new FileInfo(filePath)))
            {
                var sheet = GetFirstSheet(package);
                int lastRow = sheet.Dimension?.End.Row ?? 1;

                for (int row = 2; row <= lastRow; row++)
                {
                    string colA = (sheet.Cells[row, 1].Text ?? string.Empty).Trim();
                    string colB = (sheet.Cells[row, 2].Text ?? string.Empty).Trim();
                    string colC = (sheet.Cells[row, 3].Text ?? string.Empty).Trim();
                    string colD = (sheet.Cells[row, 4].Text ?? string.Empty).Trim();

                    if (string.IsNullOrEmpty(colA) && string.IsNullOrEmpty(colB)
                        && string.IsNullOrEmpty(colC) && string.IsNullOrEmpty(colD))
                        continue;

                    result.Add(new MetaPropertyRow
                    {
                        RowNumber = row,
                        InternalName = colA,
                        DisplayZhCn = colB,
                        DisplayEnUs = colC,
                        DisplayZhTw = colD
                    });
                }
            }
            return result;
        }

        /// <summary>
        /// 生成元模型属性导入 Excel 模板（4列表头）
        /// </summary>
        public static void GenerateMetaPropertyImportTemplate(string filePath)
        {
            using (var package = new ExcelPackage())
            {
                var ws = package.Workbook.Worksheets.Add("属性");

                // 第1行：表头
                ws.Cells[1, 1].Value = "属性内部名";
                ws.Cells[1, 2].Value = "中文名称";
                ws.Cells[1, 3].Value = "英文名称";
                ws.Cells[1, 4].Value = "繁体名称";
                for (int c = 1; c <= 4; c++)
                {
                    ws.Cells[1, c].Style.Font.Bold = true;
                    ws.Cells[1, c].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    ws.Cells[1, c].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(0, 120, 215));
                    ws.Cells[1, c].Style.Font.Color.SetColor(Color.White);
                }

                // 第2行：示例数据
                ws.Cells[2, 1].Value = "example_prop_code";
                ws.Cells[2, 2].Value = "示例属性中文名";
                ws.Cells[2, 3].Value = "Example Property Name";
                ws.Cells[2, 4].Value = "示例屬性中文名";
                for (int c = 1; c <= 4; c++)
                {
                    ws.Cells[2, c].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    ws.Cells[2, c].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(248, 249, 250));
                }

                // 列宽
                ws.Column(1).Width = 25;
                ws.Column(2).Width = 20;
                ws.Column(3).Width = 20;
                ws.Column(4).Width = 20;

                // 冻结第1行
                ws.View.FreezePanes(2, 1);

                package.SaveAs(new FileInfo(filePath));
            }
        }

    }
}
