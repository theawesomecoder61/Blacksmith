namespace Blacksmith.Forms
{
    partial class FindDialog
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
            this.queryTextBox = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.findNextBtn = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // queryTextBox
            // 
            this.queryTextBox.Location = new System.Drawing.Point(56, 12);
            this.queryTextBox.Name = "queryTextBox";
            this.queryTextBox.Size = new System.Drawing.Size(170, 20);
            this.queryTextBox.TabIndex = 0;
            this.queryTextBox.TextChanged += new System.EventHandler(this.queryTextBox_TextChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 15);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(38, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Query:";
            // 
            // findNextBtn
            // 
            this.findNextBtn.Location = new System.Drawing.Point(15, 41);
            this.findNextBtn.Name = "findNextBtn";
            this.findNextBtn.Size = new System.Drawing.Size(227, 23);
            this.findNextBtn.TabIndex = 0;
            this.findNextBtn.Text = "Find Next";
            this.findNextBtn.UseVisualStyleBackColor = true;
            this.findNextBtn.Click += new System.EventHandler(this.findNextBtn_Click);
            // 
            // FindDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(254, 76);
            this.Controls.Add(this.findNextBtn);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.queryTextBox);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "FindDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "Find";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox queryTextBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button findNextBtn;
    }
}