using Blacksmith.Enums;
using Blacksmith.FileTypes;
using Blacksmith.Three;
using System;
using System.IO;
using System.Windows.Forms;

namespace Blacksmith.Forms
{
    public partial class ConvertDialog : Form
    {
        private EntryTreeNode node;
        private Model model;

        public ConvertDialog()
        {
            InitializeComponent();
        }

        public void SetValues(int selectedTab, EntryTreeNode node, Model model = null)
        {
            tabControl1.SelectedIndex = selectedTab;
            this.node = node;
            this.model = model;
        }

        private void ConvertDialog_Shown(object sender, EventArgs e)
        {
            modelComboBox.SelectedIndex = normalsComboBox.SelectedIndex = textureComboBox.SelectedIndex = 0;
        }

        private void modelButton_Click(object sender, EventArgs e)
        {
            // get the entry node, not subentry node
            node = node.Type == EntryTreeNodeType.SUBENTRY ? (EntryTreeNode)node.Parent : node;

            string item = (string)modelComboBox.SelectedItem;

            saveFileDialog.FileName = node.Text;
            saveFileDialog.Filter = $"{item}|All Files|*.*";
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                if (combineMeshesCheckBox.Checked)
                {
                    switch (modelComboBox.SelectedIndex)
                    {
                        /*case 0: //dae
                            //DAE.Export(saveFileDialog.FileName, model, false, false);
                            break;*/
                        case 0: //obj
                            File.WriteAllText(saveFileDialog.FileName, OBJ.Export(model, (NormalExportMode)normalsComboBox.SelectedIndex));
                            break;
                        /*case 2: //smd
                            File.WriteAllText(saveFileDialog.FileName, SMD.Export(model, calculateNormals));
                            break;
                        case 3: //stl
                            STL.ExportBinary(saveFileDialog.FileName, model, calculateNormals);
                            break;*/
                        default:
                            break;
                    }
                    Message.Success("Saved the model into a single file.");                    
                }
                else
                {
                    for (int i = 0; i < model.Meshes.Count; i++)
                    {
                        string fileName = string.Concat(Path.Combine(Path.GetDirectoryName(saveFileDialog.FileName), Path.GetFileNameWithoutExtension(saveFileDialog.FileName)), "-", i, Path.GetExtension(saveFileDialog.FileName));
                        Model mdl = Model.CreateFromMesh(model.Meshes[i]);
                        switch (modelComboBox.SelectedIndex)
                        {
                            /*case 0: //dae
                                //DAE.Export(fileName, mdl, false, false);
                                break;*/
                            case 0: //obj
                                File.WriteAllText(fileName, OBJ.Export(mdl, (NormalExportMode)normalsComboBox.SelectedIndex, true));
                                break;
                            /*case 2: //smd
                                File.WriteAllText(fileName, SMD.Export(mdl, true));
                                break;
                            case 3: //stl
                                STL.ExportBinary(fileName, mdl, true);
                                break;
                            default:
                                break;*/
                        }
                    }
                    Message.Success("Saved the model into separate files.");
                }
            }
        }

        private void textureButton_Click(object sender, EventArgs e)
        {
            string text = node.Text;
            string tex = "";
            string item = (string)textureComboBox.SelectedItem;
            string ext = item.Substring(item.IndexOf('.') + 1);

            if (File.Exists($"{Helpers.GetTempPath(text)}.dds"))
                tex = $"{Helpers.GetTempPath(text)}.dds";
            else if (File.Exists($"{Helpers.GetTempPath(text)}_TopMip_0.dds"))
                tex = $"{Helpers.GetTempPath(text)}_TopMip_0.dds";
            else if (File.Exists($"{Helpers.GetTempPath(text)}_Mip0.dds"))
                tex = $"{Helpers.GetTempPath(text)}_Mip0.dds";

            if (!string.IsNullOrEmpty(tex))
            {
                saveFileDialog.FileName = Path.GetFileNameWithoutExtension(tex);
                saveFileDialog.Filter = item;
                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    // copy the file if the user selected DDS
                    if (textureComboBox.SelectedIndex == 0)
                    {
                        File.Copy(tex, saveFileDialog.FileName);
                    }
                    else
                    {
                        Helpers.ConvertDDS(tex, Path.GetDirectoryName(saveFileDialog.FileName), ext, (error) =>
                        {
                            if (error)
                                Message.Fail("Failed to convert the texture.");
                            else
                                Message.Success("Converted the texture.");
                        });
                    }
                }
            }
        }
    }
}