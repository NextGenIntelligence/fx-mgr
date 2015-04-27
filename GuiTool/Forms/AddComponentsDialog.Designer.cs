namespace Prosoft.FXMGR.GuiTool
{
  partial class ManageComponentsDialog
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
            this.addFolderButton = new DevExpress.XtraEditors.SimpleButton();
            this.removeFolderButton = new DevExpress.XtraEditors.SimpleButton();
            this.okButton = new DevExpress.XtraEditors.SimpleButton();
            this.foldersList = new DevExpress.XtraEditors.ListBoxControl();
            ((System.ComponentModel.ISupportInitialize)(this.foldersList)).BeginInit();
            this.SuspendLayout();
            // 
            // addFolderButton
            // 
            this.addFolderButton.Location = new System.Drawing.Point(228, 299);
            this.addFolderButton.Name = "addFolderButton";
            this.addFolderButton.Size = new System.Drawing.Size(86, 23);
            this.addFolderButton.TabIndex = 1;
            this.addFolderButton.Text = "Add";
            this.addFolderButton.Click += new System.EventHandler(this.addFolderButton_Click);
            // 
            // removeFolderButton
            // 
            this.removeFolderButton.Location = new System.Drawing.Point(320, 299);
            this.removeFolderButton.Name = "removeFolderButton";
            this.removeFolderButton.Size = new System.Drawing.Size(86, 23);
            this.removeFolderButton.TabIndex = 2;
            this.removeFolderButton.Text = "Remove";
            this.removeFolderButton.Click += new System.EventHandler(this.removeFolderButton_Click);
            // 
            // okButton
            // 
            this.okButton.Location = new System.Drawing.Point(429, 299);
            this.okButton.Name = "okButton";
            this.okButton.Size = new System.Drawing.Size(75, 23);
            this.okButton.TabIndex = 3;
            this.okButton.Text = "OK";
            this.okButton.Click += new System.EventHandler(this.okButton_Click);
            // 
            // foldersList
            // 
            this.foldersList.Location = new System.Drawing.Point(12, 12);
            this.foldersList.Name = "foldersList";
            this.foldersList.Size = new System.Drawing.Size(492, 281);
            this.foldersList.TabIndex = 4;
            // 
            // ManageComponentsDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(516, 334);
            this.Controls.Add(this.foldersList);
            this.Controls.Add(this.okButton);
            this.Controls.Add(this.removeFolderButton);
            this.Controls.Add(this.addFolderButton);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "ManageComponentsDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Manage component folders";
            this.Shown += new System.EventHandler(this.AddComponentsDialog_Shown);
            ((System.ComponentModel.ISupportInitialize)(this.foldersList)).EndInit();
            this.ResumeLayout(false);

    }

    #endregion

    private DevExpress.XtraEditors.SimpleButton addFolderButton;
    private DevExpress.XtraEditors.SimpleButton removeFolderButton;
    private DevExpress.XtraEditors.SimpleButton okButton;
    private DevExpress.XtraEditors.ListBoxControl foldersList;

  }
}