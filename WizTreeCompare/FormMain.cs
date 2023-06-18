namespace WizTreeCompare
{
    public partial class FormMain : Form
    {
        public FormMain()
        {
            InitializeComponent();

            txtPast.AllowDrop = true;
            txtFuture.AllowDrop = true;

            txtPast.DragEnter += Txt_DragEnter;
            txtFuture.DragEnter += Txt_DragEnter;

            txtPast.DragDrop += Txt_DragDrop;
            txtFuture.DragDrop += Txt_DragDrop;

            ToolTip tipNegatives = new ToolTip();
            tipNegatives.InitialDelay = 1000;
            tipNegatives.SetToolTip(chkIncludeNegatives, "Not currently compatiable with WizTree");

#if DEBUG
            chkDry.Checked = true;
#endif
        }

        private void Txt_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                if (((string[])e.Data.GetData(DataFormats.FileDrop)).Length == 1)
                {
                    e.Effect = DragDropEffects.Copy;
                    return;
                }
            }
            e.Effect = DragDropEffects.None;
        }
        private void Txt_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                ((TextBox)sender).Text = ((string[])e.Data.GetData(DataFormats.FileDrop))[0];
            }
        }

        private void btnPast_Click(object sender, EventArgs e)
        {
            SelectFile(txtPast);
        }

        private void btnFuture_Click(object sender, EventArgs e)
        {
            SelectFile(txtFuture);
        }

        private void SelectFile(TextBox tb)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Multiselect = false;
                ofd.Filter = "CSV Files (*.csv)|*.csv";

                if (ofd.ShowDialog() != DialogResult.OK)
                    return;

                tb.Text = ofd.FileName;
            }
        }

        CancellationTokenSource tokenSource = new CancellationTokenSource();
        Task tcompare = Task.CompletedTask;
        internal string lastsavepath = null;
        private void btnCompare_Click(object sender, EventArgs e)
        {
            if (!tcompare.IsCompleted)
            {
                tokenSource.Cancel();
                return;
            }

            string output = null;
            tcompare = new Task(() =>
            {
                //Run comparison
                WTComparer comparer = new WTComparer(txtPast.Text, txtFuture.Text)
                {
                    Dry = chkDry.Checked,
                    IncludeNegatives = chkIncludeNegatives.Checked,
                    IncludeUnchanged = chkIncludeUnchanged.Checked,
                    IncludeDirectories = chkIncludeDirs.Checked,
                    CancellationToken = tokenSource.Token
                };
                comparer.CompareAndSave(output);
                if (!chkDry.Checked) lastsavepath = output;

                //Restore compare button
                this.Invoke(() =>
                {
                    btnCompare.Text = "Compare...";
                    btnCompare.Enabled = true;
                    btnClose.Enabled = true;

                    tokenSource.Dispose();
                    tokenSource = new CancellationTokenSource();
                });
            });

            /* Save file dialog */
            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.FileName = $"WizTreeCompare_{DateTime.Now:yyyyMMddHHmmss}.csv";
                sfd.Filter = "CSV File (*.csv)|*.csv";

                if (sfd.ShowDialog() != DialogResult.OK)
                    return;

                output = sfd.FileName;
            }

            /* Switch 'Compare' button to 'Cancel' */
            btnCompare.Enabled = false;
            btnClose.Enabled = false;
            btnCompare.Text = "Cancel";
            Task.Run(async () =>
            {
                await Task.Delay(1000);
                this.Invoke(() => { btnCompare.Enabled = true; });
            });

            /* Compare */
            tcompare.Start();
        }

        FormViewer viewer = null;
        private void btnViewer_Click(object sender, EventArgs e)
        {
            if (viewer == null || viewer.IsDisposed)
            {
                viewer = new FormViewer();
                viewer.Show(this);
            }
            else if (viewer.Disposing)
                return;

            viewer.BringToFront();
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            if (viewer != null && !viewer.IsDisposed && !viewer.Disposing)
            {
                viewer.Close();
                viewer.Dispose();
            }

            this.Close();
            Environment.Exit(0);
        }
    }
}