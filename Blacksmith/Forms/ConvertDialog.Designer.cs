namespace Blacksmith.Forms
{
    partial class ConvertDialog
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.label3 = new System.Windows.Forms.Label();
            this.normalsComboBox = new System.Windows.Forms.ComboBox();
            this.combineMeshesCheckBox = new System.Windows.Forms.CheckBox();
            this.modelButton = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.modelComboBox = new System.Windows.Forms.ComboBox();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.textureButton = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.textureComboBox = new System.Windows.Forms.ComboBox();
            this.saveFileDialog = new System.Windows.Forms.SaveFileDialog();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl1
            // 
            this.tabControl1.Appearance = System.Windows.Forms.TabAppearance.Buttons;
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.ItemSize = new System.Drawing.Size(0, 1);
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Multiline = true;
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(254, 150);
            this.tabControl1.SizeMode = System.Windows.Forms.TabSizeMode.Fixed;
            this.tabControl1.TabIndex = 0;
            // 
            // tabPage1
            // 
            this.tabPage1.BackColor = System.Drawing.SystemColors.Control;
            this.tabPage1.Controls.Add(this.label3);
            this.tabPage1.Controls.Add(this.normalsComboBox);
            this.tabPage1.Controls.Add(this.combineMeshesCheckBox);
            this.tabPage1.Controls.Add(this.modelButton);
            this.tabPage1.Controls.Add(this.label1);
            this.tabPage1.Controls.Add(this.modelComboBox);
            this.tabPage1.Location = new System.Drawing.Point(4, 5);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(246, 141);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Model";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(8, 59);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(48, 13);
            this.label3.TabIndex = 0;
            this.label3.Text = "Normals:";
            // 
            // normalsComboBox
            // 
            this.normalsComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.normalsComboBox.FormattingEnabled = true;
            this.normalsComboBox.Items.AddRange(new object[] {
            "Extracted from Model Data",
            "Calculated",
            "None/Excluded"});
            this.normalsComboBox.Location = new System.Drawing.Point(62, 56);
            this.normalsComboBox.Name = "normalsComboBox";
            this.normalsComboBox.Size = new System.Drawing.Size(176, 21);
            this.normalsComboBox.TabIndex = 2;
            // 
            // combineMeshesCheckBox
            // 
            this.combineMeshesCheckBox.AutoSize = true;
            this.combineMeshesCheckBox.Checked = true;
            this.combineMeshesCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.combineMeshesCheckBox.Location = new System.Drawing.Point(11, 33);
            this.combineMeshesCheckBox.Name = "combineMeshesCheckBox";
            this.combineMeshesCheckBox.Size = new System.Drawing.Size(187, 17);
            this.combineMeshesCheckBox.TabIndex = 1;
            this.combineMeshesCheckBox.Text = "Combine Meshes into a Single File";
            this.combineMeshesCheckBox.UseVisualStyleBackColor = true;
            // 
            // modelButton
            // 
            this.modelButton.Location = new System.Drawing.Point(8, 93);
            this.modelButton.Name = "modelButton";
            this.modelButton.Size = new System.Drawing.Size(230, 40);
            this.modelButton.TabIndex = 3;
            this.modelButton.Text = "Save";
            this.modelButton.UseVisualStyleBackColor = true;
            this.modelButton.Click += new System.EventHandler(this.modelButton_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(8, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(42, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Format:";
            // 
            // modelComboBox
            // 
            this.modelComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.modelComboBox.FormattingEnabled = true;
            this.modelComboBox.Location = new System.Drawing.Point(56, 6);
            this.modelComboBox.Name = "modelComboBox";
            this.modelComboBox.Size = new System.Drawing.Size(182, 21);
            this.modelComboBox.TabIndex = 0;
            // 
            // tabPage2
            // 
            this.tabPage2.BackColor = System.Drawing.SystemColors.Control;
            this.tabPage2.Controls.Add(this.textureButton);
            this.tabPage2.Controls.Add(this.label2);
            this.tabPage2.Controls.Add(this.textureComboBox);
            this.tabPage2.Location = new System.Drawing.Point(4, 5);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(246, 141);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Texture";
            // 
            // textureButton
            // 
            this.textureButton.Location = new System.Drawing.Point(8, 93);
            this.textureButton.Name = "textureButton";
            this.textureButton.Size = new System.Drawing.Size(230, 40);
            this.textureButton.TabIndex = 1;
            this.textureButton.Text = "Save";
            this.textureButton.UseVisualStyleBackColor = true;
            this.textureButton.Click += new System.EventHandler(this.textureButton_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(8, 9);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(42, 13);
            this.label2.TabIndex = 0;
            this.label2.Text = "Format:";
            // 
            // textureComboBox
            // 
            this.textureComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.textureComboBox.FormattingEnabled = true;
            this.textureComboBox.Items.AddRange(new object[] {
            "DDS|*.dds",
            "JPEG|*.jpg",
            "PNG|*.png",
            "TGA|*.tga",
            "TIFF|*.tif"});
            this.textureComboBox.Location = new System.Drawing.Point(56, 6);
            this.textureComboBox.Name = "textureComboBox";
            this.textureComboBox.Size = new System.Drawing.Size(182, 21);
            this.textureComboBox.TabIndex = 0;
            // 
            // ConvertDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(254, 150);
            this.Controls.Add(this.tabControl1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.KeyPreview = true;
            this.Name = "ConvertDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "Save As...";
            this.Load += new System.EventHandler(this.ConvertDialog_Load);
            this.Shown += new System.EventHandler(this.ConvertDialog_Shown);
            this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.ConvertDialog_KeyUp);
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage1.PerformLayout();
            this.tabPage2.ResumeLayout(false);
            this.tabPage2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox modelComboBox;
        private System.Windows.Forms.Button modelButton;
        private System.Windows.Forms.CheckBox combineMeshesCheckBox;
        private System.Windows.Forms.Button textureButton;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox textureComboBox;
        private System.Windows.Forms.SaveFileDialog saveFileDialog;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox normalsComboBox;
    }
}