using Blacksmith.Compressions;
using Blacksmith.Enums;
using Blacksmith.FileTypes;
using Blacksmith.Games;
using Blacksmith.ThreeD;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.ES20;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

/*
 * Steep fans, I finally figured out Steep's compression. The game uses zstd.
 * 
 * Sincerely, Andrew M. (theawesomecoder61)
 */

namespace Blacksmith
{
    public partial class Form1 : Form
    {
        private const string FORGE_ENTRY_IDENTIFIER = "|";
        private const string FORGE_SUBENTRY_IDENTIFIER = "~";

        private Forge currentForge;
        //private SceneView scene;
        private GameWindow scene;

        private EntryTreeNode odysseyNode;
        private EntryTreeNode originsNode;
        private EntryTreeNode steepNode;

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
            //
            // set up UI
            //

            // hide the progress bar
            toolStripProgressBar.Visible = false;

            // load the games' directories into the tree view
            LoadGamesIntoTreeView();

            // load settings
            LoadSettings();

            /*scene = new SceneView
            {
                Dock = DockStyle.Fill
            };
            tabPage1.Controls.Add(scene);*/

            /*using (SceneWindow scene = new SceneWindow())
            {
                scene.Run(60);
            }*/
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

            EntryTreeNode curNode = (EntryTreeNode)e.Node;
            string text = curNode.Text;
            string tag = (string)curNode.Tag;

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
                    if (currentForge.FileEntries.Length > 10000 && MessageBox.Show("This forge file contains more " +
                        "than 10000 entries (" + currentForge.FileEntries.Length + " exactly). Loading all these " +
                        "entries will take a while and Blacksmith will freeze.\n\nContinue?", "Large forge file!",
                        MessageBoxButtons.YesNo) != DialogResult.Yes)
                        return;

                    // delete the empty tree node
                    e.Node.Nodes.RemoveAt(0);

                    // show and update the progress bar
                    toolStripProgressBar.Visible = true;
                    toolStripProgressBar.Value = 0;
                    toolStripProgressBar.Maximum = currentForge.FileEntries.Length;

                    // load entries into this forge's tree node
                    for (int i = 0; i < currentForge.FileEntries.Length; i++)
                    {
                        string name = currentForge.FileEntries[i].NameTable.Name;
                        EntryTreeNode node = new EntryTreeNode
                        {
                            Text = name,
                            Tag = $"{curNode.Tag}{FORGE_ENTRY_IDENTIFIER}{name}", // set the tag of this file's tree node
                            Size = currentForge.FileEntries[i].IndexTable.RawDataSize,
                            Game = curNode.Game
                        };
                        node.Nodes.Add(new EntryTreeNode()); // add empty node (for entry's contents)
                        curNode.Nodes.Add(node);
                        toolStripProgressBar.Value++;
                    }

                    // reset and hide the progress bar
                    toolStripProgressBar.Value = toolStripProgressBar.Maximum = 0;
                    toolStripProgressBar.Visible = false;
                });
            }
            // forge entry
            else if (tag.Contains(FORGE_ENTRY_IDENTIFIER) && !tag.Contains(FORGE_SUBENTRY_IDENTIFIER))
            {
                // extract, if the entry has an empty node
                if (curNode.Nodes.Count == 1 && curNode.Nodes[0].Text == "")
                {
                    BeginMarquee();

                    Helpers.DoBackgroundWork(() =>
                    {
                        Forge.FileEntry entry = currentForge.GetFileEntry(text);
                        byte[] rawData = currentForge.GetRawData(entry);
                        Helpers.WriteToTempFile(text, rawData);

                        if (curNode.Game == Game.ODYSSEY || curNode.Game == Game.ORIGINS)
                            Odyssey.ReadFile(Helpers.GetTempPath(text));
                        else
                            Steep.ReadFile(Helpers.GetTempPath(text));
                    }, () =>
                    {
                        EndMarquee();

                        // remove nodes
                        curNode.Nodes.Clear();

                        // look for supported resource types. steep stays out, for now (because it causes crashes).
                        string combined = $"{Helpers.GetTempPath(text)}.dec";
                        if (curNode.Game != Game.STEEP)
                        {
                            if (File.Exists(combined))
                            {
                                using (Stream stream = new FileStream(combined, FileMode.Open, FileAccess.Read, FileShare.Read))
                                {
                                    using (BinaryReader reader = new BinaryReader(stream))
                                    {
                                        // create nodes based on located resource types
                                        foreach (Helpers.ResourceLocation loc in Helpers.LocateResourceIdentifiers(reader))
                                        {
                                            curNode.Nodes.Add(new EntryTreeNode
                                            {
                                                Text = loc.Type.ToString(),
                                                Tag = $"{tag}{FORGE_SUBENTRY_IDENTIFIER}{loc.Type.ToString()}",
                                                ResourceType = loc.Type,
                                                Game = curNode.Game
                                            });
                                        }
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
                    UpdateContextMenu(false, false, true);
                }
                else // forge entry/subentry
                {
                    // forge entry
                    if (tag.Contains(FORGE_ENTRY_IDENTIFIER) && !tag.Contains(FORGE_SUBENTRY_IDENTIFIER))
                        UpdateContextMenu(true, true, false);
                    else
                        UpdateContextMenu(false, false, false);
                }
            }
        }

        private void treeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (e.Node == null || e.Node.Tag == null)
                return;

            EntryTreeNode node = (EntryTreeNode)e.Node;
            TreeNode parent = (EntryTreeNode)node.Parent;
            string text = node.Text;
            string tag = (string)node.Tag;
            long size = node.Size;
            ResourceType type = node.ResourceType;

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
                }
                else // forge entry/subentry
                {
                    // forge subentry
                    if (tag.Contains(FORGE_ENTRY_IDENTIFIER) && tag.Contains(FORGE_SUBENTRY_IDENTIFIER))
                    {
                        if (type == ResourceType.TEXTURE_MAP)
                        {
                            string parentText = Helpers.GetTempPath(parent.Text);
                            if (node.Game == Game.ODYSSEY || node.Game == Game.ORIGINS)
                            {
                                Odyssey.ExtractTextureMap(parentText, currentForge, () =>
                                {
                                    if (File.Exists($"{Helpers.GetTempPath(parent.Text)}.png"))
                                        pictureBox.Image = Image.FromFile($"{Helpers.GetTempPath(parent.Text)}.png");
                                    else if (File.Exists($"{Helpers.GetTempPath(parent.Text)}_TopMip_0.png"))
                                        pictureBox.Image = Image.FromFile($"{Helpers.GetTempPath(parent.Text)}_TopMip_0.png");
                                    pictureBox.Refresh();
                                    tabControl.SelectedIndex = 1;

                                    imageDimensStatusLabel.Text = $"Dimensions: {pictureBox.Image.Width}x{pictureBox.Image.Height}";
                                });
                            }
                            else
                            {
                                /*Steep.ExtractTextureMap(parentText, currentForge, new EventHandler(delegate (object s, EventArgs a)
                                {
                                    pictureBox.Image = Image.FromFile($"{Helpers.GetTempPath(parent.Text)}.png");
                                    tabControl.SelectedIndex = 1;
                                }));*/
                            }
                        }
                    }
                }
            }
            else if (text.EndsWith(".png"))
            {
                pictureBox.ImageLocation = tag;
                pictureBox.Refresh();
                imageDimensStatusLabel.Text = $"Dimensions: {pictureBox.Image.Width}x{pictureBox.Image.Height}";

                tabControl.SelectedIndex = 1;
            }
            else if (text.EndsWith(".ini") || text.EndsWith(".txt") || text.EndsWith(".log"))
            {
                richTextBox.Text = string.Join("\n", File.ReadAllLines(tag));
                tabControl.SelectedIndex = 2;
            }

            // update path status label
            pathStatusLabel.Text = tag;

            // update size status label
            if (size > -1)
                sizeStatusLabel.Text = $"Size: {Helpers.BytesToString(size)}";
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
            saveFileDialog.FileName = node.Text;
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                Forge.FileEntry entry = currentForge.GetFileEntry(node.Text);
                byte[] data = currentForge.GetRawData(entry);
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

    
        private void saveDecompressedDataAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (treeView.SelectedNode == null)
                return;

            EntryTreeNode node = (EntryTreeNode)treeView.SelectedNode;
            string text = node.Text;
            byte[] decompressedData = null;

            BeginMarquee();

            Helpers.DoBackgroundWork(() =>
            {
                Forge.FileEntry entry = currentForge.GetFileEntry(text);
                byte[] rawData = currentForge.GetRawData(entry);
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
            if (currentForge != null)
            {
                string filelist = currentForge.CreateFilelist();
                if (filelist.Length > 0)
                {
                    using (SaveFileDialog dialog = new SaveFileDialog())
                    {
                        dialog.FileName = $"{currentForge.Name}-filelist.txt";
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
            if (currentForge != null && currentForge.FileEntries.Length > 0)
            {
                using (FolderBrowserDialog dialog = new FolderBrowserDialog())
                {
                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        BeginMarquee();
                        Parallel.ForEach(currentForge.FileEntries, (fe) =>
                        {
                            string name = fe.NameTable.Name;
                            byte[] data = currentForge.GetRawData(fe);
                            File.WriteAllBytes(Path.Combine(dialog.SelectedPath, name), data);
                        });
                        EndMarquee();
                        MessageBox.Show($"Extracted all of {currentForge.Name}.", "Done");
                    }
                }
            }
        }
        // end Forge
        #endregion

        #region Menus
        #region File
        private void quitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
        #endregion
        #region Tools
        private void decompressFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    using (Stream stream = new FileStream(dialog.FileName, FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        using (BinaryReader reader = new BinaryReader(stream))
                        {
                            long secondRawData = Helpers.LocateRawDataIdentifier(reader)[1];
                            if (secondRawData > 0)
                            {
                                reader.BaseStream.Seek(10, SeekOrigin.Current); // ignore everything until the compression byte
                                byte compression = reader.ReadByte();
                                if (compression == 0x08)
                                {
                                    if (Odyssey.ReadFile(dialog.FileName))
                                        MessageBox.Show("Decompressed file successfully. Check the folder where the compressed file is located.", "Done");
                                }
                                else if (compression == 0x05)
                                {
                                    if (Steep.ReadFile(dialog.FileName))
                                        MessageBox.Show("Decompressed file successfully. Check at the folder where the compressed file is located.", "Done");
                                }
                                else
                                    MessageBox.Show("Unknown compression type.", "Failure");
                            }
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
            MessageBox.Show("Not yet implemented");
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new AboutBox().ShowDialog();
        }
        #endregion
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
            // ToDo: add the clear/bg color of 3D viewer
            imagePanel.BackColor = Properties.Settings.Default.imageBG;
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

        public void UpdateContextMenu(bool enableConvert, bool enableDatafile, bool enableForge/*, bool enableTexture*/)
        {
            convertToolStripMenuItem.Enabled = enableConvert;
            datafileToolStripMenuItem.Enabled = enableDatafile;
            forgeToolStripMenuItem.Enabled = enableForge;
            //textureToolStripMenuItem.Enabled = enableTexture;
        }
        #endregion
    }
}