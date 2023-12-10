using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WizTreeCompare
{
    public partial class FormViewer
    {
        void SetupSearch()
        {
            //menuSearch.Paint += MenuSearch_Paint; //This seriously doesn't fucking work? Are you kidding me?
            menuSearch.KeyDown += MenuSearch_KeyDown;
        }

        //private void MenuSearch_Paint(object sender, PaintEventArgs e)
        //{
            
        //}

        private void MenuSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                ApplyFilter(menuSearch.TextBox.Text); //TODO: Apply filter
            }
        }

        private void ApplyFilter(string filter)
        {
            //TODO: Something less stupid than this
            try
            {
                treeMain.Enabled = false;
                treeMain.Nodes.Clear();

                if (string.IsNullOrWhiteSpace(filter))
                    TreeSetFilter(null);
                else
                {
                    HashSet<string> paths = new(64);
                    foreach (var kv in tvstruct)
                    {
                        if (kv.Key.Contains(filter))
                        {
                            /* Add entire chain to whitelist */
                            string[] spl = kv.Key.Split(dirchars);
                            string path = spl[0];
                            paths.Add(path);
                            for (int i = 1; i < spl.Length; i++)
                            {
                                path += dirchars[0] + spl[1];
                                paths.Add(path);
                            }

                            /* Add children */
                            foreach (string child in kv.Value.Keys)
                                paths.Add(Path.Combine(kv.Key, child));
                        }
                        else
                        {
                            var fchildren = kv.Value.Keys.Where(x => x.Contains(filter));
                            if (fchildren.Any())
                            {
                                /* Add entire chain to whitelist */
                                string[] spl = kv.Key.Split(dirchars);
                                string path = spl[0];
                                paths.Add(path);
                                for (int i = 1; i < spl.Length; i++)
                                {
                                    path += dirchars[0] + spl[i];
                                    paths.Add(path);
                                }

                                /* Add children */
                                foreach (string child in fchildren)
                                    paths.Add(Path.Combine(kv.Key, child));
                            }
                        }
                    }
                    TreeSetFilter(paths);
                }

                TreeDiscover("", treeMain.Nodes);
            }
            finally
            {
                treeMain.Enabled = true;
            }
        }
    }
}