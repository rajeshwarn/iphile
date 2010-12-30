/*
 * Copyright (c) 2010 ebbes <ebbes.ebbes@gmail.com>
 * All rights reserved.
 * 
 * This file is part of iPhile.
 *
 * iPhile is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * iPhile is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with iPhile.  If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using Dokan;
using Manzana;

namespace iPhile
{
    public sealed class iPhile
    {
        #region Types
        //Listens to iDevice connect/disconnect events
        private MultiPhone PhoneListener;

        //Contains every mounted iDevice
        private List<iPhone> iDevices = new List<iPhone>();
        //Contains the threads which are used to mount the iDevices
        private List<Thread> PhoneThreads = new List<Thread>();

        public NotifyIcon notifyIcon;
        private ContextMenuStrip notifyMenu;
        #endregion

        #region Constructor
        public iPhile(bool SkipInfo)
        {
            Debugger.Log_Clear();
            if (!SkipInfo)
            {
                MessageBox.Show("iPhile Copyright (c) 2010 ebbes <ebbes.ebbes@gmail.com>\r\n"
                + "This program comes with ABSOLUTELY NO WARRANTY.\r\n"
                + "This is free software, and you are welcome to redistribute it\r\n"
                + "under certain conditions; see \"COPYING.txt\".\r\n"
                + "\r\n"
                + "Debug logging enabled. Loglevel: " + (Debugger.LLevel == Debugger.LogLevel.Error ? "Errors" : (Debugger.LLevel == Debugger.LogLevel.Event ? "Errors + Events" : "All")) + "\r\n"
                + "You can switch the LogLevel by calling iPhile with arguments -loglevel0 -loglevel1 -loglevel2\r\n"
                + "Log will be in file \"log.txt\".\r\n"
                + "\r\n"
                + "Note: You can skip this message by calling iPhile with parameter -skipinfo", "iPhile", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            notifyIcon = new NotifyIcon();
            notifyMenu = new ContextMenuStrip();
            notifyIcon.Icon = iPhileResources.iPhileIcon;
            notifyIcon.ContextMenuStrip = notifyMenu;
            notifyIcon.ContextMenuStrip.Opening += new System.ComponentModel.CancelEventHandler(ContextMenuStrip_Opening);

            PhoneListener = new MultiPhone();
            PhoneListener.Connect += new ConnectEventHandler(Connect_iPhone);
            PhoneListener.Disconnect += new ConnectEventHandler(Connect_iPhone);
        }

        #endregion

        #region Methods
        /// <summary>
        /// Get the lowest available Drive Letter
        /// Function assumes all drive letters from C to Z are free.
        /// (A and B are ignored because Windows won't let us map iPhone Filesystem to them.)
        /// Then every occupied letter is removed from the list and the lowest will be given back.
        /// </summary>
        /// <returns>char containing the lowest available drive letter or '0' (zero char) if no letter available</returns>
        private char DriveLetter()
        {
            List<char> AvailableLetters = new List<char>();

            //don't use a and b cause they only work for floppy drives
            for (int i = Convert.ToInt16('c'); i < Convert.ToInt16('z'); i++)
                AvailableLetters.Add((char)i);

            foreach (DriveInfo Drive in DriveInfo.GetDrives())
                //AvailableLetters.Remove(Convert.ToChar(Drive.Name.Substring(0, 1).ToLower()));
                AvailableLetters.Remove(Drive.Name.ToLower().ToCharArray()[0]); //Should be more clean.

            if (AvailableLetters.Count == 0)
                return '0';
            else
                return AvailableLetters[0];
        }

        /// <summary>
        /// iPhone connected, so connect filesystem
        /// </summary>
        private void Connect_FS(object iDevice)
        {
            iPhone Device = (iPhone)iDevice;
            Device.DriveLetter =  DriveLetter();

            if (Device.DriveLetter == '0')
            {
                MessageBox.Show("No drive letter available to mount device.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Debugger.Log("ERROR: No drive letter available.", 0);
                return;
            }

            Console.WriteLine("Connecting filesystem under " + Device.DriveLetter.ToString().ToUpper() + ":\\>...\r\n");

            Debugger.Log(string.Format("EVENT: {0} connected. Mapping under {1}:\\>", Device.DeviceName, Device.DriveLetter.ToString().ToUpper()), Debugger.LogLevel.Event);

            DokanOptions opt = new DokanOptions();
            opt.DriveLetter = Device.DriveLetter;
            opt.DebugMode = false;
            opt.UseStdErr = false;
            opt.VolumeLabel = Device.DeviceName + (Device.IsJailbreak ? " [root]" : " [Media]");
            opt.UseKeepAlive = false;
            opt.NetworkDrive = false;
            opt.UseAltStream = false;
            opt.Removable = true;
            opt.ThreadCount = 1;

            DokanNet.DokanMain(opt, new iPhoneFS(Device));
        }

        /// <summary>
        /// iPhone disconnected, disconnect filesystem
        /// </summary>
        private void Disconnect_FS(iPhone iDevice)
        {
            Console.WriteLine("Disconnecting filesystem under " + iDevice.DriveLetter.ToString().ToUpper() + ":\\>...\r\n");
            Debugger.Log(string.Format("EVENT: Disconnecting device under {0}:\\>", iDevice.DriveLetter.ToString().ToUpper()), Debugger.LogLevel.Event);
            DokanNet.DokanUnmount(iDevice.DriveLetter);
        }
        #endregion

        #region Event Handlers
        /// <summary>
        /// iDevice connected/disconnected
        /// </summary>
        private void Connect_iPhone(object sender, ConnectEventArgs args)
        {
            if (args.Message == NotificationMessage.Connected)
            {
                //iPhone connected
                iPhone iDevice = new iPhone(args);

                Console.WriteLine(iDevice.DeviceType + " connected");
                Console.WriteLine(iDevice.DeviceName +
                    "\r\niOS " + iDevice.DeviceVersion + (iDevice.IsJailbreak ? " jailbroken " : " ") +
                    iDevice.ActivationState);

                iDevices.Add(iDevice);

                PhoneThreads.Add(new Thread(new ParameterizedThreadStart(Connect_FS)));
                PhoneThreads[PhoneThreads.Count - 1].Start(iDevices[iDevices.Count - 1]);
            }
            else
            {
                Console.WriteLine("Device disconnected");
                //Cycle through mounted iDevices...
                for (int i = 0; i < iDevices.Count; i++)
                {
                    if (!iDevices[i].IsDirectory("/")) //Only way I found so far to check if iPhone is still connected.
                    {                                  //Should also work on non-jailbroken iPhones.
                        PhoneThreads[i].Abort();
                        PhoneThreads.RemoveAt(i);
                        Disconnect_FS(iDevices[i]);
                        iDevices.RemoveAt(i);
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Populates context menu
        /// </summary>
        void ContextMenuStrip_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            notifyMenu.Items.Clear();
            ToolStripMenuItem mnuAbout = new ToolStripMenuItem("About", null, mnuAbout_Click, "mnuAbout");
            mnuAbout.Image = iPhileResources.iPhileIcon.ToBitmap();
            notifyMenu.Items.Add(mnuAbout);

            notifyMenu.Items.Add("-");
            if (iDevices.Count > 0)
            {
                foreach (iPhone iDevice in iDevices)
                {
                    ToolStripMenuItem mnuDevice = new ToolStripMenuItem(iDevice.DeviceName + " (" + iDevice.DriveLetter.ToString().ToUpper() + ":\\)");
                    if (iDevice.DeviceType == "iPhone")
                        mnuDevice.Image = iPhileResources.iphone.ToBitmap();
                    else if (iDevice.DeviceType == "iPod")
                        mnuDevice.Image = iPhileResources.ipod.ToBitmap();
                    else if (iDevice.DeviceType == "iPad")
                        mnuDevice.Image = iPhileResources.ipad.ToBitmap();
                    ToolStripMenuItem subItem = new ToolStripMenuItem(iDevice.DeviceName);
                    subItem.Enabled = false;
                    mnuDevice.DropDownItems.Add(subItem);
                    subItem = new ToolStripMenuItem(iDevice.DeviceVersion + (iDevice.IsJailbreak ? " jailbroken" : ""));
                    subItem.Enabled = false;
                    mnuDevice.DropDownItems.Add(subItem);
                    subItem = new ToolStripMenuItem(iDevice.ActivationState);
                    subItem.Enabled = false;
                    mnuDevice.DropDownItems.Add(subItem);
                    mnuDevice.DropDownItems.Add("-");
                    subItem = new ToolStripMenuItem("Mounted to " + iDevice.DriveLetter.ToString().ToUpper() + ":\\ [open]", null, mnuOpen_Click, iDevice.DriveLetter.ToString().ToUpper());
                    mnuDevice.DropDownItems.Add(subItem);
                    subItem = new ToolStripMenuItem("Unmount device", null, mnuDismount_Click, iDevice.DeviceId);
                    mnuDevice.DropDownItems.Add(subItem);

                    notifyMenu.Items.Add(mnuDevice);
                }
            }
            else
            {
                notifyMenu.Items.Add("No devices mounted.");
            }

            notifyMenu.Items.Add("-");
            ToolStripMenuItem mnuExit = new ToolStripMenuItem("Exit", null, mnuExit_Click, "mnuAbout");
            notifyMenu.Items.Add(mnuExit);
            e.Cancel = false;
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
            foreach (iPhone iDevice in iDevices)
            {
                Disconnect_FS(iDevice);
            }
            Debugger.Close();
            Application.Exit();
        }

        /// <summary>
        /// Dismount selected device (until it gets disconnected and reconnected)
        /// </summary>
        private void mnuDismount_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem Sender = (ToolStripMenuItem)sender;
            string UDID = Sender.Name;

            for (int i = 0; i < iDevices.Count; i++)
            {
                if (iDevices[i].DeviceId == UDID) //Only way I found so far to check if iPhone is still connected.
                {                                  //Should also work on non-jailbroken iPhones.
                    PhoneThreads[i].Abort();
                    PhoneThreads.RemoveAt(i);
                    Disconnect_FS(iDevices[i]);
                    iDevices.RemoveAt(i);
                    break;
                }
            }
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
        #endregion
    }

}
