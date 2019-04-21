using System.Windows.Forms;

namespace Blacksmith.Forms
{
    public partial class ResourceViewer : Form
    {
        public ResourceViewer()
        {
            InitializeComponent();
        }

        private void ResourceViewer_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
                Close();
        }

        public void LoadNode(EntryTreeNode node)
        {
            foreach (EntryTreeNode child in node.Nodes)
            {
                DataGridViewRow row = new DataGridViewRow();

                DataGridViewTextBoxCell type = new DataGridViewTextBoxCell
                {
                    Value = Helpers.FormatNicely(child.ResourceIdentifier)
                };
                row.Cells.Add(type);

                DataGridViewTextBoxCell offset = new DataGridViewTextBoxCell
                {
                    Value = child.ResourceOffset
                };
                row.Cells.Add(offset);

                dataGridView.Rows.Add(row);
            }
        }
    }
}