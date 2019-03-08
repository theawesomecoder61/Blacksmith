using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Blacksmith.FileTypes
{
    public class Forge
    {
        public string Path { get; private set; }
        public string Name { get; private set; }
        public HeaderBlock Header { get; private set; }
        public DataHeader1Block DataHeader1 { get; private set; }
        public DataHeader2Block DataHeader2 { get; private set; }
        public FileEntry[] FileEntries { get; private set; }

        private IndexTable[] Indices;
        private NameTable[] Names;
        private bool isFullyRead;

        public class HeaderBlock
        {
            public char[] Magic { get; set; } //8
            public byte Unknown1 { get; set; }
            public int FileVersionIdentifier { get; set; }
            public ulong OffsetToDataHeader { get; set; }
        }

        public class DataHeader1Block
        {
            public int NumOfEntries { get; set; }
            public int[] Unknown1 { get; set; } //4
            public long Unknown2 { get; set; }
            public int MaxFilesForThisIndex { get; set; } // always seems to be 5000
            public int Unknown3 { get; set; }
            public long OffsetToData { get; set; }
        }

        public class DataHeader2Block
        {
            public int IndexCount { get; set; }
            public int Unknown1 { get; set; }
            public long OffsetToIndexTable { get; set; }
            public long OffsetToNextDataSection { get; set; } // -1 = no more sections
            public int IndexStart { get; set; }
            public int IndexEnd { get; set; }
            public long OffsetToNameTable { get; set; }
            public long Unknown2 { get; set; }
        }

        public class IndexTable
        {
            public long OffsetToRawDataTable { get; set; }
            public long FileDataID { get; set; }
            public int RawDataSize { get; set; }
        }

        public class NameTable
        {
            public int RawDataSize { get; set; }
            public long FileDataID { get; set; }
            public int[] Unknown1 { get; set; } //4
            public int NextFileCount { get; set; }
            public int PreviousFileCount { get; set; }
            public int Unknown2 { get; set; }
            public int Timestamp { get; set; } // really an int
            public string Name { get; set; } // actually char [128], but strings are MUCH easier to work with
            public int[] Unknown3 { get; set; } //5
        }

        public struct FileEntry
        {
            public IndexTable IndexTable;
            public NameTable NameTable;
        }

        //

        public Forge(string path)
        {
            Path = path;
            Name = System.IO.Path.GetFileNameWithoutExtension(Path);
            isFullyRead = false;
        }

        /// <summary>
        /// Returns the number of entries [does not require Read()]
        /// </summary>
        /// <returns></returns>
        public int GetEntryCount()
        {
            int ct = 0;
            using (Stream stream = new FileStream(Path, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                using (BinaryReader reader = new BinaryReader(stream))
                {
                    stream.Position = 13;
                    ulong offsetToDataHeader = reader.ReadUInt64();
                    stream.Position = (int)offsetToDataHeader;
                    ct = reader.ReadInt32();
                }
            }
            return ct;
        }

        /// <summary>
        /// Read data and populate the fields
        /// </summary>
        public void Read()
        {
            using (Stream stream = new FileStream(Path, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                using (BinaryReader reader = new BinaryReader(stream))
                {
                    // Header block
                    Header = new HeaderBlock
                    {
                        Magic = reader.ReadChars(8),
                        Unknown1 = reader.ReadByte(),
                        FileVersionIdentifier = reader.ReadInt32(),
                        OffsetToDataHeader = reader.ReadUInt64()
                    };

                    // skip to DataHeader1
                    stream.Position = (int)Header.OffsetToDataHeader;

                    // Data Header 1
                    DataHeader1 = new DataHeader1Block
                    {
                        NumOfEntries = reader.ReadInt32(),
                        Unknown1 = Helpers.ReadInt32s(reader, 4),
                        Unknown2 = reader.ReadInt64(),
                        MaxFilesForThisIndex = reader.ReadInt32(),
                        Unknown3 = reader.ReadInt32(),
                        OffsetToData = reader.ReadInt64()
                    };

                    // skip to DataHeader2
                    stream.Position = (int)DataHeader1.OffsetToData;

                    // Data Header 2
                    DataHeader2 = new DataHeader2Block
                    {
                        IndexCount = reader.ReadInt32(),
                        Unknown1 = reader.ReadInt32(),
                        OffsetToIndexTable = reader.ReadInt64(),
                        OffsetToNextDataSection = reader.ReadInt64(),
                        IndexStart = reader.ReadInt32(),
                        IndexEnd = reader.ReadInt32(),
                        OffsetToNameTable = reader.ReadInt64(),
                        Unknown2 = reader.ReadInt64()
                    };

                    // File Entries
                    FileEntries = new FileEntry[DataHeader2.IndexCount];

                    // Index Table
                    Indices = new IndexTable[DataHeader2.IndexCount];
                    for (int i = 0; i < DataHeader2.IndexCount; i++)
                    {
                        Indices[i] = new IndexTable
                        {
                            OffsetToRawDataTable = reader.ReadInt64(),
                            FileDataID = reader.ReadInt64(),
                            RawDataSize = reader.ReadInt32()
                        };

                        FileEntries[i].IndexTable = Indices[i];
                    }

                    // skip to Name Table
                    stream.Position = DataHeader2.OffsetToNameTable;

                    // Name Table
                    Names = new NameTable[DataHeader2.IndexCount];
                    for (int i = 0; i < DataHeader2.IndexCount; i++)
                    {
                        Names[i] = new NameTable
                        {
                            RawDataSize = reader.ReadInt32(),
                            FileDataID = reader.ReadInt64(),
                            Unknown1 = Helpers.ReadInt32s(reader, 4),
                            NextFileCount = reader.ReadInt32(),
                            PreviousFileCount = reader.ReadInt32(),
                            Unknown2 = reader.ReadInt32(),
                            Timestamp = reader.ReadInt32(),
                            Name = new string(reader.ReadChars(128)),
                            Unknown3 = Helpers.ReadInt32s(reader, 5)
                        };

                        // remove non-ASCII characters from the name
                        Names[i].Name = Regex.Replace(Names[i].Name, @"[^\u0020-\u007E]", string.Empty);

                        FileEntries[i].NameTable = Names[i];
                    }

                    // alphabetically sort NameTables
                    Array.Sort(FileEntries, new Comparison<FileEntry>((x, y) =>
                    {
                        return x.NameTable.Name.CompareTo(y.NameTable.Name);
                    }));

                    isFullyRead = true;
                }
            }
        }

        /// <summary>
        /// Returns the first corresponding FileEntry with the given file name (case insensitive)
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public FileEntry GetFileEntry(string fileName)
        {
            FileEntry[] res = FileEntries.ToList().Where(x => x.NameTable.Name.Equals(fileName, StringComparison.CurrentCultureIgnoreCase)).ToArray();
            return res.Length > 0 ? res[0] : new FileEntry { };
        }

        /// <summary>
        /// Returns the raw data for the specified FileEntry
        /// </summary>
        /// <param name="fileEntry"></param>
        /// <returns></returns>
        public byte[] GetRawData(FileEntry fileEntry)
        {
            byte[] data = new byte[fileEntry.IndexTable.RawDataSize];
            using (Stream stream = new FileStream(Path, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                using (BinaryReader reader = new BinaryReader(stream))
                {
                    reader.BaseStream.Seek(fileEntry.IndexTable.OffsetToRawDataTable, SeekOrigin.Begin); // the checksum is ignored
                    data = reader.ReadBytes(fileEntry.IndexTable.RawDataSize);
                }
            }
            return data;
        }

        /// <summary>
        /// Returns the raw data with an offset and size
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public byte[] GetRawData(long offset, long size)
        {
            byte[] data = new byte[size];
            using (Stream stream = new FileStream(Path, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                using (BinaryReader reader = new BinaryReader(stream))
                {
                    reader.BaseStream.Seek(offset, SeekOrigin.Begin); // the checksum is ignored
                    data = reader.ReadBytes((int)size);
                }
            }
            return data;
        }

        /// <summary>
        /// Creates a filelist
        /// </summary>
        public string CreateFilelist()
        {
            StringBuilder sb = new StringBuilder();
            if (FileEntries != null && FileEntries.Length > 0)
            {
                sb.Append("Name\tOffset\tSize\tFile ID from Index Table\n");
                foreach (FileEntry entry in FileEntries)
                {
                    if (Properties.Settings.Default.useCSV)
                        sb.AppendFormat("{0},{1},{2},{3}\n", entry.NameTable.Name, entry.IndexTable.OffsetToRawDataTable, entry.IndexTable.RawDataSize, entry.IndexTable.FileDataID);
                    else
                        sb.AppendFormat("{0}\t{1}\t{2}\t{3}\n", entry.NameTable.Name, entry.IndexTable.OffsetToRawDataTable, entry.IndexTable.RawDataSize, entry.IndexTable.FileDataID);
                }
                return sb.ToString();
            }
            else
                return "";
        }

        public bool IsFullyRead() => isFullyRead;

        /// <summary>
        /// Sets everything to null, basically dumping the resources used by the forge's fields
        /// </summary>
        public void Dump()
        {
            isFullyRead = false;
            Path = "";
            Header = null;
            DataHeader1 = null;
            DataHeader2 = null;
            FileEntries = null;
            Indices = null;
            Names = null;
        }
    }
}