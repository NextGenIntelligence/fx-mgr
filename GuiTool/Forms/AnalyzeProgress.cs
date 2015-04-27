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

using Prosoft.FXMGR.ConfigFramework;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Prosoft.FXMGR.GuiTool
{
    public partial class AnalyzeProgressDialog : Form
    {
        private readonly StateController controller;
        private readonly Stream f;
        private bool cancel = false;
        private bool done = false;

        public AnalyzeProgressDialog(StateController controller, Stream load = null)
        {
            InitializeComponent();
            this.controller = controller;
            this.f = load;

            controller.BeforeAnalyzing += controller_BeforeSourcesAnalyzing;
            controller.ItemAnalyzed += controller_ItemAnalyzed;
            controller.AfterAnalyzing += controller_AllAnalyzed;

            this.Disposed += AnalyzeProgressDialog_Disposed;
        }

        void AnalyzeProgressDialog_Disposed(object sender, EventArgs e)
        {
            controller.BeforeAnalyzing -= controller_BeforeSourcesAnalyzing;
            controller.ItemAnalyzed -= controller_ItemAnalyzed;
            controller.AfterAnalyzing -= controller_AllAnalyzed;
        }

        private void AnalyzeProgressDialog_Shown(object sender, EventArgs e)
        {
            this.UseWaitCursor = true;
            var t = Task.Factory.StartNew((Action)(() => { controller.AnalyzeSources(f); }));
        }

        private void cancelAnalyze_Click(object sender, EventArgs e)
        {
            if (!done)
            {
                cancelButton.Enabled = false;
                cancel = true;
            }
            else
            {
                this.DialogResult = DialogResult.OK;
            }
        }

        void controller_BeforeSourcesAnalyzing(object sender, BeforeAnalyzingEventArgs e)
        {
            BeginInvoke((Action)(() =>
            {
                analyzeProgressBar.Properties.Minimum = 0;
                analyzeProgressBar.Properties.Maximum = e.count;
                analyzeProgressBar.Properties.PercentView = true;
                analyzeProgressBar.Properties.Step = 1;
            }));
        }

        void controller_ItemAnalyzed(object sender, ItemAnalyzedEventArgs e)
        {
            BeginInvoke((Action)(() =>
            {
                fileName.Text = e.filePath;
                analyzeProgressBar.PerformStep();
                analyzeProgressBar.Update();
            }));

            if (cancel)
            {
                e.abort = true;
            }
        }

        void controller_AllAnalyzed(object sender, AfterAnalyzingEventArgs e)
        {
            done = true;

            BeginInvoke((Action)(() => { this.UseWaitCursor = false; this.cancelButton.Text = "OK"; }));
        }
    }
}
