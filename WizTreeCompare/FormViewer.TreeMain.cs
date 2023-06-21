using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace WizTreeCompare
{
    public partial class FormViewer : Form
    {
        void SetupTreeView()
        {
            /* Event bindings */
            treeMain.DrawMode = TreeViewDrawMode.OwnerDrawAll;
            treeMain.DrawNode += TreeMain_DrawNode;

            treeMain.MouseMove += TreeMain_MouseMove;
            treeMain.BeforeSelect += TreeMain_BeforeSelect;

            treeMain.BeforeExpand += TreeMain_BeforeExpand;
            treeMain.AfterExpand += TreeMain_AfterExpand;

            /* Icons */
            treeMain.ImageList = new ImageList();
            treeMain.ImageList.ColorDepth = ColorDepth.Depth32Bit;
            treeMain.ImageList.ImageSize = new Size(16, 16);
            treeMain.ImageList.Images.Add("dir", GetShell32Icon(3));
            treeMain.ImageList.Images.Add("device", GetShell32Icon(15));
            treeMain.ImageList.Images.Add("file", GetShell32Icon(0));
            treeMain.ImageList.Images.Add("file.exe", GetShell32Icon(2));
            Icon txt = GetShell32Icon(70);
            treeMain.ImageList.Images.Add("file.txt", txt);
            treeMain.ImageList.Images.Add("file.nfo", txt);
            Icon compressed = GetShell32Icon(54);
            treeMain.ImageList.Images.Add("file.zip", compressed);
            treeMain.ImageList.Images.Add("file.7z", compressed);
            treeMain.ImageList.Images.Add("file.rar", compressed);
            treeMain.ImageList.Images.Add("file.bzip", compressed);
            treeMain.ImageList.Images.Add("file.bz2", compressed);
            treeMain.ImageList.Images.Add("file.gz", compressed);
            treeMain.ImageList.Images.Add("file.xz", compressed);
            treeMain.ImageList.Images.Add("file.tar", compressed);
            Icon img = GetShell32Icon(325);
            treeMain.ImageList.Images.Add("file.jpeg", img);
            treeMain.ImageList.Images.Add("file.jpg", img);
            treeMain.ImageList.Images.Add("file.jxl", img);
            treeMain.ImageList.Images.Add("file.avif", img);
            treeMain.ImageList.Images.Add("file.png", img);
            treeMain.ImageList.Images.Add("file.tga", img);
            treeMain.ImageList.Images.Add("file.tif", img);
            treeMain.ImageList.Images.Add("file.tiff", img);
            Icon vid = GetShell32Icon(115);
            treeMain.ImageList.Images.Add("file.wmv", vid);
            treeMain.ImageList.Images.Add("file.mov", vid);
            treeMain.ImageList.Images.Add("file.avi", vid);
            treeMain.ImageList.Images.Add("file.mp4", vid);
            treeMain.ImageList.Images.Add("file.webm", vid);
            treeMain.ImageList.Images.Add("file.mkv", vid);
            Icon mus = GetShell32Icon(116);
            treeMain.ImageList.Images.Add("file.wav", mus);
            treeMain.ImageList.Images.Add("file.ape", mus);
            treeMain.ImageList.Images.Add("file.flac", mus);
            treeMain.ImageList.Images.Add("file.alac", mus);
            treeMain.ImageList.Images.Add("file.aiff", mus);
            treeMain.ImageList.Images.Add("file.mp2", mus);
            treeMain.ImageList.Images.Add("file.mp3", mus);
            treeMain.ImageList.Images.Add("file.ogg", mus);
            treeMain.ImageList.Images.Add("file.vorbis", mus);
            treeMain.ImageList.Images.Add("file.opus", mus);
            treeMain.ImageList.Images.Add("file.m4a", mus);
            Icon recycle = GetShell32Icon(62);
            treeMain.ImageList.Images.Add("file.bin", recycle);
            treeMain.ImageList.Images.Add("$recycle.bin", recycle);

            /* Sparkle */
            //TODO: Change to TreeView.DoubleBuffered in dotnet 8
            var method = typeof(System.Windows.Forms.Design.SelectionRules).Assembly
                .GetType("System.Windows.Forms.Design.DesignerUtils")
                .GetMethod("ApplyTreeViewThemeStyles", BindingFlags.Public | BindingFlags.Static);

            ParameterInfo[] _p;
            if (method != null && (_p = method.GetParameters()).Length == 1 && _p[0].ParameterType == typeof(TreeView))
                method.Invoke(null, new object[] { treeMain });
        }

        //https://stackoverflow.com/a/62504226
        public static Icon GetShell32Icon(int index)
        {
            IntPtr hIcon;
            ExtractIconEx(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "shell32.dll"), index, IntPtr.Zero, out hIcon, 1);

            return hIcon != IntPtr.Zero ? Icon.FromHandle(hIcon) : null;
        }

        [DllImport("shell32", CharSet = CharSet.Unicode)]
        private static extern int ExtractIconEx(string lpszFile, int nIconIndex, out IntPtr phiconLarge, IntPtr phiconSmall, int nIcons);

        [DllImport("shell32", CharSet = CharSet.Unicode)]
        private static extern int ExtractIconEx(string lpszFile, int nIconIndex, IntPtr phiconLarge, out IntPtr phiconSmall, int nIcons);

        private void TreeDiscover(string path, TreeNodeCollection col)
        {
            if (!col.ContainsKey(NODE_PLACEHOLDER) && path != "") return;

            col.RemoveByKey(NODE_PLACEHOLDER);

            Dictionary<string, long> dir;
            if (!tvstruct.TryGetValue(path, out dir))
                throw new Exception("Node missing in tvstruct");

            long sumpos = 0, sumneg = 0;
            foreach (var kv in dir)
                if (!kv.Key.StartsWith('^'))
                    if (kv.Value > 0)
                        sumpos += kv.Value;
                    else if (kv.Value < 0)
                        sumneg += -kv.Value;

            long maxmag = (long)Math.Max(sumpos, sumneg);

            List<TreeNode> nodes = new List<TreeNode>();
            foreach (var kv in dir)
            {
                if (kv.Key.StartsWith('^')) continue;
                
                bool isdir = (dirchars.Any(x => tvstruct.ContainsKey(path + x + kv.Key)) || path == "");

                string imagekey =
                    kv.Key.EndsWith(":") || kv.Key.StartsWith(@"\\") || kv.Key.StartsWith(@"//")
                    ? "device"
                    : (isdir ? "dir" : "file" + Path.GetExtension(kv.Key));
                imagekey = imagekey.ToLower();
                if (treeMain.ImageList.Images.ContainsKey(kv.Key.ToLower())) imagekey = kv.Key.ToLower();
                else if (!treeMain.ImageList.Images.ContainsKey(imagekey)) imagekey = "file";

                TreeNode tn = new TreeNode()
                {
                    Name = kv.Key,
                    Text = kv.Key,
                    Tag = new NodeTag() { Size = kv.Value, Influence = maxmag > 0 ? (float)kv.Value / (float)maxmag : (float)Math.Sign(kv.Value) },
                    ImageKey = imagekey,
                    SelectedImageKey = imagekey
                };

                if (isdir)
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

        /* Events */
        volatile TreeNode nodelasthover = null;
        private void SetNodeLastHover(TreeNode n)
        {
            if (nodelasthover == n)
                return;

            TreeNode old = nodelasthover;

            nodelasthover = n;

            Rectangle updatearea = new Rectangle(NODE_PAINT_LEFT, 0, treeMain.Bounds.Width - NODE_PAINT_LEFT, treeMain.Height);
            if (old != null && n.Parent == old.Parent)
            {

                treeMain.Invalidate(new Rectangle(NODE_PAINT_LEFT + NODE_INDENT_LEFT * n.Level, n.Bounds.Top, NODE_TEXT_WIDTH + NODE_DIFF_WIDTH, n.Bounds.Height));
                treeMain.Invalidate(new Rectangle(NODE_PAINT_LEFT + NODE_INDENT_LEFT * old.Level, old.Bounds.Top, NODE_TEXT_WIDTH + NODE_DIFF_WIDTH, old.Bounds.Height));
                //treeMain.Update();
            }
            else
                treeMain.Invalidate(updatearea);
        }

        int ignoreMouseMove = 0;
        Point prevloc = new Point(-1, -1);
        private void TreeMain_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Location == prevloc)
                return;
            prevloc = e.Location;

            TreeNode hover = treeMain.GetNodeAt(e.Location);
            if (hover == null) hover = treeMain.SelectedNode;
            if (hover == null && treeMain.Nodes.Count > 0) hover = treeMain.Nodes[0];
            SetNodeLastHover(hover);
        }

        private void TreeMain_BeforeSelect(object sender, TreeViewCancelEventArgs e)
        {
            ignoreMouseMove = 5;
            SetNodeLastHover(e.Node);
        }

        private void TreeMain_BeforeExpand(object sender, TreeViewCancelEventArgs e)
        {
            TreeDiscover(e.Node.FullPath, e.Node.Nodes);
        }

        private void TreeMain_AfterExpand(object sender, TreeViewEventArgs e)
        {
            ignoreMouseMove = 3;
            SetNodeLastHover(e.Node);
        }

        /* Paint */
        const string NODE_PLACEHOLDER = "^placeholder";
        const byte NODE_INACTIVE_ALPHA = 100;
        const int NODE_TEXT_WIDTH = 100;
        const int NODE_DIFF_WIDTH = 100;
        const int NODE_PAINT_LEFT = 200;
        const int NODE_INDENT_LEFT = 25;
        LinearGradientBrush diffback = null, diffpos = null, diffneg = null, diffbackinactive = null, diffposinactive = null, diffneginactive = null;
        SolidBrush difftextpos = new SolidBrush(Color.FromArgb(0, 0, 150));
        SolidBrush difftextneg = new SolidBrush(Color.FromArgb(150, 0, 0));
        SolidBrush difftextnone = new SolidBrush(Color.FromArgb(18, 10, 26));
        Pen linecolor = new Pen(Color.FromArgb(80, SystemColors.ActiveBorder));
        Pen linecolorvalue = new Pen(Color.FromArgb(80, SystemColors.ActiveBorder));
        Pen sparklepen = new Pen(Color.FromArgb(120, Color.White));
        ImageAttributes attribActive = new ImageAttributes();
        ImageAttributes attribParentFocus = new ImageAttributes();
        ImageAttributes attribInactive = new ImageAttributes();
        private void TreeMain_DrawNode(object sender, DrawTreeNodeEventArgs e)
        {
            e.DrawDefault = true;

            if (!e.Node.IsVisible || e.Node.Name == NODE_PLACEHOLDER)
                return;

            e.Graphics.FillRectangle(SystemBrushes.Window, e.Bounds);

            int leftindent = NODE_PAINT_LEFT + NODE_INDENT_LEFT * e.Node.Level;
            Rectangle bounds = new Rectangle(leftindent, e.Bounds.Top, NODE_TEXT_WIDTH + NODE_DIFF_WIDTH, e.Bounds.Height);
            using (Bitmap bmp = new Bitmap(bounds.Width, bounds.Height))
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
                    new Rectangle(0, 0, 100, bmp.Height),
                    new StringFormat()
                    {
                        Alignment = StringAlignment.Far
                    });

                /* Diff rect */
                var diffrect = new Rectangle(0 + NODE_TEXT_WIDTH, 0, NODE_DIFF_WIDTH, e.Bounds.Height);
                diffrect.Inflate(-10, -2);
                g.FillRectangle(diffback, diffrect);

                int midwidth = (int)Math.Round(diffrect.Width / 2f);
                int middle = diffrect.Left + midwidth;
                float value = (int)Math.Abs(midwidth * influence);
                var diffvalrect = new Rectangle(influence > 0 ? middle : (int)(middle - value), diffrect.Y, (int)value, diffrect.Height);
                g.FillRectangle(influence > 0 ? diffpos : (influence < 0 ? diffneg : Brushes.Transparent), diffvalrect);

                /* Sparkle */
                //g.CompositingMode = CompositingMode.SourceCopy;
                var sparkle = diffvalrect;
                sparkle.Inflate(-1, -1);
                g.DrawLine(sparklepen, sparkle.Left, sparkle.Bottom, sparkle.Right, sparkle.Bottom);
                //g.CompositingMode = CompositingMode.SourceOver;

                /* Lines */
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

                e.Graphics.DrawImage(bmp, bounds, 0, 0, bmp.Width, bmp.Height, GraphicsUnit.Pixel, opacityMatrix);
            }
        }
    }
}
