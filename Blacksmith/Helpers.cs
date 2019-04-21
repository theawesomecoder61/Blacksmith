using Blacksmith.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Globalization;
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
        public const string DECOMPRESSED_FILE_FORMATS = "Assassin's Creed: Origins decompressed file|*.acor|Assassin's Creed: Odyssey decompressed file|*.acod|Steep decompressed file|*.stp|All files|*.*";
        public const string MODEL_CONVERSION_FORMATS = "Collada|*.dae|Valve SMD|*.smd|STL|*.stl|Wavefront OBJ|*.obj|All files|*.*";
        private const string SUPPORTED_FILES = @"(.forge|.pck|.png|.txt|.ini|.log)";
        public const string TEXTURE_CONVERSION_FORMATS = "Targa|*.tga|Portable Network Graphics|*.png|Tagged Image File Format|*.tif|Joint Photographic Experts Group|*.jpg|All files|*.*";

        public struct ResourceLocation
        {
            public long Offset { get; internal set; }
            public ResourceIdentifier Type { get; internal set; }
        }

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
        /// Returns the offsets of where 0x1004FA9957FBAA33 can be found in a raw data file
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
        
        /// <summary>
        /// Returns the offsets of any located ResourceIdentifier
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static ResourceLocation[] LocateResourceIdentifiers(BinaryReader reader)
        {
            List<ResourceLocation> locs = new List<ResourceLocation>();
            reader.BaseStream.Seek(0, SeekOrigin.Begin);

            // read the entire stream if the data is under 2 MB
            if (reader.BaseStream.Length < 2 * 1024 * 1024)
            {
                byte[] allData = reader.ReadBytes((int)reader.BaseStream.Length);

                // find all supported ResourceIdentifiers
                foreach (ResourceIdentifier type in Enum.GetValues(typeof(ResourceIdentifier)))
                {
                    byte[] typeAsBytes = BitConverter.GetBytes((uint)type);
                    foreach (Tuple<int, int> tuple in RecurringIndexes(allData, typeAsBytes, 4))
                    {
                        locs.Add(new ResourceLocation
                        {
                            Offset = tuple.Item1,
                            Type = type
                        });
                    }
                }

                return locs.ToArray();
            }
            else // return the first Resource Type
            {
                return new ResourceLocation[]
                {
                    new ResourceLocation
                    {
                        Offset = 0,
                        Type = (ResourceIdentifier)reader.ReadUInt32()
                    }
               };
            }
        }

        /// <summary>
        /// Returns the offsets of where find can be found. The long holds the original position of the BinaryReader.
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="find"></param>
        /// <returns></returns>
        public static Tuple<int[], long> LocateBytes(BinaryReader reader, byte[] find)
        {
            List<int> offs = new List<int>();
            long pos = reader.BaseStream.Position;
            reader.BaseStream.Seek(0, SeekOrigin.Begin);
            byte[] allData = reader.ReadBytes((int)reader.BaseStream.Length);
            foreach (Tuple<int, int> tuple in RecurringIndexes(allData, find, 4))
            {
                offs.Add(tuple.Item1);
            }
            return new Tuple<int[], long>(offs.ToArray(), pos);
        }

        // source: https://stackoverflow.com/a/17803911
        /// <summary>
        /// Searches master for toFind with length precision. Returns a List of Tuples
        /// </summary>
        /// <param name="master"></param>
        /// <param name="toFind"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static IList<Tuple<int, int>> RecurringIndexes(byte[] master, byte[] toFind, int length)
        {
            List<Tuple<int, int>> result = new List<Tuple<int, int>>();

            // Let's return empty list ... or throw appropriate exception
            if (ReferenceEquals(null, master))
                return result;
            else if (ReferenceEquals(null, toFind))
                return result;
            else if (length < 0)
                return result;
            else if (length > toFind.Length)
                return result;

            byte[] subRegion = new byte[length];

            for (int i = 0; i <= toFind.Length - length; ++i)
            {
                for (int j = 0; j < length; ++j)
                    subRegion[j] = toFind[j + i];

                for (int j = 0; j < master.Length - length + 1; ++j)
                {
                    bool counterExample = false;

                    for (int k = 0; k < length; ++k)
                        if (master[j + k] != subRegion[k])
                        {
                            counterExample = true;
                            break;
                        }

                    if (counterExample)
                        continue;

                    result.Add(new Tuple<int, int>(j, j + length - 1));
                }
            }
            return result;
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
            if (data != null && data.Length > 0)
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
        public static void WriteToFile(string fileName, byte[] data, bool writeToTempFolder = false)
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
            try
            {
                using (FileStream stream = new FileStream($"{GetTempPath(fileName)}.dds", FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    using (BinaryWriter writer = new BinaryWriter(stream))
                    {
                        foreach (char c in new char[] { 'D', 'D', 'S', ' ' }) // DDS magic
                            writer.Write(c);
                        writer.Write(BitConverter.GetBytes(124)); // size of DDS header
                        writer.Write(new byte[]{ 0x7, 0x10, 0x8, 0x0 }); // flags
                        writer.Write(height); // height
                        writer.Write(width); // width
                        writer.Write(BitConverter.GetBytes(2048)); // pitch or linear size
                        writer.Write(1); // depth
                        writer.Write(1); // mipmap count, "1" for now
                        for (int i = 0; i < 11; i++) // reserved
                            writer.Write(0);

                        // DDS_PIXELFORMAT
                        writer.Write(32); // size of PIXELFORMAT
                        if (dxtType == 0 ? false : (int)dxtType != 7) // flags
                            writer.Write(BitConverter.GetBytes(4));
                        else
                            writer.Write(BitConverter.GetBytes(64));
                        foreach (char c in DXTExtensions.GetDXTAsChars((int)dxtType)) // DXT type/four CC
                            writer.Write(c);
                        for (int n = 0; n < 5; n++) // RGBBitCount, RBitMask, GBitMask, BBitMask, ABitMask
                            writer.Write(0);

                        writer.Write(4198408); // caps
                        for (int i = 0; i < 4; i++) // caps2, caps3, caps4, reserved2
                            writer.Write(0);

                        // DDS_HEADER_DX10
                        if (dxtType.ToString().Contains("DX10")) // add the DX10 header, if necessary
                        {
                            string fileNameNoExt = Path.GetFileNameWithoutExtension(fileName);
                            if (fileNameNoExt.Contains("NormalMap") || fileNameNoExt.EndsWith("_Map")) // this stays until I devise a better tactic
                                writer.Write(BitConverter.GetBytes(98)); // BC7_UNORM (normal maps)
                            else if (false)
                                writer.Write(BitConverter.GetBytes(71)); // BC1_UNORM (mask maps)
                            else
                                writer.Write(BitConverter.GetBytes(72)); // BC1_UNORM_SRGB
                            writer.Write(BitConverter.GetBytes(3)); // resource dimension
                            writer.Write(0); // misc flags
                            writer.Write(BitConverter.GetBytes(1)); // array size
                            writer.Write(0); // misc flags 2
                        }

                        // image data
                        writer.Write(imageData);
                    }
                }
            }
            catch (IOException e)
            {
                Message.Fail("Could not create a DDS file. " + e.Message + e.StackTrace);
            }

            completedAction();
        }

        /// <summary>
        /// Converts a DDS texture with texconv. Has a callback if you wish to use it.
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="format"></param>
        /// <param name="completionAction"></param>
        /*public static void ConvertDDS(string fileName, string format = "png", bool fixNormals = false, Action<bool> completionAction = null)
        {
            string texconv = Application.StartupPath + "\\Binaries\\x86\\texconv.exe";
            if (File.Exists(texconv))
            {
                string args = $"-ft {format} -f R8G8B8A8_UNORM -m 1 {(fixNormals ? "-inverty" : "")} -o \"{GetTempPath()}\" \"{fileName}\"";
                Console.WriteLine($"> {0} {1}", texconv, args);
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
                    Console.WriteLine($">> {output}");
                    bool error = output.Contains("FAILED (");
                    completionAction?.Invoke(error);
                });

                p.WaitForExit();
            }
            else
                throw new Exception("texconv is not found. Blacksmith needs it to convert the texture.");
        }*/

        /// <summary>
        /// Converts a .dds texture with texconv. Outputs to outputDir. Has a callback if you wish to use it.
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="outputDir"></param>
        /// <param name="format"></param>
        /// <param name="completionAction"></param>
        public static void ConvertDDS(string fileName, string outputDir = "", string format = "png", bool fixNormals = false, Action<bool> completionAction = null)
        {
            string texconv = Application.StartupPath + "\\Binaries\\x86\\texconv.exe";
            if (outputDir == "")
                outputDir = GetTempPath();
            if (File.Exists(texconv))
            {
                string args = $"-ft {format} -f R8G8B8A8_UNORM -m 1 {(fixNormals && Properties.Settings.Default.fixNormals ? "-invertz" : "")} -o \"{outputDir}\" \"{fileName}\"";
                Console.WriteLine($"> {0} {1}", texconv, args);
                Process p = new Process();
                p.StartInfo.FileName = texconv;
                p.StartInfo.Arguments = args;
                p.EnableRaisingEvents = true;
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.RedirectStandardOutput = true;
                p.Start();

                StreamReader reader = p.StandardOutput;
                string output = reader.ReadToEnd();

                p.Exited += new EventHandler(delegate (object s, EventArgs a)
                {
                    Console.WriteLine($">> {output}");
                    bool error = output.Contains("FAILED (");
                    completionAction?.Invoke(error);
                });

                p.WaitForExit();
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
        public static bool MagicMatches(byte[] magic, byte[] comparison) => magic == comparison; /*magic[0] == comparison[0] && magic[1] == comparison[1] && magic[2] == comparison[2] && magic[3] == comparison[3];*/

        /// <summary>
        /// Returns if the magic matches the comparison
        /// </summary>
        /// <param name="magic"></param>
        /// <param name="comparison"></param>
        /// <returns></returns>
        public static bool MagicMatches(char[] magic, char[] comparison) => magic == comparison; /* magic[0] == comparison[0] && magic[1] == comparison[1] &&  magic[2] == comparison[2] && magic[3] == comparison[3]; */

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
        public static string GameToExtension(Game game) => game == Game.ODYSSEY ? "acod" : (game == Game.ORIGINS ? "acor" : "stp");

        /// <summary>
        /// Returns a Game for the extension
        /// </summary>
        /// <param name="extension"></param>
        /// <returns></returns>
        public static Game ExtensionToGame(string ext) => ext == "acod" ? Game.ODYSSEY : (ext == "acor" ? Game.ORIGINS : Game.STEEP);

        /// <summary>
        /// Returns the name of a Game
        /// </summary>
        /// <param name="game"></param>
        /// <returns></returns>
        public static string NameOfGame(Game game) => game == Game.ODYSSEY ? "Assassin's Creed: Odyssey" : (game == Game.ORIGINS ? "Assassin's Creed: Origins" : "Steep");

        // source: https://stackoverflow.com/a/30300521
        public static string WildcardToRegEx(string value) => "^" + Regex.Escape(value).Replace("\\?", ".").Replace("\\*", ".*") + "$";

        /// <summary>
        /// Returns if the Stream is at end-of-stream (EOS)
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public static bool CheckIfEOS(Stream stream) => stream.Position > stream.Length;

        /// <summary>
        /// Returns an array of int32s using a BinaryReader
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public static int[] ReadInt32s(BinaryReader reader, int count)
        {
            int[] ints = new int[count];
            for (int i = 0; i < count; i++)
                ints[i] = reader.ReadInt32();
            return ints;
        }

        /// <summary>
        /// Get the first ResourceIdentifier in a file (provided from fileName)
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static ResourceIdentifier GetFirstResourceIdentifier(string fileName)
        {
            using (Stream stream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                if (stream.Length == 0)
                    return ResourceIdentifier._NONE;

                using (BinaryReader reader = new BinaryReader(stream))
                {
                    LocateResourceIdentifiers(reader).ToList().ForEach(x => Console.WriteLine(x.Type));
                    if (LocateResourceIdentifiers(reader) != null && LocateResourceIdentifiers(reader).Length > 0)
                    {
                        if (LocateResourceIdentifiers(reader).Where(x => x.Offset == 0).Count() > 0)
                        {
                            return LocateResourceIdentifiers(reader).Where(x => x.Offset == 0).First().Type;
                        }
                    }
                }
            }
            return ResourceIdentifier._NONE;
        }

        /// <summary>
        /// Converts a string into title case
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string ToTitleCase(string str)
        {
            TextInfo info = new CultureInfo("en-US", false).TextInfo;
            return info.ToTitleCase(str);
        }
        
        /// <summary>
        /// Formats a string nicely
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static string FormatNicely(string type) => ToTitleCase(type.ToLower().Replace("_", " "));

        /// <summary>
        /// Formats a ResourceIdentifier nicely
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static string FormatNicely(ResourceIdentifier type) => FormatNicely(type.ToString());

        public static string FullPathWithoutExtension(string str) => Path.Combine(Path.GetDirectoryName(str), Path.GetFileNameWithoutExtension(str));

        // unused
        public static int PeekInt32(BinaryReader r1, out BinaryReader r2)
        {
            int i = r1.ReadInt32();
            r2 = r1;
            return i;
        }
    }
}