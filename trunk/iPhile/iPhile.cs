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
    public sealed partial class iPhile
    {
        #region Types
        //Listens to iDevice connect/disconnect events
        private MultiPhone PhoneListener;

        public NotifyIcon notifyIcon;
        private ContextMenuStrip notifyMenu;

        private bool AutoMount;

        //Preferred mount points for iDevices. UDID => char
        private Dictionary<string, char> PreferredMountPoints = new Dictionary<string, char>();
        //Every connected iDevice. UDID => iPhone
        private Dictionary<string, iPhone> iDevices = new Dictionary<string, iPhone>();
        //Threads used to mount iDevices. UDID => Thread
        private Dictionary<string, Thread> PhoneThreads = new Dictionary<string, Thread>();
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

            LoadPreferredMountPoints();

            PhoneListener = new MultiPhone();
            PhoneListener.Connect += new ConnectEventHandler(Connect_iPhone);
            PhoneListener.Disconnect += new ConnectEventHandler(Connect_iPhone);
        }
        #endregion

        #region Methods


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
                    iDevices[iDevice.DeviceIdFixed] = iDevice;
                    PhoneThreads[iDevice.DeviceIdFixed] = new Thread(new ParameterizedThreadStart(Connect_FS));
                    PhoneThreads[iDevice.DeviceIdFixed].Start(iDevices[iDevice.DeviceIdFixed]);
                }
                else
                {
                    iDevice.DriveLetter = '0';
                    iDevices[iDevice.DeviceIdFixed] = iDevice;
                    PhoneThreads[iDevice.DeviceIdFixed] = null;
                }
            }
            else
            {
                foreach (iPhone iDevice in iDevices.Values)
                {
                    if (!iDevice.IsDirectory("/")) //Only way I found so far to check if iPhone is still connected.
                    {                              //Should also work on non-jailbroken iPhones.
                        if (PhoneThreads[iDevice.DeviceIdFixed] != null)
                            PhoneThreads[iDevice.DeviceIdFixed].Abort();
                        PhoneThreads.Remove(iDevice.DeviceIdFixed);
                        Disconnect_FS(iDevice);
                        iDevices.Remove(iDevice.DeviceIdFixed);
                        break;
                    }
                }
            }
        }
        #endregion
    }
}
