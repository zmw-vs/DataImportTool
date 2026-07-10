using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace DataImportTool
{
    /// <summary>
    /// 导入预览 TreeView 节点构建帮助类
    /// 为各导入功能构建预览 TreeView 节点
    /// </summary>
    public static class ImportPreviewHelper
    {
        /// <summary>
        /// 构建分类组/分类混合导入预览节点
        /// </summary>
        /// <param name="rows">分类组导入行数据</param>
        /// <returns>TreeView 节点列表</returns>
        public static List<TreeNode> BuildGroupImportPreview(List<ClassificationNodeRow> rows)
        {
            var nodes = new List<TreeNode>();
            if (rows == null || rows.Count == 0) return nodes;

            // 使用字典建立父子关系
            var nodeDict = new Dictionary<string, TreeNode>(StringComparer.OrdinalIgnoreCase);
            var parentNodeNames = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            // 第一遍：创建所有节点并记录父子关系
            foreach (var row in rows)
            {
                string nodeText = string.Format("{0} [{1}]", row.Name ?? "(空)", row.NodeType ?? "未知");
                var node = new TreeNode(nodeText);
                node.Tag = row;
                nodeDict[row.Name ?? string.Empty] = node;

                if (!string.IsNullOrWhiteSpace(row.ParentName))
                {
                    parentNodeNames[row.Name ?? string.Empty] = row.ParentName;
                }
            }

            // 第二遍：建立父子层级
            foreach (var kvp in nodeDict)
            {
                string nodeName = kvp.Key;
                var node = kvp.Value;

                if (parentNodeNames.TryGetValue(nodeName, out string parentName))
                {
                    if (nodeDict.TryGetValue(parentName, out var parentNode))
                    {
                        parentNode.Nodes.Add(node);
                    }
                    else
                    {
                        // 父节点不存在，作为根节点
                        nodes.Add(node);
                    }
                }
                else
                {
                    // 根节点
                    nodes.Add(node);
                }
            }

            return nodes;
        }

        /// <summary>
        /// 构建分类插入预览节点
        /// </summary>
        /// <param name="rows">分类插入行数据</param>
        /// <returns>TreeView 节点列表</returns>
        public static List<TreeNode> BuildClassificationInsertPreview(List<KeyValuePair<int, string>> rows)
        {
            var nodes = new List<TreeNode>();
            if (rows == null || rows.Count == 0) return nodes;

            var root = new TreeNode(string.Format("分类列表（共 {0} 个）", rows.Count));

            foreach (var row in rows)
            {
                string name = row.Value ?? "(空)";
                var node = new TreeNode(string.Format("行 {0}：{1}", row.Key, name));
                root.Nodes.Add(node);
            }

            nodes.Add(root);
            return nodes;
        }

        /// <summary>
        /// 构建批量属性值导入（物品维度）预览节点
        /// 分类下直接显示属性层级，不显示物品中间层
        /// </summary>
        /// <param name="clsName">分类名称</param>
        /// <param name="rows">批量属性值导入行数据</param>
        /// <returns>TreeView 节点列表</returns>
        public static List<TreeNode> BuildPropertyImportPreview(string clsName, List<PropertyBatchRow> rows)
        {
            var nodes = new List<TreeNode>();
            if (rows == null || rows.Count == 0) return nodes;

            var root = new TreeNode(string.Format("分类 [{0}] - 批量属性值导入（共 {1} 行）",
                clsName ?? "未指定", rows.Count));

            // 按行展示，每行下直接挂属性节点
            foreach (var row in rows)
            {
                string rowLabel = string.IsNullOrWhiteSpace(row.ItemName)
                    ? string.Format("行 {0}", row.RowNumber)
                    : string.Format("行 {0}：{1}", row.RowNumber, row.ItemName);
                var rowNode = new TreeNode(rowLabel);

                foreach (var kvp in row.PropertyValues)
                {
                    if (string.IsNullOrEmpty(kvp.Value)) continue;

                    var propNode = new TreeNode(string.Format("{0} = {1}", kvp.Key, kvp.Value));
                    rowNode.Nodes.Add(propNode);
                }

                if (rowNode.Nodes.Count > 0)
                {
                    root.Nodes.Add(rowNode);
                }
            }

            nodes.Add(root);
            return nodes;
        }

        /// <summary>
        /// 构建批量属性值导入（属性值维度）预览节点
        /// </summary>
        /// <param name="propName">属性名称</param>
        /// <param name="rows">属性值导入行数据</param>
        /// <returns>TreeView 节点列表</returns>
        public static List<TreeNode> BuildPropertyValueImportPreview(string propName, List<PropertyValueBatchRow> rows)
        {
            var nodes = new List<TreeNode>();
            if (rows == null || rows.Count == 0) return nodes;

            var root = new TreeNode(string.Format("属性 [{0}] - 属性值导入（共 {1} 条）",
                propName ?? "未指定", rows.Count));

            foreach (var row in rows)
            {
                var node = new TreeNode(string.Format("行 {0}：{1} = {2}",
                    row.RowNumber, row.PropertyName ?? "(空)", row.PropertyValue ?? "(空)"));
                root.Nodes.Add(node);
            }

            nodes.Add(root);
            return nodes;
        }

        /// <summary>
        /// 构建属性值导入（单属性）预览节点
        /// </summary>
        /// <param name="propName">属性名称</param>
        /// <param name="rows">属性值行数据</param>
        /// <returns>TreeView 节点列表</returns>
        public static List<TreeNode> BuildSinglePropertyValueImportPreview(string propName, List<PropertyValueRow> rows)
        {
            var nodes = new List<TreeNode>();
            if (rows == null || rows.Count == 0) return nodes;

            var root = new TreeNode(string.Format("属性 [{0}] - 属性值导入（共 {1} 条）",
                propName ?? "未指定", rows.Count));

            foreach (var row in rows)
            {
                var node = new TreeNode(string.Format("行 {0}：编号={1}，值={2}",
                    row.RowNumber, row.PropertyNumber ?? "(无)", row.PropertyValue ?? "(空)"));
                root.Nodes.Add(node);
            }

            nodes.Add(root);
            return nodes;
        }

        /// <summary>
        /// 将错误列表转换为字符串列表
        /// </summary>
        /// <param name="errors">错误列表</param>
        /// <returns>错误字符串列表</returns>
        public static List<string> ErrorsToStrings(List<ImportError> errors)
        {
            var list = new List<string>();
            if (errors == null) return list;

            foreach (var err in errors)
            {
                list.Add(err.ToString());
            }

            return list;
        }
    }
}
