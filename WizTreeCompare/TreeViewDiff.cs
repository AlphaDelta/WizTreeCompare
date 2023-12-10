using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace WizTreeCompare
{
    public class TreeViewDiff : TreeView
    {
        public class NodeTag
        {
            internal long Size;
            internal float Influence = 0f;
        }

        public TreeViewDiff()
        {
            /* Event bindings */
            this.DrawMode = TreeViewDrawMode.OwnerDrawText;

            /* Icons */
            this.ImageList = new ImageList();
            this.ImageList.ColorDepth = ColorDepth.Depth32Bit;
            this.ImageList.ImageSize = new Size(16, 16);
            this.ImageList.Images.Add("dir", GetShell32Icon(3));
            this.ImageList.Images.Add("device", GetShell32Icon(15));
            this.ImageList.Images.Add("file", GetShell32Icon(0));
            this.ImageList.Images.Add("file.exe", GetShell32Icon(2));
            Icon txt = GetShell32Icon(70);
            this.ImageList.Images.Add("file.txt", txt);
            this.ImageList.Images.Add("file.nfo", txt);
            Icon compressed = GetShell32Icon(54);
            this.ImageList.Images.Add("file.zip", compressed);
            this.ImageList.Images.Add("file.7z", compressed);
            this.ImageList.Images.Add("file.rar", compressed);
            this.ImageList.Images.Add("file.bzip", compressed);
            this.ImageList.Images.Add("file.bz2", compressed);
            this.ImageList.Images.Add("file.gz", compressed);
            this.ImageList.Images.Add("file.xz", compressed);
            this.ImageList.Images.Add("file.tar", compressed);
            Icon img = GetShell32Icon(325);
            this.ImageList.Images.Add("file.jpeg", img);
            this.ImageList.Images.Add("file.jpg", img);
            this.ImageList.Images.Add("file.jxl", img);
            this.ImageList.Images.Add("file.avif", img);
            this.ImageList.Images.Add("file.png", img);
            this.ImageList.Images.Add("file.tga", img);
            this.ImageList.Images.Add("file.tif", img);
            this.ImageList.Images.Add("file.tiff", img);
            Icon vid = GetShell32Icon(115);
            this.ImageList.Images.Add("file.wmv", vid);
            this.ImageList.Images.Add("file.mov", vid);
            this.ImageList.Images.Add("file.avi", vid);
            this.ImageList.Images.Add("file.mp4", vid);
            this.ImageList.Images.Add("file.webm", vid);
            this.ImageList.Images.Add("file.mkv", vid);
            Icon mus = GetShell32Icon(116);
            this.ImageList.Images.Add("file.wav", mus);
            this.ImageList.Images.Add("file.ape", mus);
            this.ImageList.Images.Add("file.flac", mus);
            this.ImageList.Images.Add("file.alac", mus);
            this.ImageList.Images.Add("file.aiff", mus);
            this.ImageList.Images.Add("file.mp2", mus);
            this.ImageList.Images.Add("file.mp3", mus);
            this.ImageList.Images.Add("file.ogg", mus);
            this.ImageList.Images.Add("file.vorbis", mus);
            this.ImageList.Images.Add("file.opus", mus);
            this.ImageList.Images.Add("file.m4a", mus);
            Icon recycle = GetShell32Icon(62);
            this.ImageList.Images.Add("file.bin", recycle);
            this.ImageList.Images.Add("$recycle.bin", recycle);

            /* Sparkle */
            //System.ComponentModel.Design.ObjectSelectorEditor.ApplyTreeViewThemeStyles(this); //Not the same thing??
            //this.DoubleBuffered = true;
            //this.SetStyle(ControlStyles.); /???

            //TODO: Change to TreeView.DoubleBuffered in dotnet 8
            var method = typeof(System.Windows.Forms.Design.SelectionRules).Assembly
                .GetType("System.Windows.Forms.Design.DesignerUtils")
                .GetMethod("ApplyTreeViewThemeStyles", BindingFlags.Public | BindingFlags.Static);

            ParameterInfo[] _p;
            if (method != null && (_p = method.GetParameters()).Length == 1 && _p[0].ParameterType == typeof(TreeView))
                method.Invoke(null, new object[] { this });
        }

        //https://stackoverflow.com/a/62504226
        internal static Icon GetShell32Icon(int index)
        {
            IntPtr hIcon;
            ExtractIconEx(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "shell32.dll"), index, IntPtr.Zero, out hIcon, 1);

            return hIcon != IntPtr.Zero ? Icon.FromHandle(hIcon) : null;
        }

        [DllImport("shell32", CharSet = CharSet.Unicode)]
        internal static extern int ExtractIconEx(string lpszFile, int nIconIndex, out IntPtr phiconLarge, IntPtr phiconSmall, int nIcons);

        [DllImport("shell32", CharSet = CharSet.Unicode)]
        internal static extern int ExtractIconEx(string lpszFile, int nIconIndex, IntPtr phiconLarge, out IntPtr phiconSmall, int nIcons);

        /* Events */
        volatile TreeNode nodelasthover = null;
        private void SetNodeLastHover(TreeNode n)
        {
            if (nodelasthover == n || n == null)
                return;

            TreeNode old = nodelasthover;

            nodelasthover = n;

            int width = NODE_TEXT_WIDTH + NODE_DIFF_WIDTH;

            if (old != null)
            {
                if (old.Parent != null && old.Parent != n.Parent)
                    old = old.Parent;

                int oldcount = old.Nodes.Count;
                if (old.IsExpanded && oldcount > 0)
                    this.Invalidate(new Rectangle(old.Bounds.X + NODE_PAINT_LEFT, old.Bounds.Y, this.Width, old.LastNode.Bounds.Y - old.Bounds.Y + old.LastNode.Bounds.Height));
                else
                    this.Invalidate(new Rectangle(old.Bounds.X + NODE_PAINT_LEFT, old.Bounds.Y, width, old.Bounds.Height));
            }

            int newcount = n.Nodes.Count;
            if (n.IsExpanded && newcount > 0)
                this.Invalidate(new Rectangle(n.Bounds.X + NODE_PAINT_LEFT, n.Bounds.Y, this.Width, n.LastNode.Bounds.Y - n.Bounds.Y + n.LastNode.Bounds.Height));
            else
                this.Invalidate(new Rectangle(n.Bounds.X + NODE_PAINT_LEFT, n.Bounds.Y, width, n.Bounds.Height));

            this.Update();
        }

        Point prevloc = new Point(-1, -1);
        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (e.Location == prevloc)
                return;
            prevloc = e.Location;

            TreeNode hover = this.GetNodeAt(e.Location);
            if (hover == null) hover = this.SelectedNode;
            if (hover == null && this.Nodes.Count > 0) hover = this.Nodes[0];
            SetNodeLastHover(hover);
        }

        protected override void OnBeforeSelect(TreeViewCancelEventArgs e)
        {
            SetNodeLastHover(e.Node);
        }

        protected override void OnAfterExpand(TreeViewEventArgs e)
        {
            SetNodeLastHover(e.Node);
        }

        const string NODE_PLACEHOLDER = "^placeholder";
        const byte NODE_INACTIVE_ALPHA = 100;
        const int NODE_TEXT_WIDTH = 75;
        const int NODE_DIFF_WIDTH = 100;
        const int NODE_PAINT_LEFT = 200;
        const int NODE_INDENT_LEFT = 25;
        LinearGradientBrush diffback = null, diffpos = null, diffneg = null, diffbackinactive = null, diffposinactive = null, diffneginactive = null;
        Color difftextpos = Color.FromArgb(0, 0, 150);
        Color difftextneg = Color.FromArgb(150, 0, 0);
        Color difftextnone = Color.FromArgb(18, 10, 26);
        Pen linecolor = new Pen(Color.FromArgb(80, SystemColors.ActiveBorder));
        Pen linecolorvalue = new Pen(Color.FromArgb(80, SystemColors.ActiveBorder));
        Pen sparklepen = new Pen(Color.FromArgb(120, Color.White));
        ImageAttributes attribActive = new ImageAttributes();
        ImageAttributes attribParentFocus = new ImageAttributes();
        ImageAttributes attribInactive = new ImageAttributes();
        protected override void OnDrawNode(DrawTreeNodeEventArgs e)
        {
            if (!e.Node.IsVisible || e.Node.Name == NODE_PLACEHOLDER)
                return;

            //e.Graphics.FillRectangle(Brushes.Transparent, e.Bounds);
            e.Graphics.FillRectangle(SystemBrushes.Window, e.Bounds);

            /* Draw node text */
            //e.Graphics.CompositingMode = CompositingMode.SourceOver;
            TextRenderer.DrawText(e.Graphics,
                e.Node.Text, e.Node.NodeFont, new Rectangle(e.Bounds.Left, e.Bounds.Top, NODE_PAINT_LEFT, e.Bounds.Height), e.Node.ForeColor, Color.Transparent,
                TextFormatFlags.VerticalCenter | TextFormatFlags.Left | TextFormatFlags.WordEllipsis | TextFormatFlags.SingleLine | TextFormatFlags.LeftAndRightPadding);

            /* Draw diff bar */
            int leftindent = e.Bounds.Left + NODE_PAINT_LEFT;// + NODE_INDENT_LEFT * e.Node.Level;
            Rectangle bounds = new Rectangle(leftindent, e.Bounds.Top, NODE_TEXT_WIDTH + NODE_DIFF_WIDTH, e.Bounds.Height);
            using (Bitmap bmp = new Bitmap(bounds.Width, bounds.Height))
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.Transparent);

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
                //g.DrawString(
                //    WTComparer.BytesToString(diff),
                //    e.Node.TreeView.Font,
                //    diff > 0 ? difftextpos : (diff < 0 ? difftextneg : difftextnone),
                //    new Rectangle(0, 0, NODE_TEXT_WIDTH, bmp.Height),
                //    new StringFormat()
                //    {
                //        Alignment = StringAlignment.Far
                //    });
                TextRenderer.DrawText(g, WTComparer.BytesToString(diff), e.Node.TreeView.Font,
                    new Rectangle(0, 0, NODE_TEXT_WIDTH, bmp.Height),
                    diff > 0 ? difftextpos : (diff < 0 ? difftextneg : difftextnone),
                    SystemColors.Window,
                    TextFormatFlags.VerticalCenter | TextFormatFlags.Right | TextFormatFlags.SingleLine | TextFormatFlags.LeftAndRightPadding
                );

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

                e.Graphics.CompositingMode = CompositingMode.SourceOver;
                e.Graphics.CompositingQuality = CompositingQuality.HighSpeed;
                e.Graphics.DrawImage(bmp, bounds, 0, 0, bmp.Width, bmp.Height, GraphicsUnit.Pixel, opacityMatrix);
            }
        }
    }
}
