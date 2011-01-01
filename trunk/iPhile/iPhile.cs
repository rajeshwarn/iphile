﻿/*
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

        private bool AutoMount;

        private Dictionary<string, char> PreferredMountPoints = new Dictionary<string, char>();
        #endregion

        #region Constructor
        public iPhile(bool SkipInfo, bool AutoMount)
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
                + "Note: You can skip this message by calling iPhile with parameter -skipinfo\r\n"
                + "You can disable automatic mounting with parameter -noautomount",
                "iPhile", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            this.AutoMount = AutoMount;

            notifyIcon = new NotifyIcon();
            notifyMenu = new ContextMenuStrip();
            notifyIcon.Icon = iPhileResources.iPhileIcon;
            notifyIcon.ContextMenuStrip = notifyMenu;
            notifyIcon.ContextMenuStrip.Opening += new System.ComponentModel.CancelEventHandler(ContextMenuStrip_Opening);

            notifyIcon.Text = "iPhile";

            if (File.Exists("PreferredMountPoints.ini"))
            {
                try
                {
                    StreamReader sr = new StreamReader("PreferredMountPoints.ini");

                    while (!sr.EndOfStream)
                    {
                        string Value = sr.ReadLine();
                        PreferredMountPoints[Value.Split(':')[0].ToLower()] = Value.Split(':')[1].ToLower().ToCharArray(0, 1)[0];
                    }

                    sr.Close();
                    sr.Dispose();
                }
                catch (Exception)
                {
                    MessageBox.Show("Error while opening PreferredMountPoints.ini", "iPhile", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

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
            for (int i = Convert.ToInt16('c'); i <= Convert.ToInt16('z'); i++)
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
        /// Returns a list of every available drive letter
        /// </summary>
        private List<char> AvailableDriveLetters()
        {
            List<char> AvailableLetters = new List<char>();

            //don't use a and b cause they only work for floppy drives
            for (int i = Convert.ToInt16('c'); i <= Convert.ToInt16('z'); i++)
                AvailableLetters.Add((char)i);

            foreach (DriveInfo Drive in DriveInfo.GetDrives())
                //AvailableLetters.Remove(Convert.ToChar(Drive.Name.Substring(0, 1).ToLower()));
                AvailableLetters.Remove(Drive.Name.ToLower().ToCharArray()[0]); //Should be more clean.

            return AvailableLetters;
        }

        /// <summary>
        /// Checks whether the specified drive letter is available
        /// </summary>
        private bool IsDriveLetterAvailable(char DriveLetter)
        {
            bool Available = true;

            foreach (DriveInfo Drive in DriveInfo.GetDrives())
            {
                if (Drive.Name.ToLower().ToCharArray()[0] == DriveLetter)
                {
                    Available = false;
                    break;
                }
            }

            return Available;
        }

        /// <summary>
        /// iPhone connected, so connect filesystem
        /// </summary>
        private void Connect_FS(object iDevice)
        {
            iPhone Device = (iPhone)iDevice;

            if (Device.DriveLetter == '0')
            {
                MessageBox.Show("No drive letter available to mount device.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Debugger.Log("ERROR: No drive letter available.", 0);
                return;
            }

            Debugger.Log(string.Format("EVENT: Mounting {0} under {1}:\\>", Device.DeviceName, Device.DriveLetter.ToString().ToUpper()), Debugger.LogLevel.Event);

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

            int Return = DokanNet.DokanMain(opt, new iPhoneFS(Device));

            if (Return < 0) //Dokan has had an error
            {
                string ErrorString;

                switch (Return)
                {
                    case DokanNet.DOKAN_ERROR:
                        ErrorString = "Dokan encountered a general error.";
                        break;
                    case DokanNet.DOKAN_DRIVE_LETTER_ERROR:
                        ErrorString = "Bad drive letter specified.";
                        break;
                    case DokanNet.DOKAN_DRIVER_INSTALL_ERROR:
                        ErrorString = "Dokan was unable to install its driver.";
                        break;
                    case DokanNet.DOKAN_START_ERROR:
                        ErrorString = "Something seems to be wrong with your Dokan driver.";
                        break;
                    case DokanNet.DOKAN_MOUNT_ERROR:
                        ErrorString = "Dokan was unable to assign the drive letter.";
                        break;
                    default:
                        ErrorString = "Dokan encountered an unknown error.";
                        break;
                }
                MessageBox.Show("An error occured:\r\n" + ErrorString, "iPhile", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// iPhone disconnected, disconnect filesystem
        /// </summary>
        private void Disconnect_FS(iPhone iDevice)
        {
            Debugger.Log(string.Format("EVENT: Dismounting {0} under {1}:\\>", iDevice.DeviceNameFixed, iDevice.DriveLetter.ToString().ToUpper()), Debugger.LogLevel.Event);
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
                
                char Letter;
                if (AutoMount)
                {
                    if (PreferredMountPoints.TryGetValue(iDevice.DeviceIdFixed, out Letter))
                    {
                        if (IsDriveLetterAvailable(Letter))
                            iDevice.DriveLetter = Letter;
                        else
                        {
                            MessageBox.Show("Preferred mount point not available. Using first free point instead.", "iPhile", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                            Letter = DriveLetter();
                            iDevice.DriveLetter = Letter;
                            PreferredMountPoints[iDevice.DeviceIdFixed] = Letter;
                        }
                    }
                    else
                    {
                        Letter = DriveLetter();
                        iDevice.DriveLetter = Letter;
                        PreferredMountPoints[iDevice.DeviceIdFixed] = Letter;
                    }
                    iDevices.Add(iDevice);
                    PhoneThreads.Add(new Thread(new ParameterizedThreadStart(Connect_FS)));
                    PhoneThreads[PhoneThreads.Count - 1].Start(iDevices[iDevices.Count - 1]);
                }
                else
                {
                    iDevice.DriveLetter = '0';
                    iDevices.Add(iDevice);
                    PhoneThreads.Add(null);
                }
            }
            else
            {
                //Cycle through mounted iDevices...
                for (int i = 0; i < iDevices.Count; i++)
                {
                    if (!iDevices[i].IsDirectory("/")) //Only way I found so far to check if iPhone is still connected.
                    {                                  //Should also work on non-jailbroken iPhones.
                        if (PhoneThreads[i] != null)
                        {
                            PhoneThreads[i].Abort();
                        }
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
            ToolStripMenuItem mnuItem = new ToolStripMenuItem("About", iPhileResources.question.ToBitmap(), mnuAbout_Click, "mnuAbout");
            notifyMenu.Items.Add(mnuItem);

            //Separator
            notifyMenu.Items.Add("-");
            if (iDevices.Count > 0)
            {
                foreach (iPhone iDevice in iDevices)
                {
                    //Main Entry
                    ToolStripMenuItem mnuDevice = new ToolStripMenuItem(iDevice.DeviceNameFixed + (iDevice.DriveLetter != '0' ?  " (" + iDevice.DriveLetter.ToString().ToUpper() + ":\\)" : ""));
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
            else
            {
                notifyMenu.Items.Add("No devices connected.");
            }

            notifyMenu.Items.Add("-");
            ToolStripMenuItem mnuExit = new ToolStripMenuItem("Exit", iPhileResources.x.ToBitmap(), mnuExit_Click, "mnuAbout");
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

            try
            {
                if (File.Exists("PreferredMountPoints.ini"))
                    File.Delete("PreferredMountPoints.ini");
                StreamWriter sw = new StreamWriter("PreferredMountPoints.ini");

                foreach (KeyValuePair<string, char> Pair in PreferredMountPoints)
                {
                    if (Pair.Value != '0')
                        sw.WriteLine(Pair.Key + ":" + Pair.Value);
                }
                sw.Close();
                sw.Dispose();
            }
            catch (Exception)
            {
                MessageBox.Show("Error while writing PreferredMountPoints.ini", "iPhile", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

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
                if (iDevices[i].DeviceIdFixed == UDID)
                {
                    PhoneThreads[i].Abort();
                    PhoneThreads[i] = null;
                    Disconnect_FS(iDevices[i]);
                    iDevices[i].DriveLetter = '0';
                    break;
                }
            }
        }

        private void mnuMount_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem Sender = (ToolStripMenuItem)sender;
            char Letter = Sender.Name.Split(';')[0].ToLower().ToCharArray(0, 1)[0];
            string UDID = Sender.Name.Split(';')[1];

            for (int i = 0; i < iDevices.Count; i++)
            {
                if (iDevices[i].DeviceId == UDID) //Only way I found so far to check if iPhone is still connected.
                {                                  //Should also work on non-jailbroken iPhones.
                    if (Letter == '0')
                    {
                        if (PreferredMountPoints.TryGetValue(iDevices[i].DeviceIdFixed, out Letter))
                        {
                            if (IsDriveLetterAvailable(Letter))
                                iDevices[i].DriveLetter = Letter;
                            else
                            {
                                MessageBox.Show("Preferred mount point not available. Using first free point instead.", "iPhile", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                                Letter = DriveLetter();
                                iDevices[i].DriveLetter = Letter;
                                PreferredMountPoints[iDevices[i].DeviceIdFixed] = Letter;
                            }
                        }
                        else
                        {
                            Letter = DriveLetter();
                            iDevices[i].DriveLetter = Letter;
                            PreferredMountPoints[iDevices[i].DeviceIdFixed] = Letter;
                        }
                    }
                    else
                    {
                        iDevices[i].DriveLetter = Letter;
                        PreferredMountPoints[iDevices[i].DeviceIdFixed] = Letter;
                    }

                    PhoneThreads[i] = new Thread(new ParameterizedThreadStart(Connect_FS));
                    PhoneThreads[i].Start(iDevices[i]);
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

        /// <summary>
        /// Copy UDID to clipboard
        /// </summary>
        private void mnuUDID_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(((ToolStripMenuItem)sender).Name);
            MessageBox.Show(string.Format("UDID {0} was copied to clipboard.", ((ToolStripMenuItem)sender).Name), "iPhile", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        #endregion
    }
}