using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Blacksmith.Forms
{
    public partial class ResourceViewer : Form
    {
        public ResourceViewer()
        {
            InitializeComponent();
        }

        public void LoadNode(EntryTreeNode node)
        {
            foreach (EntryTreeNode child in node.Nodes)
            {
                DataGridViewRow row = new DataGridViewRow();

                DataGridViewTextBoxCell type = new DataGridViewTextBoxCell
                {
                    Value = child.ResourceType
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