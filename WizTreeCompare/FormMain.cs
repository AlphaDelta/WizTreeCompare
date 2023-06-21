using System.Text;

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
            chkViewer.Checked = chkDry.Checked = true;
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
            if (String.IsNullOrWhiteSpace(txtPast.Text) || String.IsNullOrWhiteSpace(txtFuture.Text))
            {
                MessageBox.Show(this, "Both a past and future csv is required", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!tcompare.IsCompleted)
            {
                tokenSource.Cancel();
                return;
            }

            string output = null;
            tcompare = new Task(() =>
            {
                WTComparer comparer = null;
                Stream underlying = null;
                try
                {
                    //Run comparison
                    comparer = new WTComparer(txtPast.Text, txtFuture.Text)
                    {
                        Dry = chkDry.Checked,
                        IncludeNegatives = chkIncludeNegatives.Checked,
                        IncludeUnchanged = chkIncludeUnchanged.Checked,
                        IncludeDirectories = chkIncludeDirs.Checked,
                        CancellationToken = tokenSource.Token,
                        Stream =
                            chkDry.Checked
                                ? (chkViewer.Checked ? new StreamWriter(underlying = new MemoryStream(), Encoding.UTF8, 4096, true) : StreamWriter.Null)
                                : new StreamWriter(output, false, Encoding.UTF8)
                    };
                    comparer.CompareAndSave(output);
                    if (!chkDry.Checked) lastsavepath = output;

                }
                finally
                {
                    //Restore compare button
                    this.Invoke(() =>
                    {
                        btnCompare.Text = "Compare...";
                        btnCompare.Enabled = true;
                        btnClose.Enabled = true;

                        tokenSource.Dispose();
                        tokenSource = new CancellationTokenSource();

                        if (chkViewer.Checked && comparer != null)
                        {
                            if (chkDry.Checked)
                            {
                                underlying.Seek(0, SeekOrigin.Begin);
                                new FormViewer(underlying).Show(); //Dispose will be called on close
                            }
                            else
                                new FormViewer(output).Show(); //Dispose will be called on close
                        }
                    });
                }
            });

            /* Save file dialog */
            if (chkDry.Checked)
                output = "NUL";
            else
                using (SaveFileDialog sfd = new SaveFileDialog())
                {
                    sfd.FileName = $"WizTreeCompare_{DateTime.Now:yyyyMMddHHmmss}.csv";
                    sfd.Filter = "CSV File (*.csv)|*.csv";

                    if (sfd.ShowDialog() != DialogResult.OK)
                    {
                        tcompare = Task.CompletedTask;
                        return;
                    }

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
                viewer.Show();
            }
            else if (viewer.Disposing)
                return;

            viewer.Focus();
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