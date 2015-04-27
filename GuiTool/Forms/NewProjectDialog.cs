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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace Prosoft.FXMGR.GuiTool
{
    public partial class NewProjectDialog : Form
    {
        private readonly StateController controller;

        public NewProjectDialog(StateController controller)
        {
            InitializeComponent();

            this.controller = controller;
        }

        private void projectTypeRadio_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (projectTypeRadio.SelectedIndex == 1)
            {
                templateFile.Enabled = true;
                templateFolderBrowse.Enabled = true;
            }
            else
            {
                templateFile.Enabled = false;
                templateFolderBrowse.Enabled = false;
            }
        }

        private void templateFileBrowse_Click(object sender, EventArgs e)
        {
            using (var fileBrowserDialog = new OpenFileDialog())
            {
                fileBrowserDialog.Filter = "Mapping files (.map)|*.map";
                if (fileBrowserDialog.ShowDialog() == DialogResult.OK)
                {
                    templateFile.Text = fileBrowserDialog.FileName;
                }
            }
        }

        private void projectType_PageValidating(object sender, DevExpress.XtraWizard.WizardPageValidatingEventArgs e)
        {
            bool templateProject = projectTypeRadio.SelectedIndex == 1;

            e.Valid = false;

            if (templateProject)
            {
                if (!File.Exists(templateFile.Text))
                {
                    MessageBox.Show("Please, specify template folder!");
                    return;
                }
            }

            var projectNameRegex = new Regex("^[^\\d\\W]\\w*");

            if (!projectNameRegex.Match(projectName.Text).Success)
            {
                MessageBox.Show("Project name may contains only digits, letters and _ symbol!");
                return;
            }

            e.Valid = true;
        }

        private void projectLocationBrowse_Click(object sender, EventArgs e)
        {
            using (var folderBrowserDialog = new FolderBrowserDialog())
            {
                if (folderBrowserDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    projectLocation.Text = folderBrowserDialog.SelectedPath;
                }
            }
        }

        private void projectLocation_PageValidating(object sender, DevExpress.XtraWizard.WizardPageValidatingEventArgs e)
        {
            e.Valid = false;

            if (!Directory.Exists(projectLocation.Text))
            {
                MessageBox.Show("Please, specify project folder!");
                return;
            }

            e.Valid = true;
        }

        private void addComponentFolder_Click(object sender, EventArgs e)
        {
            using (var folderBrowserDialog = new FolderBrowserDialog())
            {
                if (folderBrowserDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    componentFolderList.Items.Add(folderBrowserDialog.SelectedPath);
                    removeComponentFolder.Enabled = true;
                }
            }
        }

        private void removeComponentFolder_Click(object sender, EventArgs e)
        {
            componentFolderList.Items.Remove(componentFolderList.Items[componentFolderList.SelectedIndex]);

            if (componentFolderList.Items.Count == 0)
            {
                removeComponentFolder.Enabled = false;
            }
        }

        private void foldersWizardPage_PageValidating(object sender, DevExpress.XtraWizard.WizardPageValidatingEventArgs e)
        {
            e.Valid = false;

            foreach (string folder in componentFolderList.Items)
            {
                if (!Directory.Exists(folder))
                {
                    MessageBox.Show("All folders specified as component sources should be valid!");
                    return;
                }
            }

            if (componentFolderList.Items.Count > 0)
            {
                e.Valid = true;
            }
        }

        private void wizardControl1_FinishClick(object sender, CancelEventArgs e)
        {
            ProjectInfo projectInfo = new ProjectInfo();

            projectInfo.templateProject = projectTypeRadio.SelectedIndex == 1;
            projectInfo.templateFilePath = templateFile.Text;
            projectInfo.projectName = projectName.Text;
            projectInfo.projectPath = projectLocation.Text;
            projectInfo.sourceFolders = new List<string>();

            foreach (string folder in componentFolderList.Items)
            {
                projectInfo.sourceFolders.Add(folder);
            }

            controller.projectInfo = projectInfo;

            if (analyzeAfterCompletion.Checked)
            {
                using (var analyzeSourcesDialog = new AnalyzeProgressDialog(controller))
                {
                    analyzeSourcesDialog.ShowDialog();
                }
            }
        }
    }
}
