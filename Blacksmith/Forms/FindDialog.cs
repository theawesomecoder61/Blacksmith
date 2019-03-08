using Blacksmith.Enums;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Blacksmith.Forms
{
    public partial class FindDialog : Form
    {
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
                CaseSensitive = ((string)filterComboBox.SelectedItem).Contains("Case-Sensitive")
            });
        }

        public void AddOrRemoveForge(string forge)
        {
            if (forgeComboBox.Items.Contains(forge))
                forgeComboBox.Items.Remove(forge);
            else
                forgeComboBox.Items.Add(forge);
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
                    Value = node.Size//Helpers.BytesToString()
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
    }

    public class FindEventArgs : EventArgs
    {
        public string Query;
        public FindType Type;
        public bool CaseSensitive;
    }

    public class ShowInListArgs : EventArgs
    {
        public string Name;
    }
}