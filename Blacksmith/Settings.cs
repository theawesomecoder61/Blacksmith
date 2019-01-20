using System;
using System.IO;
using System.Windows.Forms;

namespace Blacksmith
{
    public partial class Settings : Form
    {
        public Settings()
        {
            InitializeComponent();
        }

        private void Settings_Load(object sender, EventArgs e)
        {
            acOdTextBox.Text = Properties.Settings.Default.odysseyPath;
            acOrTextBox.Text = Properties.Settings.Default.originsPath;
            steepTextBox.Text = Properties.Settings.Default.steepPath;
            tempTextBox.Text = Properties.Settings.Default.tempPath;
            deleteTempCheckbox.Checked = Properties.Settings.Default.deleteTemp;
        }

        #region Odyssey
        private void acOdTextBox_TextChanged(object sender, EventArgs e)
        {
            SetOdysseyPath(acOdTextBox.Text);
        }

        private void acOdButton_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                SetOdysseyPath(folderBrowserDialog.SelectedPath);
                acOdTextBox.Text = folderBrowserDialog.SelectedPath;
            }
        }

        private void SetOdysseyPath(string path)
        {
            if (Directory.Exists(path))
            {
                Properties.Settings.Default.odysseyPath = path;
                Properties.Settings.Default.Save();
            }
        }
        #endregion

        #region Origins
        private void acOrTextBox_TextChanged(object sender, EventArgs e)
        {
            SetOriginsPath(acOrTextBox.Text);
        }

        private void acOrButton_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                SetOriginsPath(folderBrowserDialog.SelectedPath);
                acOrTextBox.Text = folderBrowserDialog.SelectedPath;
            }
        }

        private void SetOriginsPath(string path)
        {
            if (Directory.Exists(path))
            {
                Properties.Settings.Default.originsPath = path;
                Properties.Settings.Default.Save();
            }
        }
        #endregion

        #region Steep
        private void steepTextBox_TextChanged(object sender, EventArgs e)
        {
            SetTempPath(steepTextBox.Text);
        }

        private void steepButton_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                SetTempPath(folderBrowserDialog.SelectedPath);
                steepTextBox.Text = folderBrowserDialog.SelectedPath;
            }
        }

        private void SetSteepPath(string path)
        {
            if (Directory.Exists(path))
            {
                Properties.Settings.Default.steepPath = path;
                Properties.Settings.Default.Save();
            }
        }
        #endregion

        #region Temporary path
        private void tempTextBox_TextChanged(object sender, EventArgs e)
        {
            SetTempPath(tempTextBox.Text);
        }

        private void tempButton_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                SetTempPath(folderBrowserDialog.SelectedPath);
                tempTextBox.Text = folderBrowserDialog.SelectedPath;
            }
        }

        private void SetTempPath(string path)
        {
            if (Directory.Exists(path))
            {
                Properties.Settings.Default.tempPath = path;
                Properties.Settings.Default.Save();
            }
        }
        #endregion

        #region Delete temporary files
        private void deleteTempCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.deleteTemp = ((CheckBox)sender).Checked;
            Properties.Settings.Default.Save();
        }
        #endregion
    }
}