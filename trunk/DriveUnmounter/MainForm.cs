using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.IO;
using Dokan;

namespace DriveUnmounter
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            MessageBox.Show("DriveUnmounter Copyright (c) 2010 ebbes <ebbes.ebbes@gmail.com>\r\n"
                + "This program comes with ABSOLUTELY NO WARRANTY.\r\n"
                + "This is free software, and you are welcome to redistribute it\r\n"
                + "under certain conditions; see \"COPYING.txt\".\r\n"
                + "\r\n"
                + "Use this tool to dismount \"ghost\" drives shown in Windows Explorer.\r\n"
                + "Do NOT use this tool to dismount other drives.",
                "DriveUnmounter", MessageBoxButtons.OK, MessageBoxIcon.Information);
            RefreshDrives();
        }

        private List<char> DriveLetters;

        private void RefreshDrives()
        {
            DriveLetters = DismountableDriveLetters();

            comboDrives.Items.Clear();

            if (DriveLetters.Count > 0)
            {
                foreach (char DriveLetter in DriveLetters)
                {
                    comboDrives.Items.Add(string.Format("{0}:\\", DriveLetter.ToString().ToUpper()));
                }
                btnDismount.Enabled = true;
                comboDrives.Enabled = true;
            }
            else
            {
                comboDrives.Items.Add("N/A");
                btnDismount.Enabled = false;
                comboDrives.Enabled = false;
            }

            comboDrives.SelectedIndex = 0;
        }

        private List<char> DismountableDriveLetters()
        {
            List<char> DismountableLetters = new List<char>();

            foreach (DriveInfo Drive in DriveInfo.GetDrives())
            {
                if (Drive.DriveType == DriveType.Removable)
                {
                    DismountableLetters.Add(Drive.Name.ToLower().ToCharArray(0, 1)[0]);
                }
            }

            return DismountableLetters;
        }

        private void btnDismount_Click(object sender, EventArgs e)
        {
            if (DriveLetters.Count > 0)
            {
                DokanNet.DokanUnmount(DriveLetters[comboDrives.SelectedIndex]);
                if (!DismountableDriveLetters().Contains(DriveLetters[comboDrives.SelectedIndex]))
                    MessageBox.Show("The drive was successfully dismounted.", "DriveUnmounter", MessageBoxButtons.OK, MessageBoxIcon.Information);
                else
                    MessageBox.Show("DriveUnmounter was unable to dismount the drive.\r\n"
                        + "Rebooting Windows could result in dismounting the drive.", "DriveUnmounter", MessageBoxButtons.OK, MessageBoxIcon.Error);
                RefreshDrives();
            }
        }
    }
}
