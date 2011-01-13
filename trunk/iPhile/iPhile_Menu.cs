using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using Manzana;
using System.Threading;

namespace iPhile
{
    public sealed partial class iPhile
    {

        /// <summary>
        /// Populates context menu
        /// </summary>
        void ContextMenuStrip_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            notifyMenu.Items.Clear();

            //About
            ToolStripMenuItem mnuItem = new ToolStripMenuItem("About", iPhileResources.question.ToBitmap(), mnuAbout_Click, "mnuAbout");
            notifyMenu.Items.Add(mnuItem);

            //Separator
            notifyMenu.Items.Add("-");
            if (iDevices.Count > 0)
            {
                //extracted to separate call to make this void here readable.
                PopulateDeviceMenu();
            }
            else
            {
                mnuItem = new ToolStripMenuItem("No devices connected.");
                mnuItem.Enabled = false;
                notifyMenu.Items.Add(mnuItem);
            }

            //Separator
            notifyMenu.Items.Add("-");

            //Exit
            ToolStripMenuItem mnuExit = new ToolStripMenuItem("Exit", iPhileResources.x.ToBitmap(), mnuExit_Click, "mnuAbout");
            notifyMenu.Items.Add(mnuExit);

            //Don't cancel showing the menu
            e.Cancel = false;
        }

        /// <summary>
        /// populates the device context menu with connected devices
        /// </summary>
        private void PopulateDeviceMenu()
        {
            foreach (iPhone iDevice in iDevices.Values)
            {
                //Main Entry
                ToolStripMenuItem mnuDevice = new ToolStripMenuItem(iDevice.DeviceNameFixed + (iDevice.DriveLetter != '0' ? " (" + iDevice.DriveLetter.ToString().ToUpper() + ":\\)" : ""));
                if (iDevice.DeviceTypeFixed == "iPhone")
                    mnuDevice.Image = iPhileResources.iphone.ToBitmap();
                else if (iDevice.DeviceTypeFixed == "iPod")
                    mnuDevice.Image = iPhileResources.ipod.ToBitmap();
                else if (iDevice.DeviceTypeFixed == "iPad") //I hope this actually says "iPad" and nothing else
                    mnuDevice.Image = iPhileResources.ipad.ToBitmap(); //I want an iPad :-(

                //Sub entries
                //DeviceName
                ToolStripMenuItem subItem = new ToolStripMenuItem(iDevice.DeviceNameFixed);
                subItem.Image = mnuDevice.Image;
                subItem.Enabled = false;
                mnuDevice.DropDownItems.Add(subItem);

                //Version & jailbreak status
                if (iDevice.IsJailbreak)
                {
                    subItem = new ToolStripMenuItem(iDevice.DeviceVersionFixed + " jailbroken");
                    subItem.Image = iPhileResources.pwnapple.ToBitmap();
                }
                else
                {
                    subItem = new ToolStripMenuItem(iDevice.DeviceVersionFixed);
                    subItem.Image = iPhileResources.apple.ToBitmap();
                }
                subItem.Enabled = false;
                mnuDevice.DropDownItems.Add(subItem);

                //ActivationState
                subItem = new ToolStripMenuItem(iDevice.ActivationStateFixed);
                if (iDevice.ActivationStateFixed == "Activated" || iDevice.ActivationStateFixed == "WildcardActivated")
                    subItem.Image = iPhileResources.activated.ToBitmap();
                else
                    subItem.Image = iPhileResources.unactivated.ToBitmap();
                subItem.Enabled = false;
                mnuDevice.DropDownItems.Add(subItem);

                //Separator
                mnuDevice.DropDownItems.Add("-");

                if (iDevice.DriveLetter != '0')
                {
                    //Link to Explorer
                    subItem = new ToolStripMenuItem("Mounted to " + iDevice.DriveLetter.ToString().ToUpper() + ":\\ [Explorer]", iPhileResources.e.ToBitmap(), mnuOpen_Click, iDevice.DriveLetter.ToString().ToUpper());
                    mnuDevice.DropDownItems.Add(subItem);
                    //Link to unmount
                    subItem = new ToolStripMenuItem("Unmount device", iPhileResources.u.ToBitmap(), mnuDismount_Click, iDevice.DeviceIdFixed);
                    mnuDevice.DropDownItems.Add(subItem);
                }
                else
                {
                    if (AvailableDriveLetters().Count > 0)
                    {
                        //Link to mount
                        subItem = new ToolStripMenuItem("Mount device", iPhileResources.M.ToBitmap(), mnuMount_Click, "0;" + iDevice.DeviceIdFixed);
                        mnuDevice.DropDownItems.Add(subItem);

                        subItem = new ToolStripMenuItem("Mount device under...");
                        subItem.Image = iPhileResources.M.ToBitmap();

                        foreach (char DriveLetter in AvailableDriveLetters())
                        {
                            ToolStripMenuItem subsubItem = new ToolStripMenuItem(DriveLetter.ToString().ToUpper() + ":\\", null, mnuMount_Click, DriveLetter.ToString() + ";" + iDevice.DeviceIdFixed);
                            subItem.DropDownItems.Add(subsubItem);
                        }

                        mnuDevice.DropDownItems.Add(subItem);
                    }
                    else
                    {
                        subItem = new ToolStripMenuItem("No drive letter available");
                        mnuDevice.DropDownItems.Add(subItem);
                    }
                }

                notifyMenu.Items.Add(mnuDevice);
            }
        }

        /// <summary>
        /// Show about screen
        /// </summary>
        private void mnuAbout_Click(object Sender, EventArgs e)
        {
            MessageBox.Show("iPhile Copyright (c) 2010 ebbes <ebbes.ebbes@gmail.com>\r\n"
                + "This program comes with ABSOLUTELY NO WARRANTY.\r\n"
                + "This is free software, and you are welcome to redistribute it\r\n"
                + "under certain conditions; see \"COPYING.txt\".\r\n", "iPhile", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        /// <summary>
        /// Exit iPhile
        /// </summary>
        private void mnuExit_Click(object sender, EventArgs e)
        {
            //Dismount every iDevice
            foreach (iPhone iDevice in iDevices.Values)
            {
                Disconnect_FS(iDevice);
            }
            Debugger.Close();

            SavePreferredMountPoints();

            Application.Exit();
        }

        /// <summary>
        /// Dismount selected device (until it gets disconnected and reconnected)
        /// </summary>
        private void mnuDismount_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem Sender = (ToolStripMenuItem)sender;
            string UDID = Sender.Name;

            PhoneThreads[UDID].Abort();
            PhoneThreads[UDID] = null;
            Disconnect_FS(iDevices[UDID]);
            iDevices[UDID].DriveLetter = '0';
        }

        private void mnuMount_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem Sender = (ToolStripMenuItem)sender;
            char Letter = Sender.Name.Split(';')[0].ToLower().ToCharArray(0, 1)[0];
            string UDID = Sender.Name.Split(';')[1];

            if (Letter == '0')
            {
                if (PreferredMountPoints.TryGetValue(iDevices[UDID].DeviceIdFixed, out Letter))
                {
                    if (IsDriveLetterAvailable(Letter))
                        iDevices[UDID].DriveLetter = Letter;
                    else
                    {
                        MessageBox.Show("Preferred mount point not available. Using first free point instead.", "iPhile", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        Letter = DriveLetter();
                        iDevices[UDID].DriveLetter = Letter;
                        PreferredMountPoints[iDevices[UDID].DeviceIdFixed] = Letter;
                    }
                }
                else
                {
                    Letter = DriveLetter();
                    iDevices[UDID].DriveLetter = Letter;
                    PreferredMountPoints[iDevices[UDID].DeviceIdFixed] = Letter;
                }
            }
            else
            {
                iDevices[UDID].DriveLetter = Letter;
                PreferredMountPoints[iDevices[UDID].DeviceIdFixed] = Letter;
            }

            PhoneThreads[UDID] = new Thread(new ParameterizedThreadStart(Connect_FS));
            PhoneThreads[UDID].Start(iDevices[UDID]);
        }

        /// <summary>
        /// Open mount point in explorer
        /// </summary>
        private void mnuOpen_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem Sender = (ToolStripMenuItem)sender;
            string Path = Sender.Name + ":\\";
            System.Diagnostics.Process.Start("explorer.exe", Path);
        }

        /// <summary>
        /// Copy UDID to clipboard
        /// </summary>
        private void mnuUDID_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(((ToolStripMenuItem)sender).Name);
            MessageBox.Show(string.Format("UDID {0} was copied to clipboard.", ((ToolStripMenuItem)sender).Name), "iPhile", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
