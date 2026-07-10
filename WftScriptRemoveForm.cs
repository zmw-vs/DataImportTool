using System;
using System.IO;
using System.Windows.Forms;
using System.Xml;

namespace DataImportTool
{
    public partial class WftScriptRemoveForm : Form
    {
        private string _sourceFolder = string.Empty;
        private string _destFolder = string.Empty;

        public WftScriptRemoveForm()
        {
            InitializeComponent();
        }

        private void WftScriptRemoveForm_Load(object sender, EventArgs e)
        {
            // 设置默认值（多个名称请用逗号或分号分隔）
            txtNodeName.Text = "服务端提交_签名";
        }

        private void btnSelectSource_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog dialog = new FolderBrowserDialog())
            {
                dialog.Description = "请选择包含WFT文件的源文件夹";
                dialog.ShowNewFolderButton = false;

                if (!string.IsNullOrEmpty(_sourceFolder))
                    dialog.SelectedPath = _sourceFolder;

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    _sourceFolder = dialog.SelectedPath;
                    if (!_sourceFolder.EndsWith("\\"))
                        _sourceFolder += "\\";
                    txtSourcePath.Text = _sourceFolder;
                }
            }
        }

        private void btnSelectDest_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog dialog = new FolderBrowserDialog())
            {
                dialog.Description = "请选择处理后文件的保存位置";
                dialog.ShowNewFolderButton = true;

                if (!string.IsNullOrEmpty(_destFolder))
                    dialog.SelectedPath = _destFolder;

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    _destFolder = dialog.SelectedPath;
                    if (!_destFolder.EndsWith("\\"))
                        _destFolder += "\\";
                    txtDestPath.Text = _destFolder;
                }
            }
        }

        private void btnProcess_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_sourceFolder))
            {
                MessageBox.Show("请选择源文件夹！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrEmpty(_destFolder))
            {
                MessageBox.Show("请选择保存位置！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrEmpty(txtNodeName.Text.Trim()))
            {
                MessageBox.Show("请输入要删除的节点名称（多个名称请用逗号或分号分隔）！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtNodeName.Focus();
                return;
            }

            btnProcess.Enabled = false;
            lblStatus.Text = "正在处理...";
            lblStatus.ForeColor = System.Drawing.Color.Blue;
            progressBar.Value = 0;
            listBox.Items.Clear();

            backgroundWorker.RunWorkerAsync(txtNodeName.Text.Trim());
        }

        private void backgroundWorker_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            try
            {
                string nodeNamesInput = e.Argument as string;
                if (string.IsNullOrEmpty(nodeNamesInput))
                    nodeNamesInput = "服务端提交_签名";

                string[] wftFiles = Directory.GetFiles(_sourceFolder, "*.wft", SearchOption.AllDirectories);

                if (wftFiles.Length == 0)
                {
                    this.Invoke(new Action(() =>
                    {
                        listBox.Items.Add("源文件夹中没有找到.wft文件！");
                        lblStatus.Text = "处理完成";
                        lblStatus.ForeColor = System.Drawing.Color.Orange;
                    }));
                    return;
                }

                int total = wftFiles.Length;
                int success = 0;
                int failed = 0;
                int removedAppCount = 0;
                int removedObjEventCount = 0;

                for (int i = 0; i < total; i++)
                {
                    string filePath = wftFiles[i];
                    string fileName = Path.GetFileName(filePath);

                    try
                    {
                        XmlDocument doc = new XmlDocument();
                        doc.Load(filePath);

                        int removedApp = RemoveApplicationNode(doc, nodeNamesInput);
                        removedAppCount += removedApp;

                        int removedObjEvent = RemoveObjectEventNode(doc, nodeNamesInput);
                        removedObjEventCount += removedObjEvent;

                        string relativePath = filePath.Substring(_sourceFolder.Length);
                        string destPath = _destFolder + relativePath;
                        string destDir = Path.GetDirectoryName(destPath);
                        if (!string.IsNullOrEmpty(destDir) && !Directory.Exists(destDir))
                            Directory.CreateDirectory(destDir);

                        doc.Save(destPath);

                        if (File.Exists(destPath))
                        {
                            File.SetAttributes(destPath, FileAttributes.Normal);
                            success++;

                            this.Invoke(new Action(() =>
                            {
                                string msg = string.Format("✓ {0}", fileName);
                                if (removedApp > 0 || removedObjEvent > 0)
                                {
                                    msg += " (已删除";
                                    if (removedApp > 0)
                                        msg += string.Format(" App×{0}", removedApp);
                                    if (removedObjEvent > 0)
                                        msg += string.Format(" ObjEvt×{0}", removedObjEvent);
                                    msg += ")";
                                }
                                else
                                {
                                    msg += " (未找到匹配节点)";
                                }
                                listBox.Items.Add(msg);
                                lblStatus.Text = string.Format("正在处理: {0}/{1}", i + 1, total);
                                progressBar.Value = (int)((i + 1) * 100.0 / total);
                            }));
                        }
                        else
                        {
                            failed++;
                            this.Invoke(new Action(() =>
                            {
                                listBox.Items.Add("✗ " + fileName + ": 文件保存失败！");
                            }));
                        }
                    }
                    catch (Exception ex)
                    {
                        failed++;
                        this.Invoke(new Action(() =>
                        {
                            listBox.Items.Add("✗ " + fileName + ": " + ex.Message);
                        }));
                    }
                }

                this.Invoke(new Action(() =>
                {
                    string resultMsg = string.Format("处理完成！成功: {0}, 失败: {1}", success, failed);
                    if (removedAppCount > 0 || removedObjEventCount > 0)
                    {
                        resultMsg += "\n删除节点统计：";
                        if (removedAppCount > 0)
                            resultMsg += string.Format(" Application × {0}", removedAppCount);
                        if (removedObjEventCount > 0)
                            resultMsg += string.Format(" ObjectEvent × {0}", removedObjEventCount);
                    }
                    else
                    {
                        resultMsg += "\n未删除任何节点（可能未找到匹配项）";
                    }
                    lblStatus.Text = string.Format("成功: {0}, 失败: {1}", success, failed);
                    lblStatus.ForeColor = failed > 0 ? System.Drawing.Color.Orange : System.Drawing.Color.Green;
                    MessageBox.Show(resultMsg, "完成", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }));
            }
            catch (Exception ex)
            {
                this.Invoke(new Action(() =>
                {
                    listBox.Items.Add("错误: " + ex.Message);
                    lblStatus.Text = "处理失败";
                    lblStatus.ForeColor = System.Drawing.Color.Red;
                    MessageBox.Show(string.Format("处理过程中发生错误:\n{0}", ex.Message), "错误",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }));
            }
        }

        /// <summary>
        /// 删除 Application 节点（支持多个名称，用逗号或分号分隔）
        /// </summary>
        private int RemoveApplicationNode(XmlDocument doc, string nodeNames)
        {
            if (string.IsNullOrEmpty(nodeNames))
                return 0;

            // 分隔符：仅支持分号、英文逗号、中文逗号（名称本身可包含空格）
            char[] separators = new char[] { ';', ',', '，' };
            string[] nameArray = nodeNames.Split(separators, StringSplitOptions.RemoveEmptyEntries);
            if (nameArray.Length == 0)
                return 0;

            int removedCount = 0;

            foreach (string name in nameArray)
            {
                string trimmed = name.Trim();
                if (string.IsNullOrEmpty(trimmed))
                    continue;

                string xpath = string.Format("//Application[@Name='{0}']", trimmed);
                XmlNodeList nodes = doc.SelectNodes(xpath);
                if (nodes == null || nodes.Count == 0)
                {
                    xpath = string.Format("//Application[@Name=\"{0}\"]", trimmed);
                    nodes = doc.SelectNodes(xpath);
                }

                if (nodes != null && nodes.Count > 0)
                {
                    var toRemove = new System.Collections.Generic.List<XmlNode>();
                    foreach (XmlNode node in nodes)
                        toRemove.Add(node);
                    foreach (XmlNode node in toRemove)
                    {
                        if (node.ParentNode != null)
                        {
                            node.ParentNode.RemoveChild(node);
                            removedCount++;
                        }
                    }
                }
            }

            return removedCount;
        }

        /// <summary>
        /// 删除 ObjectEvent 节点（位于 ObjectEvents 下，支持多个名称，用逗号或分号分隔）
        /// </summary>
        private int RemoveObjectEventNode(XmlDocument doc, string nodeNames)
        {
            if (string.IsNullOrEmpty(nodeNames))
                return 0;

            // 分隔符：仅支持分号、英文逗号、中文逗号（名称本身可包含空格）
            char[] separators = new char[] { ';', ',', '，' };
            string[] nameArray = nodeNames.Split(separators, StringSplitOptions.RemoveEmptyEntries);
            if (nameArray.Length == 0)
                return 0;

            int removedCount = 0;

            foreach (string name in nameArray)
            {
                string trimmed = name.Trim();
                if (string.IsNullOrEmpty(trimmed))
                    continue;

                string xpath = string.Format("//ObjectEvents/ObjectEvent[@Name='{0}']", trimmed);
                XmlNodeList nodes = doc.SelectNodes(xpath);
                if (nodes == null || nodes.Count == 0)
                {
                    xpath = string.Format("//ObjectEvents/ObjectEvent[@Name=\"{0}\"]", trimmed);
                    nodes = doc.SelectNodes(xpath);
                }

                if (nodes != null && nodes.Count > 0)
                {
                    var toRemove = new System.Collections.Generic.List<XmlNode>();
                    foreach (XmlNode node in nodes)
                        toRemove.Add(node);
                    foreach (XmlNode node in toRemove)
                    {
                        if (node.ParentNode != null)
                        {
                            node.ParentNode.RemoveChild(node);
                            removedCount++;
                        }
                    }
                }
            }

            return removedCount;
        }

        private void backgroundWorker_ProgressChanged(object sender, System.ComponentModel.ProgressChangedEventArgs e)
        {
            // 进度更新在 DoWork 中处理
        }

        private void backgroundWorker_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            btnProcess.Enabled = true;
        }

        private void label2_Click(object sender, EventArgs e)
        {
        }
    }
}