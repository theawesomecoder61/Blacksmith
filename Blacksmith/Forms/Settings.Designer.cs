namespace Blacksmith.Forms
{
    partial class Settings
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
            this.groupBox = new System.Windows.Forms.GroupBox();
            this.panel3 = new System.Windows.Forms.Panel();
            this.label3 = new System.Windows.Forms.Label();
            this.acOrTextBox = new System.Windows.Forms.TextBox();
            this.acOrButton = new System.Windows.Forms.Button();
            this.panel2 = new System.Windows.Forms.Panel();
            this.label2 = new System.Windows.Forms.Label();
            this.acOdTextBox = new System.Windows.Forms.TextBox();
            this.acOdButton = new System.Windows.Forms.Button();
            this.panel1 = new System.Windows.Forms.Panel();
            this.label1 = new System.Windows.Forms.Label();
            this.steepTextBox = new System.Windows.Forms.TextBox();
            this.steepButton = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this.tempTextBox = new System.Windows.Forms.TextBox();
            this.tempButton = new System.Windows.Forms.Button();
            this.folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label13 = new System.Windows.Forms.Label();
            this.popupComboBox = new System.Windows.Forms.ComboBox();
            this.label5 = new System.Windows.Forms.Label();
            this.filelistSeparatorComboBox = new System.Windows.Forms.ComboBox();
            this.deleteTempCheckbox = new System.Windows.Forms.CheckBox();
            this.threeDBGColorBtn = new System.Windows.Forms.Button();
            this.colorDialog = new System.Windows.Forms.ColorDialog();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.label10 = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.sensitivityTrackBar = new System.Windows.Forms.TrackBar();
            this.label12 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.pointSizeBar = new System.Windows.Forms.TrackBar();
            this.label7 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.renderModeComboBox = new System.Windows.Forms.ComboBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.saveAndLoadToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.loadFromFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveToFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveFileDialog = new System.Windows.Forms.SaveFileDialog();
            this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.fixNormalsCheckBox = new System.Windows.Forms.CheckBox();
            this.groupBox.SuspendLayout();
            this.panel3.SuspendLayout();
            this.panel2.SuspendLayout();
            this.panel1.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.sensitivityTrackBar)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pointSizeBar)).BeginInit();
            this.groupBox3.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox
            // 
            this.groupBox.Controls.Add(this.panel3);
            this.groupBox.Controls.Add(this.panel2);
            this.groupBox.Controls.Add(this.panel1);
            this.groupBox.Location = new System.Drawing.Point(12, 27);
            this.groupBox.Name = "groupBox";
            this.groupBox.Size = new System.Drawing.Size(362, 188);
            this.groupBox.TabIndex = 0;
            this.groupBox.TabStop = false;
            this.groupBox.Text = "Game Paths";
            // 
            // panel3
            // 
            this.panel3.Controls.Add(this.label3);
            this.panel3.Controls.Add(this.acOrTextBox);
            this.panel3.Controls.Add(this.acOrButton);
            this.panel3.Location = new System.Drawing.Point(6, 75);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(350, 50);
            this.panel3.TabIndex = 2;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(3, 2);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(124, 13);
            this.label3.TabIndex = 0;
            this.label3.Text = "Assassin\'s Creed: Origins";
            // 
            // acOrTextBox
            // 
            this.acOrTextBox.Location = new System.Drawing.Point(6, 22);
            this.acOrTextBox.Name = "acOrTextBox";
            this.acOrTextBox.Size = new System.Drawing.Size(260, 20);
            this.acOrTextBox.TabIndex = 0;
            this.acOrTextBox.TextChanged += new System.EventHandler(this.acOrTextBox_TextChanged);
            // 
            // acOrButton
            // 
            this.acOrButton.Location = new System.Drawing.Point(272, 20);
            this.acOrButton.Name = "acOrButton";
            this.acOrButton.Size = new System.Drawing.Size(75, 23);
            this.acOrButton.TabIndex = 0;
            this.acOrButton.Text = "Choose";
            this.acOrButton.UseVisualStyleBackColor = true;
            this.acOrButton.Click += new System.EventHandler(this.acOrButton_Click);
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.label2);
            this.panel2.Controls.Add(this.acOdTextBox);
            this.panel2.Controls.Add(this.acOdButton);
            this.panel2.Location = new System.Drawing.Point(6, 19);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(350, 50);
            this.panel2.TabIndex = 1;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(3, 2);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(132, 13);
            this.label2.TabIndex = 0;
            this.label2.Text = "Assassin\'s Creed: Odyssey";
            // 
            // acOdTextBox
            // 
            this.acOdTextBox.Location = new System.Drawing.Point(6, 22);
            this.acOdTextBox.Name = "acOdTextBox";
            this.acOdTextBox.Size = new System.Drawing.Size(260, 20);
            this.acOdTextBox.TabIndex = 0;
            this.acOdTextBox.TextChanged += new System.EventHandler(this.acOdTextBox_TextChanged);
            // 
            // acOdButton
            // 
            this.acOdButton.Location = new System.Drawing.Point(272, 20);
            this.acOdButton.Name = "acOdButton";
            this.acOdButton.Size = new System.Drawing.Size(75, 23);
            this.acOdButton.TabIndex = 0;
            this.acOdButton.Text = "Choose";
            this.acOdButton.UseVisualStyleBackColor = true;
            this.acOdButton.Click += new System.EventHandler(this.acOdButton_Click);
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.label1);
            this.panel1.Controls.Add(this.steepTextBox);
            this.panel1.Controls.Add(this.steepButton);
            this.panel1.Location = new System.Drawing.Point(6, 131);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(350, 50);
            this.panel1.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 2);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(35, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Steep";
            // 
            // steepTextBox
            // 
            this.steepTextBox.Location = new System.Drawing.Point(6, 22);
            this.steepTextBox.Name = "steepTextBox";
            this.steepTextBox.Size = new System.Drawing.Size(260, 20);
            this.steepTextBox.TabIndex = 0;
            this.steepTextBox.TextChanged += new System.EventHandler(this.steepTextBox_TextChanged);
            // 
            // steepButton
            // 
            this.steepButton.Location = new System.Drawing.Point(272, 20);
            this.steepButton.Name = "steepButton";
            this.steepButton.Size = new System.Drawing.Size(75, 23);
            this.steepButton.TabIndex = 0;
            this.steepButton.Text = "Choose";
            this.steepButton.UseVisualStyleBackColor = true;
            this.steepButton.Click += new System.EventHandler(this.steepButton_Click);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(9, 16);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(101, 13);
            this.label4.TabIndex = 0;
            this.label4.Text = "Temporary File Path";
            // 
            // tempTextBox
            // 
            this.tempTextBox.Location = new System.Drawing.Point(12, 36);
            this.tempTextBox.Name = "tempTextBox";
            this.tempTextBox.Size = new System.Drawing.Size(260, 20);
            this.tempTextBox.TabIndex = 0;
            this.tempTextBox.TextChanged += new System.EventHandler(this.tempTextBox_TextChanged);
            // 
            // tempButton
            // 
            this.tempButton.Location = new System.Drawing.Point(278, 34);
            this.tempButton.Name = "tempButton";
            this.tempButton.Size = new System.Drawing.Size(75, 23);
            this.tempButton.TabIndex = 0;
            this.tempButton.Text = "Choose";
            this.tempButton.UseVisualStyleBackColor = true;
            this.tempButton.Click += new System.EventHandler(this.tempButton_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.fixNormalsCheckBox);
            this.groupBox1.Controls.Add(this.label13);
            this.groupBox1.Controls.Add(this.popupComboBox);
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Controls.Add(this.filelistSeparatorComboBox);
            this.groupBox1.Location = new System.Drawing.Point(380, 221);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(220, 125);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Miscellaneous";
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(8, 49);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(71, 13);
            this.label13.TabIndex = 0;
            this.label13.Text = "Hide Popups:";
            // 
            // popupComboBox
            // 
            this.popupComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.popupComboBox.FormattingEnabled = true;
            this.popupComboBox.Items.AddRange(new object[] {
            "Success",
            "Failure",
            "Success & Failure",
            "None"});
            this.popupComboBox.Location = new System.Drawing.Point(81, 46);
            this.popupComboBox.Name = "popupComboBox";
            this.popupComboBox.Size = new System.Drawing.Size(133, 21);
            this.popupComboBox.TabIndex = 0;
            this.popupComboBox.SelectedIndexChanged += new System.EventHandler(this.popupComboBox_SelectedIndexChanged);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(8, 22);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(87, 13);
            this.label5.TabIndex = 0;
            this.label5.Text = "Filelist Separator:";
            // 
            // filelistSeparatorComboBox
            // 
            this.filelistSeparatorComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.filelistSeparatorComboBox.FormattingEnabled = true;
            this.filelistSeparatorComboBox.Items.AddRange(new object[] {
            "Tabs",
            "Commas (CSV)"});
            this.filelistSeparatorComboBox.Location = new System.Drawing.Point(101, 19);
            this.filelistSeparatorComboBox.Name = "filelistSeparatorComboBox";
            this.filelistSeparatorComboBox.Size = new System.Drawing.Size(113, 21);
            this.filelistSeparatorComboBox.TabIndex = 0;
            this.filelistSeparatorComboBox.SelectedIndexChanged += new System.EventHandler(this.filelistSeparatorComboBox_SelectedIndexChanged);
            // 
            // deleteTempCheckbox
            // 
            this.deleteTempCheckbox.AutoSize = true;
            this.deleteTempCheckbox.Location = new System.Drawing.Point(12, 62);
            this.deleteTempCheckbox.Name = "deleteTempCheckbox";
            this.deleteTempCheckbox.Size = new System.Drawing.Size(198, 17);
            this.deleteTempCheckbox.TabIndex = 0;
            this.deleteTempCheckbox.Text = "Delete Temporary Files upon Closing";
            this.deleteTempCheckbox.UseVisualStyleBackColor = true;
            this.deleteTempCheckbox.CheckedChanged += new System.EventHandler(this.deleteTempCheckbox_CheckedChanged);
            // 
            // threeDBGColorBtn
            // 
            this.threeDBGColorBtn.Location = new System.Drawing.Point(11, 153);
            this.threeDBGColorBtn.Name = "threeDBGColorBtn";
            this.threeDBGColorBtn.Size = new System.Drawing.Size(203, 23);
            this.threeDBGColorBtn.TabIndex = 0;
            this.threeDBGColorBtn.Text = "Background Color";
            this.threeDBGColorBtn.UseVisualStyleBackColor = true;
            this.threeDBGColorBtn.Click += new System.EventHandler(this.threeDBGColorBtn_Click);
            // 
            // colorDialog
            // 
            this.colorDialog.AnyColor = true;
            this.colorDialog.FullOpen = true;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.label10);
            this.groupBox2.Controls.Add(this.label11);
            this.groupBox2.Controls.Add(this.sensitivityTrackBar);
            this.groupBox2.Controls.Add(this.label12);
            this.groupBox2.Controls.Add(this.label9);
            this.groupBox2.Controls.Add(this.label8);
            this.groupBox2.Controls.Add(this.pointSizeBar);
            this.groupBox2.Controls.Add(this.label7);
            this.groupBox2.Controls.Add(this.label6);
            this.groupBox2.Controls.Add(this.threeDBGColorBtn);
            this.groupBox2.Controls.Add(this.renderModeComboBox);
            this.groupBox2.Location = new System.Drawing.Point(380, 27);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(220, 188);
            this.groupBox2.TabIndex = 0;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "3D Viewer";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.BackColor = System.Drawing.SystemColors.Control;
            this.label10.Enabled = false;
            this.label10.Location = new System.Drawing.Point(186, 129);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(29, 13);
            this.label10.TabIndex = 0;
            this.label10.Text = "High";
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.BackColor = System.Drawing.SystemColors.Control;
            this.label11.Enabled = false;
            this.label11.Location = new System.Drawing.Point(107, 129);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(27, 13);
            this.label11.TabIndex = 0;
            this.label11.Text = "Low";
            // 
            // sensitivityTrackBar
            // 
            this.sensitivityTrackBar.Enabled = false;
            this.sensitivityTrackBar.LargeChange = 2;
            this.sensitivityTrackBar.Location = new System.Drawing.Point(106, 97);
            this.sensitivityTrackBar.Minimum = 1;
            this.sensitivityTrackBar.Name = "sensitivityTrackBar";
            this.sensitivityTrackBar.Size = new System.Drawing.Size(108, 45);
            this.sensitivityTrackBar.TabIndex = 0;
            this.sensitivityTrackBar.Value = 5;
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Enabled = false;
            this.label12.Location = new System.Drawing.Point(8, 104);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(92, 13);
            this.label12.TabIndex = 0;
            this.label12.Text = "Mouse Sensitivity:";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.BackColor = System.Drawing.SystemColors.Control;
            this.label9.Location = new System.Drawing.Point(191, 81);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(19, 13);
            this.label9.TabIndex = 0;
            this.label9.Text = "10";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.BackColor = System.Drawing.SystemColors.Control;
            this.label8.Location = new System.Drawing.Point(78, 81);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(13, 13);
            this.label8.TabIndex = 0;
            this.label8.Text = "1";
            // 
            // pointSizeBar
            // 
            this.pointSizeBar.LargeChange = 2;
            this.pointSizeBar.Location = new System.Drawing.Point(71, 49);
            this.pointSizeBar.Minimum = 1;
            this.pointSizeBar.Name = "pointSizeBar";
            this.pointSizeBar.Size = new System.Drawing.Size(143, 45);
            this.pointSizeBar.TabIndex = 0;
            this.pointSizeBar.Value = 5;
            this.pointSizeBar.Scroll += new System.EventHandler(this.pointSizeBar_Scroll);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(8, 56);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(57, 13);
            this.label7.TabIndex = 0;
            this.label7.Text = "Point Size:";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(8, 25);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(75, 13);
            this.label6.TabIndex = 0;
            this.label6.Text = "Render Mode:";
            // 
            // renderModeComboBox
            // 
            this.renderModeComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.renderModeComboBox.FormattingEnabled = true;
            this.renderModeComboBox.Items.AddRange(new object[] {
            "Solid",
            "Wireframe",
            "Points"});
            this.renderModeComboBox.Location = new System.Drawing.Point(89, 22);
            this.renderModeComboBox.Name = "renderModeComboBox";
            this.renderModeComboBox.Size = new System.Drawing.Size(125, 21);
            this.renderModeComboBox.TabIndex = 0;
            this.renderModeComboBox.SelectedIndexChanged += new System.EventHandler(this.renderModeComboBox_SelectedIndexChanged);
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.label4);
            this.groupBox3.Controls.Add(this.tempTextBox);
            this.groupBox3.Controls.Add(this.deleteTempCheckbox);
            this.groupBox3.Controls.Add(this.tempButton);
            this.groupBox3.Location = new System.Drawing.Point(12, 221);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(362, 92);
            this.groupBox3.TabIndex = 0;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Temporary Files";
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.saveAndLoadToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(614, 24);
            this.menuStrip1.TabIndex = 1;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // saveAndLoadToolStripMenuItem
            // 
            this.saveAndLoadToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.loadFromFileToolStripMenuItem,
            this.saveToFileToolStripMenuItem});
            this.saveAndLoadToolStripMenuItem.Name = "saveAndLoadToolStripMenuItem";
            this.saveAndLoadToolStripMenuItem.Size = new System.Drawing.Size(95, 20);
            this.saveAndLoadToolStripMenuItem.Text = "Save and Load";
            // 
            // loadFromFileToolStripMenuItem
            // 
            this.loadFromFileToolStripMenuItem.Name = "loadFromFileToolStripMenuItem";
            this.loadFromFileToolStripMenuItem.Size = new System.Drawing.Size(161, 22);
            this.loadFromFileToolStripMenuItem.Text = "Load From File...";
            this.loadFromFileToolStripMenuItem.Click += new System.EventHandler(this.loadFromFileToolStripMenuItem_Click);
            // 
            // saveToFileToolStripMenuItem
            // 
            this.saveToFileToolStripMenuItem.Name = "saveToFileToolStripMenuItem";
            this.saveToFileToolStripMenuItem.Size = new System.Drawing.Size(161, 22);
            this.saveToFileToolStripMenuItem.Text = "Save To File...";
            this.saveToFileToolStripMenuItem.Click += new System.EventHandler(this.saveToFileToolStripMenuItem_Click);
            // 
            // saveFileDialog
            // 
            this.saveFileDialog.FileName = "settings";
            this.saveFileDialog.Filter = "INI Files|*.ini|All Files|*.l*";
            // 
            // openFileDialog
            // 
            this.openFileDialog.Filter = "INI Files|*.ini|All Files|*.l*";
            // 
            // fixNormalsCheckBox
            // 
            this.fixNormalsCheckBox.AutoSize = true;
            this.fixNormalsCheckBox.Location = new System.Drawing.Point(11, 73);
            this.fixNormalsCheckBox.Name = "fixNormalsCheckBox";
            this.fixNormalsCheckBox.Size = new System.Drawing.Size(187, 43);
            this.fixNormalsCheckBox.TabIndex = 0;
            this.fixNormalsCheckBox.Text = "Automatically fix normal maps (flips\r\nblue channel, fixes green/yellow\r\nappearanc" +
    "e)";
            this.fixNormalsCheckBox.UseVisualStyleBackColor = true;
            this.fixNormalsCheckBox.CheckedChanged += new System.EventHandler(this.fixNormalsCheckBox_CheckedChanged);
            // 
            // Settings
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(614, 361);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.groupBox);
            this.Controls.Add(this.menuStrip1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.KeyPreview = true;
            this.MainMenuStrip = this.menuStrip1;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Settings";
            this.ShowIcon = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.Text = "Settings";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Settings_FormClosing);
            this.Load += new System.EventHandler(this.Settings_Load);
            this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.Settings_KeyUp);
            this.groupBox.ResumeLayout(false);
            this.panel3.ResumeLayout(false);
            this.panel3.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.sensitivityTrackBar)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pointSizeBar)).EndInit();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox acOrTextBox;
        private System.Windows.Forms.TextBox acOdTextBox;
        private System.Windows.Forms.TextBox steepTextBox;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.Button acOrButton;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Button acOdButton;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button steepButton;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox tempTextBox;
        private System.Windows.Forms.Button tempButton;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.CheckBox deleteTempCheckbox;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.ComboBox filelistSeparatorComboBox;
        private System.Windows.Forms.Button threeDBGColorBtn;
        private System.Windows.Forms.ColorDialog colorDialog;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.ComboBox renderModeComboBox;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TrackBar pointSizeBar;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.TrackBar sensitivityTrackBar;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.ComboBox popupComboBox;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem saveAndLoadToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem loadFromFileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveToFileToolStripMenuItem;
        private System.Windows.Forms.SaveFileDialog saveFileDialog;
        private System.Windows.Forms.OpenFileDialog openFileDialog;
        private System.Windows.Forms.CheckBox fixNormalsCheckBox;
    }
}