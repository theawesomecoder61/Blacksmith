using Blacksmith.Enums;
using Blacksmith.FileTypes;
using Blacksmith.Forms;
using Blacksmith.Games;
using Blacksmith.Three;
using OpenTK;
using OpenTK.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using Timer = System.Windows.Forms.Timer;

namespace Blacksmith
{
    public partial class Form1 : Form
    {
        private static Vector3 MODEL_SCALE = new Vector3(.005f, .005f, .005f);

        private double[] zoomLevels = new double[]
        {
            .25f, .5f, .75f, 1, 1.5f, 2, 2.5f, 3, 4
        };
        private GLViewer gl;
        private EntryTreeNode odysseyNode;
        private EntryTreeNode originsNode;
        private EntryTreeNode steepNode;
        private ConvertDialog convertDialog = null;
        private FindDialog findDialog = null;
        private string currentImgPath;
        private bool useAlpha = true;
        private EntryTreeNode nodeToSearchIn = null;

        public Form1()
        {
            InitializeComponent();
            treeView.MouseDown += (sender, args) => treeView.SelectedNode = treeView.GetNodeAt(args.X, args.Y);
        }

        #region Events
        private void Form1_Load(object sender, EventArgs e)
        {
            // hide the progress bar
            toolStripProgressBar.Visible = false;

            // load the games' directories into the tree view
            LoadGamesIntoTreeView();

            // set up the zoom dropdown in the Image viewer
            foreach (double z in zoomLevels)
            {
                ToolStripMenuItem item = new ToolStripMenuItem();
                item.Text = $"{z * 100}%";
                item.Click += new EventHandler(delegate (object s, EventArgs a)
                {
                    ChangeZoom(z);
                });
                zoomDropDownButton.DropDownItems.Add(item);
            }

            // create a GLControl and a GLViewer
            GLControl glControl = new GLControl(new GraphicsMode(32, 24, 0, 4), 3, 0, GraphicsContextFlags.ForwardCompatible);
            glControl.Dock = DockStyle.Fill;
            threeSplitContainer.Panel1.Controls.Add(glControl);
            gl = new GLViewer(glControl);
            gl.BackgroundColor = Properties.Settings.Default.threeBG;
            gl.Init();
            glControl.Focus();

            // a timer to constantly render the 3D Viewer
            Timer t = new Timer();
            t.Interval = (int)(1 / 70f * 1000); // oddly results in 60 FPS
            t.Tick += new EventHandler(delegate (object s, EventArgs a)
            {
                gl.Render();
                cameraStripLabel.Text = $"Camera: {gl.Camera.Position} {gl.Camera.Orientation}";
                if (gl.Model != null)
                {
                    sceneInfoStripLabel.Text = string.Concat("Vertices: ", string.Format("{0:n0}", gl.Model.Meshes.Select(x => x.Vertices.Count).Sum()), " | Faces: ", string.Format("{0:n0}", gl.Model.Meshes.Select(x => x.FaceCount).Sum()), " | Meshes: ", gl.Model.Meshes.Count);
                }
                else
                {
                    sceneInfoStripLabel.Text = "Vertices: 0 | Faces: 0 | Meshes: 0";
                }
            });
            t.Start();

            // load settings
            LoadSettings();

            // load the Utah teapot
            gl.Model = OBJ.LoadFromFile(Application.ExecutablePath + "\\..\\Shaders\\teapot.obj");

            //gl.Model = new Model(new Cube());

            /*gl.TextRenderer.Clear(Color.Red);
            gl.TextRenderer.DrawString("Text", new Font(FontFamily.GenericMonospace, 24), Brushes.White, new PointF(100, 100));*/
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            if (pictureBox.Image != null)
                UpdatePictureBox(pictureBox.Image, false);
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            // delete all files in the temporary path
            if (Properties.Settings.Default.deleteTemp)
            {
                foreach (string f in Directory.GetFiles(Helpers.GetTempPath()))
                    File.Delete(f);
            }
        }

        private void treeView_BeforeExpand(object sender, TreeViewCancelEventArgs e)
        {
            if (e.Node == null)
                return;
            EntryTreeNode node = (EntryTreeNode)e.Node;

            if (node.Type == EntryTreeNodeType.FORGE) // populate the tree with the forge's entries
            {
                List<EntryTreeNode> nodes = null;
                string[] names = null;

                treeView.Enabled = false; // prevent user-induced damages

                // work in the background
                Helpers.DoBackgroundWork(() =>
                {
                    nodes = PopulateForge(node);
                    if (nodes != null)
                        names = nodes.Select(x => x.Text).ToArray();
                }, () =>
                {
                    // prevent a crash
                    if (nodes == null || names == null)
                    {
                        treeView.Enabled = true;
                        return;
                    }

                    node.Nodes.AddRange(nodes.ToArray());
                    node.Nodes[0].Remove(); // remove the placeholder node
                    node.Tag = names;

                    treeView.Enabled = true;
                    MessageBox.Show($"Loaded the entries from {node.Text}.", "Success");
                });

                // update the "Forge to search in" combobox in the Find dialog
                if (findDialog != null && node.GetForge() != null)
                    findDialog.AddOrRemoveForge(node.GetForge());
            }
            else if (node.Type == EntryTreeNodeType.ENTRY) // populate with subentries (resource types)
            {
                List<EntryTreeNode> nodes = new List<EntryTreeNode>();
                treeView.Enabled = false; // prevent user-induced damages

                // work in the background
                Helpers.DoBackgroundWork(() =>
                {
                    nodes = PopulateEntry(node);
                }, () =>
                {
                    // prevent a crash
                    if (nodes == null)
                    {
                        treeView.Enabled = true;
                        return;
                    }

                    node.Nodes.AddRange(nodes.ToArray());
                    node.Nodes[0].Remove(); // remove the placeholder node

                    treeView.Enabled = true;
                    MessageBox.Show($"Loaded the subentries from {node.Text}.", "Success");
                });
            }
        }

        private void treeView_AfterCollapse(object sender, TreeViewEventArgs e)
        {
            if (e.Node == null)
                return;
            EntryTreeNode node = (EntryTreeNode)e.Node;

            BeginMarquee();

            if (node.Type == EntryTreeNodeType.FORGE)
            {
                node.Nodes.Clear(); // remove the entry nodes
                treeView.Refresh();
                node.Nodes.Add(new EntryTreeNode()); // add a placeholder child node

                // update the "Forge to search in" combobox in the Find dialog
                if (findDialog != null && node.GetForge() != null)
                    findDialog.AddOrRemoveForge(node.GetForge());
            }
            else if (node.Type == EntryTreeNodeType.ENTRY)
            {
                node.Nodes.Clear(); // remove the subentry nodes
                treeView.Refresh();
                node.Nodes.Add(new EntryTreeNode()); // add a placeholder child node
            }

            EndMarquee();
        }

        private void treeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (e.Node == null)
                return;
            EntryTreeNode node = (EntryTreeNode)e.Node;

            // clear the 3D/Image/Text Viewers
            ClearTheViewers();

            Console.WriteLine("Resource Type: " + node.ResourceType);

            if (node.Type == EntryTreeNodeType.SUBENTRY) // forge subentry
                HandleSubentryNode(node);
            else if (node.Type == EntryTreeNodeType.IMAGE) // image file
                HandleImageNode(node);
            else if (node.Type == EntryTreeNodeType.PCK) // soundpack
                HandlePCKNode(node);
            else if (node.Type == EntryTreeNodeType.TEXT) // text file
                HandleTextNode(node);

            // focus on the treeview and select again the node
            treeView.Focus();
            treeView.SelectedNode = node;

            // update the path status label
            pathStatusLabel.Text = node.Path;

            // update size status label
            if (node.Size > -1)
                sizeStatusLabel.Text = $"Size: {Helpers.BytesToString(node.Size)}";
        }

        private void treeView_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                EntryTreeNode node = (EntryTreeNode)treeView.GetNodeAt(e.Location); // I have both, in case one holds the true selected node
                EntryTreeNode sNode = (EntryTreeNode)treeView.SelectedNode;
                if (node == null || sNode == null)
                {
                    treeNodeContextMenuStrip.Close();
                    return;
                }

                // if the user right-clicked on a game folder node, close the context menu
                if (node == odysseyNode || node == originsNode || node == steepNode ||
                    sNode == odysseyNode || sNode == originsNode || sNode == steepNode)
                {
                    treeNodeContextMenuStrip.Close();
                }

                // show in explorer item
                showInExplorerToolStripMenuItem.Visible = !string.IsNullOrEmpty(node.Path) && File.Exists(node.Path);

                // filter which menus are visible
                if ((node.Type == EntryTreeNodeType.FORGE && node.Nodes.Count > 1) || (sNode.Type == EntryTreeNodeType.FORGE && sNode.Nodes.Count > 1)) // forge
                {
                    UpdateContextMenu(enableForge: true);
                }
                else if (node.Type == EntryTreeNodeType.ENTRY || sNode.Type == EntryTreeNodeType.ENTRY) // forge entry
                {
                    if (node.Nodes.Count > 0 || sNode.Nodes.Count > 0)
                    {
                        IEnumerable<TreeNode> nodes = node.Nodes.Cast<TreeNode>();
                        IEnumerable<TreeNode> sNodes = sNode.Nodes.Cast<TreeNode>();
                        Console.WriteLine("Nodes: " + string.Join(" ", nodes.Select(x => x.Text).ToArray()));
                        Console.WriteLine("SNodes: " + string.Join(" ", sNodes.Select(x => x.Text).ToArray()));

                        bool hasBuildTable = nodes.Where(x => x.Text == ResourceType.BUILD_TABLE.ToString()).Count() > 0 || sNodes.Where(x => x.Text == ResourceType.BUILD_TABLE.ToString()).Count() > 0;
                        bool hasMesh = nodes.Where(x => x.Text == ResourceType.MESH.ToString()).Count() > 0 || sNodes.Where(x => x.Text == ResourceType.MESH.ToString()).Count() > 0;
                        bool hasTextureMap = nodes.Where(x => x.Text == ResourceType.TEXTURE_MAP.ToString()).Count() > 0 || sNodes.Where(x => x.Text == ResourceType.TEXTURE_MAP.ToString()).Count() > 0;
                            
                        UpdateContextMenu(enableBuildTable: hasBuildTable, enableDatafile: true, enableModel: hasMesh, enableTexture: hasTextureMap);
                    }
                }
                else if (node.Type == EntryTreeNodeType.SUBENTRY || sNode.Type == EntryTreeNodeType.SUBENTRY)
                {
                    UpdateContextMenu(enableBuildTable: node.ResourceType == ResourceType.BUILD_TABLE || sNode.ResourceType == ResourceType.BUILD_TABLE, enableDatafile: true, enableModel: node.ResourceType == ResourceType.MESH || sNode.ResourceType == ResourceType.MESH, enableTexture: node.ResourceType == ResourceType.TEXTURE_MAP || sNode.ResourceType == ResourceType.TEXTURE_MAP);
                }
            }
        }

        // source: https://stackoverflow.com/a/6522741
        private void splitContainer_MouseDown(object sender, MouseEventArgs e)
        {
            ((SplitContainer)sender).IsSplitterFixed = true;
        }

        // source: https://stackoverflow.com/a/6522741
        private void splitContainer_MouseUp(object sender, MouseEventArgs e)
        {
            ((SplitContainer)sender).IsSplitterFixed = false;
        }

        // source: https://stackoverflow.com/a/6522741
        private void splitContainer_MouseMove(object sender, MouseEventArgs e)
        {
            if (((SplitContainer)sender).IsSplitterFixed)
            {
                if (e.Button.Equals(MouseButtons.Left))
                {
                    if (((SplitContainer)sender).Orientation.Equals(Orientation.Vertical))
                    {
                        if (e.X > 0 && e.X < ((SplitContainer)sender).Width)
                        {
                            ((SplitContainer)sender).SplitterDistance = e.X;
                            ((SplitContainer)sender).Refresh();
                        }
                    }
                    else
                    {
                        if (e.Y > 0 && e.Y < ((SplitContainer)sender).Height)
                        {
                            ((SplitContainer)sender).SplitterDistance = e.Y;
                            ((SplitContainer)sender).Refresh();
                        }
                    }
                }
                else
                {
                    ((SplitContainer)sender).IsSplitterFixed = false;
                }
            }
        }

        private void resetCameraStripButton_Click(object sender, EventArgs e)
        {
            gl.ResetCamera();
        }

        private void meshCheckedListBox_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            if (gl.Model != null)
                gl.Model.Meshes[e.Index].IsVisible = e.NewValue == CheckState.Checked;
        }

        private void toggleAlphaButton_Click(object sender, EventArgs e)
        {
            useAlpha = !useAlpha;
            if (useAlpha)
                imagePanel.BackgroundImage = Properties.Resources.grid;
            else
                imagePanel.BackgroundImage = null;
        }
        #endregion

        #region Context menu
        private void copyNameToClipboardToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (treeView.SelectedNode == null)
                return;

            EntryTreeNode node = (EntryTreeNode)treeView.SelectedNode;
            Clipboard.SetText(node.Text);

            MessageBox.Show("Copied the name to the clipboard.", "Success");
        }

        private void showInExplorerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (treeView.SelectedNode == null)
                return;

            EntryTreeNode node = (EntryTreeNode)treeView.SelectedNode;
            string path = node.Path;
            Console.WriteLine("Opening " + path);
            Process.Start("explorer.exe", $"/select, {path}");
        }

        // Build Table
        private void extractAllEntriesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (treeView.SelectedNode == null)
                return;

            EntryTreeNode node = (EntryTreeNode)treeView.SelectedNode.Parent;

        }
        // end Build Table

        // Datafile
        private void saveRawDataAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (treeView.SelectedNode == null)
                return;

            EntryTreeNode node = (EntryTreeNode)treeView.SelectedNode;
            Forge forge = node.GetForge();
            saveFileDialog.FileName = $"{node.Text}.dat";
            saveFileDialog.Filter = "Raw Data|*.dat|All Files|*.*";
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                Forge.FileEntry entry = forge.GetFileEntry(node.Text);
                byte[] data = forge.GetRawData(entry);
                try
                {
                    File.WriteAllBytes(saveFileDialog.FileName, data);
                }
                catch (IOException ee)
                {
                    Console.WriteLine(ee);
                }
                finally
                {
                    MessageBox.Show("Extracted decompressed data.", "Success");
                }
            }
        }

        private void saveDecompressedDataAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (treeView.SelectedNode == null)
                return;

            EntryTreeNode node = (EntryTreeNode)treeView.SelectedNode;
            Forge forge = node.GetForge();
            string text = node.Text;
            byte[] decompressedData = null;

            BeginMarquee();

            Helpers.DoBackgroundWork(() =>
            {
                Forge.FileEntry entry = forge.GetFileEntry(text);
                byte[] rawData = forge.GetRawData(entry);
                decompressedData = Odyssey.ReadFile(rawData);
            }, () =>
            {
                EndMarquee();

                // failure
                if (decompressedData.Length == 0 || decompressedData == null)
                {
                    MessageBox.Show("Could not decompress data.", "Failure");
                    return;
                }

                saveFileDialog.FileName = string.Concat(node.Text, ".", Helpers.GameToExtension(node.Game));
                saveFileDialog.Filter = $"{Helpers.NameOfGame(node.Game)} Data|*.{Helpers.GameToExtension(node.Game)}|All Files|*.*";
                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        File.WriteAllBytes(saveFileDialog.FileName, decompressedData);
                    }
                    catch (IOException ee)
                    {
                        Console.WriteLine(ee);
                    }
                    finally
                    {
                        MessageBox.Show("Saved compressed data.", "Success");
                    }
                }
            });
        }
        
        private void showResourceViewerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (treeView.SelectedNode == null)
                return;

            EntryTreeNode node = (EntryTreeNode)treeView.SelectedNode;
            if (node.Nodes.Count == 0)
                return;

            ResourceViewer viewer = new ResourceViewer();
            viewer.LoadNode(node);
            viewer.Show();
        }
        // end Datafile

        // Forge
        private void createFilelistToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (treeView.SelectedNode == null)
                return;

            EntryTreeNode node = (EntryTreeNode)treeView.SelectedNode;
            Forge forge = node.GetForge();

            if (forge != null)
            {
                string filelist = forge.CreateFilelist();
                if (filelist.Length > 0)
                {
                    using (SaveFileDialog dialog = new SaveFileDialog())
                    {
                        dialog.FileName = $"{forge.Name}-filelist.txt";
                        dialog.Filter = "Text Files|*.txt|All Files|*.*";
                        if (dialog.ShowDialog() == DialogResult.OK)
                        {
                            File.WriteAllText(dialog.FileName, filelist);
                            MessageBox.Show("Created filelist.", "Success");
                        }
                    }
                }
            }
        }

        private void extractAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (treeView.SelectedNode == null)
                return;
            EntryTreeNode node = (EntryTreeNode)treeView.SelectedNode;

            Forge forge = node.GetForge();
            if (forge != null && forge.FileEntries.Length > 0)
            {
                using (FolderBrowserDialog dialog = new FolderBrowserDialog())
                {
                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        Parallel.ForEach(forge.FileEntries, (fe) =>
                        {
                            string name = fe.NameTable.Name;
                            byte[] data = forge.GetRawData(fe);
                            File.WriteAllBytes(Path.Combine(dialog.SelectedPath, name), data);
                        });
                        MessageBox.Show($"Extracted all of {forge.Name}.", "Success");
                    }
                }
            }
        }
        // end Forge

        // Model
        private void saveAsModelToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (treeView.SelectedNode == null || gl.Model == null)
                return;
            EntryTreeNode node = (EntryTreeNode)treeView.SelectedNode;
            node = node.Type == EntryTreeNodeType.SUBENTRY ? (EntryTreeNode)node.Parent : node; // get the entry node, not subentry node
            ShowConvertDialog(0, node, gl.Model);
        }
        // end Model

        // Texture
        private void saveAsTextureToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (treeView.SelectedNode == null)
                return;
            EntryTreeNode node = (EntryTreeNode)treeView.SelectedNode;
            node = node.Type == EntryTreeNodeType.SUBENTRY ? (EntryTreeNode)node.Parent : node; // get the entry node, not subentry node
            ShowConvertDialog(1, node);
        }
        // end Texture
        #endregion

        #region Menus
        #region File
        private void quitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
        #endregion
        #region Find
        private void findToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ShowFindDialog();
        }
        #endregion
        #region Tools
        private void decompressFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                using (Stream stream = new FileStream(openFileDialog.FileName, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    using (BinaryReader reader = new BinaryReader(stream))
                    {
                        if (Helpers.LocateRawDataIdentifier(reader).Length < 2)
                        {
                            MessageBox.Show("Operation failed, due to improper data.", "Failure");
                            return;
                        }

                        BeginMarquee();

                        long secondRawData = Helpers.LocateRawDataIdentifier(reader)[1];
                        reader.BaseStream.Seek(10, SeekOrigin.Current); // ignore everything until the compression byte
                        byte compression = reader.ReadByte();

                        bool success = false;
                        Helpers.DoBackgroundWork(() =>
                        {
                            if (secondRawData > 0)
                            {
                                if (compression == 0x08)
                                    success = Odyssey.ReadFile(openFileDialog.FileName, false, saveFileDialog.FileName);
                                else if (compression == 0x05)
                                    success = Steep.ReadFile(openFileDialog.FileName, false, saveFileDialog.FileName);
                            }
                        }, () =>
                        {
                            EndMarquee();
                            if (success)
                            {
                                saveFileDialog.Filter = "Decompressed Data|*.dec|All Files|*.*";
                                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                                {
                                    MessageBox.Show("Decompressed file successfully. Check the folder where the compressed file is located.", "Success");
                                }
                            }
                            else
                                MessageBox.Show("Unknown compression type.", "Failure");
                        });
                    }
                }
            }
        }

#warning currently hidden (Version 1.6)
        private void showFileInTheViewersToolStripMenuItem_Click(object sender, EventArgs e)
        {
            openFileDialog.Filter = Helpers.DECOMPRESSED_FILE_FORMATS;
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                ResourceType type = Helpers.GetFirstResourceType(openFileDialog.FileName);
                Game game = Helpers.ExtensionToGame(Path.GetExtension(openFileDialog.FileName).Substring(1));
                Console.WriteLine($"Selected file: {openFileDialog.FileName}");
                Console.WriteLine($"Selected file resource type: {type}");
                Console.WriteLine($"Selected file game: {game}");

                BeginMarquee();
                ClearTheViewers(true);

                Helpers.DoBackgroundWork(() =>
                {
                    if (type == ResourceType.MESH)
                    {
                        if (game == Game.ODYSSEY)
                        {
                            gl.Model = Odyssey.ExtractModel(openFileDialog.FileName, (string text) =>
                            {
                                Invoke(new Action(() =>
                                {
                                    tabControl.SelectedIndex = 0;
                                    richTextBox.Text = text;
                                }));
                            });
                        }
                        else if (game == Game.ORIGINS)
                        {
                            gl.Model = Origins.ExtractModel(openFileDialog.FileName, (string text) =>
                            {
                                Invoke(new Action(() =>
                                {
                                    tabControl.SelectedIndex = 0;
                                    richTextBox.Text = text;
                                }));
                            });
                        }
                        else
                        {
                            MessageBox.Show("Not yet supported.", "Failure");
                        }
                    }
                    else if (type == ResourceType.TEXTURE_MAP)
                    {
                        if (game == Game.ODYSSEY || game == Game.ORIGINS)
                        {
                            Odyssey.ExtractTextureMapFromFile(openFileDialog.FileName, (string path) =>
                            {
                                HandleTextureMap(path);
                            });
                        }
                        else if (game == Game.STEEP)
                        {
                            /*Steep.ExtractTextureMap(Helpers.GetTempPath(node.Path), node, (string path) =>
                            {
                                HandleTextureMap(path);
                            });*/
                        }
                    }
                }, () =>
                {
                    EndMarquee();

                    if (type == ResourceType.MESH)
                    {
                        meshCheckedListBox.Items.Clear();
                        for (int i = 0; i < gl.Model.Meshes.Count; i++)
                        {
                            meshCheckedListBox.Items.Add($"Mesh {i}", true);
                        }

                        if (gl.Model != null)
                        {
                            Vector3 center = gl.Model.GetCenter();
                            //float farthestZ = gl.Model.Meshes.Select(x => x.Position.Z).Max();
                            gl.SetCameraResetPosition(Vector3.Add(center, new Vector3(0, 0, 30)));
                            gl.ResetCamera();
                        }
                    }
                });
            }
        }

        private void decompileLocalizationDataToolStripMenuItem_Click(object sender, EventArgs e)
        {
        }
        #endregion
        #region Settings
        private void settingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Settings settings = new Settings();
            // refresh the tree view when the Settings window is about to close
            settings.FormClosing += new FormClosingEventHandler((object o, FormClosingEventArgs args) =>
            {
                // ToDo: add Properties.Settings event handler here, so that I can detect when a game path was changed

                // reload games in the tree view
                LoadGamesIntoTreeView();

                // update settings
                LoadSettings();
            });
            settings.ShowDialog();
        }
        #endregion
        #region More
        private void sourceCodeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Process.Start("https://github.com/theawesomecoder61/Blacksmith");
        }

        private void helpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Process.Start("https://github.com/theawesomecoder61/Blacksmith/wiki/Help");
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new AboutBox().ShowDialog();
        }
        #endregion
        #endregion

        #region Find events
        private void OnFindAll(object sender, FindEventArgs args)
        {
            List<EntryTreeNode> nodeResults = new List<EntryTreeNode>();
            /*nodeToSearchIn = odysseyNode.Nodes.Cast<EntryTreeNode>().ToArray().Where(x => x.IsExpanded && x.Type == EntryTreeNodeType.FORGE).FirstOrDefault() ??
                originsNode.Nodes.Cast<EntryTreeNode>().ToArray().Where(x => x.IsExpanded && x.Type == EntryTreeNodeType.FORGE).FirstOrDefault() ??
                steepNode.Nodes.Cast<EntryTreeNode>().ToArray().Where(x => x.IsExpanded && x.Type == EntryTreeNodeType.FORGE).FirstOrDefault();*/
            nodeToSearchIn = odysseyNode.Nodes.Cast<EntryTreeNode>().Where(x => x.GetForge() == args.ForgeToSearchIn).FirstOrDefault() ?? originsNode.Nodes.Cast<EntryTreeNode>().Where(x => x.GetForge() == args.ForgeToSearchIn).FirstOrDefault() ?? steepNode.Nodes.Cast<EntryTreeNode>().Where(x => x.GetForge() == args.ForgeToSearchIn).FirstOrDefault();
            if (nodeToSearchIn == null)
                return;

            Console.WriteLine("Finding within: " + nodeToSearchIn.Text);
            string[] nameResults = null;

            Helpers.DoBackgroundWork(() =>
            {
                Invoke(new Action(() =>
                {
                    try
                    {
                        nameResults = ((string[])nodeToSearchIn.Tag).Where(x =>
                            (args.Type == FindType.NORMAL && args.CaseSensitive && x.Contains(args.Query)) ||
                            (args.Type == FindType.NORMAL && !args.CaseSensitive && x.ToLower().Contains(args.Query.ToLower())) ||
                            (args.Type == FindType.REGEX && args.CaseSensitive && Regex.IsMatch(x, args.Query)) ||
                            (args.Type == FindType.REGEX && !args.CaseSensitive && Regex.IsMatch(x.ToLower(), args.Query.ToLower())) ||
                            (args.Type == FindType.WILDCARD && args.CaseSensitive && Regex.IsMatch(x, Helpers.WildcardToRegEx(args.Query))) ||
                            (args.Type == FindType.WILDCARD && !args.CaseSensitive && Regex.IsMatch(x.ToLower(), Helpers.WildcardToRegEx(args.Query.ToLower())))
                        ).ToArray();
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show(e.Message + e.StackTrace, "Failure");
                    }
                    finally
                    {
                        if (nameResults != null)
                        {
                            foreach (EntryTreeNode n1 in nodeToSearchIn.Nodes)
                            {
                                foreach (string n2 in nameResults)
                                {
                                    if (n1.Text.Equals(n2))
                                        nodeResults.Add(n1);
                                }
                            }
                        }
                    }
                }));
            }, () =>
            {
                Console.WriteLine("Results: " + nodeResults.Count);
                findDialog.LoadResults(nodeResults);
            });
        }

        private void OnShowInList(object sender, ShowInListArgs args)
        {
            BringToFront();
            Focus();

            if (nodeToSearchIn == null)
                return;

            TreeNode[] results = nodeToSearchIn.Nodes.Cast<TreeNode>().Where(x => x.Text.Equals(args.Name, StringComparison.InvariantCultureIgnoreCase)).ToArray();
            Console.WriteLine(">> Query: " + args.Name);
            Console.WriteLine(">> Results: " + results.Length);
            if (results == null || results.Length == 0)
                return;

            treeView.SelectedNode = results[0];
            results[0].EnsureVisible();
        }
        #endregion

        #region Helpers
        private void HandlePCKNode(EntryTreeNode node)
        {
            SoundpackBrowser browser = new SoundpackBrowser();
            browser.LoadPack(node.Path);
            browser.Show();
        }

        private void HandleImageNode(EntryTreeNode node)
        {
            currentImgPath = node.Path;
            pictureBox.Image = Image.FromFile(currentImgPath);
            tabControl.SelectedIndex = 1;
        }

        private void HandleTextNode(EntryTreeNode node)
        {
            richTextBox.Text = File.ReadAllText(node.Path);
            tabControl.SelectedIndex = 2;
        }

        private void HandleSubentryNode(EntryTreeNode node)
        {
            ClearTheViewers(true);

            switch (node.ResourceType)
            {
                case ResourceType.MESH: // meshes/models                    
                    Helpers.DoBackgroundWork(() =>
                    {
                        if (node.Game == Game.ODYSSEY)
                        {
                            gl.Model = Odyssey.ExtractModel(node.Path, (string text) =>
                            {
                                Invoke(new Action(() =>
                                {
                                    tabControl.SelectedIndex = 0;
                                    richTextBox.Text = text;
                                }));
                            });
                        }
                        else if (node.Game == Game.ORIGINS)
                        {
                            gl.Model = Origins.ExtractModel(node.Path, (string text) =>
                            {
                                Invoke(new Action(() =>
                                {
                                    tabControl.SelectedIndex = 0;
                                    richTextBox.Text = text;
                                }));
                            });
                        }
                        else
                        {
                            MessageBox.Show("Not yet supported.", "Failure");
                        }
                    }, () =>
                    {
                        meshCheckedListBox.Items.Clear();
                        for (int i = 0; i < gl.Model.Meshes.Count; i++)
                        {
                            meshCheckedListBox.Items.Add($"Mesh {i}", true);
                        }

                        if (gl.Model != null)
                        {
                            gl.Model.Meshes.ForEach(x => x.Scale = MODEL_SCALE);

                            Vector3 center = gl.Model.GetCenter();
                            //float farthestZ = gl.Model.Meshes.Select(x => x.Position.Z).Max();
                            gl.SetCameraResetPosition(Vector3.Add(center, new Vector3(0, 0, 30)));
                            gl.ResetCamera();
                        }
                    });
                    break;
                case ResourceType.TEXTURE_MAP: // texture maps
                    if (node.Game == Game.ODYSSEY || node.Game == Game.ORIGINS)
                    {
                        Odyssey.ExtractTextureMap(node.Path, node, (string path) =>
                        {
                            HandleTextureMap(path, node);
                        });
                    }
                    else if (node.Game == Game.STEEP)
                    {
                        Steep.ExtractTextureMap(node.Path, node, (string path) =>
                        {
                            HandleTextureMap(path, node);
                        });
                    }
                    break;
            } 
        }

        private void HandleTextureMap(string texturePath, EntryTreeNode node = null)
        {
            if (texturePath == "FAILED")
            {
#warning remove once texconv works again
                //MessageBox.Show("Texture failed to convert to PNG. Alternatively, you can right-click the TEXTURE_MAP node, select \"Texture\">\"Save As DDS\".", "Failure");
                if (MessageBox.Show("Texture conversion failed. Show the .dds file in the associated program?", "Failure", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    if (node != null)
                    {
                        node = (EntryTreeNode)node.Parent;
                        string tex = "";
                        if (File.Exists($"{Helpers.GetTempPath(node.Text)}.dds"))
                            tex = $"{Helpers.GetTempPath(node.Text)}.dds";
                        else if (File.Exists($"{Helpers.GetTempPath(node.Text)}_TopMip_0.dds"))
                            tex = $"{Helpers.GetTempPath(node.Text)}_TopMip_0.dds";
                        else if (File.Exists($"{Helpers.GetTempPath(node.Text)}_Mip0.dds"))
                            tex = $"{Helpers.GetTempPath(node.Text)}_Mip0.dds";
                        if (tex != "")
                            Process.Start(tex.Trim());
                    }
                }
                return;
            }

            currentImgPath = texturePath;
            Console.WriteLine(">> CURRENT IMAGE PATH: " + currentImgPath);
            if (!Disposing || !IsDisposed)
            {
                Invoke(new Action(() =>
                {
                    UpdatePictureBox(Image.FromFile(currentImgPath));
                    zoomDropDownButton.Text = "Zoom Level: 100%";
                    tabControl.SelectedIndex = 1;
                }));
            }
        }

        private void LoadGamesIntoTreeView()
        {
            treeView.Nodes.Clear();

            // Odyssey
            odysseyNode = new EntryTreeNode
            {
                Game = Game.ODYSSEY,
                Text = "Assassin's Creed: Odyssey"
            };
            treeView.Nodes.Add(odysseyNode);
            if (!string.IsNullOrEmpty(Properties.Settings.Default.odysseyPath))
            {
                odysseyNode.Path = Properties.Settings.Default.odysseyPath;
                PopulateTreeView(Properties.Settings.Default.odysseyPath, odysseyNode);
            }

            // Origins
            originsNode = new EntryTreeNode
            {
                Game = Game.ORIGINS,
                Text = "Assassin's Creed: Origins"
            };
            treeView.Nodes.Add(originsNode);
            if (!string.IsNullOrEmpty(Properties.Settings.Default.originsPath))
            {
                originsNode.Path = Properties.Settings.Default.originsPath;
                PopulateTreeView(Properties.Settings.Default.originsPath, originsNode);
            }

            // Steep
            steepNode = new EntryTreeNode
            {
                Game = Game.STEEP,
                Text = "Steep"
            };
            treeView.Nodes.Add(steepNode);
            if (!string.IsNullOrEmpty(Properties.Settings.Default.steepPath))
            {
                steepNode.Path = Properties.Settings.Default.steepPath;
                PopulateTreeView(Properties.Settings.Default.steepPath, steepNode);
            }
        }
        
        private void LoadSettings()
        {
            gl.BackgroundColor = Properties.Settings.Default.threeBG;
            gl.RenderMode = Properties.Settings.Default.renderMode == 0 ? RenderMode.SOLID : Properties.Settings.Default.renderMode == 1 ? RenderMode.WIREFRAME : RenderMode.POINTS;
            gl.PointSize = Properties.Settings.Default.pointSize;

            // create a temporary directory, if the user forgot
            if (string.IsNullOrEmpty(Properties.Settings.Default.tempPath))
            {
                string dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Blacksmith");
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                    Properties.Settings.Default.tempPath = dir;
                }
            }
        }
        
        private void PopulateTreeView(string dir, EntryTreeNode parent)
        {
            foreach (string file in Directory.GetFileSystemEntries(dir))
            {
                FileInfo info = new FileInfo(file);
                EntryTreeNode node = new EntryTreeNode
                {
                    Game = parent.Game,
                    Path = Path.Combine(dir, file),
                    Size = Directory.Exists(file) ? 0 : info.Length, // directories have no size
                    Text = Path.GetFileName(file)                    
                };

                if (Directory.Exists(file) || Helpers.IsSupportedFile(Path.GetExtension(file)))
                {
                    // deal with each supported file type
                    if (Helpers.IsSupportedFile(Path.GetExtension(file)))
                    {
                        if (Path.GetExtension(file).Equals(".forge")) // forge
                        {
                            node.Forge = new Forge(file);
                            node.Forge.Game = node.Game;
                            node.Type = EntryTreeNodeType.FORGE;
                            node.Nodes.Add(new EntryTreeNode()); // used as a placeholder, will be removed later
                        }
                        else if (Path.GetExtension(file).Equals(".pck")) // pck
                            node.Type = EntryTreeNodeType.PCK;
                        else if (Path.GetExtension(file).Equals(".png")) // image
                            node.Type = EntryTreeNodeType.IMAGE;
                        else if (Regex.Matches(file, @"(.txt|.ini|.log)").Count > 0) // text
                            node.Type = EntryTreeNodeType.TEXT;
                    }

                    if (parent != null)
                        parent.Nodes.Add(node);
                    else
                        treeView.Nodes.Add(node);
                }

                // recursively call this function (for directories)
                if (Directory.Exists(file))
                {
                    node.Type = EntryTreeNodeType.DIRECTORY;
                    PopulateTreeView(file, node);
                }
            }
        }
        
        private List<EntryTreeNode> PopulateForge(EntryTreeNode node)
        {
            Forge forge = node.Forge;
            if (forge == null)
                return null;

            // read if only the forge has not been read
            if (!forge.IsFullyRead())
            {
                if (forge.GetEntryCount() > 20000)
                {
                    string entries = string.Format("{0:n0}", forge.GetEntryCount());
                    if (MessageBox.Show($"This .forge contains more than 20,000 entries ({entries} exactly). Blacksmith may freeze while loading or it may not load them at all.\nDo this at your own risk.", "Warning", MessageBoxButtons.YesNo) == DialogResult.Yes)
                        forge.Read();
                    else
                        return null;
                }
                else
                    forge.Read();
                node.Forge = forge; // set again for assurance
            }

            // populate the forge tree with its entries
            List<EntryTreeNode> entryNodes = new List<EntryTreeNode>();
            foreach (Forge.FileEntry entry in forge.FileEntries)
            {
                EntryTreeNode n = new EntryTreeNode
                {
                    Game = node.Game,
                    Offset = entry.IndexTable.OffsetToRawDataTable,
                    Path = Path.Combine(node.Path, entry.NameTable.Name),
                    Size = entry.IndexTable.RawDataSize,
                    Text = entry.NameTable.Name,
                    Type = EntryTreeNodeType.ENTRY
                };

                n.Nodes.Add(new EntryTreeNode(""));
                entryNodes.Add(n);
            }

            return entryNodes;
        }

        private List<EntryTreeNode> PopulateEntry(EntryTreeNode node)
        {
            // extract the contents from the forge
            Forge forge = node.GetForge();
            byte[] data = forge.GetRawData(node.Offset, node.Size);

            // decompress
            string file = $"{node.Text}.{Helpers.GameToExtension(node.Game)}";
            if (node.Game == Game.ODYSSEY)
                Helpers.WriteToFile(file, Odyssey.ReadFile(data), true);
            else if (node.Game == Game.ORIGINS)
                Helpers.WriteToFile(file, Odyssey.ReadFile(data), true);
            else if (node.Game == Game.STEEP)
                Helpers.WriteToFile(file, Steep.ReadFile(data), true);

            // path will hold the file name
            node.Path = Helpers.GetTempPath(file);

            // get resource locations and create nodes
            List<EntryTreeNode> nodes = new List<EntryTreeNode>();
            using (Stream stream = new FileStream(Helpers.GetTempPath(file), FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                using (BinaryReader reader = new BinaryReader(stream))
                {
                    Helpers.ResourceLocation[] locations = Helpers.LocateResourceIdentifiers(reader);
                    foreach (Helpers.ResourceLocation location in locations)
                    {
                        if (Helpers.IMPORTANT_RESOURCE_TYPES.Contains(location.Type))
                        {
                            nodes.Add(new EntryTreeNode
                            {
                                Game = node.Game,
                                ImageIndex = GetImageIndex(location.Type),
                                Path = Helpers.GetTempPath(file),
                                ResourceOffset = location.Offset,
                                ResourceType = location.Type,
                                Text = location.Type.ToString(),
                                Type = EntryTreeNodeType.SUBENTRY
                            });
                        }
                    }
                }
            }

            return nodes;
        }

        private void ClearTheViewers(bool clear3D = false)
        {
            if (clear3D)
                gl.Model = null;
            pictureBox.Image = null;
            richTextBox.Text = "";
        }

        private void BeginMarquee()
        {
            toolStripProgressBar.Visible = true;
            toolStripProgressBar.Style = ProgressBarStyle.Marquee;
        }
        
        private void EndMarquee()
        {
            toolStripProgressBar.Style = ProgressBarStyle.Blocks;
            toolStripProgressBar.Visible = false;
        }
        
        private void UpdateContextMenu(bool enableBuildTable = false, bool enableDatafile = false, bool enableForge = false, bool enableModel = false, bool enableTexture = false)
        {
            buildTableToolStripMenuItem.Visible = enableBuildTable;
            datafileToolStripMenuItem.Visible = enableDatafile;
            forgeToolStripMenuItem.Visible = enableForge;
            modelToolStripMenuItem.Visible = enableModel;
            textureToolStripMenuItem.Visible = enableTexture;
        }
        
        private void ChangeZoom(double zoom)
        {
            if (!string.IsNullOrEmpty(currentImgPath))
            {
                UpdatePictureBox(Helpers.ZoomImage(Image.FromFile(currentImgPath), zoom), true, false);
                pictureBox.Size = pictureBox.Image.Size;
                zoomDropDownButton.Text = $"Zoom Level: {zoom * 100}%";
            }
        }
        
        private void UpdatePictureBox(Image image, bool reload = true, bool flip = true, bool isZoomed = false)
        {
            if (image.Height <= pictureBox.ClientSize.Height && !isZoomed)
            {
                pictureBox.SizeMode = PictureBoxSizeMode.CenterImage;
                pictureBox.Dock = DockStyle.Fill;
            }
            else
            {
                pictureBox.SizeMode = PictureBoxSizeMode.AutoSize;
                pictureBox.Dock = DockStyle.None;
            }

            // reload, if true, causes the image to refresh and dimensions label to update
            if (!reload)
                return;

            // rotate
            if (flip)
                image.RotateFlip(RotateFlipType.Rotate180FlipX);

            pictureBox.Image = image;
            pictureBox.Refresh();
            imageDimensStatusLabel.Text = $"Dimensions: {image.Width}x{image.Height}";
        }

        private int GetImageIndex(ResourceType type)
        {
            if (type == ResourceType.COMRPESSED_LOCALIZATION_DATA)
                return 25;
            else if (type == ResourceType.LOCALIZATION_MANAGER)
                return 6;
            else if (type == ResourceType.LOCALIZATION_PACKAGE)
                return 15;
            else if (type == ResourceType.MATERIAL)
                return 16;
            else if (type == ResourceType.MESH)
                return 4;
            else if (type == ResourceType.MIPMAP)
                return 17;
            else if (type == ResourceType.TEXTURE_MAP)
                return 13;
            else
                return 0;
        }

        private void ShowConvertDialog(int selectedTab, EntryTreeNode node, Model model = null)
        {
            if (Helpers.GetOpenForms().Where((f) => f.Text == "Convert To...").Count() > 0 && convertDialog != null)
            {
                convertDialog.SetValues(selectedTab, node, model);
                convertDialog.BringToFront();
                convertDialog.Focus();
            }
            else
            {
                convertDialog = new ConvertDialog();
                convertDialog.Location = new Point(Location.X + convertDialog.Size.Width, Location.Y + convertDialog.Size.Height);
                convertDialog.SetValues(selectedTab, node, model);
                convertDialog.FormClosing += new FormClosingEventHandler(delegate (object o, FormClosingEventArgs args)
                {
                    convertDialog = null;
                });
                convertDialog.ShowDialog();
            }
        }

        private void ShowFindDialog()
        {
            if (Helpers.GetOpenForms().Where((f) => f.Text == "Find").Count() > 0 && findDialog != null)
            {
                findDialog.BringToFront();
                findDialog.Focus();
            }
            else
            {
                findDialog = new FindDialog();
                findDialog.Location = new Point(Location.X + findDialog.Size.Width, Location.Y + findDialog.Size.Height);
                findDialog.FormClosing += new FormClosingEventHandler(delegate (object o, FormClosingEventArgs args)
                {
                    findDialog = null;
                });
                findDialog.FindAll += OnFindAll;
                findDialog.ShowInList += OnShowInList;

                // add all expanded forge nodes
                odysseyNode.Nodes.Cast<EntryTreeNode>().Where(x => x.IsExpanded && x.Type == EntryTreeNodeType.FORGE).ToList().ForEach(x =>
                {
                    if (x.GetForge() != null)
                        findDialog.AddOrRemoveForge(x.GetForge());
                });
                originsNode.Nodes.Cast<EntryTreeNode>().Where(x => x.IsExpanded && x.Type == EntryTreeNodeType.FORGE).ToList().ForEach(x =>
                {
                    if (x.GetForge() != null)
                        findDialog.AddOrRemoveForge(x.GetForge());
                });
                steepNode.Nodes.Cast<EntryTreeNode>().Where(x => x.IsExpanded && x.Type == EntryTreeNodeType.FORGE).ToList().ForEach(x =>
                {
                    if (x.GetForge() != null)
                        findDialog.AddOrRemoveForge(x.GetForge());
                });

                findDialog.Show();
            }
        }
        #endregion

        
        private void export3DViewerDataToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (gl.Model != null && gl.Model.Meshes.Count > 0)
            {
                saveFileDialog.FileName = "fountain";
                saveFileDialog.Filter = "obj|*.obj|All Files|*.*";
                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    /*string[] smd = SMD.GenerateModelFromOBJ(objModels);
                    for (int i = 0; i < smd.Length; i++)
                    {
                        File.WriteAllText(Path.Combine(Path.GetDirectoryName(saveFileDialog.FileName), Path.GetFileNameWithoutExtension(saveFileDialog.FileName) + i + ".smd"), smd[i]);
                    }*/

                    File.WriteAllText(saveFileDialog.FileName, OBJ.Export(gl.Model));
                    //File.WriteAllText(saveFileDialog.FileName + ".smd", SMD.Export(gl.Model)[0]);
                    STL.ExportBinary(saveFileDialog.FileName, gl.Model);

                    MessageBox.Show("Done.", "Success");
                }
            }
        }


    }
}