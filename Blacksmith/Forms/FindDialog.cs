using Blacksmith.Enums;
using Blacksmith.FileTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace Blacksmith.Forms
{
    public partial class FindDialog : Form
    {
        private List<Forge> forges = new List<Forge>();

        public FindDialog()
        {
            InitializeComponent();
        }

        private void FindDialog_Load(object sender, EventArgs e)
        {
            queryTextBox.Focus();
            filterComboBox.SelectedIndex = 0;
        }

        private void FindDialog_Shown(object sender, EventArgs e)
        {
            FindDialog_Load(sender, e);
        }
        
        private void queryTextBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Return)
                findAllBtn_Click(sender, EventArgs.Empty);
        }

        private void findAllBtn_Click(object sender, EventArgs e)
        {
            if (forgeComboBox.SelectedIndex < 0 || forgeComboBox.Items.Count == 0)
            {
                Message.Fail("Either no .forge file is open/expanded in Blacksmith or none have been selected from the dropdown.");
                return;
            }

            dataGridView.Rows.Clear();

            FindType type = FindType.NORMAL;
            if (filterComboBox.SelectedIndex == 2 || filterComboBox.SelectedIndex == 3)
                type = FindType.REGEX;
            else if (filterComboBox.SelectedIndex == 4 || filterComboBox.SelectedIndex == 5)
                type = FindType.WILDCARD;

            OnFindAll(new FindEventArgs
            {
                Query = queryTextBox.Text,
                Type = type,
                ForgeToSearchIn = forges.Where(x => FormatName(x) == (string)forgeComboBox.SelectedItem).FirstOrDefault(),
                CaseSensitive = ((string)filterComboBox.SelectedItem).Contains("Case-Sensitive")
            });
        }

        public void AddOrRemoveForge(Forge forge)
        {
            // add to the combobox
            string listName = FormatName(forge);
            if (forgeComboBox.Items.Contains(listName))
                forgeComboBox.Items.Remove(listName);
            else
                forgeComboBox.Items.Add(listName);

            // add to the forges list
            if (forges.Contains(forge))
                forges.Remove(forge);
            else
                forges.Add(forge);

            // select the first item in the combobox
            if (forgeComboBox.Items.Count == 1)
                forgeComboBox.SelectedIndex = 0;
        }

        public void LoadResults(List<EntryTreeNode> results)
        {
            foreach (EntryTreeNode node in results)
            {
                DataGridViewRow row = new DataGridViewRow();

                DataGridViewTextBoxCell name = new DataGridViewTextBoxCell
                {
                    Value = node.Text
                };
                row.Cells.Add(name);

                DataGridViewTextBoxCell size = new DataGridViewTextBoxCell
                {
                    Value = node.Size
                };
                row.Cells.Add(size);

                DataGridViewTextBoxCell path = new DataGridViewTextBoxCell
                {
                    Value = node.FullPath
                };
                row.Cells.Add(path);

                dataGridView.Rows.Add(row);
            }
        }

        private void showInListToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (dataGridView.SelectedRows.Count != 1)
                return;

            OnShowInList(new ShowInListArgs
            {
                Name = (string)dataGridView.SelectedRows[0].Cells[0].Value
            });
        }

        private void dataGridView_MouseUp(object sender, MouseEventArgs e)
        {
            for (int i = 0; i < dataGridView.Rows.Count; i++)
            {
                if (dataGridView.GetRowDisplayRectangle(i, false).Contains(e.Location))
                {
                    dataGridView.Rows[i].Selected = true;
                }
            }
        }

        private void dataGridView_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            OnShowInList(new ShowInListArgs
            {
                Name = (string)dataGridView.SelectedRows[0].Cells[0].Value
            });
        }

        private string FormatName(Forge forge) => string.Format("({0}) {1}", Helpers.ToTitleCase(forge.Game.ToString().ToLower()), forge.Name);

        protected virtual void OnFindAll(FindEventArgs e)
        {
            FindAll?.Invoke(this, e);
        }

        protected virtual void OnShowInList(ShowInListArgs e)
        {
            ShowInList?.Invoke(this, e);
        }

        public event EventHandler<FindEventArgs> FindAll;
        public event EventHandler<ShowInListArgs> ShowInList;
    }

    public class FindEventArgs : EventArgs
    {
        public string Query;
        public FindType Type;
        public Forge ForgeToSearchIn;
        public bool CaseSensitive;
    }

    public class ShowInListArgs : EventArgs
    {
        public string Name;
    }
}