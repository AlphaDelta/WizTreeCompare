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
        }

        private void Txt_DragEnter(object sender, DragEventArgs e)
        {
            if(e.Data.GetDataPresent(DataFormats.FileDrop))
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

        private void btnCompare_Click(object sender, EventArgs e)
        {
            string output = null;

            using(SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.FileName = $"WizTreeCompare_{DateTime.Now:yyyyMMddHHmmss}.csv";
                sfd.Filter = "CSV File (*.csv)|*.csv";

                if (sfd.ShowDialog() != DialogResult.OK)
                    return;

                output = sfd.FileName;
            }

            WTComparer comparer = new WTComparer(txtPast.Text, txtFuture.Text);
            comparer.CompareAndSave(output);
        }
    }
}