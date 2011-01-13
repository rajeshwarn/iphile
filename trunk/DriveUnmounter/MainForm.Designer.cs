namespace DriveUnmounter
{
    partial class MainForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.comboDrives = new System.Windows.Forms.ComboBox();
            this.btnDismount = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // comboDrives
            // 
            this.comboDrives.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboDrives.FormattingEnabled = true;
            this.comboDrives.Location = new System.Drawing.Point(31, 13);
            this.comboDrives.Name = "comboDrives";
            this.comboDrives.Size = new System.Drawing.Size(75, 21);
            this.comboDrives.TabIndex = 0;
            // 
            // btnDismount
            // 
            this.btnDismount.Location = new System.Drawing.Point(109, 12);
            this.btnDismount.Name = "btnDismount";
            this.btnDismount.Size = new System.Drawing.Size(75, 23);
            this.btnDismount.TabIndex = 1;
            this.btnDismount.Text = "&Dismount";
            this.btnDismount.UseVisualStyleBackColor = true;
            this.btnDismount.Click += new System.EventHandler(this.btnDismount_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(217, 47);
            this.Controls.Add(this.btnDismount);
            this.Controls.Add(this.comboDrives);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "DriveUnmounter";
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ComboBox comboDrives;
        private System.Windows.Forms.Button btnDismount;
    }
}