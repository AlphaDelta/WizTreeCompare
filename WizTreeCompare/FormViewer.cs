using CsvHelper;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WizTreeCompare
{
    public partial class FormViewer : Form
    {
        public FormViewer()
        {
            InitializeComponent();


            //TODO: Draw faster plox D:<
            //treeMain.DrawMode = TreeViewDrawMode.OwnerDrawText;
            //treeMain.DrawNode += TreeMain_DrawNode;
        }

        private void TreeMain_DrawNode(object sender, DrawTreeNodeEventArgs e)
        {
            e.DrawDefault = true;

            //e.Graphics.DrawLine(Pens.Black, new Point(0, 0), new Point(8, 8));
        }

        public FormViewer(Stream s) : this()
        {
            ReadTreeFromStream(s);
        }

        private void menuOpen_Click(object sender, EventArgs e)
        {
            /* Semaphore */
            if (!tread.IsCompleted)
                return;

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
        private string[] GetPartsOfPath(string path)
        {
            path = path.TrimEnd(dirchars);

            List<string> paths = new List<string>();
            bool foundchar = false;
            for (int i = 0; i < path.Length; i++)
                if (path[i] == Path.DirectorySeparatorChar || path[i] == Path.AltDirectorySeparatorChar)
                {
                    if (!foundchar) continue;
                    paths.Add(path[..i]);
                }
                else
                    foundchar = true;

            return paths.ToArray();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private TreeNode GetNodeFromPath(string path, Dictionary<string, TreeNode> mem)
        {
            path = path.TrimEnd(dirchars);

            if (mem.ContainsKey(path))
                return mem[path];

            string[] parts = GetPartsOfPath(path).Reverse().ToArray();

            TreeNodeCollection closest = treeMain.Nodes;
            int i = 0;
            for (; i < parts.Length; i++)
            {
                if (!mem.ContainsKey(parts[i]))
                    continue;

                closest = mem[parts[i]].Nodes;
                break;
            }

            /* Create nodes where necessary */
            int lastindex = -1;
            for (i--; i > 0; i--)
            {
                //If we got to this point, a node is missing
                lastindex = parts[i].LastIndexOfAny(dirchars);
                string dirkey = lastindex > 1 ? parts[i][(lastindex + 1)..] : parts[i];
                TreeNode node = this.Invoke(() => closest.Add(dirkey, dirkey));
                closest = (mem[parts[i]] = node).Nodes;
            }

            lastindex = path.LastIndexOfAny(dirchars);
            string key = lastindex > 1 ? path[(lastindex + 1)..] : path;


            int ix = closest.IndexOfKey(key);
            if (ix < 0) return this.Invoke(() => closest.Add(key, key));
            else return closest[ix];
        }

        internal class NodeTag
        {
            internal long Size;
        }

        CancellationTokenSource tokenSource = new CancellationTokenSource();
        Task tread = Task.CompletedTask;
        private void ReadTreeFromStream(Stream s)
        {
            /* Semaphore */
            if (!tread.IsCompleted)
                return;

            treeMain.Nodes.Clear();

            /* Read stream */
            tread = Task.Run(async () =>
            {
                this.Invoke(() =>
                {
                    treeMain.BackColor = SystemColors.Control;
                    treeMain.Invalidate();
                    treeMain.Update();

                    treeMain.BeginUpdate();

                    lblStatus.Text = "";
                    lblStatus.Visible = true;
                });

                Dictionary<string, long> dirupdates = new Dictionary<string, long>();
                Dictionary<string, long> fileupdates = new Dictionary<string, long>();
                Dictionary<string, TreeNode> mem = new Dictionary<string, TreeNode>();

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
                    progress.StartTime = DateTime.Now;
                    progress.ProgressTotal = sr.BaseStream.Length;

                    var records = csv.GetRecordsAsync<WTCsvRow>(tokenSource.Token); // <x>
                    await foreach (WTCsvRow row in records)
                    {
                        progress.ProgressCurrent = progress.ProgressTotal > 0 ? sr.BaseStream.Position : -1;
                        progress.InvokeLater();

                        /* Get all directories that need updating */
                        string[] parts = GetPartsOfPath(row.FileName);

                        /* Update paths */
                        foreach (string path in parts)
                        {
                            if (dirupdates.ContainsKey(path))
                                dirupdates[path] += row.Size;
                            else
                                dirupdates[path] = row.Size;
                        }
                        if (fileupdates.ContainsKey(row.FileName))
                            fileupdates[row.FileName] += row.Size;
                        else
                            fileupdates[row.FileName] = row.Size;
                    }
                }

                /* Update directories */
                action = (ProgressContext ctx) =>
                {
                    this.Invoke(() => lblStatus.Text = $"Populating {ctx.ProgressPercent:0}%");
                };
                using (var progress = new ProgressContext(tickrate, action))
                {
                    progress.StartTime = DateTime.Now;
                    progress.ProgressTotal = dirupdates.Count + fileupdates.Count;

                    foreach (var update in dirupdates)
                    {
                        progress.ProgressCurrent++;
                        progress.InvokeLater();

                        TreeNode node = GetNodeFromPath(update.Key, mem);
                        if (node.Tag == null) node.Tag = 0L;
                        node.Tag = ((long)node.Tag) + update.Value;
                        mem[update.Key] = node;
                    }
                    dirupdates.Clear();

                    /* Update files */
                    foreach (var update in fileupdates)
                    {
                        progress.ProgressCurrent++;
                        progress.InvokeLater();

                        //TODO: Discover files later, we don't actually care about them until their parent directory has been expanded
                        //TreeNode node = GetNodeFromPath(update.Key, mem);
                        string key = Path.GetDirectoryName(update.Key) + "\\.";
                        TreeNode node = GetNodeFromPath(key, mem);
                        if (node.Tag == null) node.Tag = 0L;
                        node.Tag = ((long)node.Tag) + update.Value;
                    }
                    fileupdates.Clear();
                }

                this.Invoke(() =>
                {
                    treeMain.EndUpdate();

                    treeMain.BackColor = SystemColors.Window;
                    treeMain.Invalidate();
                    treeMain.Update();

                    lblStatus.Visible = false;
                });

                this.Invoke(() =>
                {
                    tokenSource.Dispose();
                    tokenSource = new CancellationTokenSource();

                    if (s is FileStream) s.Dispose();
                });
            });
        }
    }
}
