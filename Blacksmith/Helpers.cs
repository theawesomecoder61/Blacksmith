using Blacksmith.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace Blacksmith
{
    public static class Helpers
    {
        static string supportedFiles = @"(.forge|.pck|.png|.txt|.ini|.log)";

        /// <summary>
        /// Returns if a file path is a supported file by Blacksmith.
        /// </summary>
        /// <param name="file">File path</param>
        /// <returns></returns>
        public static bool IsSupportedFile(string file)
        {
            return Regex.Matches(file, supportedFiles).Count > 0;
        }

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
        /// <param name="reader">Binary reader containing the raw data file</param>
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
        /// <param name="reader">Binary reader containing the raw data file</param>
        /// <returns></returns>
        public static ResourceLocation[] LocateResourceIdentifiers(BinaryReader reader)
        {
            List<ResourceLocation> locs = new List<ResourceLocation>();
            long originalPos = reader.BaseStream.Position;

            reader.BaseStream.Position = 0; // do not skip the first 4 bytes
            //while (reader.BaseStream.Position <= reader.BaseStream.Length - 8)
            //{
                uint i = reader.ReadUInt32();
                ResourceType type = ResourceType._NONE;
                switch (ResourceTypeExtensions.GetResourceType(i))
                {
                    case ResourceType.COMPILED_MESH:
                        type = ResourceType.COMPILED_MESH;
                        break;
                    case ResourceType.MATERIAL:
                        type = ResourceType.MATERIAL;
                        break;
                    case ResourceType.MESH:
                        type = ResourceType.MESH;
                        break;
                    case ResourceType.MIPMAP:
                        type = ResourceType.MIPMAP;
                        break;
                    case ResourceType.TEXTURE_MAP:
                        type = ResourceType.TEXTURE_MAP;
                        break;
                    case ResourceType.TEXTURE_SET:
                        type = ResourceType.TEXTURE_SET;
                        break;
                    default:
                        //reader.BaseStream.Seek(-3, SeekOrigin.Current); // go back 3 bytes (if it were 4, infinite recursion)
                        break;
                }

            Console.WriteLine("Found " + type.ToString());

                if (type != ResourceType._NONE)
                {
                    locs.Add(new ResourceLocation
                    {
                        Type = type,
                        Offset = reader.BaseStream.Position - 4 // get the offset 4 bytes from here
                    });
                }
            //}

            reader.BaseStream.Position = originalPos;
            return locs.ToArray();
        }


        public static int IndexOfBytes(byte[] array, byte[] pattern, int startIndex, int count)
        {
            int fidx = 0;
            int result = Array.FindIndex(array, startIndex, count, (byte b) => {
                fidx = (b == pattern[fidx]) ? fidx + 1 : 0;
                return (fidx == pattern.Length);
            });
            return (result < 0) ? -1 : result - fidx + 1;
        }





















        /// <summary>
        /// Writes data to a fileName located in the temporary path
        /// </summary>
        /// <param name="fileName">File name, no directories</param>
        /// <param name="data"></param>
        public static void WriteToTempFile(string fileName, byte[] data)
        {
            string tempPath = Properties.Settings.Default.tempPath;
            if (!string.IsNullOrEmpty(tempPath))
                File.WriteAllBytes(Path.Combine(tempPath, fileName), data);
        }

        /// <summary>
        /// Writes data to a fileName located in the temporary path
        /// </summary>
        /// <param name="fileName">File name, no directories</param>
        /// <param name="data"></param>
        public static void WriteToTempFile(string fileName, byte[][] data)
        {
            string tempPath = Properties.Settings.Default.tempPath;
            if (string.IsNullOrEmpty(tempPath))
                return;

            List<byte> a = new List<byte>();
            foreach (byte[] b in data)
            {
                foreach (byte c in b)
                    a.Add(c);
            }
            File.WriteAllBytes(Path.Combine(tempPath, fileName), a.ToArray());
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
        /// Writes an entire DDS file to the temporary path using the fileName
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="imageData"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="mipmapCount"></param>
        /// <param name="dxtType"></param>
        public static void WriteTempDDS(string fileName, byte[] imageData, int width, int height, int mipmapCount, DXT dxtType)
        {
            Console.WriteLine("DXT: " + dxtType.ToString());

            char[] dxtArr = { 'D', 'X', '\x0', '\x0' };
            int pls = imageData.Length; //655362 //Math.Max(1, (width + 3) / 4) * 8;
            using (FileStream stream = new FileStream(GetTempPath(fileName) + ".dds", FileMode.Create, FileAccess.Write, FileShare.None))
            {
                using (BinaryWriter writer = new BinaryWriter(stream))
                {
                    foreach (char c in new char[]{ 'D', 'D', 'S', ' ' }) // DDS magic
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
                        if (fileName.Contains("NormalMap")) // this stays until I devise a better tactic
                            writer.Write(98); // normal maps use BC7_UNORM
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
        
        public static void ConvertDDSToPNG(string fileName)
        {
            string texconv = string.Concat(Application.StartupPath, "\\Binaries\\x86\\texconv.exe");
            if (File.Exists(texconv))
            {
                string args = string.Concat("-ft png -f R8G8B8A8_UNORM -m 1 -o ", GetTempPath(), " ", fileName);
                Process.Start(texconv, args);
            }
            else
                MessageBox.Show("texconv is not found. Blacksmith needs it to convert the texture.", "Warning");
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
    }
}