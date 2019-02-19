using System.Collections.Generic;
using System.IO;
using System.Text;

// PCK files hold Wwise sound data

namespace Blacksmith.FileTypes
{
    public class PCK
    {
        public static byte[] AKPK = new byte[]
        {
            (byte)'A', (byte)'K', (byte)'P', (byte)'K'
        };

        public static char[] BKHD = new char[]
        {
            'B', 'K', 'H', 'D'
        };

        public static byte[] RIFF = new byte[]
        {
            (byte)'R', (byte)'I', (byte)'F', (byte)'F'
        };

        public struct Folder
        {
            public uint NameOffset { get; internal set; }
            public uint ID { get; internal set; }
            public string Name { get; internal set; }
            public Entry[] Entries { get; internal set; }
        };

        public struct Entry
        {
            public bool IsSoundbank { get; internal set; }
            public uint NameHash { get; internal set; }
            public uint OffsetMultiplier { get; internal set; }
            public uint Size { get; internal set; }
            public uint Offset { get; internal set; }
            public uint FolderID { get; internal set; }
            public string Path { get; internal set; }
        };

        // adapted from https://github.com/Nibre/HaloWwise/blob/master/HaloWwise/PackManager.cs by Nibre
        public static Folder[] Read(string fileName)
        {
            Folder[] folders;

            using (Stream stream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                using (BinaryReader reader = new BinaryReader(stream))
                {
                    string magic = new string(reader.ReadChars(4));
                    if (magic != "AKPK")
                        return null;

                    // fix endianness
                    reader.BaseStream.Seek(0x8, 0);
                    reader.BaseStream.Seek(0x4, 0);

                    uint headerSize = reader.ReadUInt32();
                    reader.ReadUInt32();
                    uint folderListSize = reader.ReadUInt32();
                    uint bankTableSize = reader.ReadUInt32();
                    uint soundTableSize = reader.ReadUInt32();
                    reader.ReadUInt32();

                    uint folderListStartPos = (uint)reader.BaseStream.Position;
                    uint foldersCount = reader.ReadUInt32();

                    folders = new Folder[foldersCount];

                    for (int i = 0; i < foldersCount; i++)
                    {
                        folders[i].NameOffset = reader.ReadUInt32() + folderListStartPos;
                        folders[i].ID = reader.ReadUInt32();

                        uint folderListTempPos = (uint)reader.BaseStream.Position;

                        // folder name
                        reader.BaseStream.Seek(folders[i].NameOffset, 0);                        
                        folders[i].Name = GetString(reader);

                        // return to where we were in the List
                        reader.BaseStream.Seek(folderListTempPos, 0);
                    }

                    // jump to past the folder section
                    reader.BaseStream.Seek(folderListStartPos + folderListSize, 0);

                    for (int i = 0; i < foldersCount; i++)
                    {
                        uint fileCount = reader.ReadUInt32();

                        List<Entry> entries = new List<Entry>();
                        for (int j = 0; j < fileCount; j++)
                        {
                            Entry entry = new Entry
                            {
                                NameHash = reader.ReadUInt32(),
                                OffsetMultiplier = reader.ReadUInt32(),
                                Size = reader.ReadUInt32()
                            };
                            entry.Offset = reader.ReadUInt32() * entry.OffsetMultiplier;
                            entry.FolderID = reader.ReadUInt32();

                            string folderName = folders[entry.FolderID].Name;
                            entry.Path = $@"{folderName}\{Path.GetFileNameWithoutExtension(fileName)}\";

                            // detect if this is a soundbank using the magic
                            long off = reader.BaseStream.Position;
                            reader.BaseStream.Seek(entry.Offset, SeekOrigin.Begin);
                            entry.IsSoundbank = Helpers.MagicMatches(reader.ReadChars(4), BKHD);
                            reader.BaseStream.Seek(off, SeekOrigin.Begin);

                            entries.Add(entry);
                        }

                        folders[folders[i].ID].Entries = entries.ToArray();
                    }
                }
            }

            return folders;
        }

        private static string GetString(BinaryReader reader)
        {
            StringBuilder builder = new StringBuilder();
            char a = 'a';
            while (true)
            {
                a = reader.ReadChar();
                reader.ReadChar(); // always a null byte
                if (a == 0x0)
                    break;
                else
                    builder.Append(a);
            }
            return builder.ToString();
        }
    }
}