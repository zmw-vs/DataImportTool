using OfficeOpenXml;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;

namespace DataImportTool
{
    public static class SuperBatchImportEngine
    {
        private const string Sheet1 = "分类结构";
        private const string Sheet2 = "分类属性值";
        private const string Sheet4 = "属性（元模型）";

        // ====== 1. Excel 解析 ======
        public static SuperBatchData Parse(string filePath)
        {
            if (!filePath.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
                throw new NotSupportedException("仅支持 .xlsx 格式。");

            var data = new SuperBatchData();

            using (var pkg = new ExcelPackage(new FileInfo(filePath)))
            {
                var ws4 = pkg.Workbook.Worksheets[Sheet4];
                if (ws4 != null) ParseSheet4(ws4, data.MetaProperties);
                var ws1 = pkg.Workbook.Worksheets[Sheet1];
                if (ws1 == null) throw new Exception("缺少必需的工作表「" + Sheet1 + "」。");
                var ws2 = pkg.Workbook.Worksheets[Sheet2] ?? pkg.Workbook.Worksheets["分类属性"];
                if (ws2 == null) throw new Exception("缺少必需的工作表「" + Sheet2 + "」或「分类属性」。");

                ParseSheet1(ws1, data.ClassificationNodes);
                ParseSheet2(ws2, data.PropertyDefinitions);
            }

            return data;
        }

        private static int FindCol(ExcelWorksheet ws, string header)
        {
            if (ws.Dimension == null) return -1;
            for (int c = 1; c <= ws.Dimension.End.Column; c++)
            {
                var h = (ws.Cells[1, c].Text ?? string.Empty).Trim();
                if (string.Equals(h, header, StringComparison.OrdinalIgnoreCase))
                    return c;
            }
            return -1;
        }

        /// <summary>查找列：先精确匹配 header，再尝试匹配以 header 开头的列标题（容错"（可选）"等后缀）</summary>
        private static int FindColFlex(ExcelWorksheet ws, string header)
        {
            int col = FindCol(ws, header);
            if (col > 0) return col;
            if (ws.Dimension == null) return -1;
            for (int c = 1; c <= ws.Dimension.End.Column; c++)
            {
                var h = (ws.Cells[1, c].Text ?? string.Empty).Trim();
                if (h.StartsWith(header, StringComparison.OrdinalIgnoreCase))
                    return c;
            }
            return -1;
        }

        private static string Cell(ExcelWorksheet ws, int row, int col)
        {
            if (col < 1) return string.Empty;
            return (ws.Cells[row, col].Text ?? string.Empty).Trim();
        }

        private static bool ContainsChinese(string text)
        {
            if (string.IsNullOrEmpty(text)) return false;
            foreach (char c in text)
            {
                if (c >= 0x4E00 && c <= 0x9FFF) return true;
            }
            return false;
        }

        private static void ParseSheet1(ExcelWorksheet ws, List<SuperBatchClassificationNode> nodes)
        {
            int cName = FindCol(ws, "节点名称");
            int cType = FindCol(ws, "节点类型");
            int cParent = FindCol(ws, "父节点名称");
            int cForm = FindCol(ws, "是否生成表单");
            int cFormName = FindCol(ws, "表单名称");
            if (cName < 1 || cType < 1)
                throw new Exception("工作表「分类结构」缺少必需列「节点名称」「节点类型」。");

            for (int row = 2; row <= ws.Dimension.End.Row; row++)
            {
                string name = Cell(ws, row, cName);
                string type = Cell(ws, row, cType);
                if (string.IsNullOrEmpty(name) && string.IsNullOrEmpty(type)) continue;

                var node = new SuperBatchClassificationNode
                {
                    RowNumber = row,
                    Name = name,
                    NodeType = type,
                    ParentName = cParent > 0 ? Cell(ws, row, cParent) : string.Empty,
                    GenerateForm = string.Equals(Cell(ws, row, cForm), "是", StringComparison.OrdinalIgnoreCase),
                    FormName = cFormName > 0 ? Cell(ws, row, cFormName) : string.Empty
                };
                if (string.IsNullOrEmpty(node.Name) || string.IsNullOrEmpty(node.NodeType))
                    throw new Exception("「分类结构」第" + row + "行：节点名称和节点类型不能为空。");
                nodes.Add(node);
            }
        }

        private static void ParseSheet2(ExcelWorksheet ws, List<SuperBatchPropertyDef> defs)
        {
            int cCls = FindColFlex(ws, "分类名称");
            int cDisp = FindCol(ws, "属性显示名称");
            int cInt = FindCol(ws, "属性内部名称");
            int cNum = FindColFlex(ws, "属性编号");
            if (cNum < 1) cNum = FindCol(ws, "PROPERTY_NUMBER");
            int cVal = FindColFlex(ws, "属性值");
            if (cVal < 1) cVal = FindCol(ws, "PROPERTY_VALUE");
            int cSet = FindColFlex(ws, "属性集内部名");

            if (cCls < 1 || cDisp < 1 || cInt < 1)
                throw new Exception("工作表「" + Sheet2 + "」缺少必需列「分类名称」「属性显示名称」「属性内部名称」。");

            for (int row = 2; row <= ws.Dimension.End.Row; row++)
            {
                string cls = Cell(ws, row, cCls);
                string disp = Cell(ws, row, cDisp);
                string intn = Cell(ws, row, cInt);
                string num = cNum > 0 ? Cell(ws, row, cNum) : string.Empty;
                string val = cVal > 0 ? Cell(ws, row, cVal) : string.Empty;
                string set = cSet > 0 ? Cell(ws, row, cSet) : string.Empty;

                if (string.IsNullOrEmpty(cls) && string.IsNullOrEmpty(disp) && string.IsNullOrEmpty(intn)
                    && string.IsNullOrEmpty(num) && string.IsNullOrEmpty(val)) continue;
                if (string.IsNullOrEmpty(disp) || string.IsNullOrEmpty(intn))
                    throw new Exception("「" + Sheet2 + "」第" + row + "行：属性显示名称、属性内部名称均不能为空（分类名称可为空，为空时插入根属性）。");
                if (intn.Contains(" "))
                    throw new Exception("「" + Sheet2 + "」第" + row + "行：属性内部名 [" + intn + "] 包含空格，不允许。");
                defs.Add(new SuperBatchPropertyDef
                {
                    RowNumber = row,
                    ClassificationName = cls,
                    DisplayName = disp,
                    InternalName = intn,
                    Number = num,
                    Value = val,
                    PropertySetInternalName = set
                });
            }
        }

        private static void ParseSheet4(ExcelWorksheet ws, List<SuperBatchMetaProperty> props)
        {
            int cInt = FindCol(ws, "属性内部名");
            int cZh = FindCol(ws, "中文名称");
            int cEn = FindCol(ws, "英文名称");
            int cTw = FindCol(ws, "繁体名称");
            int cSet = FindColFlex(ws, "属性集内部名");
            if (cInt < 1)
                throw new Exception("工作表「" + Sheet4 + "」缺少必需列「属性内部名」。");

            for (int row = 2; row <= ws.Dimension.End.Row; row++)
            {
                string intn = Cell(ws, row, cInt);
                string zh = cZh > 0 ? Cell(ws, row, cZh) : string.Empty;
                string en = cEn > 0 ? Cell(ws, row, cEn) : string.Empty;
                string tw = cTw > 0 ? Cell(ws, row, cTw) : string.Empty;
                string set = cSet > 0 ? Cell(ws, row, cSet) : string.Empty;

                if (string.IsNullOrEmpty(intn) && string.IsNullOrEmpty(zh)
                    && string.IsNullOrEmpty(en) && string.IsNullOrEmpty(tw) && string.IsNullOrEmpty(set)) continue;
                if (string.IsNullOrEmpty(intn))
                    throw new Exception("「" + Sheet4 + "」第" + row + "行：属性内部名不能为空。");
                if (intn.Contains(" "))
                    throw new Exception("「" + Sheet4 + "」第" + row + "行：属性内部名 [" + intn + "] 包含空格，不允许。");

                props.Add(new SuperBatchMetaProperty
                {
                    RowNumber = row,
                    InternalName = intn,
                    DisplayZhCn = zh,
                    DisplayEnUs = en,
                    DisplayZhTw = tw,
                    PropertySetInternalName = set
                });
            }
        }

        // ====== 2. 生成模板 ======
        public static void GenerateTemplate(string filePath)
        {
            using (var pkg = new ExcelPackage())
            {
                var ws4 = pkg.Workbook.Worksheets.Add(Sheet4);
                ws4.Cells[1, 1].Value = "属性内部名";
                ws4.Cells[1, 2].Value = "中文名称";
                ws4.Cells[1, 3].Value = "英文名称";
                ws4.Cells[1, 4].Value = "繁体名称";
                ws4.Cells[1, 5].Value = "属性集内部名（可选）";
                ws4.Cells[2, 1].Value = "example_prop_code";
                ws4.Cells[2, 2].Value = "示例属性中文名";
                ws4.Cells[2, 3].Value = "Example Property";
                ws4.Cells[2, 4].Value = "示例屬性中文名";
                ws4.Cells[2, 5].Value = "my_property_set";

                var ws1 = pkg.Workbook.Worksheets.Add(Sheet1);
                ws1.Cells[1, 1].Value = "节点名称";
                ws1.Cells[1, 2].Value = "节点类型";
                ws1.Cells[1, 3].Value = "父节点名称";
                ws1.Cells[1, 4].Value = "是否生成表单";
                ws1.Cells[1, 5].Value = "表单名称";
                ws1.Cells[2, 1].Value = "根分类组";
                ws1.Cells[2, 2].Value = "分类组";

                var ws2 = pkg.Workbook.Worksheets.Add(Sheet2);
                ws2.Cells[1, 1].Value = "分类名称（为空则插入根属性）";
                ws2.Cells[1, 2].Value = "属性显示名称";
                ws2.Cells[1, 3].Value = "属性内部名称";
                ws2.Cells[1, 4].Value = "属性编号(PROPERTY_NUMBER)";
                ws2.Cells[1, 5].Value = "属性值(PROPERTY_VALUE)";

                pkg.SaveAs(new FileInfo(filePath));
            }
        }

        // ====== 3. 预校验 ======
        public static SuperBatchValidationResult Validate(SuperBatchData data, DbHelper db)
        {
            var result = new SuperBatchValidationResult();

            var nodeNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var clsNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var groupNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var duplicateNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var n in data.ClassificationNodes)
            {
                if (nodeNames.Contains(n.Name))
                {
                    if (!duplicateNames.Contains(n.Name))
                    {
                        result.Errors.Add("「分类结构」中存在重复的节点名称「" + n.Name + "」。");
                        duplicateNames.Add(n.Name);
                    }
                }
                else
                {
                    nodeNames.Add(n.Name);
                }

                if (string.Equals(n.NodeType, "分类", StringComparison.OrdinalIgnoreCase))
                    clsNames.Add(n.Name);
                else
                    groupNames.Add(n.Name);
            }

            foreach (var n in data.ClassificationNodes)
            {
                if (!string.IsNullOrEmpty(n.ParentName) && !nodeNames.Contains(n.ParentName))
                    result.Errors.Add("「分类结构」第" + n.RowNumber + "行：父节点「" + n.ParentName + "」尚未定义。");

                if (!string.Equals(n.NodeType, "分类组", StringComparison.OrdinalIgnoreCase) &&
                    !string.Equals(n.NodeType, "分类", StringComparison.OrdinalIgnoreCase))
                {
                    result.Errors.Add("「分类结构」第" + n.RowNumber + "行：节点类型必须为「分类组」或「分类」，当前值「" + n.NodeType + "」。");
                }

                if (!string.IsNullOrEmpty(n.ParentName) && clsNames.Contains(n.ParentName))
                    result.Errors.Add("「分类结构」第" + n.RowNumber + "行：父节点「" + n.ParentName + "」的类型是「分类」，不能作为父节点。");
            }

            var allInternalNames = data.PropertyDefinitions.Select(p => p.InternalName).Distinct(StringComparer.OrdinalIgnoreCase);
            var invalidNames = db.GetInvalidPropertyNames(allInternalNames);

            if (data.MetaProperties != null && data.MetaProperties.Count > 0)
            {
                var metaByInternal = data.MetaProperties
                    .Where(p => !string.IsNullOrWhiteSpace(p.InternalName))
                    .GroupBy(p => p.InternalName.Trim(), StringComparer.OrdinalIgnoreCase)
                    .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);
                var toAllow = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                foreach (var inv in invalidNames.ToList())
                {
                    if (metaByInternal.TryGetValue(inv, out var metaDef))
                    {
                        bool matched = data.PropertyDefinitions
                            .Any(p => string.Equals(p.InternalName, inv, StringComparison.OrdinalIgnoreCase)
                                      && string.Equals(p.DisplayName, metaDef.DisplayZhCn, StringComparison.OrdinalIgnoreCase));
                        if (matched)
                            toAllow.Add(inv);
                    }
                }
                foreach (var name in toAllow)
                    invalidNames.Remove(name);
            }

            foreach (var pd in data.PropertyDefinitions)
            {
                // 分类名称为空时作为根属性插入，跳过分类存在性校验
                if (!string.IsNullOrEmpty(pd.ClassificationName) && !clsNames.Contains(pd.ClassificationName))
                    result.Errors.Add("「" + Sheet2 + "」第" + pd.RowNumber + "行：分类名称「" + pd.ClassificationName + "」不在「分类结构」中。");
            }

            // key -> 首次出现的行号，报错时指明与哪一行重复，便于用户定位
            var seenProps = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            foreach (var pd in data.PropertyDefinitions)
            {
                var key = pd.ClassificationName + "::" + pd.InternalName + "::" + (pd.Number ?? "") + "::" + (pd.Value ?? "");
                int firstRow;
                if (seenProps.TryGetValue(key, out firstRow))
                {
                    result.Errors.Add("「" + Sheet2 + "」第" + pd.RowNumber + "行：" + (string.IsNullOrEmpty(pd.ClassificationName) ? "根属性" : "分类「" + pd.ClassificationName + "」") + "下属性内部名称「" + pd.InternalName + "」+ 编号「" + (pd.Number ?? string.Empty) + "」/ 值「" + (pd.Value ?? string.Empty) + "」与第" + firstRow + "行完全重复。");
                }
                else
                {
                    seenProps[key] = pd.RowNumber;
                }

                if (invalidNames.Contains(pd.InternalName))
                    result.Errors.Add("「" + Sheet2 + "」第" + pd.RowNumber + "行：字段(属性)「" + pd.InternalName + "」不存在，请检查导入模板或者系统元模型。");
            }

            if (data.MetaProperties != null && data.MetaProperties.Count > 0)
            {
                var setNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                try
                {
                    foreach (var s in db.GetPropertySets())
                        if (!string.IsNullOrEmpty(s.InternalName)) setNames.Add(s.InternalName);
                }
                catch { }

                foreach (var mp in data.MetaProperties)
                {
                    if (string.IsNullOrWhiteSpace(mp.InternalName))
                    {
                        result.Errors.Add("「" + Sheet4 + "」第" + mp.RowNumber + "行：属性内部名为空。");
                        continue;
                    }
                    if (ContainsChinese(mp.InternalName))
                    {
                        result.Errors.Add("「" + Sheet4 + "」第" + mp.RowNumber + "行：属性内部名 [" + mp.InternalName + "] 包含中文字符。");
                    }
                    if (mp.InternalName.Contains(" "))
                    {
                        result.Errors.Add("「" + Sheet4 + "」第" + mp.RowNumber + "行：属性内部名 [" + mp.InternalName + "] 包含空格，不允许。");
                    }
                    if (!string.IsNullOrEmpty(mp.PropertySetInternalName) && !setNames.Contains(mp.PropertySetInternalName.Trim()))
                    {
                        result.Errors.Add("「" + Sheet4 + "」第" + mp.RowNumber + "行：属性集 [" + mp.PropertySetInternalName + "] 不存在。");
                    }
                }
            }

            result.IsValid = result.Errors.Count == 0;
            return result;
        }

        // ====== 4. 构建预览树 ======
        public static List<PreviewNode> BuildPreviewTree(SuperBatchData data)
        {
            var tree = new List<PreviewNode>();
            var byName = new Dictionary<string, PreviewNode>(StringComparer.OrdinalIgnoreCase);

            foreach (var n in data.ClassificationNodes)
            {
                var node = new PreviewNode
                {
                    Name = n.Name,
                    NodeType = n.NodeType,
                    GenerateForm = n.GenerateForm,
                    FormName = n.FormName
                };
                byName[n.Name] = node;
                if (string.IsNullOrEmpty(n.ParentName))
                    tree.Add(node);
                else if (byName.TryGetValue(n.ParentName, out var parent))
                    parent.Children.Add(node);
            }

            foreach (var pd in data.PropertyDefinitions)
            {
                PreviewNode targetNode = null;

                if (string.IsNullOrEmpty(pd.ClassificationName))
                {
                    // 根属性：挂到"（根属性）"虚拟节点
                    const string rootNodeName = "（根属性）";
                    if (!byName.TryGetValue(rootNodeName, out targetNode))
                    {
                        targetNode = new PreviewNode { Name = rootNodeName, NodeType = "根属性" };
                        byName[rootNodeName] = targetNode;
                        tree.Add(targetNode);
                    }
                }
                else
                {
                    byName.TryGetValue(pd.ClassificationName, out targetNode);
                }

                if (targetNode != null)
                {
                    var propInfo = new PreviewPropertyInfo
                    {
                        DisplayName = pd.DisplayName,
                        InternalName = pd.InternalName
                    };
                    if (!string.IsNullOrEmpty(pd.Number) || !string.IsNullOrEmpty(pd.Value))
                    {
                        propInfo.Values.Add(new PreviewPropertyValueInfo
                        {
                            Number = pd.Number,
                            Value = pd.Value
                        });
                    }
                    targetNode.Properties.Add(propInfo);
                }
            }

            return tree;
        }

        // ====== 5. 计算统计 ======
        public static SuperBatchStats ComputeStats(SuperBatchData data)
        {
            var stats = new SuperBatchStats();
            stats.Groups = data.ClassificationNodes.Count(n => !n.IsClassification);
            stats.Classifications = data.ClassificationNodes.Count(n => n.IsClassification);
            stats.Forms = data.GenerateForms ? data.ClassificationNodes.Count(n => n.IsClassification) : 0;
            stats.Properties = data.PropertyDefinitions.Count;
            stats.MetaProperties = data.MetaProperties != null ? data.MetaProperties.Count : 0;
            return stats;
        }

        // ====== 6. 原子导入 ======
        public static ImportResult Import(SuperBatchData data, DbHelper db, Action<string> log, Action<int> progress, string templateUiContent = null)
        {
            var result = new ImportResult();
            string connStr = null;
            var field = typeof(DbHelper).GetField("_connStr",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null)
            {
                connStr = field.GetValue(db) as string;
            }
            if (string.IsNullOrEmpty(connStr))
            {
                throw new Exception("无法获取数据库连接字符串。");
            }

            log = log ?? (s => { });
            progress = progress ?? (p => { });

            // 兼容 2026 / 2027：SOURCE_ABSTRACT_CLS_ID 只有 2027 的表里才有，
            // 存在则写入默认值 0，不存在则整列跳过（每个 DbHelper 实例只查一次元数据）
            bool clsHasSourceAbstractClsId = db.ClassificationHasSourceAbstractClsId();
            bool groupHasSourceAbstractClsId = db.ClassificationGroupHasSourceAbstractClsId();
            log("结构自适应检测：分类表 SOURCE_ABSTRACT_CLS_ID=" + (clsHasSourceAbstractClsId ? "存在（写入默认值 0）" : "不存在（已跳过）")
                + "；分类组表=" + (groupHasSourceAbstractClsId ? "存在（写入默认值 0）" : "不存在（已跳过）"));

            int totalGroups = data.ClassificationNodes.Count(n => !n.IsClassification);
            int totalCls = data.ClassificationNodes.Count(n => n.IsClassification);
            int totalProps = data.PropertyDefinitions.Count;
            int totalMeta = data.MetaProperties != null ? data.MetaProperties.Count : 0;
            // 每个属性定义最多 3 步（属性2 + 属性值3），有值的额外 3 步
            int totalValSteps = data.PropertyDefinitions.Count(pd => !string.IsNullOrEmpty(pd.Number) || !string.IsNullOrEmpty(pd.Value)) * 3;
            int totalSteps = totalGroups * 2 + totalCls * 2 + totalProps * 2 + totalValSteps +
                             (data.GenerateForms ? data.ClassificationNodes.Count(n => n.IsClassification) * 4 : 0) + totalMeta;
            int stepDone = 0;
            Action step = () => { stepDone++; progress(Math.Min(99, stepDone * 100 / Math.Max(1, totalSteps))); };

            using (var conn = new SqlConnection(connStr))
            {
                conn.Open();
                using (var tran = conn.BeginTransaction())
                {
                    try
                    {
                        var nodeIds = new Dictionary<string, long>(StringComparer.OrdinalIgnoreCase);

                        // 元模型属性（工作表3）—— 先导入
                        if (data.MetaProperties != null && data.MetaProperties.Count > 0)
                        {
                            // 汇总所有属性行用于冲突检测
                            var allMetaRows = data.MetaProperties
                                .Where(p => !string.IsNullOrWhiteSpace(p.InternalName))
                                .Select(p => new MetaPropertyRow
                                {
                                    RowNumber = p.RowNumber,
                                    InternalName = p.InternalName,
                                    DisplayZhCn = p.DisplayZhCn,
                                    DisplayEnUs = p.DisplayEnUs,
                                    DisplayZhTw = p.DisplayZhTw
                                }).ToList();

                            // 检测冲突：预览元模型属性导入
                            var preview = db.PreviewMetaPropertyImport(allMetaRows);
                            var conflicts = preview.Rows.Where(r => r.Action == "更新").ToList();
                            var skipUpdates = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                            if (conflicts.Count > 0)
                            {
                                var waitHandle = new System.Threading.ManualResetEvent(false);
                                var userAborted = false;

                                if (System.Windows.Forms.Application.OpenForms.Count > 0)
                                {
                                    System.Windows.Forms.Application.OpenForms[0].Invoke(new Action(() =>
                                    {
                                        try
                                        {
                                            using (var pf = new MetaPropertyPreviewForm(preview))
                                            {
                                                pf.Text = "元模型属性冲突处理（超级批量导入）";
                                                pf.ShowDialog();
                                                if (pf.Aborted)
                                                {
                                                    userAborted = true;
                                                    log("用户终止导入，所有数据将回滚");
                                                    return;
                                                }
                                                if (pf.Confirmed)
                                                {
                                                    foreach (var row in preview.Rows)
                                                    {
                                                        if (row.Action == "更新" && !row.ShouldUpdate)
                                                            skipUpdates.Add(row.InternalName);
                                                    }
                                                }
                                                else
                                                {
                                                    foreach (var row in conflicts)
                                                        skipUpdates.Add(row.InternalName);
                                                }
                                            }
                                        }
                                        finally
                                        {
                                            waitHandle.Set();
                                        }
                                    }));
                                }
                                else
                                {
                                    foreach (var row in conflicts)
                                        skipUpdates.Add(row.InternalName);
                                    waitHandle.Set();
                                }
                                waitHandle.WaitOne();

                                if (userAborted)
                                    throw new OperationCanceledException("用户终止导入");

                                log(string.Format("元模型属性冲突处理：{0} 个冲突，{1} 个选择忽略",
                                    conflicts.Count, skipUpdates.Count));
                            }

                            var bySet = data.MetaProperties
                                .GroupBy(p => string.IsNullOrEmpty(p.PropertySetInternalName) ? string.Empty : p.PropertySetInternalName.Trim())
                                .ToList();
                            foreach (var g in bySet)
                            {
                                var rows = g.Select(p => new MetaPropertyRow
                                {
                                    RowNumber = p.RowNumber,
                                    InternalName = p.InternalName,
                                    DisplayZhCn = p.DisplayZhCn,
                                    DisplayEnUs = p.DisplayEnUs,
                                    DisplayZhTw = p.DisplayZhTw
                                }).ToList();
                                var r = db.ImportMetaPropertiesInTran(conn, tran, g.Key, rows, log, skipUpdates: skipUpdates);
                                result.SuccessCount += r.Inserted + r.Updated;
                                log("元模型属性：插入 " + r.Inserted + " 条，更新 " + r.Updated + " 条，属性集跳过 " + r.PropertySetSkipped + " 条");
                                foreach (var name in r.UpdatedNames)
                                    result.Changes.Add("更新元模型属性：" + name);
                                foreach (var name in r.SkippedExistNames)
                                    result.Changes.Add("跳过元模型属性：" + name);
                                for (int k = 0; k < rows.Count; k++) step();
                            }
                        }

                            // 按 name 匹配模式下，记录被复用（而非新建）的分类/分类组名，供后续属性/属性值查重复用
                            var reusedNodeNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                        foreach (var n in data.ClassificationNodes)
                        {
                            long parentId = 0;
                            if (!string.IsNullOrEmpty(n.ParentName))
                            {
                                if (!nodeIds.TryGetValue(n.ParentName, out parentId))
                                    throw new Exception("父节点「" + n.ParentName + "」未找到（第" + n.RowNumber + "行）。");
                            }

                            if (n.IsGroup)
                            {
                                long newId = InsertGroupInTran(conn, tran, n.Name, parentId, groupHasSourceAbstractClsId);
                                string action = "新增分类组";
                                if (data.ClassificationMatchMode == ClassificationMatchMode.MatchByName)
                                {
                                    long? existingId = FindClassificationGroupIdByNameInTran(conn, tran, n.Name);
                                    if (existingId.HasValue)
                                    {
                                        newId = existingId.Value;
                                        reusedNodeNames.Add(n.Name);
                                        action = "复用已有分类组（按分类组名匹配）";
                                        log("分类组「" + n.Name + "」在库中已存在，按分类组名匹配复用（OBJECT_ID=" + newId + "）");
                                    }
                                }
                                nodeIds[n.Name] = newId;
                                result.SuccessCount++;
                                result.Changes.Add(action + "：" + n.Name);
                                step(); step();
                            }
                            else
                            {
                                long newId;
                                string action;
                                if (data.ClassificationMatchMode == ClassificationMatchMode.MatchByName)
                                {
                                    long? existingId = FindClassificationIdByNameInTran(conn, tran, n.Name);
                                    if (existingId.HasValue)
                                    {
                                        newId = existingId.Value;
                                        reusedNodeNames.Add(n.Name);
                                        action = "复用已有分类（按分类名匹配）";
                                        log("分类「" + n.Name + "」在库中已存在，按分类名匹配复用（OBJECT_ID=" + newId + "）");
                                    }
                                    else
                                    {
                                        newId = InsertClassificationInTran(conn, tran, n.Name, parentId, clsHasSourceAbstractClsId);
                                        action = "新增分类";
                                        log("已创建分类「" + n.Name + "」");
                                    }
                                }
                                else
                                {
                                    newId = InsertClassificationInTran(conn, tran, n.Name, parentId, clsHasSourceAbstractClsId);
                                    action = "新增分类";
                                    log("已创建分类「" + n.Name + "」");
                                }
                                nodeIds[n.Name] = newId;
                                result.SuccessCount++;
                                result.Changes.Add(action + "：" + n.Name);
                                step(); step();

                                if (data.GenerateForms)
                                {
                                    GenerateFormInTran(conn, tran, n.Name, newId, db, n.FormName, templateUiContent, log);
                                    result.SuccessCount += 4;
                                    log("已为分类「" + n.Name + "」生成表单");
                                    step(); step(); step(); step();
                                }
                            }
                        }

                        var propCache = new Dictionary<string, long>(StringComparer.OrdinalIgnoreCase);
                        foreach (var pd in data.PropertyDefinitions)
                        {
                            bool isRoot = string.IsNullOrEmpty(pd.ClassificationName);
                            long clsId = 0;
                            string clsLabel = "根属性";
                            if (!isRoot)
                            {
                                if (!nodeIds.TryGetValue(pd.ClassificationName, out clsId))
                                    throw new Exception("分类「" + pd.ClassificationName + "」未找到（第" + pd.RowNumber + "行）。");
                                clsLabel = pd.ClassificationName;
                            }

                            string normalizedInternalName = (pd.InternalName ?? string.Empty).Trim().ToLower();
                            var propKey = clsLabel + "::" + normalizedInternalName;
                            bool allowReuse = !isRoot && reusedNodeNames.Contains(pd.ClassificationName);
                            long propId;
                            if (!propCache.TryGetValue(propKey, out propId))
                            {
                                propId = InsertPropertyInTran(conn, tran, normalizedInternalName, pd.DisplayName, clsId, isRoot, allowReuse);
                                propCache[propKey] = propId;
                                result.SuccessCount++;
                                result.Changes.Add(string.Format("新增属性：[{0}] {1} ({2})",
                                    clsLabel, pd.DisplayName, normalizedInternalName));
                                log("已为" + (isRoot ? "根目录" : "分类「" + clsLabel + "」") + "创建属性「" + pd.DisplayName + "」");
                                step(); step();

                                if (!string.IsNullOrWhiteSpace(pd.PropertySetInternalName))
                                {
                                    AddPropertyToSetInTran(conn, tran, normalizedInternalName, pd.PropertySetInternalName.Trim());
                                    log("已将属性「" + normalizedInternalName + "」加入属性集「" + pd.PropertySetInternalName.Trim() + "」");
                                }
                            }

                            // 属性值（同行的 D/E 列）
                            if (!string.IsNullOrEmpty(pd.Number) || !string.IsNullOrEmpty(pd.Value))
                            {
                                if (data.ClassificationMatchMode == ClassificationMatchMode.MatchByName
                                    && PropertyValueExistsInTran(conn, tran, propId, pd.Number, pd.Value))
                                {
                                    log("属性「" + pd.DisplayName + "」的属性值已存在（按名称匹配），跳过重复插入。");
                                }
                                else
                                {
                                    long valId = InsertPropertyValueInTran(conn, tran, pd.Number, pd.Value);
                                    InsertRelationInTran(conn, tran, propId, valId);
                                    result.SuccessCount++;
                                    result.Changes.Add(string.Format("新增属性值：[{0}] {1} = {2}",
                                        pd.DisplayName, pd.Number, pd.Value));
                                    log("已为属性「" + pd.DisplayName + "」创建值：" + pd.Number + " = " + pd.Value);
                                }
                                step(); step(); step();
                            }
                        }

                        tran.Commit();
                        progress(100);
                        log("==== 导入完成 ====");
                    }
                    catch (OperationCanceledException)
                    {
                        try { tran.Rollback(); } catch { }
                        progress(100);
                        log("用户终止导入，所有数据已回滚。");
                        Logger.Instance.Log("超级批量导入", "用户终止导入，数据已回滚");
                        result.Aborted = true;
                        return result;
                    }
                    catch
                    {
                        try { tran.Rollback(); } catch { }
                        log("导入失败，所有数据已回滚。");
                        throw;
                    }
                }
            }

            return result;
        }

        // ====== 事务内辅助方法 ======

        private static void AddPropertyToSetInTran(SqlConnection conn, SqlTransaction tran, string propertyInternalName, string propertySetInternalName)
        {
            // 统一标准化：去空格 + 转小写
            propertyInternalName = (propertyInternalName ?? string.Empty).Trim().ToLower();
            if (propertyInternalName.Contains(" "))
                throw new Exception(string.Format("属性内部名 [{0}] 包含空格，不允许", propertyInternalName));

            const string selSql = @"SELECT meta FROM [plm_config_v2].[dbo].[meta_element] WITH (UPDLOCK, ROWLOCK) WHERE meta_type = 'property_set' AND internal_name = @name";
            string metaStr;
            using (var cmd = new SqlCommand(selSql, conn, tran))
            {
                cmd.Parameters.AddWithValue("@name", propertySetInternalName);
                var v = cmd.ExecuteScalar();
                if (v == null || v == DBNull.Value)
                    throw new Exception("属性集 [" + propertySetInternalName + "] 不存在，无法将属性 [" + propertyInternalName + "] 加入。");
                metaStr = v.ToString();
            }

            JObject propSetMeta;
            try { propSetMeta = string.IsNullOrEmpty(metaStr) ? new JObject() : JObject.Parse(metaStr); }
            catch { propSetMeta = new JObject(); }

            JArray propsArr = propSetMeta["properties"] as JArray;
            if (propsArr == null)
            {
                propsArr = new JArray();
                propSetMeta["properties"] = propsArr;
            }

            foreach (var p in propsArr)
            {
                if (p != null && p.Type == JTokenType.String && string.Equals((string)p, propertyInternalName, StringComparison.OrdinalIgnoreCase))
                    return;
            }

            propsArr.Add(propertyInternalName);

            const string updSql = @"UPDATE [plm_config_v2].[dbo].[meta_element] SET meta = @meta WHERE meta_type = 'property_set' AND internal_name = @name";
            using (var cmd = new SqlCommand(updSql, conn, tran))
            {
                cmd.Parameters.AddWithValue("@meta", propSetMeta.ToString(Newtonsoft.Json.Formatting.None));
                cmd.Parameters.AddWithValue("@name", propertySetInternalName);
                cmd.ExecuteNonQuery();
            }
        }

        private static long GetNextId(SqlConnection conn, SqlTransaction tran, string tableName)
        {
            using (var cmd = new SqlCommand("SELECT NEXT VALUE FOR [dbo].[" + GetSeqName(tableName) + "]", conn, tran))
            {
                return Convert.ToInt64(cmd.ExecuteScalar());
            }
        }

        private static string GetSeqName(string table)
        {
            switch (table)
            {
                case "t_object_classification_group": return "SEQ_ID_OBJECT_CLASSIFICATION_GROUP";
                case "t_object_classification_group_to_child": return "SEQ_ID_OBJECT_CLASSIFICATION_GROUP_TO_CHILD";
                case "t_object_classification": return "SEQ_ID_OBJECT_CLASSIFICATION";
                case "t_object_classification_group_to_classification": return "SEQ_ID_OBJECT_CLASSIFICATION_GROUP_TO_CLASSIFICATION";
                case "t_property": return "SEQ_ID_PROPERTY";
                case "t_property_value": return "SEQ_ID_PROPERTY_VALUE";
                case "t_property_to_property_value": return "SEQ_ID_PROPERTY_TO_PROPERTY_VALUE";
                default: throw new InvalidOperationException("无对应序列：" + table);
            }
        }

        private static long InsertGroupInTran(SqlConnection conn, SqlTransaction tran, string name, long parentId, bool hasSourceAbstractClsId)
        {
            var now = DateTime.Now;
            long newId = GetNextId(conn, tran, "t_object_classification_group");
            // 注意：SQL 中若含 '{}' 等花括号字面量，不能用 string.Format（会被当占位符），故用拼接
            string sourceCol = hasSourceAbstractClsId ? ", " + DbHelper.ColSourceAbstractClsId : string.Empty;
            string sourceVal = hasSourceAbstractClsId ? ", 0" : string.Empty;
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
            long relId = GetNextId(conn, tran, "t_object_classification_group_to_child");
            using (var cmd = new SqlCommand(@"
                INSERT INTO [dbo].[t_object_classification_group_to_child]
                    (OBJECT_ID, INTERNAL_NAME, ROLEA_INTERNAL_NAME, ROLEA_OBJECT_ID,
                     ROLEB_INTERNAL_NAME, ROLEB_OBJECT_ID, NAME, DESCRIPTION,
                     CREATOR, CREATOR_OBJECT_ID, CREATOR_INTERNAL_NAME, CREATION_DATETIME,
                     LASTMODIFIER, LASTMODIFIER_OBJECT_ID, LASTMODIFIER_INTERNAL_NAME)
                VALUES
                    (@oid, 'object_classification_group_to_child', 'object_classification_group', @parentId,
                     'object_classification_group', @childId, NULL, NULL,
                     '试用员', 9007199254740990, 'person', @dt,
                     NULL, NULL, NULL)", conn, tran))
            {
                cmd.Parameters.AddWithValue("@oid", relId);
                cmd.Parameters.AddWithValue("@parentId", parentId);
                cmd.Parameters.AddWithValue("@childId", newId);
                cmd.Parameters.AddWithValue("@dt", now);
                cmd.ExecuteNonQuery();
            }
            return newId;
        }

        private static long InsertClassificationInTran(SqlConnection conn, SqlTransaction tran, string name, long groupId, bool hasSourceAbstractClsId)
        {
            var now = DateTime.Now;
            long newId = GetNextId(conn, tran, "t_object_classification");
            // 注意：SQL 中含 '{}' 字面量，不能用 string.Format（花括号会被当占位符），故用拼接
            string sourceCol = hasSourceAbstractClsId ? ", " + DbHelper.ColSourceAbstractClsId : string.Empty;
            string sourceVal = hasSourceAbstractClsId ? ", 0" : string.Empty;
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
            long linkId = GetNextId(conn, tran, "t_object_classification_group_to_classification");
            using (var cmd = new SqlCommand(@"
                INSERT INTO [dbo].[t_object_classification_group_to_classification]
                    (OBJECT_ID, INTERNAL_NAME, ROLEA_INTERNAL_NAME, ROLEA_OBJECT_ID,
                     ROLEB_INTERNAL_NAME, ROLEB_OBJECT_ID, NAME, DESCRIPTION,
                     CREATOR, CREATOR_OBJECT_ID, CREATOR_INTERNAL_NAME, CREATION_DATETIME,
                     LASTMODIFIER, LASTMODIFIER_OBJECT_ID, LASTMODIFIER_INTERNAL_NAME)
                VALUES
                    (@oid, 'object_classification_group_to_classification', 'object_classification_group', @gid,
                     'object_classification', @cid, NULL, NULL,
                     '试用员', 9007199254740990, 'person', @dt,
                     NULL, NULL, NULL)", conn, tran))
            {
                cmd.Parameters.AddWithValue("@oid", linkId);
                cmd.Parameters.AddWithValue("@gid", groupId);
                cmd.Parameters.AddWithValue("@cid", newId);
                cmd.Parameters.AddWithValue("@dt", now);
                cmd.ExecuteNonQuery();
            }

            InsertPropertySetMetaElementInTran(conn, tran, newId);
            return newId;
        }

        private static void InsertPropertySetMetaElementInTran(SqlConnection conn, SqlTransaction tran, long clsId)
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

        private static void GenerateFormInTran(SqlConnection conn, SqlTransaction tran, string name, long clsObjId, DbHelper db, string formName, string templateUiContent, Action<string> log)
        {
            var now = DateTime.Now;

            const string existSql = @"
                SELECT COUNT(1)
                FROM [plm_config_v2].[dbo].[form_meta_template_definition] f
                JOIN [dbo].[t_object_classification] c ON f.classification_object_id = c.OBJECT_ID
                WHERE f.class_internal_name = 'part' AND c.NAME = @name";
            using (var chk = new SqlCommand(existSql, conn, tran))
            {
                chk.Parameters.AddWithValue("@name", name);
                int cnt = Convert.ToInt32(chk.ExecuteScalar());
                if (cnt > 0)
                {
                    log("分类「" + name + "」已存在对应表单模板，跳过表单插入。");
                    return;
                }
            }

            string templateUi = templateUiContent;
            if (string.IsNullOrEmpty(templateUi))
            {
                const string templateSql = @"
                    SELECT TOP 1 content FROM [plm_config_v2].[dbo].[form_meta_template_ui]
                    WHERE template_id = (
                        SELECT TOP 1 id FROM [plm_config_v2].[dbo].[form_meta_template_definition]
                        WHERE class_internal_name = 'part'
                        ORDER BY creation_datetime DESC
                    )";
                using (var cmd = new SqlCommand(templateSql, conn, tran))
                {
                    var val = cmd.ExecuteScalar();
                    if (val != null && val != DBNull.Value) templateUi = val.ToString();
                }
            }

            string defId = Guid.NewGuid().ToString("D");
            string dn = new JObject
            {
                ["en-us"] = string.IsNullOrEmpty(formName) ? name : formName,
                ["zh-cn"] = string.IsNullOrEmpty(formName) ? name : formName,
                ["zh-tw"] = ""
            }.ToString(Newtonsoft.Json.Formatting.None);

            using (var cmd = new SqlCommand(@"
                INSERT INTO [plm_config_v2].[dbo].[form_meta_template_definition]
                    (id, internal_name, class_internal_name, classification_object_id,
                     property_condition, display_name, description,
                     creator, creation_datetime, version, owner_type, owner_internal_name, state)
                VALUES
                    (@id, @intName, 'part', @clsObjId,
                     @propCond, @dn, @desc,
                     '试用员', @dt, 'v1.0.0', 1, 'person', 0)", conn, tran))
            {
                cmd.Parameters.AddWithValue("@id", defId);
                cmd.Parameters.AddWithValue("@intName", "part-classification" + clsObjId.ToString());
                cmd.Parameters.AddWithValue("@clsObjId", clsObjId);
                cmd.Parameters.AddWithValue("@propCond", "{\"property_internal_name\":\"\",\"property_value\":\"\"}");
                cmd.Parameters.AddWithValue("@dn", dn);
                cmd.Parameters.AddWithValue("@desc", dn);
                cmd.Parameters.AddWithValue("@dt", now);
                cmd.ExecuteNonQuery();
            }

            string settingId = Guid.NewGuid().ToString("D");
            using (var cmd = new SqlCommand(@"
                INSERT INTO [plm_config_v2].[dbo].[form_meta_template_setting]
                    (id, template_id, setting_type, content, creator, creation_datetime)
                VALUES
                    (@id, @tplId, 'export', '{}', '试用员', @dt)", conn, tran))
            {
                cmd.Parameters.AddWithValue("@id", settingId);
                cmd.Parameters.AddWithValue("@tplId", defId);
                cmd.Parameters.AddWithValue("@dt", now);
                cmd.ExecuteNonQuery();
            }

            string uiId = Guid.NewGuid().ToString("D");
            using (var cmd = new SqlCommand(@"
                INSERT INTO [plm_config_v2].[dbo].[form_meta_template_ui]
                    (id, template_id, content, creator, creation_datetime)
                VALUES
                    (@id, @tplId, @content, '试用员', @dt)", conn, tran))
            {
                cmd.Parameters.AddWithValue("@id", uiId);
                cmd.Parameters.AddWithValue("@tplId", defId);
                cmd.Parameters.AddWithValue("@content", templateUi);
                cmd.Parameters.AddWithValue("@dt", now);
                cmd.ExecuteNonQuery();
            }
        }

        private static long InsertPropertyInTran(SqlConnection conn, SqlTransaction tran, string internalName, string displayName, long clsId, bool isRoot = false, bool allowReuseExisting = false)
        {
            // 统一标准化：去空格 + 转小写
            internalName = (internalName ?? string.Empty).Trim().ToLower();
            if (internalName.Contains(" "))
                throw new Exception(string.Format("属性内部名 [{0}] 包含空格，不允许", internalName));

            // 按分类名匹配模式下，若分类已存在且同分类下已有同名属性，则复用已有属性，避免重复插入
            if (!isRoot && allowReuseExisting)
            {
                const string findSql = "SELECT OBJECT_ID FROM [dbo].[t_property] WHERE CLS_ID = @clsId AND PROPERTY_INTERNAL_NAME = @intName";
                using (var fcmd = new SqlCommand(findSql, conn, tran))
                {
                    fcmd.Parameters.AddWithValue("@clsId", clsId);
                    fcmd.Parameters.AddWithValue("@intName", internalName);
                    var fobj = fcmd.ExecuteScalar();
                    if (fobj != null && fobj != DBNull.Value)
                    {
                        return Convert.ToInt64(fobj);
                    }
                }
            }

            var now = DateTime.Now;
            long newId = GetNextId(conn, tran, "t_property");

            // 根模式：CLS_INTERNAL_NAME = NULL, CLS_ID = 0
            // 分类模式：CLS_INTERNAL_NAME = 'object_classification', CLS_ID = 分类OBJECT_ID
            string clsInternalName = isRoot ? null : "object_classification";
            long actualClsId = isRoot ? 0 : clsId;

            using (var cmd = new SqlCommand(@"
                INSERT INTO [dbo].[t_property]
                    (OBJECT_ID, INTERNAL_NAME, CLASS_INTERNAL_NAME, CLS_INTERNAL_NAME, CLS_ID,
                     PROPERTY_INTERNAL_NAME, SUPPORT_MANUAL_FLAG, SELECTION_PROPERTIES,
                     NAME, CREATOR, CREATOR_OBJECT_ID, CREATOR_INTERNAL_NAME, CREATION_DATETIME)
                VALUES
                    (@oid, 'property', 'part',
                     @clsIntName, @clsId,
                     @propIntName, 0, '{}',
                     @name, '试用员', 9007199254740990, 'person', @dt)", conn, tran))
            {
                cmd.Parameters.AddWithValue("@oid", newId);
                cmd.Parameters.AddWithValue("@clsIntName", (object)clsInternalName ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@clsId", actualClsId);
                cmd.Parameters.AddWithValue("@propIntName", internalName);
                cmd.Parameters.AddWithValue("@name", displayName);
                cmd.Parameters.AddWithValue("@dt", now);
                cmd.ExecuteNonQuery();
            }
            return newId;
        }

        private static long? FindClassificationIdByNameInTran(SqlConnection conn, SqlTransaction tran, string name)
        {
            const string sql = "SELECT OBJECT_ID FROM [dbo].[t_object_classification] WHERE NAME = @name";
            using (var cmd = new SqlCommand(sql, conn, tran))
            {
                cmd.Parameters.AddWithValue("@name", name);
                var obj = cmd.ExecuteScalar();
                if (obj == null || obj == DBNull.Value) return null;
                return Convert.ToInt64(obj);
            }
        }

        private static long? FindClassificationGroupIdByNameInTran(SqlConnection conn, SqlTransaction tran, string name)
        {
            const string sql = "SELECT OBJECT_ID FROM [dbo].[t_object_classification_group] WHERE NAME = @name";
            using (var cmd = new SqlCommand(sql, conn, tran))
            {
                cmd.Parameters.AddWithValue("@name", name);
                var obj = cmd.ExecuteScalar();
                if (obj == null || obj == DBNull.Value) return null;
                return Convert.ToInt64(obj);
            }
        }

        private static bool PropertyValueExistsInTran(SqlConnection conn, SqlTransaction tran, long propId, string number, string value)
        {
            const string sql = @"
                SELECT COUNT(1) FROM [dbo].[t_property_to_property_value] r
                JOIN [dbo].[t_property_value] v ON r.ROLEB_OBJECT_ID = v.OBJECT_ID
                WHERE r.ROLEA_OBJECT_ID = @propId
                  AND ISNULL(v.PROPERTY_NUMBER, '') = @num
                  AND ISNULL(v.PROPERTY_VALUE, '') = @val";
            using (var cmd = new SqlCommand(sql, conn, tran))
            {
                cmd.Parameters.AddWithValue("@propId", propId);
                cmd.Parameters.AddWithValue("@num", (object)(number ?? string.Empty));
                cmd.Parameters.AddWithValue("@val", (object)(value ?? string.Empty));
                return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
            }
        }

        private static long InsertPropertyValueInTran(SqlConnection conn, SqlTransaction tran, string number, string value)
        {
            var now = DateTime.Now;
            long newId = GetNextId(conn, tran, "t_property_value");
            using (var cmd = new SqlCommand(@"
                INSERT INTO [dbo].[t_property_value]
                    (OBJECT_ID, INTERNAL_NAME, PROPERTY_VALUE, PROPERTY_NUMBER,
                     PROPERTY_VALUE_NUMBER, DEFAULT_VALUE_FLAG,
                     CREATOR, CREATOR_OBJECT_ID, CREATOR_INTERNAL_NAME, CREATION_DATETIME)
                VALUES
                    (@oid, 'property_value', @val, @num,
                     2, 0,
                     '试用员', 9007199254740990, 'person', @dt)", conn, tran))
            {
                cmd.Parameters.AddWithValue("@oid", newId);
                cmd.Parameters.AddWithValue("@num", number);
                cmd.Parameters.AddWithValue("@val", value);
                cmd.Parameters.AddWithValue("@dt", now);
                cmd.ExecuteNonQuery();
            }
            return newId;
        }

        private static void InsertRelationInTran(SqlConnection conn, SqlTransaction tran, long propId, long valId)
        {
            var now = DateTime.Now;
            long newId = GetNextId(conn, tran, "t_property_to_property_value");
            using (var cmd = new SqlCommand(@"
                INSERT INTO [dbo].[t_property_to_property_value]
                    (OBJECT_ID, INTERNAL_NAME, ROLEA_INTERNAL_NAME, ROLEA_OBJECT_ID,
                     ROLEB_INTERNAL_NAME, ROLEB_OBJECT_ID,
                     CREATOR, CREATOR_OBJECT_ID, CREATOR_INTERNAL_NAME, CREATION_DATETIME)
                VALUES
                    (@oid, 'property_to_property_value', 'property', @propId,
                     'property_value', @valId,
                     '试用员', 9007199254740990, 'person', @dt)", conn, tran))
            {
                cmd.Parameters.AddWithValue("@oid", newId);
                cmd.Parameters.AddWithValue("@propId", propId);
                cmd.Parameters.AddWithValue("@valId", valId);
                cmd.Parameters.AddWithValue("@dt", now);
                cmd.ExecuteNonQuery();
            }
        }

        private static long ResolvePropertyIdInTran(SqlConnection conn, SqlTransaction tran, string clsName, string propDisplayName, Dictionary<string, long> nodeIds)
        {
            if (!nodeIds.TryGetValue(clsName, out long clsId)) return -1;
            const string sql = "SELECT OBJECT_ID FROM [dbo].[t_property] WHERE CLS_ID = @clsId AND NAME = @name";
            using (var cmd = new SqlCommand(sql, conn, tran))
            {
                cmd.Parameters.AddWithValue("@clsId", clsId);
                cmd.Parameters.AddWithValue("@name", propDisplayName);
                var val = cmd.ExecuteScalar();
                return val == null || val == DBNull.Value ? -1 : Convert.ToInt64(val);
            }
        }

        // ====== 7. 元模型属性替换到表单模板 Content（删除不匹配、更新匹配、新增未匹配）======

        /// <summary>检测模板 content 中与 Sheet4 匹配的属性冲突</summary>
        public static List<PropertyConflict> DetectContentConflicts(string templateContent, List<SuperBatchMetaProperty> metaProps)
        {
            var conflicts = new List<PropertyConflict>();
            if (string.IsNullOrEmpty(templateContent) || metaProps == null || metaProps.Count == 0)
                return conflicts;

            JObject root;
            try { root = JObject.Parse(templateContent); }
            catch { return conflicts; }

            var existing = new Dictionary<string, JObject>(StringComparer.OrdinalIgnoreCase);
            CollectPropertyItems(root, existing);

            foreach (var mp in metaProps)
            {
                if (string.IsNullOrWhiteSpace(mp.InternalName)) continue;
                string normName = (mp.InternalName ?? string.Empty).Trim().ToLower();
                if (existing.TryGetValue(normName, out var item))
                {
                    string oldZhCn = item["display_name"]?["zh-cn"]?.ToString() ?? string.Empty;
                    conflicts.Add(new PropertyConflict
                    {
                        InternalName = normName,
                        OldZhCn = oldZhCn,
                        NewZhCn = mp.DisplayZhCn ?? string.Empty
                    });
                }
            }
            return conflicts;
        }

        /// <summary>
        /// 用 Sheet4 元模型属性完全替换模板 content 中的 form items：
        /// ① 匹配到的属性 → 更新 display_name（除非 skipUpdates）
        /// ② content 中不匹配的属性 → 删除
        /// ③ Sheet4 新增的属性 → 追加到 content
        /// </summary>
        public static string MergeMetaIntoContent(string templateContent, List<SuperBatchMetaProperty> metaProps, HashSet<string> skipUpdates = null)
        {
            if (string.IsNullOrEmpty(templateContent))
                return templateContent;

            JObject root;
            try { root = JObject.Parse(templateContent); }
            catch { return templateContent; }

            skipUpdates = skipUpdates ?? new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            var sheetProps = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var mpDict = new Dictionary<string, SuperBatchMetaProperty>(StringComparer.OrdinalIgnoreCase);
            foreach (var mp in metaProps)
            {
                if (!string.IsNullOrWhiteSpace(mp.InternalName))
                {
                    string normName = (mp.InternalName ?? string.Empty).Trim().ToLower();
                    sheetProps.Add(normName);
                    mpDict[normName] = mp;
                }
            }

            var existing = new Dictionary<string, JObject>(StringComparer.OrdinalIgnoreCase);
            CollectPropertyItems(root, existing);

            JArray mainRules = root["rules"] as JArray;
            JArray lastFormArray = null;
            if (mainRules != null && mainRules.Count > 0)
                lastFormArray = FindFormItemArray(mainRules);
            if (lastFormArray == null)
                lastFormArray = mainRules;

            RemoveUnmatchedProperties(root, sheetProps, mpDict, skipUpdates);

            if (lastFormArray != null)
            {
                foreach (var mp in metaProps)
                {
                    if (string.IsNullOrWhiteSpace(mp.InternalName)) continue;
                    if (!existing.ContainsKey(mp.InternalName))
                        lastFormArray.Add(CreateFormItem(mp));
                }
            }

            return root.ToString(Newtonsoft.Json.Formatting.None);
        }

        private static void RemoveUnmatchedProperties(JToken node, HashSet<string> sheetProps, Dictionary<string, SuperBatchMetaProperty> mpDict, HashSet<string> skipUpdates)
        {
            if (node is JObject obj)
            {
                foreach (var kv in obj)
                    RemoveUnmatchedProperties(kv.Value, sheetProps, mpDict, skipUpdates);
            }
            else if (node is JArray arr)
            {
                var toRemove = new List<JToken>();
                foreach (var item in arr)
                {
                    if (item is JObject o && o["property"] != null)
                    {
                        string propName = o["property"]?.ToString();
                        if (!string.IsNullOrEmpty(propName) && sheetProps.Contains(propName))
                        {
                            if (!skipUpdates.Contains(propName) && mpDict.TryGetValue(propName, out var mp))
                                UpdateFormItemDisplay(o, mp);
                        }
                        else
                        {
                            toRemove.Add(item);
                        }
                    }
                    else
                    {
                        RemoveUnmatchedProperties(item, sheetProps, mpDict, skipUpdates);
                    }
                }
                foreach (var item in toRemove)
                    arr.Remove(item);
            }
        }

        private static JArray FindFormItemArray(JToken node)
        {
            JArray result = null;
            FindFormItemArrayRecursive(node, ref result);
            return result;
        }

        private static void FindFormItemArrayRecursive(JToken node, ref JArray result)
        {
            if (node is JObject obj)
            {
                foreach (var kv in obj)
                    FindFormItemArrayRecursive(kv.Value, ref result);
            }
            else if (node is JArray arr)
            {
                foreach (var item in arr)
                {
                    if (item is JObject o && o["property"] != null)
                    {
                        result = arr;
                        return;
                    }
                }
                foreach (var item in arr)
                {
                    FindFormItemArrayRecursive(item, ref result);
                    if (result != null) return;
                }
            }
        }

        private static void CollectPropertyItems(JToken node, Dictionary<string, JObject> result)
        {
            if (node is JObject obj)
            {
                var prop = obj["property"];
                if (prop != null && prop.Type == JTokenType.String && !string.IsNullOrEmpty((string)prop))
                    result[(string)prop] = obj;
                foreach (var kv in obj)
                    CollectPropertyItems(kv.Value, result);
            }
            else if (node is JArray arr)
            {
                foreach (var item in arr)
                    CollectPropertyItems(item, result);
            }
        }

        private static void UpdateFormItemDisplay(JObject item, SuperBatchMetaProperty mp)
        {
            var dn = item["display_name"] as JObject;
            if (dn == null) { dn = new JObject(); item["display_name"] = dn; }
            dn["en-us"] = mp.DisplayEnUs ?? string.Empty;
            dn["zh-cn"] = mp.DisplayZhCn ?? string.Empty;
            dn["zh-tw"] = mp.DisplayZhTw ?? string.Empty;

            var desc = item["description"] as JObject;
            if (desc == null) { desc = new JObject(); item["description"] = desc; }
            desc["en-us"] = mp.DisplayEnUs ?? string.Empty;
            desc["zh-cn"] = mp.DisplayZhCn ?? string.Empty;
            desc["zh-tw"] = mp.DisplayZhTw ?? string.Empty;
        }

        private static JObject CreateFormItem(SuperBatchMetaProperty mp)
        {
            string id = "m" + Guid.NewGuid().ToString("N").Substring(0, 12);
            string name = (mp.InternalName ?? string.Empty).Trim().ToLower();
            string en = mp.DisplayEnUs ?? string.Empty;
            string zh = mp.DisplayZhCn ?? string.Empty;
            string tw = mp.DisplayZhTw ?? string.Empty;

            return new JObject
            {
                ["_fc_drag_tag"] = "input",
                ["_id"] = id,
                ["advancedConfig"] = new JArray(),
                ["children"] = new JArray(),
                ["data_type"] = "string",
                ["default_val"] = string.Empty,
                ["description"] = new JObject { ["en-us"] = en, ["zh-cn"] = zh, ["zh-tw"] = tw },
                ["display"] = true,
                ["display_name"] = new JObject { ["en-us"] = en, ["zh-cn"] = zh, ["zh-tw"] = tw },
                ["field"] = "part-" + name,
                ["hidden"] = false,
                ["labelWidth"] = null,
                ["meta"] = new JArray { "part" },
                ["property"] = name,
                ["props"] = new JObject
                {
                    ["addSetting"] = true,
                    ["allowClear"] = true,
                    ["disableCtrl"] = 0,
                    ["disabled"] = false,
                    ["display"] = true,
                    ["displayCtrl"] = 1,
                    ["maxRows"] = 1,
                    ["requireCtrl"] = 1,
                    ["required"] = true,
                    ["size"] = "small"
                },
                ["request"] = new JObject
                {
                    ["icon"] = "op_reload",
                    ["method"] = "GET",
                    ["params"] = new JArray(),
                    ["response"] = string.Empty,
                    ["tooltip"] = new JObject { ["en-us"] = string.Empty, ["zh-cn"] = string.Empty, ["zh-tw"] = string.Empty },
                    ["url"] = string.Empty
                },
                ["status"] = new JObject
                {
                    ["disableRule"] = new JArray(),
                    ["displayRule"] = new JArray(),
                    ["requireRule"] = new JArray(),
                    ["subscriber"] = new JArray()
                },
                ["type"] = "input",
                ["validate"] = new JArray { new JObject { ["message"] = string.Empty, ["required"] = true, ["type"] = "string" } },
                ["value"] = string.Empty,
                ["valueConfig"] = new JObject
                {
                    ["matchProperty"] = new JArray(),
                    ["type"] = "common"
                },
                ["wrap"] = new JObject()
            };
        }
    }
}
