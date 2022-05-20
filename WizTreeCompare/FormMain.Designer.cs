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
            this.gbFiles = new System.Windows.Forms.GroupBox();
            this.btnFuture = new System.Windows.Forms.Button();
            this.btnPast = new System.Windows.Forms.Button();
            this.txtFuture = new System.Windows.Forms.TextBox();
            this.lblPast = new System.Windows.Forms.Label();
            this.txtPast = new System.Windows.Forms.TextBox();
            this.lblFuture = new System.Windows.Forms.Label();
            this.btnCompare = new System.Windows.Forms.Button();
            this.gbFiles.SuspendLayout();
            this.SuspendLayout();
            // 
            // gbFiles
            // 
            this.gbFiles.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gbFiles.Controls.Add(this.btnFuture);
            this.gbFiles.Controls.Add(this.btnPast);
            this.gbFiles.Controls.Add(this.txtFuture);
            this.gbFiles.Controls.Add(this.lblPast);
            this.gbFiles.Controls.Add(this.txtPast);
            this.gbFiles.Controls.Add(this.lblFuture);
            this.gbFiles.Location = new System.Drawing.Point(12, 12);
            this.gbFiles.Name = "gbFiles";
            this.gbFiles.Size = new System.Drawing.Size(460, 84);
            this.gbFiles.TabIndex = 0;
            this.gbFiles.TabStop = false;
            this.gbFiles.Text = "Files";
            // 
            // btnFuture
            // 
            this.btnFuture.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnFuture.Location = new System.Drawing.Point(425, 50);
            this.btnFuture.Name = "btnFuture";
            this.btnFuture.Size = new System.Drawing.Size(29, 23);
            this.btnFuture.TabIndex = 3;
            this.btnFuture.Text = "...";
            this.btnFuture.UseVisualStyleBackColor = true;
            this.btnFuture.Click += new System.EventHandler(this.btnFuture_Click);
            // 
            // btnPast
            // 
            this.btnPast.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnPast.Location = new System.Drawing.Point(425, 21);
            this.btnPast.Name = "btnPast";
            this.btnPast.Size = new System.Drawing.Size(29, 24);
            this.btnPast.TabIndex = 1;
            this.btnPast.Text = "...";
            this.btnPast.UseVisualStyleBackColor = true;
            this.btnPast.Click += new System.EventHandler(this.btnPast_Click);
            // 
            // txtFuture
            // 
            this.txtFuture.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtFuture.BackColor = System.Drawing.SystemColors.Control;
            this.txtFuture.Location = new System.Drawing.Point(80, 51);
            this.txtFuture.Name = "txtFuture";
            this.txtFuture.Size = new System.Drawing.Size(339, 23);
            this.txtFuture.TabIndex = 2;
            // 
            // lblPast
            // 
            this.lblPast.AutoSize = true;
            this.lblPast.Location = new System.Drawing.Point(18, 25);
            this.lblPast.Name = "lblPast";
            this.lblPast.Size = new System.Drawing.Size(56, 15);
            this.lblPast.TabIndex = 0;
            this.lblPast.Text = "Past CSV:";
            // 
            // txtPast
            // 
            this.txtPast.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtPast.BackColor = System.Drawing.SystemColors.Control;
            this.txtPast.Location = new System.Drawing.Point(80, 22);
            this.txtPast.Name = "txtPast";
            this.txtPast.Size = new System.Drawing.Size(339, 23);
            this.txtPast.TabIndex = 0;
            // 
            // lblFuture
            // 
            this.lblFuture.AutoSize = true;
            this.lblFuture.Location = new System.Drawing.Point(6, 54);
            this.lblFuture.Name = "lblFuture";
            this.lblFuture.Size = new System.Drawing.Size(68, 15);
            this.lblFuture.TabIndex = 0;
            this.lblFuture.Text = "Future CSV:";
            // 
            // btnCompare
            // 
            this.btnCompare.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCompare.Location = new System.Drawing.Point(12, 102);
            this.btnCompare.Name = "btnCompare";
            this.btnCompare.Size = new System.Drawing.Size(460, 43);
            this.btnCompare.TabIndex = 4;
            this.btnCompare.Text = "Compare...";
            this.btnCompare.UseVisualStyleBackColor = true;
            this.btnCompare.Click += new System.EventHandler(this.btnCompare_Click);
            // 
            // FormMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Window;
            this.ClientSize = new System.Drawing.Size(484, 157);
            this.Controls.Add(this.btnCompare);
            this.Controls.Add(this.gbFiles);
            this.MaximumSize = new System.Drawing.Size(1200, 196);
            this.MinimumSize = new System.Drawing.Size(300, 196);
            this.Name = "FormMain";
            this.Text = "WizTree Compare";
            this.gbFiles.ResumeLayout(false);
            this.gbFiles.PerformLayout();
            this.ResumeLayout(false);

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
    }
}