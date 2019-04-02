using IniParser;
using IniParser.Model;
using System;
using System.Drawing;
using System.IO;
using System.Linq;
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
            renderModeComboBox.SelectedIndex = Properties.Settings.Default.renderMode;
            pointSizeBar.Value = Properties.Settings.Default.pointSize;
            filelistSeparatorComboBox.SelectedIndex = Properties.Settings.Default.useCSV ? 1 : 0;
            popupComboBox.SelectedIndex = Properties.Settings.Default.hidePopups;
            colorDialog.Color = Properties.Settings.Default.threeBG;
        }

        private void Settings_FormClosing(object sender, FormClosingEventArgs e)
        {
            Properties.Settings.Default.Save();
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
            if (Directory.Exists(path) && Properties.Settings.Default.odysseyPath != path)
            {
                Properties.Settings.Default.odysseyPath = path;
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
            if (Directory.Exists(path) && Properties.Settings.Default.originsPath != path)
            {
                Properties.Settings.Default.originsPath = path;
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
            if (Directory.Exists(path) && Properties.Settings.Default.steepPath != path)
            {
                Properties.Settings.Default.steepPath = path;
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
            if (Directory.Exists(path) && Properties.Settings.Default.tempPath != path)
            {
                Properties.Settings.Default.tempPath = path;
            }
        }
        #endregion

        #region Delete temporary files
        private void deleteTempCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.deleteTemp = ((CheckBox)sender).Checked;
        }
        #endregion

        #region Filelist output
        private void filelistSeparatorComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.useCSV = filelistSeparatorComboBox.SelectedIndex == 1;
        }
        #endregion

        #region 3D viewer render mode
        private void renderModeComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.renderMode = renderModeComboBox.SelectedIndex;
        }
        #endregion

        #region 3D viewer point size
        private void pointSizeBar_Scroll(object sender, EventArgs e)
        {
            Properties.Settings.Default.pointSize = pointSizeBar.Value;
        }
        #endregion

        #region 3D viewer bg color
        private void threeDBGColorBtn_Click(object sender, EventArgs e)
        {
            if (colorDialog.ShowDialog() == DialogResult.OK)
            {
                Properties.Settings.Default.threeBG = colorDialog.Color;
            }
        }
        #endregion

        #region Hide popups
        private void popupComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.hidePopups = popupComboBox.SelectedIndex;
        }
        #endregion

        #region Saving and loading
        private void loadFromFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                var parser = new FileIniDataParser();
                IniData data = parser.ReadFile(openFileDialog.FileName);

                acOdTextBox.Text = data["Games"]["Odyssey"];
                acOrTextBox.Text = data["Games"]["Origins"];
                steepTextBox.Text = data["Games"]["Steep"];
                tempTextBox.Text = data["Temp"]["Path"];
                deleteTempCheckbox.Checked = bool.Parse(data["Temp"]["DeleteOnExit"]);
                renderModeComboBox.SelectedIndex = int.Parse(data["3D"]["RenderMode"]);
                pointSizeBar.Value = int.Parse(data["3D"]["PointSize"]);
                filelistSeparatorComboBox.SelectedIndex = int.Parse(data["Misc"]["FilelistSeparator"]);
                popupComboBox.SelectedIndex = int.Parse(data["Misc"]["Popups"]);

                int[] values = data["3D"]["Background"].Split(',').ToList().Select(x => int.Parse(x)).ToArray();
                colorDialog.Color = Color.FromArgb(values[3], values[0], values[1], values[2]);

                Message.Success("Loaded settings from file.");
            }
        }

        private void saveToFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                var parser = new FileIniDataParser();
                IniData data = new IniData();                

                data["Games"]["Odyssey"] = acOdTextBox.Text;
                data["Games"]["Origins"] = acOrTextBox.Text;
                data["Games"]["Steep"] = steepTextBox.Text;
                data["Temp"]["Path"] = tempTextBox.Text;
                data["Temp"]["DeleteOnExit"] = deleteTempCheckbox.Checked.ToString();
                data["3D"]["RenderMode"] = renderModeComboBox.SelectedIndex.ToString();         
                data["3D"]["PointSize"] = pointSizeBar.Value.ToString();
                data["Misc"]["FilelistSeparator"] = filelistSeparatorComboBox.SelectedIndex.ToString();
                data["Misc"]["Popups"] = popupComboBox.SelectedIndex.ToString();
                data["3D"]["Background"] = string.Format("{0},{1},{2},{3}", colorDialog.Color.R, colorDialog.Color.G, colorDialog.Color.B, colorDialog.Color.A);

                parser.WriteFile(saveFileDialog.FileName, data);
                Message.Success("Saved settings to file.");
            }
        }
        #endregion
    }
}