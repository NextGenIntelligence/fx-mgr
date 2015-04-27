namespace Prosoft.FXMGR.GuiTool
{
  partial class AnalyzeProgressDialog
  {
    /// <summary>
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
      if (disposing && (components != null))
      {
        components.Dispose();
      }
      base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
            this.analyzeProgressBar = new DevExpress.XtraEditors.ProgressBarControl();
            this.fileName = new DevExpress.XtraEditors.LabelControl();
            this.cancelButton = new DevExpress.XtraEditors.SimpleButton();
            ((System.ComponentModel.ISupportInitialize)(this.analyzeProgressBar.Properties)).BeginInit();
            this.SuspendLayout();
            // 
            // analyzeProgressBar
            // 
            this.analyzeProgressBar.Location = new System.Drawing.Point(12, 23);
            this.analyzeProgressBar.Name = "analyzeProgressBar";
            this.analyzeProgressBar.Size = new System.Drawing.Size(576, 24);
            this.analyzeProgressBar.TabIndex = 3;
            this.analyzeProgressBar.UseWaitCursor = true;
            // 
            // fileName
            // 
            this.fileName.Location = new System.Drawing.Point(12, 4);
            this.fileName.Name = "fileName";
            this.fileName.Size = new System.Drawing.Size(0, 13);
            this.fileName.TabIndex = 4;
            this.fileName.UseWaitCursor = true;
            // 
            // cancelButton
            // 
            this.cancelButton.Location = new System.Drawing.Point(513, 53);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(75, 23);
            this.cancelButton.TabIndex = 5;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.Click += new System.EventHandler(this.cancelAnalyze_Click);
            // 
            // AnalyzeProgressDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(600, 85);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.fileName);
            this.Controls.Add(this.analyzeProgressBar);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "AnalyzeProgressDialog";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "AnalyzeProgress";
            this.UseWaitCursor = true;
            this.Shown += new System.EventHandler(this.AnalyzeProgressDialog_Shown);
            ((System.ComponentModel.ISupportInitialize)(this.analyzeProgressBar.Properties)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

    }

    #endregion

    private DevExpress.XtraEditors.ProgressBarControl analyzeProgressBar;
    private DevExpress.XtraEditors.LabelControl fileName;
    private DevExpress.XtraEditors.SimpleButton cancelButton;
  }
}