namespace WizTreeCompare
{
    partial class FormViewer
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
            menu = new MenuStrip();
            menuOpen = new ToolStripMenuItem();
            menuSearch = new ToolStripTextBox();
            split = new SplitContainer();
            treeMain = new TreeView();
            treeAnc = new TreeView();
            lblStatus = new Label();
            menu.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)split).BeginInit();
            split.Panel1.SuspendLayout();
            split.Panel2.SuspendLayout();
            split.SuspendLayout();
            SuspendLayout();
            // 
            // menu
            // 
            menu.Items.AddRange(new ToolStripItem[] { menuOpen, menuSearch });
            menu.Location = new Point(0, 0);
            menu.Name = "menu";
            menu.Size = new Size(800, 26);
            menu.TabIndex = 0;
            menu.Text = "Menu";
            // 
            // menuOpen
            // 
            menuOpen.Name = "menuOpen";
            menuOpen.Size = new Size(57, 22);
            menuOpen.Text = "&Open...";
            menuOpen.Click += menuOpen_Click;
            // 
            // menuSearch
            // 
            menuSearch.BorderStyle = BorderStyle.FixedSingle;
            menuSearch.Font = new Font("Segoe UI", 8F, FontStyle.Regular, GraphicsUnit.Point);
            menuSearch.Name = "menuSearch";
            menuSearch.Size = new Size(300, 22);
            // 
            // split
            // 
            split.Dock = DockStyle.Fill;
            split.FixedPanel = FixedPanel.Panel2;
            split.Location = new Point(0, 26);
            split.Name = "split";
            // 
            // split.Panel1
            // 
            split.Panel1.Controls.Add(lblStatus);
            split.Panel1.Controls.Add(treeMain);
            // 
            // split.Panel2
            // 
            split.Panel2.Controls.Add(treeAnc);
            split.Size = new Size(800, 424);
            split.SplitterDistance = 533;
            split.TabIndex = 1;
            split.TabStop = false;
            // 
            // treeMain
            // 
            treeMain.Dock = DockStyle.Fill;
            treeMain.Location = new Point(0, 0);
            treeMain.Name = "treeMain";
            treeMain.PathSeparator = "|";
            treeMain.Size = new Size(533, 424);
            treeMain.TabIndex = 0;
            // 
            // treeAnc
            // 
            treeAnc.Dock = DockStyle.Fill;
            treeAnc.Location = new Point(0, 0);
            treeAnc.Name = "treeAnc";
            treeAnc.PathSeparator = "|";
            treeAnc.Size = new Size(263, 424);
            treeAnc.TabIndex = 1;
            // 
            // lblStatus
            // 
            lblStatus.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            lblStatus.BackColor = Color.MediumTurquoise;
            lblStatus.Location = new Point(0, 0);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(533, 24);
            lblStatus.TabIndex = 2;
            lblStatus.Text = "Loading 0%";
            lblStatus.TextAlign = ContentAlignment.MiddleCenter;
            lblStatus.Visible = false;
            // 
            // FormViewer
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(split);
            Controls.Add(menu);
            MainMenuStrip = menu;
            Name = "FormViewer";
            Text = "Viewer";
            menu.ResumeLayout(false);
            menu.PerformLayout();
            split.Panel1.ResumeLayout(false);
            split.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)split).EndInit();
            split.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private MenuStrip menu;
        private SplitContainer split;
        private TreeView treeMain;
        private TreeView treeAnc;
        private ToolStripMenuItem menuOpen;
        private ToolStripTextBox menuSearch;
        private Label lblStatus;
    }
}