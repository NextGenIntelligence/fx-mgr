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

using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Controls;
using DevExpress.XtraEditors.Repository;
using DevExpress.XtraVerticalGrid.Rows;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace Prosoft.FXMGR.GuiTool.Forms
{
    public partial class OutputFiles : Form
    {
        private readonly OutputSettings settings;
        private readonly bool optionsAvailable;

        public OutputFiles(OutputSettings settings, bool optionsAvailable)
        {
            InitializeComponent();
            this.settings = settings;
            this.optionsAvailable = optionsAvailable;
        }

        private void simpleButton1_Click(object sender, EventArgs e)
        {
            GetSettings();
            Close();
        }

        private void OutputFiles_Shown(object sender, EventArgs e)
        {
            OutputSettings defaultSettings = settings;

            vGridControl1.BeginUpdate();
            vGridControl1.Rows[0].ChildRows[0].Properties.Value = defaultSettings.flatListGeneration;
            vGridControl1.Rows[0].ChildRows[1].Properties.Value = defaultSettings.flatListPath;

            vGridControl1.Rows[1].ChildRows[0].Properties.Value = defaultSettings.mapHeaderGeneration;
            vGridControl1.Rows[1].ChildRows[1].Properties.Value = defaultSettings.mapHeaderPath;
            vGridControl1.Rows[1].ChildRows[2].Properties.Value = defaultSettings.mapHeaderPrefix;

            vGridControl1.Rows[2].ChildRows[0].Properties.Value = defaultSettings.gvGeneration;
            vGridControl1.Rows[2].ChildRows[1].Properties.Value = defaultSettings.gvPath;

            RepositoryItemCheckedComboBoxEdit modules = (RepositoryItemCheckedComboBoxEdit)vGridControl1.Rows[2].ChildRows[2].Properties.RowEdit;
            modules.Items.BeginUpdate();
            modules.Items.AddRange(defaultSettings.gvContent);
            modules.Items.EndUpdate();

            vGridControl1.Rows[3].ChildRows[0].Properties.Value = defaultSettings.initGeneration;
            vGridControl1.Rows[3].ChildRows[1].Properties.Value = defaultSettings.initPath;

            vGridControl1.Rows[4].ChildRows[0].Properties.Value = defaultSettings.optionsGeneration;
            vGridControl1.Rows[4].ChildRows[1].Properties.Value = defaultSettings.optionsPath;

            vGridControl1.Rows[5].ChildRows[0].Properties.Value = defaultSettings.mappingGeneration;
            vGridControl1.Rows[5].ChildRows[1].Properties.Value = defaultSettings.mappingPath;
            vGridControl1.EndUpdate();

            if (!this.optionsAvailable)
            {
                this.editorRow14.Enabled = false;
            }

            foreach (BaseRow row in vGridControl1.Rows)
            {
                for (int i = 1; i < row.ChildRows.Count; ++i)
                {
                    EditorRow editorRow = (EditorRow)row.ChildRows[i];
                    editorRow.Enabled = false;
                }
            }
        }

        private void GetSettings()
        {
            settings.flatListGeneration = (bool)vGridControl1.Rows[0].ChildRows[0].Properties.Value;

            string flatListPath = (string)vGridControl1.Rows[0].ChildRows[1].Properties.Value;

            if (Directory.Exists(Path.GetDirectoryName(flatListPath)))
            {
                settings.flatListPath = flatListPath;
            }

            settings.mapHeaderGeneration = (bool)vGridControl1.Rows[1].ChildRows[0].Properties.Value;

            string mapHeaderPath = (string)vGridControl1.Rows[1].ChildRows[1].Properties.Value;

            if (Directory.Exists(Path.GetDirectoryName(mapHeaderPath)))
            {
                settings.mapHeaderPath = mapHeaderPath;
            }

            settings.gvGeneration = (bool)vGridControl1.Rows[2].ChildRows[0].Properties.Value;

            string gvPath = (string)vGridControl1.Rows[2].ChildRows[1].Properties.Value;

            if (Directory.Exists(Path.GetDirectoryName(gvPath)))
            {
                settings.gvPath = gvPath;
            }

            RepositoryItemCheckedComboBoxEdit modules = (RepositoryItemCheckedComboBoxEdit)vGridControl1.Rows[2].ChildRows[2].Properties.RowEdit;
            List<string> temp = new List<string>();

            foreach (CheckedListBoxItem moduleName in modules.Items)
            {
                if (moduleName.CheckState == CheckState.Checked)
                {
                    temp.Add((string)moduleName.Value);
                }
            }
            settings.gvContent = temp.ToArray();

            settings.initGeneration = (bool)vGridControl1.Rows[3].ChildRows[0].Properties.Value;

            string initPath = (string)vGridControl1.Rows[3].ChildRows[1].Properties.Value;

            if (Directory.Exists(Path.GetDirectoryName(initPath)))
            {
                settings.initPath = initPath;
            }

            settings.optionsGeneration = (bool)vGridControl1.Rows[4].ChildRows[0].Properties.Value;

            string optionsPath = (string)vGridControl1.Rows[4].ChildRows[1].Properties.Value;

            if (Directory.Exists(Path.GetDirectoryName(optionsPath)))
            {
                settings.optionsPath = optionsPath;
            }
            
            settings.mappingGeneration = (bool)vGridControl1.Rows[5].ChildRows[0].Properties.Value;

            string mappingPath = (string)vGridControl1.Rows[5].ChildRows[1].Properties.Value;

            if (Directory.Exists(Path.GetDirectoryName(mappingPath)))
            {
                settings.mappingPath = mappingPath;
            }
        }

        private void repositoryItemCheckEdit1_CheckedChanged(object sender, EventArgs e)
        {
            CheckEdit editor = (CheckEdit)sender;
            BaseRow parentRow = vGridControl1.FocusedRow.ParentRow;

            vGridControl1.PostEditor();

            for (int i = 1; i < parentRow.ChildRows.Count; ++i)
            {
                EditorRow editorRow = (EditorRow)parentRow.ChildRows[i];
                editorRow.Enabled = editor.Checked;
            }
        }
    }
}
