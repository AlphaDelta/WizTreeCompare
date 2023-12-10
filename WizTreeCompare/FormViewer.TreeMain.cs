using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace WizTreeCompare
{
    public partial class FormViewer
    {
        void SetupTreeView()
        {
            treeMain.BeforeExpand += TreeMain_BeforeExpand;
        }

        private void TreeMain_BeforeExpand(object sender, TreeViewCancelEventArgs e)
        {
            TreeDiscover(e.Node.FullPath, e.Node.Nodes);
        }

        const string NODE_PLACEHOLDER = "^placeholder";
        private void TreeDiscover(string path, TreeNodeCollection col)
        {
            if (!col.ContainsKey(NODE_PLACEHOLDER) && path != "") return;

            col.RemoveByKey(NODE_PLACEHOLDER);

            Dictionary<string, long> dir;
            if (!tvstruct.TryGetValue(path, out dir))
                throw new Exception("Node missing in tvstruct");

            var nodes = GetNodes(path, dir);

            int parentsign = Math.Sign(dir.Sum(x => x.Value));
            col.AddRange(
                nodes
                .OrderByDescending(x => Math.Sign(((TreeViewDiff.NodeTag)x.Tag).Size) * parentsign)
                .ThenByDescending(x => Math.Abs(((TreeViewDiff.NodeTag)x.Tag).Size))
                .ToArray()); //TODO: Add option for val vs mag
        }

        private FrozenSet<string> _whitelist = null;
        private void TreeSetFilter(HashSet<string> whitelist)
        {
            this._whitelist = whitelist.ToFrozenSet();
        }

        private List<TreeNode> GetNodes(string path, Dictionary<string, long> dir)
        {
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
                bool whitelisted = _whitelist != null && _whitelist.Contains(Path.Combine(path, kv.Key));
                //if (!whitelisted && _whitelist != null) continue;

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
                    Tag = new TreeViewDiff.NodeTag() { Size = kv.Value, Influence = maxmag > 0 ? (float)kv.Value / (float)maxmag : (float)Math.Sign(kv.Value) },
                    ImageKey = imagekey,
                    SelectedImageKey = imagekey,

                    ForeColor = whitelisted ? Color.OrangeRed : SystemColors.ControlText,
                };

                if (isdir)
                    tn.Nodes.Add(NODE_PLACEHOLDER, "");

                nodes.Add(tn);
            }

            return nodes;
        }
    }
}
