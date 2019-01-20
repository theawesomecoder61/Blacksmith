using Blacksmith.Enums;
using Blacksmith.FileTypes;
using Blacksmith.Games;
using Blacksmith.ThreeD;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.ES20;
using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

/*
 * Steep fans, I myself am one and I understand your ferocity that Steep is not supported yet, though it
 * is referenced numerous times in the code. Until I figure out what the heck Ubisoft Annecy did with
 * these files, Steep support will not be ready.
 * 
 * Steep is Ubisoft Annecy's first big project, and they decided to use a compression no other AnvilNext
 * game used, LZO1C. Ubisoft Montreal and Ubisoft Paris used LZO1X (a much better and more well-known 
 * compression algorithm) for the Assassin's Creed games (save for Origins and Odyssey), Rainbow Six
 * Siege, & Ghost Recon: Wildlands.
 * 
 * I will surely update Blacksmith once I figure out this stupid compression and Steep's file structures.
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

            //
            // two decompression tests
            //

            /*byte[] acOdWhiteTexture = new byte[]
            {
                    0x8C, 0x02, 0x00, 0x6E, 0xFF, 0x0D, 0x17, 0xE9, 0xB7, 0xA2, 0xF3, 0x00,
                    0x00, 0x00, 0x0C, 0x00, 0x00, 0x00, 0x57, 0x68, 0x69, 0x74, 0x65, 0x54, 0x65, 0x78, 0x74, 0x75,
                    0x72, 0x65, 0x00, 0x01, 0x21, 0x00, 0x91, 0x00, 0x22, 0x00, 0xF4, 0x04, 0x00, 0x00, 0x00, 0x84,
                    0xF1, 0x01, 0x9C, 0x00, 0x18, 0x00, 0xF2, 0x01, 0x00, 0xB1, 0xF2, 0x0B, 0x00, 0xD9, 0x01, 0xF8,
                    0x16, 0x00, 0x54, 0x10, 0xB5, 0x52, 0x86, 0x13, 0x00, 0xF1, 0x01, 0x01, 0x15, 0x00, 0x32, 0x00,
                    0x02, 0x2B, 0x00, 0x04, 0xE9, 0x7F, 0x23, 0x13, 0x5D, 0x00, 0xE4, 0x07, 0x00, 0x00, 0x00, 0x6D,
                    0x00, 0xE0, 0x71, 0x00, 0xF0, 0xE9, 0xF5, 0x40, 0x00, 0x00, 0x00, 0xFF, 0xF9, 0x24, 0x09, 0xFF,
                    0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF
            };
            Console.WriteLine(acOdWhiteTexture.Length);
            Oodle.Decompress(acOdWhiteTexture, acOdWhiteTexture.Length, 268);*/
            //Odyssey.ReadFile(@"C:\Users\pinea\Desktop\ACOdyssey datapc\WhiteTexture");

            /*byte[] stpWhiteTexture = new byte[]
            {
                0x28, 0xB5, 0x2F, 0xFD, 0x20, 0xD5, 0x8D, 0x02, 0x00, 0xB4, 0x02, 0x17, 0xE9, 0xB7, 0xA2, 0xBC,
                0x00, 0x00, 0x00, 0x0C, 0x57, 0x68, 0x69, 0x74, 0x65, 0x54, 0x65, 0x78, 0x74, 0x75, 0x72, 0x65,
                0x00, 0x21, 0x00, 0x17, 0xE9, 0xB7, 0xA2, 0x01, 0x04, 0x00, 0x00, 0x00, 0x01, 0x0B, 0x05, 0xE9,
                0x7F, 0x23, 0x13, 0x07, 0x40, 0xFF, 0x10, 0x00, 0x74, 0x25, 0x71, 0xC1, 0xE1, 0x44, 0x28, 0x81,
                0x80, 0x11, 0x10, 0x00, 0x70, 0x9D, 0x0A, 0x04, 0x0C, 0xCC, 0x34, 0x0E, 0x00, 0xBB, 0xC1, 0x56,
                0x2C, 0x6A, 0xC6, 0x75, 0xC3, 0xE1, 0x0E, 0x15, 0x70, 0xE5
            };
            LZO.Decompress(LZO.Algorithm.LZO1C, stpWhiteTexture, 213).ToList().ForEach(x =>
            {
                Console.WriteLine(x.ToString("X2"));
            });*/

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
                            Tag = string.Format("{0}{1}{2}", curNode.Tag, FORGE_ENTRY_IDENTIFIER, name), // set the tag of this file's tree node
                            Size = currentForge.FileEntries[i].IndexTable.RawDataSize
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
                // extract, if the entry has an empty node or if data as a file does not exist
                if (curNode.Nodes.Count == 1 && curNode.Nodes[0].Text == ""
                    && !File.Exists(Helpers.GetTempPath(text)))
                {
                    BeginMarquee();

                    Helpers.DoBackgroundWork(() =>
                    {
                        Forge.FileEntry entry = currentForge.GetFileEntry(text);
                        byte[] rawData = currentForge.GetRawData(entry);
                        Helpers.WriteToTempFile(text, rawData);

                        Odyssey.ReadFile(Helpers.GetTempPath(text));
                    }, () =>
                    {
                        EndMarquee();

                        // remove nodes
                        curNode.Nodes.Clear();

                        string combined = $"{Helpers.GetTempPath(text)}-combined";

                        // look for supported resource types
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
                                        Tag = string.Format("{0}{1}{2}", tag, FORGE_SUBENTRY_IDENTIFIER, loc.Type.ToString()),
                                        ResourceType = loc.Type
                                    });
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
                if (node == null || node.Tag == null)
                    return;

                string text = node.Text;
                string tag = (string)node.Tag;

                if (tag.EndsWith(".forge")) // forge file
                {
                    UpdateContextMenu(false, true);
                }
                else // forge entry/subentry
                {
                    // forge subentry
                    if (tag.Contains(FORGE_ENTRY_IDENTIFIER) && !tag.Contains(FORGE_SUBENTRY_IDENTIFIER))
                    {
                        UpdateContextMenu(true, false);
                    }
                    else
                    {
                        UpdateContextMenu(false, false);
                    }
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
            Console.WriteLine(tag);

            // reset the picture box and rich text box
            // ToDo: empty 3D viewer
            pictureBox.ImageLocation = "";
            richTextBox.Text = "";

            // retrieve the file names the forge
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
                        /*if (type == ResourceType.MIPMAP)
                        {
                            string parentText = Helpers.GetTempPath(parent.Text);
                            string dds = Odyssey.ExtractMipMap(parentText);
                            tabControl.SelectedIndex = 1;

                            //scene.DisplayTexture(dds);
                        }
                        else*/ if (type == ResourceType.TEXTURE_MAP)
                        {
                            string parentText = Helpers.GetTempPath(parent.Text);
                            Odyssey.ExtractTextureMap(parentText, currentForge);
                        }
                        tabControl.SelectedIndex = 1;
                    }
                }
            }
            else if (node.Text.EndsWith(".png"))
            {
                pictureBox.ImageLocation = tag;
                tabControl.SelectedIndex = 1;
            }
            else if (node.Text.EndsWith(".ini") || node.Text.EndsWith(".txt"))
            {
                richTextBox.Text = string.Join("\n", File.ReadAllLines(tag));
                tabControl.SelectedIndex = 2;
            }

            // update path status label
            pathStatusLabel.Text = tag;

            // update size status label
            if (size > -1)
                sizeStatusLabel.Text = "Size: " + Helpers.BytesToString(size);

            // update the status labels for the picture box
            if (pictureBox.Image != null)
                imageDimensStatusLabel.Text = string.Format("Dimensions: {0}x{1}", pictureBox.Image.Width, pictureBox.Image.Height);
        }
        #endregion

        #region Context menu
        // Datafile
        private void saveRawDataAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
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
        }

        private void saveDecompressedDataAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            BeginMarquee();

            EntryTreeNode node = (EntryTreeNode)treeView.SelectedNode;
            string text = node.Text;
            byte[] decompressedData = null;

            Helpers.DoBackgroundWork(() =>
            {
                Forge.FileEntry entry = currentForge.GetFileEntry(text);
                byte[] rawData = currentForge.GetRawData(entry);
                decompressedData = Odyssey.ReadFile(rawData);
            }, () =>
            {
                EndMarquee();

                saveFileDialog.FileName = node.Text;
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

        }

        private void extractAllToolStripMenuItem_Click(object sender, EventArgs e)
        {

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
        #region Settings
        private void settingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Settings settings = new Settings();
            // refresh the tree view when the Settings window is about to close
            settings.FormClosing += new FormClosingEventHandler((object o, FormClosingEventArgs args) =>
            {
                LoadGamesIntoTreeView();
            });
            settings.ShowDialog();
        }
        #endregion
        #region More
        private void sourceCodeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Process.Start("");
        }

        private void helpToolStripMenuItem_Click(object sender, EventArgs e)
        {
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
            EntryTreeNode odysseyNode = new EntryTreeNode
            {
                Text = "Assassin's Creed: Odyssey"
            };
            treeView.Nodes.Add(odysseyNode);
            if (!string.IsNullOrEmpty(Properties.Settings.Default.odysseyPath))
                PopulateTreeView(Properties.Settings.Default.odysseyPath, odysseyNode);

            // Origins
            EntryTreeNode originsNode = new EntryTreeNode
            {
                Text = "Assassin's Creed: Origins"
            };
            treeView.Nodes.Add(originsNode);
            if (!string.IsNullOrEmpty(Properties.Settings.Default.originsPath))
                PopulateTreeView(Properties.Settings.Default.originsPath, originsNode);

            // Steep
            EntryTreeNode steepNode = new EntryTreeNode
            {
                Text = "Steep"
            };
            treeView.Nodes.Add(steepNode);
            if (!string.IsNullOrEmpty(Properties.Settings.Default.steepPath))
                PopulateTreeView(Properties.Settings.Default.steepPath, steepNode);
        }

        public void PopulateTreeView(string dir, TreeNode parent)
        {
            foreach (string file in Directory.GetFileSystemEntries(dir))
            {
                FileInfo info = new FileInfo(file);
                EntryTreeNode node = new EntryTreeNode
                {
                    Text = Path.GetFileName(file),
                    Tag = Path.Combine(dir, file), // tree node tags for files contain the file's path,
                    Size = Directory.Exists(file) ? 0 : info.Length // directories have no size
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

        public void UpdateContextMenu(bool enableDatafile, bool enableForge)
        {
            datafileToolStripMenuItem.Visible = enableDatafile;
            forgeToolStripMenuItem.Visible = enableForge;
        }
        #endregion
    }
}