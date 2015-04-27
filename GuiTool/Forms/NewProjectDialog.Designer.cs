namespace Prosoft.FXMGR.GuiTool
{
    partial class NewProjectDialog
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
            this.NewProjectWizard = new DevExpress.XtraWizard.WizardControl();
            this.welcomeWizardPage = new DevExpress.XtraWizard.WelcomeWizardPage();
            this.projectNameWizardPage = new DevExpress.XtraWizard.WizardPage();
            this.templateFolderBrowse = new DevExpress.XtraEditors.SimpleButton();
            this.templateFile = new DevExpress.XtraEditors.TextEdit();
            this.templateFolderLabel = new DevExpress.XtraEditors.LabelControl();
            this.projectNameLabel = new DevExpress.XtraEditors.LabelControl();
            this.projectTypeLabel = new DevExpress.XtraEditors.LabelControl();
            this.projectTypeRadio = new DevExpress.XtraEditors.RadioGroup();
            this.projectName = new DevExpress.XtraEditors.TextEdit();
            this.completionWizardPage = new DevExpress.XtraWizard.CompletionWizardPage();
            this.analyzeAfterCompletion = new DevExpress.XtraEditors.CheckEdit();
            this.foldersWizardPage = new DevExpress.XtraWizard.WizardPage();
            this.removeComponentFolder = new DevExpress.XtraEditors.SimpleButton();
            this.addComponentFolder = new DevExpress.XtraEditors.SimpleButton();
            this.componentFolderList = new DevExpress.XtraEditors.ListBoxControl();
            this.projectLocationWizardPage = new DevExpress.XtraWizard.WizardPage();
            this.projectLocationBrowse = new DevExpress.XtraEditors.SimpleButton();
            this.projectLocation = new DevExpress.XtraEditors.TextEdit();
            ((System.ComponentModel.ISupportInitialize)(this.NewProjectWizard)).BeginInit();
            this.NewProjectWizard.SuspendLayout();
            this.projectNameWizardPage.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.templateFile.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.projectTypeRadio.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.projectName.Properties)).BeginInit();
            this.completionWizardPage.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.analyzeAfterCompletion.Properties)).BeginInit();
            this.foldersWizardPage.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.componentFolderList)).BeginInit();
            this.projectLocationWizardPage.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.projectLocation.Properties)).BeginInit();
            this.SuspendLayout();
            // 
            // NewProjectWizard
            // 
            this.NewProjectWizard.Controls.Add(this.welcomeWizardPage);
            this.NewProjectWizard.Controls.Add(this.projectNameWizardPage);
            this.NewProjectWizard.Controls.Add(this.completionWizardPage);
            this.NewProjectWizard.Controls.Add(this.foldersWizardPage);
            this.NewProjectWizard.Controls.Add(this.projectLocationWizardPage);
            this.NewProjectWizard.Dock = System.Windows.Forms.DockStyle.Fill;
            this.NewProjectWizard.Location = new System.Drawing.Point(0, 0);
            this.NewProjectWizard.Name = "NewProjectWizard";
            this.NewProjectWizard.Pages.AddRange(new DevExpress.XtraWizard.BaseWizardPage[] {
            this.welcomeWizardPage,
            this.projectNameWizardPage,
            this.projectLocationWizardPage,
            this.foldersWizardPage,
            this.completionWizardPage});
            this.NewProjectWizard.Size = new System.Drawing.Size(512, 325);
            this.NewProjectWizard.Text = "New project wizard";
            this.NewProjectWizard.WizardStyle = DevExpress.XtraWizard.WizardStyle.WizardAero;
            this.NewProjectWizard.FinishClick += new System.ComponentModel.CancelEventHandler(this.wizardControl1_FinishClick);
            // 
            // welcomeWizardPage
            // 
            this.welcomeWizardPage.Name = "welcomeWizardPage";
            this.welcomeWizardPage.Size = new System.Drawing.Size(452, 163);
            this.welcomeWizardPage.Text = "Welcome";
            // 
            // projectNameWizardPage
            // 
            this.projectNameWizardPage.Controls.Add(this.templateFolderBrowse);
            this.projectNameWizardPage.Controls.Add(this.templateFile);
            this.projectNameWizardPage.Controls.Add(this.templateFolderLabel);
            this.projectNameWizardPage.Controls.Add(this.projectNameLabel);
            this.projectNameWizardPage.Controls.Add(this.projectTypeLabel);
            this.projectNameWizardPage.Controls.Add(this.projectTypeRadio);
            this.projectNameWizardPage.Controls.Add(this.projectName);
            this.projectNameWizardPage.Name = "projectNameWizardPage";
            this.projectNameWizardPage.Size = new System.Drawing.Size(452, 163);
            this.projectNameWizardPage.Text = "Project type and name";
            this.projectNameWizardPage.PageValidating += new DevExpress.XtraWizard.WizardPageValidatingEventHandler(this.projectType_PageValidating);
            // 
            // templateFolderBrowse
            // 
            this.templateFolderBrowse.Enabled = false;
            this.templateFolderBrowse.Location = new System.Drawing.Point(374, 91);
            this.templateFolderBrowse.Name = "templateFolderBrowse";
            this.templateFolderBrowse.Size = new System.Drawing.Size(75, 25);
            this.templateFolderBrowse.TabIndex = 6;
            this.templateFolderBrowse.Text = "Browse...";
            this.templateFolderBrowse.Click += new System.EventHandler(this.templateFileBrowse_Click);
            // 
            // templateFolder
            // 
            this.templateFile.Enabled = false;
            this.templateFile.Location = new System.Drawing.Point(0, 96);
            this.templateFile.Name = "templateFolder";
            this.templateFile.Properties.ReadOnly = true;
            this.templateFile.Size = new System.Drawing.Size(368, 20);
            this.templateFile.TabIndex = 5;
            // 
            // templateFolderLabel
            // 
            this.templateFolderLabel.Location = new System.Drawing.Point(3, 77);
            this.templateFolderLabel.Name = "templateFolderLabel";
            this.templateFolderLabel.Size = new System.Drawing.Size(75, 13);
            this.templateFolderLabel.TabIndex = 4;
            this.templateFolderLabel.Text = "Template folder";
            // 
            // projectNameLabel
            // 
            this.projectNameLabel.Location = new System.Drawing.Point(3, 121);
            this.projectNameLabel.Name = "projectNameLabel";
            this.projectNameLabel.Size = new System.Drawing.Size(63, 13);
            this.projectNameLabel.TabIndex = 3;
            this.projectNameLabel.Text = "Project name";
            // 
            // projectTypeLabel
            // 
            this.projectTypeLabel.Location = new System.Drawing.Point(3, 0);
            this.projectTypeLabel.Name = "projectTypeLabel";
            this.projectTypeLabel.Size = new System.Drawing.Size(59, 13);
            this.projectTypeLabel.TabIndex = 2;
            this.projectTypeLabel.Text = "Project type";
            // 
            // projectTypeRadio
            // 
            this.projectTypeRadio.Location = new System.Drawing.Point(0, 19);
            this.projectTypeRadio.Name = "projectTypeRadio";
            this.projectTypeRadio.Properties.Items.AddRange(new DevExpress.XtraEditors.Controls.RadioGroupItem[] {
            new DevExpress.XtraEditors.Controls.RadioGroupItem(null, "New project from scratch"),
            new DevExpress.XtraEditors.Controls.RadioGroupItem(null, "New project by template")});
            this.projectTypeRadio.Size = new System.Drawing.Size(452, 52);
            this.projectTypeRadio.TabIndex = 1;
            this.projectTypeRadio.SelectedIndexChanged += new System.EventHandler(this.projectTypeRadio_SelectedIndexChanged);
            // 
            // projectName
            // 
            this.projectName.Location = new System.Drawing.Point(0, 140);
            this.projectName.Name = "projectName";
            this.projectName.Size = new System.Drawing.Size(452, 20);
            this.projectName.TabIndex = 0;
            // 
            // completionWizardPage
            // 
            this.completionWizardPage.Controls.Add(this.analyzeAfterCompletion);
            this.completionWizardPage.Name = "completionWizardPage";
            this.completionWizardPage.Size = new System.Drawing.Size(452, 163);
            // 
            // analyzeAfterCompletion
            // 
            this.analyzeAfterCompletion.Location = new System.Drawing.Point(3, 3);
            this.analyzeAfterCompletion.Name = "analyzeAfterCompletion";
            this.analyzeAfterCompletion.Properties.Caption = "Run source analyzis on wizard exit";
            this.analyzeAfterCompletion.Size = new System.Drawing.Size(446, 19);
            this.analyzeAfterCompletion.TabIndex = 0;
            // 
            // foldersWizardPage
            // 
            this.foldersWizardPage.Controls.Add(this.removeComponentFolder);
            this.foldersWizardPage.Controls.Add(this.addComponentFolder);
            this.foldersWizardPage.Controls.Add(this.componentFolderList);
            this.foldersWizardPage.Name = "foldersWizardPage";
            this.foldersWizardPage.Size = new System.Drawing.Size(452, 163);
            this.foldersWizardPage.Text = "Component folders";
            this.foldersWizardPage.PageValidating += new DevExpress.XtraWizard.WizardPageValidatingEventHandler(this.foldersWizardPage_PageValidating);
            // 
            // removeComponentFolder
            // 
            this.removeComponentFolder.Enabled = false;
            this.removeComponentFolder.Location = new System.Drawing.Point(374, 132);
            this.removeComponentFolder.Name = "removeComponentFolder";
            this.removeComponentFolder.Size = new System.Drawing.Size(75, 23);
            this.removeComponentFolder.TabIndex = 2;
            this.removeComponentFolder.Text = "Remove";
            this.removeComponentFolder.Click += new System.EventHandler(this.removeComponentFolder_Click);
            // 
            // addComponentFolder
            // 
            this.addComponentFolder.Location = new System.Drawing.Point(293, 132);
            this.addComponentFolder.Name = "addComponentFolder";
            this.addComponentFolder.Size = new System.Drawing.Size(75, 23);
            this.addComponentFolder.TabIndex = 1;
            this.addComponentFolder.Text = "Add";
            this.addComponentFolder.Click += new System.EventHandler(this.addComponentFolder_Click);
            // 
            // componentFolderList
            // 
            this.componentFolderList.Location = new System.Drawing.Point(3, 3);
            this.componentFolderList.Name = "componentFolderList";
            this.componentFolderList.Size = new System.Drawing.Size(446, 123);
            this.componentFolderList.TabIndex = 0;
            // 
            // projectLocationWizardPage
            // 
            this.projectLocationWizardPage.Controls.Add(this.projectLocationBrowse);
            this.projectLocationWizardPage.Controls.Add(this.projectLocation);
            this.projectLocationWizardPage.Name = "projectLocationWizardPage";
            this.projectLocationWizardPage.Size = new System.Drawing.Size(452, 163);
            this.projectLocationWizardPage.Text = "Project location";
            this.projectLocationWizardPage.PageValidating += new DevExpress.XtraWizard.WizardPageValidatingEventHandler(this.projectLocation_PageValidating);
            // 
            // projectLocationBrowse
            // 
            this.projectLocationBrowse.Location = new System.Drawing.Point(374, 0);
            this.projectLocationBrowse.Name = "projectLocationBrowse";
            this.projectLocationBrowse.Size = new System.Drawing.Size(75, 23);
            this.projectLocationBrowse.TabIndex = 1;
            this.projectLocationBrowse.Text = "Browse...";
            this.projectLocationBrowse.Click += new System.EventHandler(this.projectLocationBrowse_Click);
            // 
            // projectLocation
            // 
            this.projectLocation.Location = new System.Drawing.Point(3, 3);
            this.projectLocation.Name = "projectLocation";
            this.projectLocation.Properties.ReadOnly = true;
            this.projectLocation.Size = new System.Drawing.Size(365, 20);
            this.projectLocation.TabIndex = 0;
            // 
            // NewProjectDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(512, 325);
            this.ControlBox = false;
            this.Controls.Add(this.NewProjectWizard);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "NewProjectDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "New project...";
            ((System.ComponentModel.ISupportInitialize)(this.NewProjectWizard)).EndInit();
            this.NewProjectWizard.ResumeLayout(false);
            this.projectNameWizardPage.ResumeLayout(false);
            this.projectNameWizardPage.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.templateFile.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.projectTypeRadio.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.projectName.Properties)).EndInit();
            this.completionWizardPage.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.analyzeAfterCompletion.Properties)).EndInit();
            this.foldersWizardPage.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.componentFolderList)).EndInit();
            this.projectLocationWizardPage.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.projectLocation.Properties)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private DevExpress.XtraWizard.WizardControl NewProjectWizard;
        private DevExpress.XtraWizard.WelcomeWizardPage welcomeWizardPage;
        private DevExpress.XtraWizard.WizardPage projectNameWizardPage;
        private DevExpress.XtraWizard.CompletionWizardPage completionWizardPage;
        private DevExpress.XtraWizard.WizardPage foldersWizardPage;
        private DevExpress.XtraEditors.LabelControl projectNameLabel;
        private DevExpress.XtraEditors.LabelControl projectTypeLabel;
        private DevExpress.XtraEditors.RadioGroup projectTypeRadio;
        private DevExpress.XtraEditors.TextEdit projectName;
        private DevExpress.XtraWizard.WizardPage projectLocationWizardPage;
        private DevExpress.XtraEditors.ListBoxControl componentFolderList;
        private DevExpress.XtraEditors.TextEdit templateFile;
        private DevExpress.XtraEditors.LabelControl templateFolderLabel;
        private DevExpress.XtraEditors.CheckEdit analyzeAfterCompletion;
        private DevExpress.XtraEditors.TextEdit projectLocation;
        private DevExpress.XtraEditors.SimpleButton templateFolderBrowse;
        private DevExpress.XtraEditors.SimpleButton removeComponentFolder;
        private DevExpress.XtraEditors.SimpleButton addComponentFolder;
        private DevExpress.XtraEditors.SimpleButton projectLocationBrowse;
    }
}