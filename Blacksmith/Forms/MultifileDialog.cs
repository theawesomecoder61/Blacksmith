using Blacksmith.Enums;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Linq;
using System.IO;
using System;

namespace Blacksmith.Forms
{
    public partial class MultifileDialog : Form
    {
        private Structs.MultifileEntry[] entries;
        private Game game;

        public MultifileDialog()
        {
            InitializeComponent();
        }

        private void MultifileDialog_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
                Close();
        }

        private void MultifileDialog_Load(object sender, EventArgs e)
        {
            dataGridView.Columns[0].AutoSizeMode = dataGridView.Columns[2].AutoSizeMode = dataGridView.Columns[3].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
        }
                
        private void button1_Click(object sender, System.EventArgs e)
        {
            List<Structs.MultifileEntry> toExport = new List<Structs.MultifileEntry>();
            
            foreach (DataGridViewRow row in dataGridView.Rows)
            {
                if (row.Cells[0] is DataGridViewCheckBoxCell cb)
                {
                    if ((bool)cb.Value)
                    {
                        toExport.Add(entries.Where(x => new string(x.Header.FileName) == (string)row.Cells[1].Value).First());
                    }
                }
            }

            if (toExport.Count > 0 && folderBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                Helpers.DoBackgroundWork(() =>
                {
                    foreach (Structs.MultifileEntry x in toExport)
                    {
                        try
                        {
                            File.WriteAllBytes(Path.Combine(folderBrowserDialog.SelectedPath, new string(x.Header.FileName) + "." + Helpers.GameToExtension(game)), x.AllData);
                        }
                        catch (Exception ee)
                        {
                            Message.Fail(ee.Message + ee.StackTrace);
                        }
                    }
                },() =>
                {
                    Message.Success("Exported all files.");
                });
            }
        }

        private void dataGridView_CellEnter(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex != 0)
            {
                dataGridView.EditMode = DataGridViewEditMode.EditProgrammatically;
            }
            else
            {
                dataGridView.EditMode = DataGridViewEditMode.EditOnKeystrokeOrF2;
            }
        }

        public void LoadEntries(Structs.MultifileEntry[] entries)
        {
            this.entries = entries;
            foreach (Structs.MultifileEntry entry in entries)
            {
                DataGridViewRow row = new DataGridViewRow();

                DataGridViewCheckBoxCell cb = new DataGridViewCheckBoxCell();
                cb.Value = true;
                row.Cells.Add(cb);

                DataGridViewTextBoxCell name = new DataGridViewTextBoxCell
                {
                    Value = new string(entry.Header.FileName)
                };
                row.Cells.Add(name);

                DataGridViewTextBoxCell id = new DataGridViewTextBoxCell
                {
                    Value = Helpers.FormatNicely(Helpers.FormatNicely((ResourceIdentifier)entry.Header.ResourceIdentifier))
                };
                row.Cells.Add(id);

                DataGridViewTextBoxCell size = new DataGridViewTextBoxCell
                {
                    Value = entry.AllData.Length
                };
                row.Cells.Add(size);

                dataGridView.Rows.Add(row);
            }
        }

        public void SetGame(Game game)
        {
            this.game = game;
        }

    }
}