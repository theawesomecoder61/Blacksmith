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
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Timer = System.Windows.Forms.Timer;

namespace Blacksmith
{
    public partial class Form1 : Form
    {
        private double[] zoomLevels = new double[]
        {
            .25f, .5f, .75f, 1, 1.5f, 2, 2.5f, 3, 4
        };
        private GLViewer gl;
        private EntryTreeNode odysseyNode;
        private EntryTreeNode originsNode;
        private EntryTreeNode steepNode;
        private FindDialog findDialog = null;
        private List<KeyValuePair<Forge.FileEntry, EntryTreeNode>> findResults;
        private string currentImgPath;
        private DateTime launchTime = DateTime.Now;

        public Form1()
        {
            InitializeComponent();
            treeView.MouseDown += (sender, args) => treeView.SelectedNode = treeView.GetNodeAt(args.X, args.Y);
        }

        #region Events
        private void Form1_Load(object sender, EventArgs e)
        {
            findResults = new List<KeyValuePair<Forge.FileEntry, EntryTreeNode>>();

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
            tabPage1.Controls.Add(glControl);
            gl = new GLViewer(glControl);
            gl.BackgroundColor = Properties.Settings.Default.threeBG;
            gl.Init();
            glControl.Focus();

            // create a timer to constantly render the 3D Viewer
            Timer t = new Timer();
            t.Interval = (int)(1 / 70f * 1000); // 60 FPS
            t.Tick += new EventHandler(delegate (object s, EventArgs a) {
                gl.Render();
            });
            t.Start();

            // load settings
            LoadSettings();

            // load the Utah teapot
            OBJMesh teapot = OBJMesh.LoadFromFile(Application.ExecutablePath + "\\..\\Shaders\\teapot.obj");
            gl.Meshes.Add(teapot);
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
                List<EntryTreeNode> nodes = new List<EntryTreeNode>();
                treeView.Enabled = false; // prevent user-induced damages

                // work in the background
                Helpers.DoBackgroundWork(() =>
                {
                    nodes = PopulateForge(node);
                }, () =>
                {
                    node.Nodes.AddRange(nodes.ToArray());
                    node.Nodes[0].Remove(); // remove the placeholder node

                    treeView.Enabled = true;
                    MessageBox.Show($"Loaded the entries from {node.Text}.", "Success");
                });
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

            if (node.Type == EntryTreeNodeType.FORGE)
            {
                node.Nodes.Clear(); // remove the entry nodes
                node.Nodes.Add(new EntryTreeNode()); // add a placeholder child node
            }
            else if (node.Type == EntryTreeNodeType.ENTRY)
            {
                node.Nodes.Clear(); // remove the subentry nodes
                node.Nodes.Add(new EntryTreeNode()); // add a placeholder child node
            }
        }

        private void treeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (e.Node == null)
                return;
            EntryTreeNode node = (EntryTreeNode)e.Node;

            // clear the 3D/Image/Text viewers
            ClearTheViewers();

            if (node.Type == EntryTreeNodeType.ENTRY)
                HandleEntryNode(node);
            else if (node.Type == EntryTreeNodeType.SUBENTRY)
                HandleSubentryNode(node);
            else if (node.Type == EntryTreeNodeType.IMAGE)
                HandleImageNode(node);
            else if (node.Type == EntryTreeNodeType.PCK)
                HandlePCKNode(node);
            else if (node.Type == EntryTreeNodeType.TEXT)
                HandleTextNode(node);

            // update size status label
            if (node.Size > -1)
                sizeStatusLabel.Text = $"Size: {Helpers.BytesToString(node.Size)}";
        }

        private void treeView_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                EntryTreeNode node = (EntryTreeNode)treeView.GetNodeAt(e.Location);
                EntryTreeNode sNode = (EntryTreeNode)treeView.SelectedNode;
                if (node == null || node.Tag == null || sNode == null)
                {
                    treeNodeContextMenuStrip.Close();
                    return;
                }
                
                string tag = (string)node.Tag;

                // if the user right-clicked on a game folder node, close the context menu
                if (node == odysseyNode || node == originsNode || node == steepNode ||
                    sNode == odysseyNode || sNode == originsNode || sNode == steepNode)
                {
                    treeNodeContextMenuStrip.Close();
                }

                if (node.Type == EntryTreeNodeType.FORGE && node.Nodes.Count > 1) // forge
                {
                    UpdateContextMenu(false, false, true, false);
                }
                else // forge entry or subentry
                {
                    if (node.Type == EntryTreeNodeType.ENTRY) // forge entry
                    {
                        UpdateContextMenu(true, true, false, true);
                    }
                    else if (node.Type == EntryTreeNodeType.SUBENTRY) // forge subentry
                    {
                        if (node.ResourceType == ResourceType.TEXTURE_MAP)
                            UpdateContextMenu(false, false, false, true);
                        else
                            UpdateContextMenu(false, false, false, false);
                    }
                }
            }
        }
        #endregion

        #region Context menu
        // Convert
        private void convertToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (treeView.SelectedNode == null)
                return;

            EntryTreeNode node = (EntryTreeNode)treeView.SelectedNode;
            string text = node.Text;

            MessageBox.Show("Expect the conversion feature in Version 1.2.", "Sorry");
        }
        // end Convert

        // Datafile
        private void saveRawDataAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (treeView.SelectedNode == null)
                return;

            EntryTreeNode node = (EntryTreeNode)treeView.SelectedNode;
            Forge forge = node.Forge;
            saveFileDialog.FileName = node.Text;
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
                    MessageBox.Show("Done");
                }
            }
        }

        private void saveDecompressedDataAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (treeView.SelectedNode == null)
                return;

            EntryTreeNode node = (EntryTreeNode)treeView.SelectedNode;
            Forge forge = node.Forge;
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

                saveFileDialog.FileName = $"{node.Text}.dec";
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
                        MessageBox.Show("Done");
                    }
                }
            });
        }
        // end Datafile

        // Forge
        private void createFilelistToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (treeView.SelectedNode == null)
                return;

            EntryTreeNode node = (EntryTreeNode)treeView.SelectedNode;
            Forge forge = node.Forge;

            if (forge != null)
            {
                string filelist = forge.CreateFilelist();
                if (filelist.Length > 0)
                {
                    using (SaveFileDialog dialog = new SaveFileDialog())
                    {
                        dialog.FileName = $"{forge.Name}-filelist.txt";
                        dialog.Filter = "Text files|*.txt|All files|*.*";
                        if (dialog.ShowDialog() == DialogResult.OK)
                        {
                            File.WriteAllText(dialog.FileName, filelist);
                            MessageBox.Show("Created filelist.", "Done");
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
            Forge forge = node.Forge;

            if (forge != null && forge.FileEntries.Length > 0)
            {
                using (FolderBrowserDialog dialog = new FolderBrowserDialog())
                {
                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        BeginMarquee();
                        Parallel.ForEach(forge.FileEntries, (fe) =>
                        {
                            string name = fe.NameTable.Name;
                            byte[] data = forge.GetRawData(fe);
                            File.WriteAllBytes(Path.Combine(dialog.SelectedPath, name), data);
                        });
                        EndMarquee();
                        MessageBox.Show($"Extracted all of {forge.Name}.", "Done");
                    }
                }
            }
        }
        // end Forge

        // Texture
        private void convertToAnotherFormatToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (treeView.SelectedNode == null)
                return;

            EntryTreeNode node = (EntryTreeNode)treeView.SelectedNode.Parent;
            string text = node.Text;
            string tex = "";

            if (File.Exists($"{Helpers.GetTempPath(text)}.dds"))
                tex = $"{Helpers.GetTempPath(text)}.dds";
            else if (File.Exists($"{Helpers.GetTempPath(text)}_TopMip_0.dds"))
                tex = $"{Helpers.GetTempPath(text)}_TopMip_0.dds";
            else if (File.Exists($"{Helpers.GetTempPath(text)}_Mip0.dds"))
                tex = $"{Helpers.GetTempPath(text)}_Mip0.dds";

            if (!string.IsNullOrEmpty(tex))
            {
                saveFileDialog.FileName = Path.GetFileNameWithoutExtension(tex);
                saveFileDialog.Filter = Helpers.TEXTURE_CONVERSION_FORMATS;
                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    Helpers.ConvertDDS(tex, Path.GetDirectoryName(saveFileDialog.FileName), () =>
                    {
                        MessageBox.Show("Converted the texture.", "Success");
                    }, Path.GetExtension(saveFileDialog.FileName).Substring(1));
                }
            }
        }

        // no need to convert, simply copy the file
        private void saveAsDDSToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (treeView.SelectedNode == null)
                return;

            EntryTreeNode node = (EntryTreeNode)treeView.SelectedNode.Parent;
            string text = node.Text;
            string dds = "";

            if (File.Exists($"{Helpers.GetTempPath(text)}.dds"))
                dds = $"{Helpers.GetTempPath(text)}.dds";
            else if (File.Exists($"{Helpers.GetTempPath(text)}_TopMip_0.dds"))
                dds = $"{Helpers.GetTempPath(text)}_TopMip_0.dds";
            else if (File.Exists($"{Helpers.GetTempPath(text)}_Mip0.dds"))
                dds = $"{Helpers.GetTempPath(text)}_Mip0.dds";

            if (!string.IsNullOrEmpty(dds))
            {
                saveFileDialog.FileName = Path.GetFileName(dds);
                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    File.Copy(dds, saveFileDialog.FileName);
                    MessageBox.Show("Saved the texture.", "Success");
                }
            }
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
            /*if (Helpers.GetOpenForms().Where((f) => f.Text == "Find").Count() > 0 && findDialog != null)
                findDialog.BringToFront();
            else
            {
                findDialog = new FindDialog();
                findDialog.PrecacheResults += OnPreacacheResults;
                findDialog.FindNext += OnFindNext;
                findDialog.Show();
            }*/

            MessageBox.Show("Find feature will be ready in Version 1.4", "Sorry");
        }
        #endregion
        #region Tools
        private void decompressFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                saveFileDialog.Filter = "Decompressed Data|*.dec|All files|*.*";
                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    using (Stream stream = new FileStream(openFileDialog.FileName, FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        using (BinaryReader reader = new BinaryReader(stream))
                        {
                            if (Helpers.LocateRawDataIdentifier(reader).Length < 2)
                            {
                                MessageBox.Show("Operation failed.", "Failure");
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
                                    MessageBox.Show("Decompressed file successfully. Check the folder where the compressed file is located.", "Success");
                                else
                                    MessageBox.Show("Unknown compression type.", "Failure");
                            });
                        }
                    }
                }
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
        private void OnPreacacheResults(object sender, FindEventArgs args)
        {
            MessageBox.Show("Find feature will be ready in Version 1.4", "Sorry");
        }

        private void OnFindNext(object sender, FindEventArgs args)
        {
            MessageBox.Show("Find feature will be ready in Version 1.4", "Sorry");
        }
        #endregion

        #region Helpers
        private void HandleEntryNode(EntryTreeNode node)
        {
        }

        private void HandlePCKNode(EntryTreeNode node)
        {
            SoundpackBrowser browser = new SoundpackBrowser();
            browser.LoadPack((string)node.Tag);
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
            if (node.ResourceType == ResourceType.MESH)
            {
                if (node.Game == Game.ODYSSEY)
                {
                }
                else if (node.Game == Game.ORIGINS)
                {
                    Origins.Submesh[] submeshes = Origins.ExtractModel(Helpers.GetTempPath(node.Path), node, () =>
                    {
                        Console.WriteLine("EXTRACTED MODEL");
                    });

                    /*foreach (Origins.Submesh submesh in submeshes)
                    {
                        gl.Meshes.Add(new DynamicMesh(submesh));
                    }*/
                }
                else if (node.Game == Game.STEEP)
                {
                }
            }
            else if (node.ResourceType == ResourceType.TEXTURE_MAP)
            {
                if (node.Game == Game.ODYSSEY || node.Game == Game.ORIGINS)
                {
                    Odyssey.ExtractTextureMap(Helpers.GetTempPath(node.Path), node, () =>
                    {
                        HandleTextureMap(node);
                    });
                }
                else if (node.Game == Game.STEEP)
                {
                    Steep.ExtractTextureMap(Helpers.GetTempPath(node.Path), node, () =>
                    {
                        HandleTextureMap(node);
                    });
                }
            } 
        }

        private void HandleTextureMap(EntryTreeNode node)
        {
            currentImgPath = Helpers.GetTempPath(node.Path) + ".png";
            Invoke(new Action(() => {
                UpdatePictureBox(Image.FromFile(currentImgPath));
                zoomDropDownButton.Text = "Zoom Level: 100%";
                tabControl.SelectedIndex = 1;
            }));
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
                odysseyNode.Tag = Properties.Settings.Default.odysseyPath;
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
                originsNode.Tag = Properties.Settings.Default.originsPath;
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
                steepNode.Tag = Properties.Settings.Default.steepPath;
                PopulateTreeView(Properties.Settings.Default.steepPath, steepNode);
            }
        }
        
        private void LoadSettings()
        {
            gl.BackgroundColor = Properties.Settings.Default.threeBG;
            gl.RenderMode = Properties.Settings.Default.renderMode == 0 ? RenderMode.SOLID : Properties.Settings.Default.renderMode == 1 ? RenderMode.WIREFRAME : RenderMode.POINTS;

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
                    Path = file,
                    Size = Directory.Exists(file) ? 0 : (int)info.Length, // directories have no size
                    Tag = Path.Combine(dir, file), // tree node tags for files contain the file's path
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
                if (forge.GetEntryCount() > 10000)
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
                    Size = entry.IndexTable.RawDataSize,
                    Tag = Path.Combine((string)node.Tag, entry.NameTable.Name), // set the tag of this file's tree node
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
            string file = node.Text;
            if (node.Game == Game.ODYSSEY)
            {
                file += ".acod";
                Helpers.WriteToFile(file, Odyssey.ReadFile(data), true);
            }
            else if (node.Game == Game.ORIGINS)
            {
                file += ".acor";
                Helpers.WriteToFile(file, Odyssey.ReadFile(data), true);
            }
            else if (node.Game == Game.STEEP)
            {
                file += ".stp";
                Helpers.WriteToFile(file, Steep.ReadFile(data), true);
            }

            // path will hold the file name
            node.Path = file;

            // get resource locations and create nodes
            List<EntryTreeNode> nodes = new List<EntryTreeNode>();
            using (Stream stream = new FileStream(Helpers.GetTempPath(file), FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                using (BinaryReader reader = new BinaryReader(stream))
                {
                    Helpers.ResourceLocation[] locations = Helpers.LocateResourceIdentifiers(reader);
                    foreach (Helpers.ResourceLocation location in locations)
                    {
                        nodes.Add(new EntryTreeNode {
                            Game = node.Game,
                            Path = file,
                            ResourceType = location.Type,
                            Text = location.Type.ToString(),
                            Type = EntryTreeNodeType.SUBENTRY
                        });
                    }
                }
            }

            return nodes;
        }

        private void ClearTheViewers()
        {
            //gl.Meshes.Clear();
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
        
        private void UpdateContextMenu(bool enableConvert, bool enableDatafile, bool enableForge, bool enableTexture)
        {
            convertToolStripMenuItem.Enabled = enableConvert;
            datafileToolStripMenuItem.Enabled = enableDatafile;
            forgeToolStripMenuItem.Enabled = enableForge;
            textureToolStripMenuItem.Enabled = enableTexture;
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
            if (image.Height <= pictureBox.Height && !isZoomed)
            {
                pictureBox.SizeMode = PictureBoxSizeMode.CenterImage;
                pictureBox.Dock = DockStyle.Fill;
            }
            else
            {
                pictureBox.SizeMode = PictureBoxSizeMode.AutoSize;
                pictureBox.Dock = DockStyle.None;
            }

            if (!reload)
                return;

            // rotate
            if (flip)
                image.RotateFlip(RotateFlipType.Rotate180FlipX);

            pictureBox.Image = image;
            pictureBox.Refresh();
            imageDimensStatusLabel.Text = $"Dimensions: {image.Width}x{image.Height}";
        }
        #endregion
    }
}