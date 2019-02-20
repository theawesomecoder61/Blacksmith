using Blacksmith.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;

// an assortment of helping functions

namespace Blacksmith
{
    public static class Helpers
    {
        public const string TEXTURE_CONVERSION_FORMATS = "Targa|*.tga|Portable Network Graphics|*.png|Tagged Image File Format|*.tif|Joint Photographic Experts Group|*.jpg|All files|*.*";
        private const string SUPPORTED_FILES = @"(.forge|.pck|.png|.txt|.ini|.log)";

        /// <summary>
        /// Returns if a file path is a supported file by Blacksmith.
        /// </summary>
        /// <param name="file">File path</param>
        /// <returns></returns>
        public static bool IsSupportedFile(string file) => Regex.Matches(file, SUPPORTED_FILES).Count > 0;

        /// <summary>
        /// Returns the path to the currently selected tree node
        /// </summary>
        /// <param name="treeView"></param>
        /// <returns></returns>
        public static string GetSelectedPath(TreeView treeView)
        {
            List<string> texts = new List<string>();
            if (treeView.SelectedNode != null)
            {
                TreeNode node = treeView.SelectedNode;
                TreeNode parent = treeView.SelectedNode.Parent;
                while ((parent = node.Parent) != null)
                {
                    node = parent;
                    texts.Add(node.Text);
                }
                texts.Reverse();
                texts.Add(treeView.SelectedNode.Text);
            }
            return string.Join("/", texts);
        }

        /// <summary>
        /// Returns the path to the specified tree node
        /// </summary>
        /// <param name="treeView"></param>
        /// <param name="oNode">Specified tree node</param>
        /// <returns></returns>
        public static string GetSelectedPath(TreeView treeView, TreeNode oNode)
        {
            List<string> texts = new List<string>();
            if (treeView.SelectedNode != null)
            {
                TreeNode node = oNode;
                TreeNode parent = oNode.Parent;
                while ((parent = node.Parent) != null)
                {
                    node = parent;
                    texts.Add(node.Text);
                }
                texts.Reverse();
                texts.Add(oNode.Text);
            }
            return string.Join("/", texts);
        }

        public static int[] ReadInts(BinaryReader reader, int count)
        {
            int[] ints = new int[count];
            for (int i = 0; i < count; i++)
            {
                ints[i] = reader.ReadInt32();
            }
            return ints;
        }

        // source: https://stackoverflow.com/a/4975942
        /// <summary>
        /// Converts bytes to a human-readable amount
        /// </summary>
        /// <param name="byteCount"></param>
        /// <returns></returns>
        public static string BytesToString(long byteCount)
        {
            string[] suf = { "B", "KB", "MB", "GB", "TB" };
            if (byteCount == 0)
                return "0 B";
            long bytes = Math.Abs(byteCount);
            int place = Convert.ToInt32(Math.Floor(Math.Log(bytes, 1024)));
            double num = Math.Round(bytes / Math.Pow(1024, place), 1);
            return (Math.Sign(byteCount) * num).ToString() + " " + suf[place];
        }

        /// <summary>
        /// Returns the offsets of where 0x33 0xAA 0xFB 0x57 0x99 0xFA 0x04 0x10 can be found in a raw data file
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static long[] LocateRawDataIdentifier(BinaryReader reader)
        {
            List<long> offsets = new List<long>();
            long originalPos = reader.BaseStream.Position;

            reader.BaseStream.Position = 0;
            while (reader.BaseStream.Position <= reader.BaseStream.Length - 8)
            {
                if (reader.ReadInt64() == 1154322941026740787) // the aforementioned bytes as a long
                    offsets.Add(reader.BaseStream.Position - 8); // get the offset 8 bytes from here
                else
                    reader.BaseStream.Seek(-7, SeekOrigin.Current); // go back 7 bytes (if it were 8, infinite recursion)
            }

            reader.BaseStream.Position = originalPos;
            return offsets.ToArray();
        }
        
        //

        public struct ResourceLocation
        {
            public ResourceType Type { get; internal set; }
            public long Offset { get; internal set; }
        }

        private static bool IntMatchesAnyResourceType(uint i)
        {
            bool b = false;
            foreach (ResourceType type in Enum.GetValues(typeof(ResourceType)))
            {
                if (ResourceTypeExtensions.GetResourceType(i) == type)
                    b = true;
            }
            return b;
        }

        //

        /// <summary>
        /// Returns the offsets of any supported resource type
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static ResourceLocation[] LocateResourceIdentifiers(BinaryReader reader)
        {
            List<ResourceLocation> locs = new List<ResourceLocation>();
            //long originalPos = reader.BaseStream.Position;
            reader.BaseStream.Position = 0;            

            ResourceType type = ResourceTypeExtensions.GetResourceType(reader.ReadUInt32());
            if (type != ResourceType._NONE)
            {
                locs.Add(new ResourceLocation
                {
                    //Offset = reader.BaseStream.Position, // - 4 // get the offset 4 bytes from here
                    Type = type
                });
            }

            //reader.BaseStream.Position = originalPos;
            return locs.ToArray();
        }

        /// <summary>
        /// Returns the first index of where pattern is located in array with a count
        /// </summary>
        /// <param name="array"></param>
        /// <param name="pattern"></param>
        /// <param name="startIndex"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public static int IndexOfBytes(byte[] array, byte[] pattern, int startIndex, int count)
        {
            int fidx = 0;
            int result = Array.FindIndex(array, startIndex, count, (byte b) => {
                fidx = (b == pattern[fidx]) ? fidx + 1 : 0;
                return fidx == pattern.Length;
            });
            return (result < 0) ? -1 : result - fidx + 1;
        }

        /// <summary>
        /// Attempt to write to a file, if it is not already being accessed
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="data"></param>
        public static bool SafelyWriteBytes(string fileName, byte[] data)
        {
            /*if (false /*File.Exists(fileName)*) // don't worry about this; experimental code
            {
                Stream stream = null;
                try
                {
                    stream = new FileStream(fileName, FileMode.Open, FileAccess.Read);
                }
                catch (IOException)
                {
                    return false;
                }
                finally
                {
                    if (stream != null && stream.Length > 0)
                    {
                        stream.Close();
                        File.WriteAllBytes(fileName, data);
                    }
                }
                return true;
            }
            else
            {*/
                File.WriteAllBytes(fileName, data);
                return true;
            //}
        }

        /// <summary>
        /// Writes data to a fileName located in the temporary path
        /// </summary>
        /// <param name="fileName">File name, no directories</param>
        /// <param name="data"></param>
        private static void WriteToTempFile(string fileName, byte[] data)
        {
            string tempPath = Properties.Settings.Default.tempPath;
            if (!string.IsNullOrEmpty(tempPath))
                SafelyWriteBytes(Path.Combine(tempPath, fileName), data);
        }

        /// <summary>
        /// Writes data to a fileName located wherever or the temporary path
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="data"></param>
        /// <param name="writeToTempFolder"></param>
        public static void WriteToFile(string fileName, byte[] data, bool writeToTempFolder)
        {
            if (writeToTempFolder)
                WriteToTempFile(fileName, data);
            else
                SafelyWriteBytes(fileName, data);
        }

        /// <summary>
        /// Returns the temporary path
        /// </summary>
        /// <returns></returns>
        public static string GetTempPath() => Properties.Settings.Default.tempPath;

        /// <summary>
        /// Returns the temporary path with the fileName
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static string GetTempPath(string fileName) => Path.Combine(Properties.Settings.Default.tempPath, fileName);

        /// <summary>
        /// Writes an entire .dds file to the temporary path using the fileName
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="imageData"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="mipmapCount"></param>
        /// <param name="dxtType"></param>
        /// <param name="completedAction"></param>
        public static void WriteTempDDS(string fileName, byte[] imageData, int width, int height, int mipmapCount, DXT dxtType, Action completedAction)
        {
            Console.WriteLine("DXT: " + dxtType.ToString());

            char[] dxtArr = { 'D', 'X', '\x0', '\x0' };
            int pls = imageData.Length; //Math.Max(1, (width + 3) / 4) * 8
            try
            {
                using (FileStream stream = new FileStream($"{GetTempPath(fileName)}.dds", FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    using (BinaryWriter writer = new BinaryWriter(stream))
                    {
                        foreach (char c in new char[] { 'D', 'D', 'S', ' ' }) // DDS magic
                            writer.Write(c);
                        writer.Write(124); // size of DDS header
                        writer.Write(659463); // flags
                        writer.Write(height); // height
                        writer.Write(width); // width
                        writer.Write(pls); // pitch or linear size
                        writer.Write(0); // depth
                        writer.Write(1); // mipmap count, "1" for now
                        for (int i = 0; i < 11; i++) // reserved
                            writer.Write(0);
                        writer.Write(32); // size of PIXELFORMAT chunk
                        if (dxtType == 0 ? false : (int)dxtType != 7) // flags
                            writer.Write(4);
                        else
                            writer.Write(64);
                        foreach (char c in DXTExtensions.GetDXTAsChars((int)dxtType)) // DXT type/four CC
                            writer.Write(c);
                        for (int n = 0; n < 5; n++) // RGBBitCount, RBitMask, GBitMask, BBitMask, ABitMask
                            writer.Write(0);
                        writer.Write(4198408); // caps
                        for (int i = 0; i < 4; i++) // caps2, caps3, caps4, reserved2
                            writer.Write(0);
                        if (dxtType.ToString().Contains("DX10")) // add the DX10 header, if necessary
                        {
                            string fileNameNoExt = Path.GetFileNameWithoutExtension(fileName);
                            if (fileNameNoExt.Contains("NormalMap") || fileNameNoExt.EndsWith("_Map")) // this stays until I devise a better tactic
                                writer.Write(98); // normal maps
                            else
                                writer.Write(72); // all others use BC1_UNORM_SRGB
                            writer.Write(3); // resourceDimension
                            writer.Write(0); // miscFlags
                            writer.Write(1); // array size
                            writer.Write(0); // miscFlags2
                        }
                        writer.Write(imageData); // image data
                    }
                }
            }
            catch (IOException e)
            {
                MessageBox.Show($"Could not create a dds file due an error:\n{e.Message}", "Failure");
            }

            completedAction();
        }

        /// <summary>
        /// Converts a DDS texture with texconv. Has a callback if you wish to use it.
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="completionAction"></param>
        /// <param name="format"></param>
        public static void ConvertDDS(string fileName, Action<bool> completionAction = null, string format = "png")
        {
            string texconv = string.Concat(Application.StartupPath, "\\Binaries\\x86\\texconv.exe");
            if (File.Exists(texconv))
            {
                string args = $"-ft {format} -f R8G8B8A8_UNORM -m 1 -o \"{GetTempPath()}\" \"{fileName}\"";
                Console.WriteLine("{0} {1}", texconv, args);
                Process p = new Process();
                p.StartInfo.FileName = texconv;
                p.StartInfo.Arguments = args;
                p.EnableRaisingEvents = true;
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.RedirectStandardOutput = true;
                p.Start();

                StreamReader reader = p.StandardOutput;
                string output = reader.ReadToEnd();

                p.Exited += new EventHandler(delegate(object s, EventArgs a)
                {

                    bool error = output.Contains("FAILED (");
                    completionAction?.Invoke(error);
                });

                p.WaitForExit();
            }
            else
                throw new Exception("texconv is not found. Blacksmith needs it to convert the texture.");
        }

        /// <summary>
        /// Converts a .dds texture with texconv. Outputs to outputDir. Has a callback if you wish to use it.
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="outputDir"></param>
        /// <param name="completionAction"></param>
        /// <param name="format"></param>
        public static void ConvertDDS(string fileName, string outputDir, Action completionAction = null, string format = "png")
        {
            string texconv = string.Concat(Application.StartupPath, "\\Binaries\\x86\\texconv.exe");
            if (File.Exists(texconv))
            {
                string args = $"-ft {format} -f R8G8B8A8_UNORM -m 1 -o \"{outputDir}\" \"{fileName}\"";
                Console.WriteLine("{0} {1}", texconv, args);
                Process p = Process.Start(texconv, args);
                p.EnableRaisingEvents = true;
                p.Exited += new EventHandler(delegate (object s, EventArgs a)
                {
                    completionAction?.Invoke();
                });
            }
            else
                throw new Exception("texconv is not found. Blacksmith needs it to convert the texture.");
        }

        /// <summary>
        /// Converts a .wem to .ogg
        /// </summary>
        /// <param name="fileName"></param>
        public static void ConvertWEMToOGG(string fileName)
        {
            string ww2ogg = string.Concat(Application.StartupPath, "\\Binaries\\x86\\ww2ogg.exe");
            string bin = string.Concat(Application.StartupPath, "\\Binaries\\x86\\packed_codebooks_aoTuV_603.bin");
            if (File.Exists(ww2ogg))
            {
                string args = $"--pcb \"{bin}\" \"{fileName}\"";
                Console.WriteLine("{0} {1}", ww2ogg, args);
                Process.Start(ww2ogg, args);
            }
            else
                throw new Exception("ww2ogg is not found. Blacksmith needs it to convert the sound data.");
        }

        /// <summary>
        /// Fixes a .ogg
        /// </summary>
        /// <param name="fileName"></param>
        public static void RevorbOGG(string fileName)
        {
            string revorb = string.Concat(Application.StartupPath, "\\Binaries\\x86\\revorb.exe");
            if (File.Exists(revorb))
            {
                string args = $"\"{fileName}\" \"{fileName.Replace(".ogg", "_c.ogg")}\""; // look below for an explanation
                Console.WriteLine("{0} {1}", revorb, args);
                Process p = Process.Start(revorb, args);
                p.EnableRaisingEvents = true;
                p.Exited += new EventHandler(delegate (object s, EventArgs a)
                {
                    Thread.Sleep(500);
                    File.Delete(fileName);
                    File.Move(fileName.Replace(".ogg", "_c.ogg"), fileName.Replace("_c.ogg", ".ogg")); // rename the converted ogg file (seems redundant, but we're actually "replacing" the raw file with the converted file)
                });
            }
            else
                throw new Exception("revorb is not found. Blacksmith needs it to convert the sound data.");
        }

        /// <summary>
        /// Extracts a soundbank
        /// </summary>
        /// <param name="fileName"></param>
        public static void ExtractBNK(string fileName)
        {
            string bnkextr = string.Concat(Application.StartupPath, "\\Binaries\\x86\\bnkextr.exe");
            if (File.Exists(bnkextr))
            {
                string args = $"\"{fileName}\"";
                Console.WriteLine("{0} {1}", bnkextr, args);

                // create dir for the soundbank
                string dir = Path.Combine(Path.GetDirectoryName(fileName), Path.GetFileNameWithoutExtension(fileName));
                Directory.CreateDirectory(dir);

                // set working dir temporarily
                string workingDir = Directory.GetCurrentDirectory();
                Directory.SetCurrentDirectory(dir);
                Process.Start(bnkextr, args);
                Directory.SetCurrentDirectory(workingDir);
            }
            else
                throw new Exception("revorb is not found. Blacksmith needs it to convert the sound data.");
        }

        /// <summary>
        /// Creates and starts a BackgroundWorker
        /// </summary>
        /// <param name="doWork"></param>
        /// <param name="completed"></param>
        public static void DoBackgroundWork(Action doWork, Action completed)
        {
            BackgroundWorker worker = new BackgroundWorker
            {
                WorkerReportsProgress = true
            };
            worker.DoWork += new DoWorkEventHandler(delegate (object o, DoWorkEventArgs args)
            {
                BackgroundWorker w = (BackgroundWorker)o;
                doWork();
            });
            worker.ProgressChanged += new ProgressChangedEventHandler(delegate (object o, ProgressChangedEventArgs args)
            {});
            worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(delegate (object o, RunWorkerCompletedEventArgs args)
            {
                completed();
            });
            worker.RunWorkerAsync();
        }

        /// <summary>
        /// Creates and starts a BackgroundWorker
        /// </summary>
        /// <param name="doWork"></param>
        /// <param name="progressChanged"></param>
        /// <param name="completed"></param>
        public static void DoBackgroundWork(Action doWork, ProgressChangedEventHandler progressChanged, Action completed)
        {
            BackgroundWorker worker = new BackgroundWorker
            {
                WorkerReportsProgress = true
            };
            worker.DoWork += new DoWorkEventHandler(delegate (object o, DoWorkEventArgs args)
            {
                BackgroundWorker w = (BackgroundWorker)o;
                doWork();
                w.ReportProgress(1);
            });
            worker.ProgressChanged += new ProgressChangedEventHandler(delegate (object o, ProgressChangedEventArgs args)
            { });
            worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(delegate (object o, RunWorkerCompletedEventArgs args)
            {
                completed();
            });
            worker.RunWorkerAsync();
        }

        public static List<Form> GetOpenForms() => Application.OpenForms.Cast<Form>().ToList();
        
        /// <summary>
        /// Rescales an image based on a zoomLevel multiplier
        /// </summary>
        /// <param name="img"></param>
        /// <param name="zoomLevel"></param>
        /// <returns></returns>
        public static Bitmap ZoomImage(Image img, double zoomLevel = 1)
        {
            int newWidth = (int)(img.Width * zoomLevel);
            int newHeight = (int)(img.Height * zoomLevel);
            
            Bitmap b = new Bitmap(newWidth, newHeight, PixelFormat.Format32bppArgb);
            b.SetResolution(img.HorizontalResolution, img.VerticalResolution);
            
            Graphics g = Graphics.FromImage(b);
            g.Clear(Color.Transparent);
            g.InterpolationMode = InterpolationMode.Low;
            g.DrawImage(img, new Rectangle(0, 0, newWidth, newHeight), new Rectangle(0, 0, img.Width, img.Height), GraphicsUnit.Pixel);
            g.Dispose();

            return b;
        }

        /// <summary>
        /// Returns if the magic matches the comparison
        /// </summary>
        /// <param name="magic"></param>
        /// <param name="comparison"></param>
        /// <returns></returns>
        public static bool MagicMatches(byte[] magic, byte[] comparison) => magic[0] == comparison[0] &&
                magic[1] == comparison[1] &&
                magic[2] == comparison[2] &&
                magic[3] == comparison[3];

        /// <summary>
        /// Returns if the magic matches the comparison
        /// </summary>
        /// <param name="magic"></param>
        /// <param name="comparison"></param>
        /// <returns></returns>
        public static bool MagicMatches(char[] magic, char[] comparison) => magic[0] == comparison[0] &&
                magic[1] == comparison[1] &&
                magic[2] == comparison[2] &&
                magic[3] == comparison[3];

        /// <summary>
        /// Counts the occurences of a string within another string
        /// </summary>
        /// <param name="text"></param>
        /// <param name="pattern"></param>
        /// <returns></returns>
        public static int CountStringOccurrences(string text, string pattern)
        {
            int count = 0, i = 0;
            while ((i = text.IndexOf(pattern, i)) != -1)
            {
                i += pattern.Length;
                count++;
            }
            return count;
        }

        /// <summary>
        /// Returns the extension for a Game
        /// </summary>
        /// <param name="game"></param>
        /// <returns></returns>
        public static string ExtensionForGame(Game game) => game == Game.ODYSSEY ? "acod" : (game == Game.ORIGINS ? "acor" : "stp");
        
        /// <summary>
        /// Returns the name of a Game
        /// </summary>
        /// <param name="game"></param>
        /// <returns></returns>
        public static string NameOfGame(Game game) => game == Game.ODYSSEY ? "Assassin's Creed: Odyssey" : (game == Game.ORIGINS ? "Assassin's Creed: Origins" : "Steep");
    }
}