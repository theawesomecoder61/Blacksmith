using Blacksmith.FileTypes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Blacksmith.Forms
{
    public partial class SoundpackBrowser : Form
    {
        public string FileName { get; set; }

        private PCK.Folder[] folders;
        private string playingFile = "";

        public SoundpackBrowser()
        {
            InitializeComponent();
        }

        public void LoadPack(string fileName)
        {
            FileName = fileName;
            int entries = 0;

            dataGridView.Enabled = false; // prevent the user-induced damages
            Thread t = new Thread(() =>
            {
                folders = PCK.Read(FileName);
                if (folders == null || folders.Length == 0)
                    return;

                List<DataGridViewRow> rows = new List<DataGridViewRow>();
                foreach (PCK.Folder folder in folders)
                {
                    foreach (PCK.Entry entry in folder.Entries)
                    {
                        DataGridViewRow row = new DataGridViewRow
                        {
                            Tag = entry
                        };

                        DataGridViewTextBoxCell name = new DataGridViewTextBoxCell
                        {
                            Value = entry.NameHash.ToString("X8")
                        };
                        row.Cells.Add(name);

                        DataGridViewTextBoxCell size = new DataGridViewTextBoxCell
                        {
                            Value = entry.Size
                        };
                        row.Cells.Add(size);

                        DataGridViewTextBoxCell offset = new DataGridViewTextBoxCell
                        {
                            Value = entry.Offset
                        };
                        row.Cells.Add(offset);

                        DataGridViewTextBoxCell isBank = new DataGridViewTextBoxCell
                        {
                            Value = entry.IsSoundbank ? "True" : "False"
                        };
                        row.Cells.Add(isBank);

                        DataGridViewTextBoxCell id = new DataGridViewTextBoxCell
                        {
                            Value = entry.FolderID
                        };
                        row.Cells.Add(id);

                        DataGridViewTextBoxCell path = new DataGridViewTextBoxCell
                        {
                            Value = entry.Path
                        };
                        row.Cells.Add(path);
                        
                        rows.Add(row);
                        entries++;
                    }
                }

                Invoke(new Action(() =>
                {
                    dataGridView.Rows.AddRange(rows.ToArray());
                    entriesToolStripLabel.Text = $"Entries: {entries}";
                    foldersToolStripLabel.Text = $"Folders: {folders.Length}";
                    dataGridView.Enabled = true;
                }));
            });

            t.Start();
        }

        private void SoundpackBrowser_Load(object sender, System.EventArgs e)
        {
            Text = $"Soundpack Browser - {FileName}";
        }

        private void dataGridView_SelectionChanged(object sender, EventArgs e)
        {
        }

        // pp = play/pause
        private void ppBtn_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(playingFile))
            {
                if (dataGridView.SelectedRows.Count == 0)
                    return;
                
                ExtractAndConvert(dataGridView.SelectedRows.Cast<DataGridViewRow>().Take(1).ToArray(), null, (list) => {
                    playingFile = list[0].Replace(".wem", ".ogg");
                });
            }
            else
            {
                /*using (var vorbisStream = new NAudio.Vorbis.VorbisWaveReader(playingFile))
                using (var waveOut = new NAudio.Wave.WaveOutEvent())
                {
                    waveOut.Init(vorbisStream);
                    waveOut.Play();
                }*/
            }
        }

        private void stopBtn_Click(object sender, EventArgs e)
        {
        }

        private void extractBtn_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog dialog = new FolderBrowserDialog())
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    ExtractAndConvert(dataGridView.SelectedRows.Cast<DataGridViewRow>().ToArray(), dialog.SelectedPath, (list) =>
                    {
                        // change the message box if a soundbank was extracted
                        if (list.Where(l => l.Contains(".bnk")).Count() > 0)
                            MessageBox.Show("Extracted all sounds and soundbanks. All sounds were converted to OGG and all soundbanks were dumped.", "Success");
                        else
                            MessageBox.Show("Extracted all sounds, which were converted to OGG.", "Success");
                    });
                }
            }
        }

        private void ExtractAndConvert(DataGridViewRow[] rows, string parentDir = null, Action<List<string>> completedAction = null)
        {
            using (Stream stream = new FileStream(FileName, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                using (BinaryReader reader = new BinaryReader(stream))
                {
                    List<string> extractedFiles = new List<string>();
                    Parallel.ForEach(rows, row =>
                    {
                        string hash = (string)row.Cells[0].Value;
                        PCK.Entry entry = (PCK.Entry)row.Tag;
                        if (row.Tag != null)
                        {
                            // go to offset, read and write data
                            reader.BaseStream.Seek(entry.Offset, SeekOrigin.Begin);
                            byte[] data = reader.ReadBytes((int)entry.Size);

                            // determine file extension
                            string ext = entry.IsSoundbank ? "bnk" : "wem";

                            // fallback to temporary path if no parentDir was supplied
                            if (parentDir == null)
                                parentDir = Helpers.GetTempPath();

                            string fn = Path.Combine(parentDir, $"{hash}.{ext}");
                            extractedFiles.Add(fn);
                            File.WriteAllBytes(fn, data);
                        }
                    });

                    // convert wwise to ogg
                    Parallel.ForEach(extractedFiles, file =>
                    {
                        if (Path.GetExtension(file).EndsWith(".wem"))
                            Helpers.ConvertWEMToOGG(file);
                        else if (Path.GetExtension(file).EndsWith(".bnk"))
                            Helpers.ExtractBNK(file);
                    });

                    // revorb (make the converted ogg files playable)
                    Parallel.ForEach(extractedFiles, file =>
                    {
                        if (Path.GetExtension(file).EndsWith(".wem"))
                            Helpers.RevorbOGG(file.Replace(".wem", ".ogg"));
                    });

                    completedAction(extractedFiles);
                }
            }
        }
    }
}