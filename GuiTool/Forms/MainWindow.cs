/*
 * Copyright (c) 2010-2015, Eremex Ltd.
 * All rights reserved.
 *
 * Redistribution and use in source and binary forms, with or without 
 * modification, are permitted provided that the following conditions 
 * are met:
 * 1. Redistributions of source code must retain the above copyright 
 *    notice, this list of conditions and the following disclaimer.  
 * 2. Redistributions in binary form must reproduce the above copyright 
 *    notice, this list of conditions and the following disclaimer in the 
 *    documentation and/or other materials provided with the distribution.
 * 3. Neither the name of the company nor the names of any contributors 
 *    may be used to endorse or promote products derived from this software 
 *    without specific prior written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE EREMEX LTD. AND CONTRIBUTORS "AS IS" AND 
 * ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE 
 * IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE 
 * ARE DISCLAIMED. IN NO EVENT SHALL THE EREMEX LTD. OR CONTRIBUTORS BE LIABLE 
 * FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL 
 * DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS 
 * OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) 
 * HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT 
 * LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY 
 * OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF 
 * SUCH DAMAGE.
 */

using DevExpress.XtraTreeList;
using DevExpress.XtraTreeList.Nodes;
using DevExpress.XtraVerticalGrid.Rows;
using Prosoft.FXMGR.ConfigFramework;
using Prosoft.FXMGR.ConfigModules;
using Prosoft.FXMGR.GuiTool.Forms;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Windows.Forms;

namespace Prosoft.FXMGR.GuiTool
{
    public partial class MainWindow : Form
    {
        private readonly StateController controller;

        public MainWindow(StateController controller)
        {
            InitializeComponent();

            disabledRadioCheckEdit.Enabled = false;
            disabledCheckEdit.Enabled = false;

            this.controller = controller;

            controller.ConfigurationChanged += controller_ConfigurationChanged;
            controller.ProjectStateChanged += controller_ProjectStateChanged;
        }

        private void newProjectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var newProjectDialog = new NewProjectDialog(controller))
            {
                newProjectDialog.ShowDialog();
            }
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var openFileDialog = new OpenFileDialog())
            {
                openFileDialog.FileOk += openFileDialog_FileOk;
                openFileDialog.ShowDialog();
            }
        }

        private void openFileDialog_FileOk(object sender, System.ComponentModel.CancelEventArgs e)
        {
            var openFileDialog = (OpenFileDialog)sender;

            using (Stream f = File.OpenRead(openFileDialog.FileName))
            {
                if (controller.Load(f))
                {
                    if (MessageBox.Show("Do you want to analyze files?", "Settings", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    {
                        using (var analyzeDialog = new AnalyzeProgressDialog(controller, f))
                        {
                            analyzeDialog.ShowDialog();
                        }
                    }
                }
            }
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ProjectInfo projectInfo = controller.projectInfo;
            string projectFilePath = Path.Combine(projectInfo.projectPath, projectInfo.projectName + ".fxp");

            using (Stream f = File.OpenWrite(projectFilePath))
            {
                controller.Save(f);
            }
        }

        private void analyzeSourcesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var analyzeDialog = new AnalyzeProgressDialog(controller))
            {
                analyzeDialog.ShowDialog();
            }
        }

        private void manageComponentFoldersToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var manageFoldersDialog = new ManageComponentsDialog(controller))
            {
                manageFoldersDialog.ShowDialog();
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void controller_ProjectStateChanged(object sender, ProjectStateChangedEventArgs e)
        {
            BeginInvoke((Action)(() =>
            {
                UpdateUiState(e.state);

                if (e.state == ControllerState.Ready)
                {
                    ShowNodes(e.model);
                    ShowOptions(e.options);
                }
            }));
        }

        private void controller_ConfigurationChanged(object sender, ConfigurationChangedEventArgs e)
        {
            UpdateComponentCheckBoxes(e.affectedNodes);
            ShowOptions(e.options);
        }

        private void checkEdit_EditValueChanging(object sender, DevExpress.XtraEditors.Controls.ChangingEventArgs e)
        {
            IModelItem node = (IModelItem)componentTreeView.FocusedNode.Tag;
            bool oldValue = (bool)e.OldValue;
            bool newValue = (bool)e.NewValue;

            if (oldValue == true && newValue == false)
            {
                if (!controller.IsUncheckAllowed(node))
                {
                    e.Cancel = true;
                }
            }
        }

        private void radioCheckEdit_EditValueChanging(object sender, DevExpress.XtraEditors.Controls.ChangingEventArgs e)
        {
            IModelItem node = (IModelItem)componentTreeView.FocusedNode.Tag;
            bool oldValue = (bool)e.OldValue;
            bool newValue = (bool)e.NewValue;

            e.Cancel = oldValue;
        }

        private void componentCheckEdit_CheckedChanged(object sender, EventArgs e)
        {
            bool nodeChecked = (bool)componentTreeView.FocusedNode.GetValue(2);
            IModelItem node = (IModelItem)componentTreeView.FocusedNode.Tag;

            componentTreeView.Enabled = false;
            componentTreeView.BeginUpdate();
            if (nodeChecked == false)
            {
                controller.HandleInclude(node);
            }
            else
            {
                controller.HandleExclude(node);
            }
            componentTreeView.EndUpdate();
            componentTreeView.Enabled = true;
        }

        private void componentRadioCheckEdit_CheckedChanged(object sender, EventArgs e)
        {
            bool nodeChecked = (bool)componentTreeView.FocusedNode.GetValue(2);

            componentTreeView.Enabled = false;
            if (nodeChecked == false)
            {
                IModelItem node = (IModelItem)componentTreeView.FocusedNode.Tag;
                IModelItem parentNode = (IModelItem)componentTreeView.FocusedNode.ParentNode.Tag;
                controller.ChangeImplementation(parentNode, node);
            }
            componentTreeView.Enabled = true;
        }

        private void componentTreeView_CustomNodeCellEdit(object sender, GetCustomNodeCellEditEventArgs e)
        {
            if ((e.Column.AbsoluteIndex == 2) && (e.Node.Tag is IModule))
            {
                IModule selectableNode = (IModule)e.Node.Tag;

                if (selectableNode is IModelItemContainer)
                {
                    e.RepositoryItem = selectableNode.enabled ? enabledCheckEdit : disabledCheckEdit;
                }
                else
                {
                    e.RepositoryItem = selectableNode.enabled ? enabledRadioCheckEdit : disabledRadioCheckEdit;
                }
            }
        }

        private void optionsView_ShownEditor(object sender, EventArgs e)
        {
            BaseOption option = (BaseOption)optionsView.FocusedRow.Tag;

            if (option.type == OptionType.Enumeration)
            {
                EnumOption enumOption = (EnumOption)option;

                DevExpress.XtraEditors.ComboBoxEdit optionComboBox = (DevExpress.XtraEditors.ComboBoxEdit)optionsView.ActiveEditor;
                optionComboBox.Properties.Items.Clear();

                optionComboBox.Properties.Items.AddRange(enumOption.friendlyName);
                optionComboBox.SelectedIndex = enumOption.index;
            }
        }

        private void optionsView_FocusedRowChanged(object sender, DevExpress.XtraVerticalGrid.Events.FocusedRowChangedEventArgs e)
        {
            if (e.Row != null)
            {
                BaseOption associatedOption = (BaseOption)e.Row.Tag;
                optionDescription.Text = associatedOption.description;
            }
            else
            {
                optionDescription.Text = "";
            }
        }

        private void optionsComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            DevExpress.XtraEditors.ComboBoxEdit optionComboBox = (DevExpress.XtraEditors.ComboBoxEdit)sender;
            EnumOption option = (EnumOption)optionsView.FocusedRow.Tag;

            option.index = optionComboBox.SelectedIndex;
        }

        private void UpdateUiState(ControllerState newState)
        {
            if (newState == ControllerState.ProjectInfoSet)
            {
                analyzeSourcesToolStripMenuItem.Enabled = true;
                manageComponentFoldersToolStripMenuItem.Enabled = true;
                sysgenToolStripMenuItem1.Enabled = false;
                saveToolStripMenuItem.Enabled = true;
                xtraTabControl1.Enabled = false;
            }
            else if (newState == ControllerState.Ready)
            {
                analyzeSourcesToolStripMenuItem.Enabled = true;
                manageComponentFoldersToolStripMenuItem.Enabled = true;
                sysgenToolStripMenuItem1.Enabled = true;
                saveToolStripMenuItem.Enabled = true;
                xtraTabControl1.Enabled = true;
            }
            else if (newState == ControllerState.Empty)
            {
                analyzeSourcesToolStripMenuItem.Enabled = false;
                manageComponentFoldersToolStripMenuItem.Enabled = false;
                sysgenToolStripMenuItem1.Enabled = false;
                saveToolStripMenuItem.Enabled = false;
                xtraTabControl1.Enabled = false;
            }
        }

        private void ShowNodes(IEnumerable<IModelItem> nodes)
        {
            componentTreeView.ClearNodes();

            componentTreeView.BeginUnboundLoad();

            foreach (IModelItem node in nodes)
            {
                ShowNode(null, node);
            }

            componentTreeView.EndUnboundLoad();
        }

        private object SetOption(EditorRow editorRow, IntegerOption option)
        {
            option.Tag = editorRow;
            editorRow.Tag = option;

            editorRow.Properties.RowEdit = optionsTextEdit;

            return (object)String.Format("0x{0:x8}", option.value.ToString("x"));
        }

        private object SetOption(EditorRow editorRow, EnumOption option)
        {
            option.Tag = editorRow;
            editorRow.Tag = option;

            editorRow.Properties.RowEdit = optionsComboBox;

            return (object)option.friendlyName[option.index];
        }

        private void ShowOptions(IEnumerable<BaseOption> options)
        {
            optionsView.BeginUpdate();

            optionsView.Rows.Clear();

            if (controller.IsOptionsPresent())
            {
                optionsView.Enabled = false;
            }

            if (!controller.IsOptionsPresent() && options != null)
            {
                foreach (BaseOption baseOption in options)
                {
                    object rowValue = null;
                    var editorRow = new EditorRow();

                    editorRow.Name = baseOption.name;
                    editorRow.Properties.Caption = baseOption.name;
                    editorRow.Properties.FieldName = baseOption.name;

                    switch (baseOption.type)
                    {
                        case OptionType.Integer:
                            {
                                IntegerOption integerOption = (IntegerOption)baseOption;
                                rowValue = SetOption(editorRow, integerOption);
                            }
                            break;
                        case OptionType.Enumeration:
                            {
                                EnumOption enumOption = (EnumOption)baseOption;
                                rowValue = SetOption(editorRow, enumOption);
                            }
                            break;
                    }

                    optionsView.Rows.Add(editorRow);
                    optionsView.SetCellValue(editorRow, 1, rowValue);
                }
            }

            optionsView.EndUpdate();
        }

        private object[] GetNodeDataRecord(IModelItem node)
        {
            object[] dataRecord = null;

            if (node is IModule)
            {
                IModule selectable = (IModule)node;
                dataRecord = new object[] { selectable.name, selectable.description, selectable.selected };
            }
            else
            {
                dataRecord = new object[] { node.name };
            }

            return dataRecord;
        }

        private void ShowNode(TreeListNode viewParentNode, IModelItem node)
        {
            object[] dataRecord = GetNodeDataRecord(node);

            TreeListNode rootNode = componentTreeView.AppendNode(dataRecord, viewParentNode);

            rootNode.Tag = node;
            node.Tag = rootNode;

            if (node is IModelItemContainer)
            {
                IModelItemContainer expandable = (IModelItemContainer)node;

                foreach (IModelItem childNode in expandable.children)
                {
                    ShowNode(rootNode, childNode);
                }
            }
        }

        private void UpdateComponentCheckBoxes(IEnumerable<IModelItem> nodes)
        {
            foreach (IModelItem node in nodes)
            {
                IModule selectable = (IModule)node;
                TreeListNode viewNode = (TreeListNode)node.Tag;

                viewNode.SetValue(2, selectable.selected);
            }
        }

        private void optionsTextEdit_Validating(object sender, System.ComponentModel.CancelEventArgs e)
        {
            DevExpress.XtraEditors.TextEdit optionTextEdit = (DevExpress.XtraEditors.TextEdit)sender;
            IntegerOption option = (IntegerOption)optionsView.FocusedRow.Tag;
            int value;
            string valueString = (string)optionTextEdit.EditValue;

            if (valueString.Length > 2 && int.TryParse(valueString.Substring(2), System.Globalization.NumberStyles.HexNumber, CultureInfo.InvariantCulture, out value))
            {
                option.value = (long)value;
            }
            else
            {
                e.Cancel = true;
            }
        }

        private void sysgenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string path = controller.GetProjectPath();
            string name = controller.GetProjectName();
            List<string> includedModules = new List<string>(controller.GetIncluded());

            OutputSettings settings = new OutputSettings()
            {
                flatListGeneration = false,
                flatListPath = Path.Combine(path, name + ".txt"),
                initGeneration = false,
                initPath = Path.Combine(path, name + "_init.c"),
                optionsGeneration = false,
                optionsPath = Path.Combine(path, name + "_options.h"),
                gvGeneration = false,
                gvContent = includedModules.ToArray(),
                gvPath = Path.Combine(path, name + ".dot"),
                mappingGeneration = false,
                mappingPath = Path.Combine(path, name + ".map"),
                mapHeaderGeneration = false,
                mapHeaderPrefix = null,
                mapHeaderPath = Path.Combine(path, name + ".h")
            };

            using (var output = new OutputFiles(settings, !controller.IsOptionsPresent()))
            {
                output.ShowDialog();
            }

            controller.GenerateOutput(settings);
        }
    }
}
