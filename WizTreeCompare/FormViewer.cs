using CsvHelper;
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
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TextBox;

namespace WizTreeCompare
{
    public partial class FormViewer : Form
    {
        public FormViewer()
        {
            InitializeComponent();

            treeMain.DrawMode = TreeViewDrawMode.OwnerDrawAll;
            treeMain.DrawNode += TreeMain_DrawNode;

            treeMain.MouseMove += TreeMain_MouseMove;

            treeMain.BeforeExpand += TreeMain_BeforeExpand;

            //TODO: Change to TreeView.DoubleBuffered in dotnet 8
            var method = typeof(System.Windows.Forms.Design.SelectionRules).Assembly
                .GetType("System.Windows.Forms.Design.DesignerUtils")
                .GetMethod("ApplyTreeViewThemeStyles", BindingFlags.Public | BindingFlags.Static);

            ParameterInfo[] _p;
            if (method != null && (_p = method.GetParameters()).Length == 1 && _p[0].ParameterType == typeof(TreeView))
                method.Invoke(null, new object[] { treeMain });
        }

        TreeNode nodelasthover = null;
        private void TreeMain_MouseMove(object sender, MouseEventArgs e)
        {
            TreeNode n = treeMain.GetNodeAt(e.Location);
            //if (n != null && n.Parent != null)
            //    n = n.Parent;

            if (nodelasthover == n)
                return;

            //TreeNode old = nodelasthover;
            nodelasthover = n;
            treeMain.Invalidate();
        }

        private void TreeMain_BeforeExpand(object sender, TreeViewCancelEventArgs e)
        {
            TreeDiscover(e.Node.FullPath, e.Node.Nodes);
        }

        public FormViewer(Stream s) : this()
        {
            ReadTreeFromStream(s);
        }

        const string NODE_PLACEHOLDER = ".!placeholder";
        const int NODE_DIFF_WIDTH = 100;
        const byte NODE_INACTIVE_ALPHA = 100;
        LinearGradientBrush diffback = null, diffpos = null, diffneg = null, diffbackinactive = null, diffposinactive = null, diffneginactive = null;
        SolidBrush difftextpos = new SolidBrush(Color.FromArgb(0, 0, 150));
        SolidBrush difftextneg = new SolidBrush(Color.FromArgb(150, 0, 0));
        SolidBrush difftextnone = new SolidBrush(Color.FromArgb(18, 10, 26));
        Pen linecolor = new Pen(Color.FromArgb(80, SystemColors.ActiveBorder));
        Pen linecolorvalue = new Pen(Color.FromArgb(80, SystemColors.ActiveBorder));
        ImageAttributes attribActive = new ImageAttributes();
        ImageAttributes attribParentFocus = new ImageAttributes();
        ImageAttributes attribInactive = new ImageAttributes();
        private void TreeMain_DrawNode(object sender, DrawTreeNodeEventArgs e)
        {
            if (e.Node.IsVisible && e.Node.Name != NODE_PLACEHOLDER)
            {
                e.Graphics.FillRectangle(SystemBrushes.Window, e.Bounds);

                using (Bitmap bmp = new Bitmap(e.Bounds.Width, e.Bounds.Height))
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    g.Clear(SystemColors.Window);

                    long diff = ((NodeTag)e.Node.Tag).Size;
                    float influence = ((NodeTag)e.Node.Tag).Influence;

                    /* Brushes */
                    if (diffback == null)
                    {
                        diffback = new LinearGradientBrush(new Point(0, 0), new Point(0, e.Bounds.Height), Color.FromArgb(235, 235, 245), Color.White);
                        diffpos = new LinearGradientBrush(new Point(0, 0), new Point(0, e.Bounds.Height), Color.White, Color.FromArgb(200, 200, 255));
                        diffneg = new LinearGradientBrush(new Point(0, 0), new Point(0, e.Bounds.Height), Color.White, Color.FromArgb(255, 200, 200));

                        attribActive.SetColorMatrix(new ColorMatrix(), ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
                        attribParentFocus.SetColorMatrix(new ColorMatrix(new float[][]
                            {
                                new float[] {.3f, .3f, .3f, 0, 0},
                                new float[] {.59f, .59f, .59f, 0, 0},
                                new float[] {.11f, .11f, .11f, 0, 0},
                                new float[] {0, 0, 0, .5f, 0},
                                new float[] {0, 0, 0, 0, 1}
                            }), ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
                        attribInactive.SetColorMatrix(new ColorMatrix(
                            new float[][]
                            {
                                new float[] {.3f, .3f, .3f, 0, 0},
                                new float[] {.59f, .59f, .59f, 0, 0},
                                new float[] {.11f, .11f, .11f, 0, 0},
                                new float[] {0, 0, 0, .20f, 0},
                                new float[] {0, 0, 0, 0, 1}
                            }), ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
                    }
                    /* Text */
                    g.DrawString(
                        WTComparer.BytesToString(diff),
                        e.Node.TreeView.Font, (diff > 0 ? difftextpos : (diff < 0 ? difftextneg : difftextnone)),
                        new Rectangle(200, 0, bmp.Width - 200 - NODE_DIFF_WIDTH, bmp.Height),
                        new StringFormat()
                        {
                            Alignment = StringAlignment.Far
                        });

                    /* Diff rect */
                    var diffrect = new Rectangle(bmp.Width - NODE_DIFF_WIDTH, 0, NODE_DIFF_WIDTH, e.Bounds.Height);
                    diffrect.Inflate(-10, -2);
                    g.FillRectangle(diffback, diffrect);

                    int midwidth = (int)Math.Round(diffrect.Width / 2f);
                    int middle = diffrect.Left + midwidth;
                    float value = (int)Math.Abs(midwidth * influence);
                    var diffvalrect = new Rectangle(influence > 0 ? middle : (int)(middle - value), diffrect.Y, (int)value, diffrect.Height);
                    g.FillRectangle(influence > 0 ? diffpos : (influence < 0 ? diffneg : Brushes.Transparent), diffvalrect);

                    float valueline = influence > 0 ? diffvalrect.Right : diffvalrect.Left;
                    g.DrawLine(linecolor, middle, diffrect.Top, middle, diffrect.Bottom); //Center line
                    g.DrawLine(linecolor, valueline, diffrect.Top, valueline, diffrect.Bottom); //Value line
                    g.DrawRectangle(SystemPens.ActiveBorder, diffrect);

                    /* Composite */
                    var opacityMatrix = e.Node == nodelasthover || e.Node.Parent == nodelasthover ? attribActive : attribInactive;
                    TreeNode p = nodelasthover;
                    if (p != null)
                        while (opacityMatrix == attribInactive && (p = p.Parent) != null)
                            if (p == e.Node)
                            {
                                opacityMatrix = attribActive;
                                break;
                            }

                    p = e.Node;
                    if (opacityMatrix == attribInactive && p.Parent != null && p.Parent.Nodes.Contains(nodelasthover))
                        opacityMatrix = attribParentFocus;
                    else
                        while (opacityMatrix == attribInactive && (p = p.Parent) != null)
                            if (p == nodelasthover)
                                opacityMatrix = attribParentFocus;

                    e.Graphics.DrawImage(bmp, e.Bounds, 0, 0, bmp.Width, bmp.Height, GraphicsUnit.Pixel, opacityMatrix);
                }
            }

            e.DrawDefault = true;
        }

        private void TreeDiscover(string path, TreeNodeCollection col)
        {
            if (!col.ContainsKey(NODE_PLACEHOLDER) && path != "") return;

            col.RemoveByKey(NODE_PLACEHOLDER);

            Dictionary<string, long> dir;
            if (!tvstruct.TryGetValue(path, out dir))
                return; //TODO: Inform user?

            long maxmag = dir.Max(x => Math.Abs(x.Value));

            List<TreeNode> nodes = new List<TreeNode>();
            foreach (var kv in dir)
            {
                TreeNode tn = new TreeNode()
                {
                    Name = kv.Key,
                    Text = kv.Key,
                    Tag = new NodeTag() { Size = kv.Value, Influence = maxmag > 0 ? (float)kv.Value / (float)maxmag : (float)Math.Sign(kv.Value) }
                };

                if (dirchars.Any(x => tvstruct.ContainsKey(path + x + kv.Key)) || path == "")
                    tn.Nodes.Add(NODE_PLACEHOLDER, "");

                nodes.Add(tn);
            }

            int parentsign = Math.Sign(dir.Sum(x => x.Value));
            col.AddRange(
                nodes
                .OrderByDescending(x => Math.Sign(((NodeTag)x.Tag).Size) * parentsign)
                .ThenByDescending(x => Math.Abs(((NodeTag)x.Tag).Size))
                .ToArray()); //TODO: Add option for val vs mag
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private string PathParent(string path)
        {
            string ret = Path.GetDirectoryName(path);
            if (String.IsNullOrWhiteSpace(ret)) ret = "";
            ret = ret.TrimEnd(dirchars);

            return ret;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private string NodeName(string path)
        {
            string ret = Path.GetFileName(path);
            if (String.IsNullOrEmpty(ret)) return path;

            return ret;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void UpdateNode(string path, long diff, bool isdir)
        {
            path = path.TrimEnd(dirchars);

            string parent = PathParent(path);
            string name = NodeName(path);

            Dictionary<string, long> dir;
            //if (!tvstruct.TryGetValue(parent, out dir)) //Broken for some reason???
            if (tvstruct.ContainsKey(parent))
                dir = tvstruct[parent];
            else
                dir = tvstruct[parent] = new Dictionary<string, long>();

            if (!dir.ContainsKey(name))
                dir[name] = diff;
            else
                dir[name] += diff;
        }

        char[] dirchars = new char[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar };
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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

        internal class NodeTag
        {
            internal long Size;
            internal float Influence = 0f;
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

            treeMain.Enabled = false;

            lblStatus.Text = "";
            lblStatus.Visible = true;

            /* Read stream */
            tread = Task.Run(async () =>
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
                    progress.StartTime = DateTime.Now;
                    progress.ProgressTotal = sr.BaseStream.Length;
                    progress.Action(progress);

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
                            UpdateNode(path, row.Size, true);
                        }

                        UpdateNode(row.FileName, row.Size, row.IsDirectory);
                    }
                }

                this.Invoke(() =>
                {
                    TreeDiscover("", treeMain.Nodes);
                    treeMain.Enabled = true;

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
