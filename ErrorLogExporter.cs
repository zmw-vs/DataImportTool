using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using OfficeOpenXml;
using OfficeOpenXml.Style;

namespace DataImportTool
{
    /// <summary>
    /// 错误日志导出工具类
    /// 将 List<ImportError> 导出为 .xlsx 文件
    /// </summary>
    public static class ErrorLogExporter
    {
        /// <summary>
        /// 导出错误列表到 Excel 文件
        /// </summary>
        /// <param name="errors">错误列表</param>
        /// <param name="defaultFileName">默认文件名（不含扩展名）</param>
        public static void Export(List<ImportError> errors, string defaultFileName)
        {
            if (errors == null || errors.Count == 0)
            {
                MessageBox.Show("没有错误记录可导出。", "提示",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (var dialog = new SaveFileDialog())
            {
                dialog.Filter = "Excel 文件 (*.xlsx)|*.xlsx";
                dialog.Title = "导出错误日志";
                dialog.FileName = defaultFileName + "_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".xlsx";

                if (dialog.ShowDialog() != DialogResult.OK)
                    return;

                try
                {
                    ExportToExcel(errors, dialog.FileName);
                    MessageBox.Show("错误日志已导出到：\n" + dialog.FileName,
                        "导出成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("导出失败：" + ex.Message,
                        "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private static void ExportToExcel(List<ImportError> errors, string filePath)
        {
            using (var package = new ExcelPackage(new FileInfo(filePath)))
            {
                var ws = package.Workbook.Worksheets.Add("错误日志");

                // 表头
                ws.Cells[1, 1].Value = "序号";
                ws.Cells[1, 2].Value = "行号";
                ws.Cells[1, 3].Value = "数据名称";
                ws.Cells[1, 4].Value = "错误原因";

                // 表头样式：红色背景、白字、加粗
                using (var range = ws.Cells[1, 1, 1, 4])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Font.Color.SetColor(System.Drawing.Color.White);
                    range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(220, 53, 69));
                }

                // 数据行
                for (int i = 0; i < errors.Count; i++)
                {
                    var err = errors[i];
                    int row = i + 2;
                    ws.Cells[row, 1].Value = i + 1;
                    ws.Cells[row, 2].Value = err.RowNumber;
                    ws.Cells[row, 3].Value = err.Name ?? "(空)";
                    ws.Cells[row, 4].Value = err.Message ?? string.Empty;
                }

                // 列宽自适应
                ws.Column(1).Width = 8;
                ws.Column(2).Width = 10;
                ws.Column(3).Width = 30;
                ws.Column(4).Width = 60;

                package.Save();
            }
        }
    }
}
