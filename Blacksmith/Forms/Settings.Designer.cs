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
            this.panel4 = new System.Windows.Forms.Panel();
            this.label4 = new System.Windows.Forms.Label();
            this.tempTextBox = new System.Windows.Forms.TextBox();
            this.tempButton = new System.Windows.Forms.Button();
            this.folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label5 = new System.Windows.Forms.Label();
            this.filelistSeparatorComboBox = new System.Windows.Forms.ComboBox();
            this.deleteTempCheckbox = new System.Windows.Forms.CheckBox();
            this.colorDialog = new System.Windows.Forms.ColorDialog();
            this.threeDBGColorBtn = new System.Windows.Forms.Button();
            this.imageBGColorBtn = new System.Windows.Forms.Button();
            this.groupBox.SuspendLayout();
            this.panel3.SuspendLayout();
            this.panel2.SuspendLayout();
            this.panel1.SuspendLayout();
            this.panel4.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox
            // 
            this.groupBox.Controls.Add(this.panel3);
            this.groupBox.Controls.Add(this.panel2);
            this.groupBox.Controls.Add(this.panel1);
            this.groupBox.Location = new System.Drawing.Point(12, 12);
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
            // panel4
            // 
            this.panel4.Controls.Add(this.label4);
            this.panel4.Controls.Add(this.tempTextBox);
            this.panel4.Controls.Add(this.tempButton);
            this.panel4.Location = new System.Drawing.Point(18, 206);
            this.panel4.Name = "panel4";
            this.panel4.Size = new System.Drawing.Size(350, 50);
            this.panel4.TabIndex = 1;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(3, 2);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(101, 13);
            this.label4.TabIndex = 0;
            this.label4.Text = "Temporary File Path";
            // 
            // tempTextBox
            // 
            this.tempTextBox.Location = new System.Drawing.Point(6, 22);
            this.tempTextBox.Name = "tempTextBox";
            this.tempTextBox.Size = new System.Drawing.Size(260, 20);
            this.tempTextBox.TabIndex = 0;
            this.tempTextBox.TextChanged += new System.EventHandler(this.tempTextBox_TextChanged);
            // 
            // tempButton
            // 
            this.tempButton.Location = new System.Drawing.Point(272, 20);
            this.tempButton.Name = "tempButton";
            this.tempButton.Size = new System.Drawing.Size(75, 23);
            this.tempButton.TabIndex = 0;
            this.tempButton.Text = "Choose";
            this.tempButton.UseVisualStyleBackColor = true;
            this.tempButton.Click += new System.EventHandler(this.tempButton_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.imageBGColorBtn);
            this.groupBox1.Controls.Add(this.threeDBGColorBtn);
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Controls.Add(this.filelistSeparatorComboBox);
            this.groupBox1.Controls.Add(this.deleteTempCheckbox);
            this.groupBox1.Location = new System.Drawing.Point(380, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(220, 244);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Behavior/Appearance";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(8, 44);
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
            this.filelistSeparatorComboBox.Location = new System.Drawing.Point(101, 41);
            this.filelistSeparatorComboBox.Name = "filelistSeparatorComboBox";
            this.filelistSeparatorComboBox.Size = new System.Drawing.Size(113, 21);
            this.filelistSeparatorComboBox.TabIndex = 0;
            this.filelistSeparatorComboBox.SelectedIndexChanged += new System.EventHandler(this.filelistSeparatorComboBox_SelectedIndexChanged);
            // 
            // deleteTempCheckbox
            // 
            this.deleteTempCheckbox.AutoSize = true;
            this.deleteTempCheckbox.Location = new System.Drawing.Point(11, 19);
            this.deleteTempCheckbox.Name = "deleteTempCheckbox";
            this.deleteTempCheckbox.Size = new System.Drawing.Size(198, 17);
            this.deleteTempCheckbox.TabIndex = 0;
            this.deleteTempCheckbox.Text = "Delete Temporary Files upon Closing";
            this.deleteTempCheckbox.UseVisualStyleBackColor = true;
            this.deleteTempCheckbox.CheckedChanged += new System.EventHandler(this.deleteTempCheckbox_CheckedChanged);
            // 
            // threeDBGColorBtn
            // 
            this.threeDBGColorBtn.Enabled = false;
            this.threeDBGColorBtn.Location = new System.Drawing.Point(11, 68);
            this.threeDBGColorBtn.Name = "threeDBGColorBtn";
            this.threeDBGColorBtn.Size = new System.Drawing.Size(203, 23);
            this.threeDBGColorBtn.TabIndex = 0;
            this.threeDBGColorBtn.Text = "3D Viewer Background Color";
            this.threeDBGColorBtn.UseVisualStyleBackColor = true;
            this.threeDBGColorBtn.Click += new System.EventHandler(this.threeDBGColorBtn_Click);
            // 
            // imageBGColorBtn
            // 
            this.imageBGColorBtn.Location = new System.Drawing.Point(11, 97);
            this.imageBGColorBtn.Name = "imageBGColorBtn";
            this.imageBGColorBtn.Size = new System.Drawing.Size(203, 23);
            this.imageBGColorBtn.TabIndex = 1;
            this.imageBGColorBtn.Text = "Image Viewer Background Color";
            this.imageBGColorBtn.UseVisualStyleBackColor = true;
            this.imageBGColorBtn.Click += new System.EventHandler(this.imageBGColorBtn_Click);
            // 
            // Settings
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(614, 271);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.panel4);
            this.Controls.Add(this.groupBox);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Settings";
            this.ShowIcon = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.Text = "Settings";
            this.Load += new System.EventHandler(this.Settings_Load);
            this.groupBox.ResumeLayout(false);
            this.panel3.ResumeLayout(false);
            this.panel3.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.panel4.ResumeLayout(false);
            this.panel4.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

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
        private System.Windows.Forms.Panel panel4;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox tempTextBox;
        private System.Windows.Forms.Button tempButton;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.CheckBox deleteTempCheckbox;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.ComboBox filelistSeparatorComboBox;
        private System.Windows.Forms.Button imageBGColorBtn;
        private System.Windows.Forms.Button threeDBGColorBtn;
        private System.Windows.Forms.ColorDialog colorDialog;
    }
}