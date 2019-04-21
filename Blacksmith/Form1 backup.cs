using Blacksmith.Enums;
using Blacksmith.FileTypes;
using Blacksmith.Forms;
using Blacksmith.Games;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Linq;
using System.Collections.Generic;
using Blacksmith.Three;
using OpenTK;
using OpenTK.Graphics;

namespace Blacksmith
{
    public partial class Form11 : Form
    {
        private const string FORGE_ENTRY_IDENTIFIER = "|";
        private const string FORGE_SUBENTRY_IDENTIFIER = "~";

        private double[] zoomLevels = new double[]
        {
            .25f, .5f, .75f, 1, 1.5f, 2, 2.5f, 3, 4
        };
        private GLViewer gl;
        private Forge currentForge;
        private EntryTreeNode odysseyNode;
        private EntryTreeNode originsNode;
        private EntryTreeNode steepNode;
        private FindDialog findDialog = null;
        private List<KeyValuePair<Forge.FileEntry, EntryTreeNode>> findResults;
        private string currentImgPath;
        private DateTime launchTime = DateTime.Now;

        // Tag structure:
        // <path to forge>|<entry name>~<subentry name>
        // ex: \path\Assassin's Creed Odyssey\DataPC_PresentDay.forge|ACD_CIN_LaylaChair_DiffuseMap~Mipmap

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

            // load the Utah teapot
            OBJMesh teapot = OBJMesh.LoadFromFile(Application.ExecutablePath + "\\..\\Shaders\\teapot.obj");
            gl.Meshes.Add(teapot);
            Console.WriteLine(teapot.GetVertices().Length);

            // create a timer to constantly render the 3D Viewer
            Timer t = new Timer();
            t.Interval = (int)(1 / 70f * 1000); // 60 FPS
            t.Tick += new EventHandler(delegate (object s, EventArgs a) {
                gl.Render();
            });
            t.Start();

            glControl.Focus();

            // load settings
            LoadSettings();
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
            if (e.Node == null || e.Node.Tag == null)
                return;

            EntryTreeNode node = (EntryTreeNode)e.Node;

            int levels = LevelsDeep(node.FullPath);
            Forge forge = levels == 1 ?
                node.Forge :
                levels == 2 ?
                    ((EntryTreeNode)node.Parent).Forge:
                    ((EntryTreeNode)node.Parent.Parent).Forge;
            Console.WriteLine(levels + " deep");

            string text = node.Text;
            string tag = (string)node.Tag;

            if (string.IsNullOrEmpty(text))
                return;

            // a forge file
            if (tag.EndsWith(".forge"))
            {
                // dump resources used by currentForge
                if (currentForge != null)
                    currentForge.Dump();

                currentForge = new Forge(tag);

                Helpers.DoBackgroundWork(() =>
                {
                    currentForge.Read();
                }, () =>
                {
                    // show message if there are over 10000 files in the forge
                    if (forge.FileEntries.Length > 10000 && MessageBox.Show("This forge file contains more " +
                        "than 10000 entries (" + forge.FileEntries.Length + " exactly). Loading all these " +
                        "entries will take a while and Blacksmith will freeze.\n\nContinue?", "Large forge file!",
                        MessageBoxButtons.YesNo) != DialogResult.Yes)
                        return;

                    // delete the empty tree node
                    e.Node.Nodes.RemoveAt(0);

                    // show and update the progress bar
                    toolStripProgressBar.Visible = true;
                    toolStripProgressBar.Value = 0;
                    toolStripProgressBar.Maximum = forge.FileEntries.Length;

                    Helpers.DoBackgroundWork(() =>
                    {
                        // load entries into this forge's tree node
                        if (!forge.IsFullyRead())
                            return;

                        for (int i = 0; i < forge.FileEntries.Length; i++)
                        {
                            if (forge.FileEntries[i].NameTable == null)
                                continue;

                            string name = forge.FileEntries[i].NameTable.Name;
                            EntryTreeNode n = new EntryTreeNode
                            {
                                Text = name,
                                Forge = forge,
                                Tag = $"{node.Tag}{FORGE_ENTRY_IDENTIFIER}{name}", // set the tag of this file's tree node
                                Offset = forge.FileEntries[i].IndexTable.OffsetToRawDataTable,
                                Size = forge.FileEntries[i].IndexTable.RawDataSize,
                                Game = node.Game
                            };
                            n.Nodes.Add(new EntryTreeNode()); // add empty node (for entry's contents)
                            Invoke(new Action(() =>
                            {
                                if (this != null && toolStripProgressBar.Value < 3001) // I recieved an error stating that the progress bar cannot go above 3001
                                {
                                    node.Nodes.Add(n);
                                    toolStripProgressBar.Value++;
                                }
                            }));
                        }
                    }, () =>
                    {
                        // reset and hide the progress bar
                        toolStripProgressBar.Value = toolStripProgressBar.Maximum = 0;
                        toolStripProgressBar.Visible = false;
                    });
                });
            }
            // forge entry
            else if (tag.Contains(FORGE_ENTRY_IDENTIFIER) && !tag.Contains(FORGE_SUBENTRY_IDENTIFIER))
            {
                // extract, if the entry has an empty node
                if (node.Nodes.Count == 1 && node.Nodes[0].Text == "")
                {
                    BeginMarquee();

                    Helpers.DoBackgroundWork(() =>
                    {
                        byte[] rawData = forge.GetRawData(node.Offset, (int)node.Size);
                        Helpers.WriteToFile($"{text}.ext", rawData, true);

                        if (node.Game == Game.ODYSSEY || node.Game == Game.ORIGINS) // temporary, until this Origins no longer works like Odyssey
                            Odyssey.ReadFile(Helpers.GetTempPath($"{text}.ext"));
                        else
                            Steep.ReadFile(Helpers.GetTempPath($"{text}.ext"));
                    }, () =>
                    {
                        EndMarquee();

                        // remove nodes
                        node.Nodes.Clear();

                        // look for supported resource types. steep stays out, for now (because it causes crashes).
                        string combined = $"{Helpers.GetTempPath(text)}.dec";
                        if (File.Exists(combined))
                        {
                            using (Stream stream = new FileStream(combined, FileMode.Open, FileAccess.Read, FileShare.Read))
                            {
                                using (BinaryReader reader = new BinaryReader(stream))
                                {
                                    // create nodes based on located resource types
                                    foreach (Helpers.ResourceLocation loc in Helpers.LocateResourceIdentifiers(reader))
                                    {
                                        node.Nodes.Add(new EntryTreeNode
                                        {
                                            Text = loc.Type.ToString(),
                                            Tag = $"{tag}{FORGE_SUBENTRY_IDENTIFIER}{loc.Type.ToString()}",
                                            ResourceIdentifier = loc.Type,
                                            Game = node.Game
                                        });
                                    }
                                }
                            }
                        }
                    });
                }
            }
        }

        private void treeView_AfterCollapse(object sender, TreeViewEventArgs e)
        {
            if (e.Node == null || e.Node.Tag == null)
                return;

            EntryTreeNode node = (EntryTreeNode)e.Node;
            string tag = (string)node.Tag;

            // delete files from this forge's tree node
            if (tag.EndsWith(".forge"))
            {
                node.Nodes.Clear();
                node.Nodes.Add(new EntryTreeNode()); // add an empty tree node
            }
            else if (tag.Contains(FORGE_ENTRY_IDENTIFIER))
            {
                //node.Nodes.Clear();
                //node.Nodes.Add(new EntryTreeNode()); // add an empty tree node
            }
        }

        private void treeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (e.Node == null || e.Node.Tag == null)
                return;

            EntryTreeNode node = (EntryTreeNode)e.Node;
            Forge forge = node.Forge;
            TreeNode parent = (EntryTreeNode)node.Parent;
            string text = node.Text;
            string tag = (string)node.Tag;
            long size = node.Size;
            ResourceIdentifier type = node.ResourceIdentifier;

            // reset the picture box and rich text box
            // ToDo: empty 3D viewer
            pictureBox.ImageLocation = "";
            richTextBox.Text = "";

            if (tag.Contains(".forge"))
            {
                if (text.EndsWith(".forge")) // forge file
                {
                    // dump resources used by currentForge
                    if (currentForge != null)
                        currentForge.Dump();

                    currentForge = new Forge(tag);
                    node.Forge = currentForge;
                }
                else // forge entry/subentry
                {
                    // forge subentry
                    if (tag.Contains(FORGE_ENTRY_IDENTIFIER) && tag.Contains(FORGE_SUBENTRY_IDENTIFIER))
                    {
                        if (type == ResourceIdentifier.TEXTURE_MAP)
                        {
                            string parentText = Helpers.GetTempPath(parent.Text);
                            if (node.Game == Game.ODYSSEY || node.Game == Game.ORIGINS)
                            {
                                Odyssey.ExtractTextureMap(parentText, forge, () =>
                                {
                                    if (File.Exists($"{Helpers.GetTempPath(parent.Text)}.png"))
                                        currentImgPath = $"{Helpers.GetTempPath(parent.Text)}.png";
                                    else if (File.Exists($"{Helpers.GetTempPath(parent.Text)}_TopMip_0.png"))
                                        currentImgPath = $"{Helpers.GetTempPath(parent.Text)}_TopMip_0.png";

                                    Invoke(new Action(() =>
                                    {
                                        if (!string.IsNullOrEmpty(currentImgPath))
                                            UpdatePictureBox(Image.FromFile(currentImgPath));

                                        zoomDropDownButton.Text = "Zoom Level: 100%";
                                        tabControl.SelectedIndex = 1;
                                    }));
                                });
                            }
                            else
                            {
                                Steep.ExtractTextureMap(parentText, forge, () =>
                                {
                                    if (File.Exists($"{Helpers.GetTempPath(parent.Text)}.png"))
                                        currentImgPath = $"{Helpers.GetTempPath(parent.Text)}.png";
                                    else if (File.Exists($"{Helpers.GetTempPath(parent.Text)}_Mip0.png"))
                                        currentImgPath = $"{Helpers.GetTempPath(parent.Text)}_Mip0.png";

                                    Invoke(new Action(() =>
                                    {
                                        if (!string.IsNullOrEmpty(currentImgPath))
                                            UpdatePictureBox(Image.FromFile(currentImgPath));

                                        zoomDropDownButton.Text = "Zoom Level: 100%";
                                        tabControl.SelectedIndex = 1;
                                    }));
                                });
                            }
                        }
                    }
                }
            }
            else if (text.EndsWith(".png"))
            {
                pictureBox.Image = Image.FromFile(tag);
                pictureBox.Refresh();

                if (pictureBox.Image != null)
                    imageDimensStatusLabel.Text = $"Dimensions: {pictureBox.Image.Width}x{pictureBox.Image.Height}";

                tabControl.SelectedIndex = 1;
            }
            else if (text.EndsWith(".ini") || text.EndsWith(".txt") || text.EndsWith(".log"))
            {
                richTextBox.Text = string.Join("\n", File.ReadAllLines(tag));
                tabControl.SelectedIndex = 2;
            }
            else if (text.EndsWith(".pck"))
            {
                SoundpackBrowser browser = new SoundpackBrowser();
                browser.LoadPack(tag);
                browser.Show();
            }

            // update path status label
            if (!tag.Contains(FORGE_ENTRY_IDENTIFIER))
                pathStatusLabel.Text = tag;

            // update size status label
            if (size > -1)
                sizeStatusLabel.Text = $"Size: {Helpers.BytesToString(size)}";
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

                string text = node.Text;
                string tag = (string)node.Tag;

                // if the user right-clicked on a game folder node, close the context menu
                if (node == odysseyNode || node == originsNode || node == steepNode ||
                    sNode == odysseyNode || sNode == originsNode || sNode == steepNode)
                    treeNodeContextMenuStrip.Close();

                if (tag.EndsWith(".forge") && node.Nodes.Count > 1) // forge file
                {
                    UpdateContextMenu(false, false, true, false);
                }
                else // forge entry/subentry
                {
                    // forge entry
                    if (tag.Contains(FORGE_ENTRY_IDENTIFIER) && !tag.Contains(FORGE_SUBENTRY_IDENTIFIER))
                        UpdateContextMenu(true, true, false, true);
                    else
                    {
                        if (text == ResourceIdentifier.TEXTURE_MAP.ToString())
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
                    Message.Success("Done");
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
                        Message.Success("Done");
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
                            Message.Success("Created filelist.");
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
                        Message.Success($"Extracted all of {forge.Name}.");
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
            if (Helpers.GetOpenForms().Where((f) => f.Text == "Find").Count() > 0 && findDialog != null)
                findDialog.BringToFront();
            else
            {
                findDialog = new FindDialog();
                findDialog.PrecacheResults += OnPreacacheResults;
                findDialog.FindNext += OnFindNext;
                findDialog.Show();
            }
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
            /*findResults.Clear();
            if (currentForge != null)
            {
                Parallel.ForEach(treeView.Nodes.Find(args.Query, true), (n) =>
                {
                    findResults.Add(new KeyValuePair<Forge.FileEntry, EntryTreeNode>(currentForge.GetFileEntry(n.Text), (EntryTreeNode)n));
                });
            }*/
        }

        private void OnFindNext(object sender, FindEventArgs args)
        {
            //treeView.SelectedNode = findResults[0].Value;
        }
        #endregion

        #region Helpers
        public void LoadGamesIntoTreeView()
        {
            treeView.Nodes.Clear();

            // Odyssey
            odysseyNode = new EntryTreeNode
            {
                Text = "Assassin's Creed: Odyssey",
                Game = Game.ODYSSEY
            };
            treeView.Nodes.Add(odysseyNode);
            if (!string.IsNullOrEmpty(Properties.Settings.Default.odysseyPath))
                PopulateTreeView(Properties.Settings.Default.odysseyPath, odysseyNode);

            // Origins
            originsNode = new EntryTreeNode
            {
                Text = "Assassin's Creed: Origins",
                Game = Game.ORIGINS
            };
            treeView.Nodes.Add(originsNode);
            if (!string.IsNullOrEmpty(Properties.Settings.Default.originsPath))
                PopulateTreeView(Properties.Settings.Default.originsPath, originsNode);

            // Steep
            steepNode = new EntryTreeNode
            {
                Text = "Steep",
                Game = Game.STEEP
            };
            treeView.Nodes.Add(steepNode);
            if (!string.IsNullOrEmpty(Properties.Settings.Default.steepPath))
                PopulateTreeView(Properties.Settings.Default.steepPath, steepNode);
        }

        public void LoadSettings()
        {
            gl.BackgroundColor = Properties.Settings.Default.threeBG;
            gl.RenderMode = Properties.Settings.Default.renderMode == 0 ? RenderMode.SOLID : Properties.Settings.Default.renderMode == 1 ? RenderMode.WIREFRAME : RenderMode.POINTS;
        }

        public void PopulateTreeView(string dir, EntryTreeNode parent)
        {
            foreach (string file in Directory.GetFileSystemEntries(dir))
            {
                FileInfo info = new FileInfo(file);
                EntryTreeNode node = new EntryTreeNode
                {
                    Text = Path.GetFileName(file),
                    Tag = Path.Combine(dir, file), // tree node tags for files contain the file's path,
                    Size = Directory.Exists(file) ? 0 : info.Length, // directories have no size,
                    Game = parent.Game
                };

                if (Directory.Exists(file) || Helpers.IsSupportedFile(Path.GetExtension(file)))
                {
                    // add an empty tree node to forge tree nodes as a placeholder
                    if (Path.GetExtension(file).Equals(".forge"))
                        node.Nodes.Add("");

                    if (parent != null)
                        parent.Nodes.Add(node);
                    else
                        treeView.Nodes.Add(node);
                }

                // recursively call this function (for directories)
                if (Directory.Exists(file))
                    PopulateTreeView(file, node);
            }
        }

        public void BeginMarquee()
        {
            toolStripProgressBar.Visible = true;
            toolStripProgressBar.Style = ProgressBarStyle.Marquee;
        }

        public void EndMarquee()
        {
            toolStripProgressBar.Style = ProgressBarStyle.Blocks;
            toolStripProgressBar.Visible = false;
        }

        public void UpdateContextMenu(bool enableConvert, bool enableDatafile, bool enableForge, bool enableTexture)
        {
            convertToolStripMenuItem.Enabled = enableConvert;
            datafileToolStripMenuItem.Enabled = enableDatafile;
            forgeToolStripMenuItem.Enabled = enableForge;
            textureToolStripMenuItem.Enabled = enableTexture;
        }

        public void ChangeZoom(double zoom)
        {
            if (!string.IsNullOrEmpty(currentImgPath))
            {
                UpdatePictureBox(Helpers.ZoomImage(Image.FromFile(currentImgPath), zoom), true, false);
                pictureBox.Size = pictureBox.Image.Size;
                zoomDropDownButton.Text = $"Zoom Level: {zoom * 100}%";
            }
        }

        public void UpdatePictureBox(Image image, bool reload = true, bool isZoomed = false)
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
            image.RotateFlip(RotateFlipType.Rotate180FlipX);

            pictureBox.Image = image;
            pictureBox.Refresh();
            imageDimensStatusLabel.Text = $"Dimensions: {image.Width}x{image.Height}";
        }

        public int LevelsDeep(string path) => Helpers.CountStringOccurrences(path, "\\");
        #endregion


        // TEMPORARY
        private void decompileLocalizationDataToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                /*using (Stream stream = new FileStream(dialog.FileName, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    /*CompressedLocalizationData l = new CompressedLocalizationData(stream);
                    foreach (var m in l.StringTables)
                    {
                        foreach (var n in m.Entries)
                            Console.WriteLine(n.DecodedString);
                    }*
                    Helpers.DoBackgroundWork(() =>
                    {
                        PCK.LocateWavData(dialog.FileName);
                    }, () =>
                    {
                        Console.WriteLine("Done");
                    });
                }*/
            }
        }
    }
}