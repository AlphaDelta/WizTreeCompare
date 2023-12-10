using CsvHelper;
using CsvHelper.Configuration;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TextBox;

namespace WizTreeCompare
{
    public partial class FormViewer : Form
    {
        //TODO: Support for directories (Actually show them, include differences unaccounted for)
        //TODO: Option to view only positives/negatives
        //TODO: Text truncation
        //TODO: Filter
        public FormViewer()
        {
            InitializeComponent();

            SetupTreeView();
            SetupSearch();

            this.FormClosed += (o, e) =>
            {
                tvstruct = null;
                GC.Collect();
            };
        }

        Stream _stream = null;
        public FormViewer(Stream s) : this()
        {
            _stream = s;
            this.Disposed += Cleanup;
            this.Load += (object sender, EventArgs e) => { ReadTreeFromStream(_stream); };
        }

        public FormViewer(string filepath) : this(File.Open(filepath, FileMode.Open, FileAccess.Read, FileShare.Read)) { }

        private void Cleanup(object sender, EventArgs e)
        {
            if (_stream != null)
            {
                _stream.Close();
                _stream.Dispose();
            }
        }

        private void menuOpen_Click(object sender, EventArgs e)
        {
            /* Semaphore */
            if (!tread.IsCompleted)
            {
                tokenSource.Cancel();

                treeMain.Nodes.Clear();
                tvstruct.Clear();

                return;
            }

            /* Get starting directory */
            string startdir = null;
            if (this.ParentForm != null && this.ParentForm is FormMain)
                startdir = Path.GetDirectoryName(((FormMain)this.ParentForm).lastsavepath);

            /* Open file dialog */
            string filepath = null;
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.CheckFileExists = true;
                ofd.CheckPathExists = true;
                ofd.InitialDirectory = startdir;
                ofd.Filter = "CSV Files (*.csv)|*.csv";

                if (ofd.ShowDialog(this) != DialogResult.OK)
                    return;

                filepath = ofd.FileName;
            }

            /* Read file */
            FileStream fs = File.Open(filepath, FileMode.Open, FileAccess.Read, FileShare.Read);
            ReadTreeFromStream(fs);
        }

        char[] dirchars = new char[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar };
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void UpdatePath(string path, long diff, bool rowisdir)
        {
            /* Prepare path */
            //if (path.Length > 0 && !dirchars.Contains(path[0])) path = dirchars[0] + path; //If it hasn't got a delimiter at the start, add one
            ReadOnlySpan<char> spanpath = path.AsSpan().TrimEnd(dirchars);
            if (spanpath.Length < 1) return;

            /* Iterate parts */
            Span<Range> ranges = stackalloc Range[128];
            int amt = spanpath.SplitAny(ranges, dirchars, StringSplitOptions.RemoveEmptyEntries);
            while (amt-- > 0)
            {
                string spath = amt > 0 ? spanpath[..(ranges[amt].Start.Value - 1)].ToString() : "";
                string snode = spanpath[ranges[amt]].ToString();

                Dictionary<string, long> dir;
                if (!tvstruct.TryGetValue(spath, out dir))
                    dir = tvstruct[spath] = new Dictionary<string, long>(); //Create directory if it doesn't already exist

                if (rowisdir)
                {
                    /* If this is a directory row, and this is the first part, use a special node to keep track of its value */
                    rowisdir = false;
                    dir['^' + snode] = diff;
                    break;
                }
                else
                {
                    if (dir.ContainsKey(snode))
                        dir[snode] += diff;
                    else
                        dir[snode] = diff;
                }
            }
        }

        CancellationTokenSource tokenSource = new CancellationTokenSource();
        Task tread = Task.CompletedTask;
        Dictionary<string, Dictionary<string, long>> tvstruct = new Dictionary<string, Dictionary<string, long>>();
        private void ReadTreeFromStream(Stream s)
        {
            /* Semaphore */
            if (!tread.IsCompleted)
                return;

            treeMain.Nodes.Clear();
            tvstruct.Clear();
            GC.Collect();

            treeMain.Enabled = false;

            lblStatus.Text = "";
            lblStatus.Visible = true;

            menuOpen.Text = "&Cancel";

            /* Read stream */
            tread = Task.Run(async () =>
            {
                try
                {
                    TimeSpan tickrate = TimeSpan.FromMilliseconds(300);
                    var action = (ProgressContext ctx) =>
                    {
                        this.Invoke(() => lblStatus.Text = $"Reading {ctx.ProgressPercent:0}%");
                    };

                    /* Read */
                    using (var progress = new ProgressContext(tickrate, action))
                    using (StreamReader sr = new StreamReader(s))
                    using (var csv = new CsvReader(sr, System.Globalization.CultureInfo.InvariantCulture))
                    {
                        csv.Read();
                        if (WTCsvRow.IsRowJunk(csv.Context.Parser.RawRecord.ToLower()))
                            csv.Read();
                        csv.ReadHeader();

                        progress.StartTime = DateTime.Now;
                        progress.ProgressTotal = sr.BaseStream.Length;
                        progress.Action(progress);

                        var records = csv.GetRecordsAsync<WTCsvRow>(tokenSource.Token); // <x>
                        await foreach (WTCsvRow row in records)
                        {
                            progress.ProgressCurrent = progress.ProgressTotal > 0 ? sr.BaseStream.Position : -1;
                            progress.InvokeLater();

                            /* Update Node */
                            UpdatePath(row.FileName, row.Size, row.IsDirectory);
                        }
                    }

                    this.Invoke(() =>
                    {
                        TreeDiscover("", treeMain.Nodes);
                        treeMain.Enabled = true;
                        lblStatus.Visible = false;
                    });

                }
                finally
                {
                    this.Invoke(() =>
                    {
                        if (tokenSource.IsCancellationRequested)
                        {
                            lblStatus.Text = "Parsing was canceled by user";
                            lblStatus.Visible = true;
                        }

                        tokenSource.Dispose();
                        tokenSource = new CancellationTokenSource();

                        s.Dispose();

                        menuOpen.Text = "&Open...";
                    });
                }
            });
        }
    }
}
