using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace Blacksmith.Forms
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
            filelistSeparatorComboBox.SelectedIndex = Properties.Settings.Default.useCSV ? 1 : 0;
            renderModeComboBox.SelectedIndex = Properties.Settings.Default.renderMode;
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
            SetSteepPath(steepTextBox.Text);
        }

        private void steepButton_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                SetSteepPath(folderBrowserDialog.SelectedPath);
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

        #region Filelist output
        private void filelistSeparatorComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.useCSV = filelistSeparatorComboBox.SelectedIndex == 1;
            Properties.Settings.Default.Save();
        }
        #endregion

        #region 3D viewer render mode
        private void renderModeComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.renderMode = renderModeComboBox.SelectedIndex;
            Properties.Settings.Default.Save();
        }
        #endregion

        #region 3D viewer bg color
        private void threeDBGColorBtn_Click(object sender, EventArgs e)
        {
            if (colorDialog.ShowDialog() == DialogResult.OK)
            {
                Properties.Settings.Default.threeBG = colorDialog.Color;
                Properties.Settings.Default.Save();
            }
        }
        #endregion
    }
}