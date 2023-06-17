namespace WizTreeCompare
{
    partial class FormMain
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            gbFiles = new GroupBox();
            btnFuture = new Button();
            btnPast = new Button();
            txtFuture = new TextBox();
            lblPast = new Label();
            txtPast = new TextBox();
            lblFuture = new Label();
            btnCompare = new Button();
            gbOptions = new GroupBox();
            chkDry = new CheckBox();
            chkIncludeDirs = new CheckBox();
            chkIncludeUnchanged = new CheckBox();
            chkIncludeNegatives = new CheckBox();
            btnViewer = new Button();
            btnClose = new Button();
            gbFiles.SuspendLayout();
            gbOptions.SuspendLayout();
            SuspendLayout();
            // 
            // gbFiles
            // 
            gbFiles.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            gbFiles.Controls.Add(btnFuture);
            gbFiles.Controls.Add(btnPast);
            gbFiles.Controls.Add(txtFuture);
            gbFiles.Controls.Add(lblPast);
            gbFiles.Controls.Add(txtPast);
            gbFiles.Controls.Add(lblFuture);
            gbFiles.Location = new Point(12, 12);
            gbFiles.Name = "gbFiles";
            gbFiles.Size = new Size(460, 84);
            gbFiles.TabIndex = 0;
            gbFiles.TabStop = false;
            gbFiles.Text = "Files";
            // 
            // btnFuture
            // 
            btnFuture.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnFuture.Location = new Point(425, 50);
            btnFuture.Name = "btnFuture";
            btnFuture.Size = new Size(29, 23);
            btnFuture.TabIndex = 3;
            btnFuture.Text = "...";
            btnFuture.UseVisualStyleBackColor = true;
            btnFuture.Click += btnFuture_Click;
            // 
            // btnPast
            // 
            btnPast.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnPast.Location = new Point(425, 21);
            btnPast.Name = "btnPast";
            btnPast.Size = new Size(29, 24);
            btnPast.TabIndex = 1;
            btnPast.Text = "...";
            btnPast.UseVisualStyleBackColor = true;
            btnPast.Click += btnPast_Click;
            // 
            // txtFuture
            // 
            txtFuture.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtFuture.BackColor = SystemColors.Control;
            txtFuture.Location = new Point(80, 51);
            txtFuture.Name = "txtFuture";
            txtFuture.Size = new Size(339, 23);
            txtFuture.TabIndex = 2;
            // 
            // lblPast
            // 
            lblPast.AutoSize = true;
            lblPast.Location = new Point(18, 25);
            lblPast.Name = "lblPast";
            lblPast.Size = new Size(56, 15);
            lblPast.TabIndex = 0;
            lblPast.Text = "Past CSV:";
            // 
            // txtPast
            // 
            txtPast.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtPast.BackColor = SystemColors.Control;
            txtPast.Location = new Point(80, 22);
            txtPast.Name = "txtPast";
            txtPast.Size = new Size(339, 23);
            txtPast.TabIndex = 0;
            // 
            // lblFuture
            // 
            lblFuture.AutoSize = true;
            lblFuture.Location = new Point(6, 54);
            lblFuture.Name = "lblFuture";
            lblFuture.Size = new Size(68, 15);
            lblFuture.TabIndex = 0;
            lblFuture.Text = "Future CSV:";
            // 
            // btnCompare
            // 
            btnCompare.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            btnCompare.Location = new Point(12, 234);
            btnCompare.Name = "btnCompare";
            btnCompare.Size = new Size(233, 43);
            btnCompare.TabIndex = 4;
            btnCompare.Text = "Compare...";
            btnCompare.UseVisualStyleBackColor = true;
            btnCompare.Click += btnCompare_Click;
            // 
            // gbOptions
            // 
            gbOptions.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            gbOptions.Controls.Add(chkDry);
            gbOptions.Controls.Add(chkIncludeDirs);
            gbOptions.Controls.Add(chkIncludeUnchanged);
            gbOptions.Controls.Add(chkIncludeNegatives);
            gbOptions.Location = new Point(12, 102);
            gbOptions.Name = "gbOptions";
            gbOptions.Size = new Size(460, 126);
            gbOptions.TabIndex = 5;
            gbOptions.TabStop = false;
            gbOptions.Text = "Options";
            // 
            // chkDry
            // 
            chkDry.AutoSize = true;
            chkDry.Location = new Point(17, 22);
            chkDry.Name = "chkDry";
            chkDry.Size = new Size(286, 19);
            chkDry.TabIndex = 0;
            chkDry.Text = "Dry run (provide information only, no file output)";
            chkDry.UseVisualStyleBackColor = true;
            // 
            // chkIncludeDirs
            // 
            chkIncludeDirs.AutoSize = true;
            chkIncludeDirs.Location = new Point(17, 97);
            chkIncludeDirs.Name = "chkIncludeDirs";
            chkIncludeDirs.Size = new Size(283, 19);
            chkIncludeDirs.TabIndex = 0;
            chkIncludeDirs.Text = "Include directories (affects output size +20~30%)";
            chkIncludeDirs.UseVisualStyleBackColor = true;
            // 
            // chkIncludeUnchanged
            // 
            chkIncludeUnchanged.AutoSize = true;
            chkIncludeUnchanged.Location = new Point(17, 72);
            chkIncludeUnchanged.Name = "chkIncludeUnchanged";
            chkIncludeUnchanged.Size = new Size(319, 19);
            chkIncludeUnchanged.TabIndex = 0;
            chkIncludeUnchanged.Text = "Include unchanged (includes files with no size changes)";
            chkIncludeUnchanged.UseVisualStyleBackColor = true;
            // 
            // chkIncludeNegatives
            // 
            chkIncludeNegatives.AutoSize = true;
            chkIncludeNegatives.ForeColor = Color.Red;
            chkIncludeNegatives.Location = new Point(17, 47);
            chkIncludeNegatives.Name = "chkIncludeNegatives";
            chkIncludeNegatives.Size = new Size(343, 19);
            chkIncludeNegatives.TabIndex = 0;
            chkIncludeNegatives.Text = "Include negatives (includes deleted files and size reductions)";
            chkIncludeNegatives.UseVisualStyleBackColor = true;
            // 
            // btnViewer
            // 
            btnViewer.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnViewer.Location = new Point(251, 234);
            btnViewer.Name = "btnViewer";
            btnViewer.Size = new Size(108, 43);
            btnViewer.TabIndex = 6;
            btnViewer.Text = "Viewer";
            btnViewer.UseVisualStyleBackColor = true;
            btnViewer.Click += btnViewer_Click;
            // 
            // btnClose
            // 
            btnClose.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnClose.Location = new Point(365, 234);
            btnClose.Name = "btnClose";
            btnClose.Size = new Size(107, 43);
            btnClose.TabIndex = 6;
            btnClose.Text = "Exit";
            btnClose.UseVisualStyleBackColor = true;
            btnClose.Click += btnClose_Click;
            // 
            // FormMain
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = SystemColors.Window;
            ClientSize = new Size(484, 289);
            Controls.Add(btnClose);
            Controls.Add(btnViewer);
            Controls.Add(gbOptions);
            Controls.Add(btnCompare);
            Controls.Add(gbFiles);
            MaximumSize = new Size(1200, 420);
            MinimumSize = new Size(300, 196);
            Name = "FormMain";
            Text = "WizTree Compare";
            gbFiles.ResumeLayout(false);
            gbFiles.PerformLayout();
            gbOptions.ResumeLayout(false);
            gbOptions.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private GroupBox gbFiles;
        private TextBox txtFuture;
        private Label lblPast;
        private TextBox txtPast;
        private Label lblFuture;
        private Button btnFuture;
        private Button btnPast;
        private Button btnCompare;
        private GroupBox gbOptions;
        private CheckBox chkIncludeNegatives;
        private CheckBox chkDry;
        private CheckBox chkIncludeUnchanged;
        private CheckBox chkIncludeDirs;
        private Button btnViewer;
        private Button btnClose;
    }
}