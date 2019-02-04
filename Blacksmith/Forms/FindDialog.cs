using System;
using System.Windows.Forms;

namespace Blacksmith.Forms
{
    public partial class FindDialog : Form
    {
        public FindDialog()
        {
            InitializeComponent();
        }

        private void queryTextBox_TextChanged(object sender, EventArgs e)
        {
            OnPrecacheResults(new FindEventArgs
            {
                Query = queryTextBox.Text
            });
        }

        private void findNextBtn_Click(object sender, EventArgs e)
        {
            OnFindNext(new FindEventArgs
            {
                Query = queryTextBox.Text
            });
        }

        protected virtual void OnPrecacheResults(FindEventArgs e)
        {
            PrecacheResults?.Invoke(this, e);
        }

        protected virtual void OnFindNext(FindEventArgs e)
        {
            FindNext?.Invoke(this, e);
        }

        public event EventHandler<FindEventArgs> PrecacheResults;
        public event EventHandler<FindEventArgs> FindNext;
    }

    public class FindEventArgs : EventArgs
    {
        public string Query;
    }
}