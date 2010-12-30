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
using Dokan;
using Manzana;

namespace iPhile
{
    class Program
    {
        static MultiPhone PhoneListener;

        static List<iPhone> iDevices = new List<iPhone>();
        static List<Thread> PhoneThreads = new List<Thread>();
        static List<char> PhoneLetters = new List<char>();
        static Version AppVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;

        static void Main(string[] args)
        {
            bool SkipInfo = false;
            foreach (string arg in args)
            {
                if (arg.ToLower() == "-skipinfo")
                {
                    SkipInfo = true;
                }
                if (arg.ToLower().StartsWith("-loglevel") && arg.Length == 10)
                {
                    if (arg.Substring(9) == "0")
                        Debugger.LLevel = Debugger.LogLevel.Error;
                    if (arg.Substring(9) == "1")
                        Debugger.LLevel = Debugger.LogLevel.Event;
                    if (arg.Substring(9) == "2")
                        Debugger.LLevel = Debugger.LogLevel.Information;
                }
                if (arg.ToLower() == "-consolelog")
                {
                    Debugger.LogToFile = false;
                }
            }
            if (!SkipInfo)
            {
                Console.Clear();
                Console.WriteLine("iPhile Copyright (C) 2010");
                Console.WriteLine("This program comes with ABSOLUTELY NO WARRANTY.");
                Console.WriteLine("This is free software, and you are welcome to redistribute it");
                Console.WriteLine("under certain conditions; see \"COPYING.txt\".");
                Console.WriteLine();
                Console.WriteLine("iPhile v" + AppVersion.ToString());
                Console.WriteLine("Mount iPhone filesystem into Windows Explorer");
                Console.WriteLine();
                Console.WriteLine("Debug logging enabled. Loglevel: " + (Debugger.LLevel == Debugger.LogLevel.Error ? "Errors" : (Debugger.LLevel == Debugger.LogLevel.Event ? "Errors + Events" : "All")));
                Console.WriteLine("You can switch the LogLevel by calling iPhile with arguments");
                Console.WriteLine("-loglevel0 -loglevel1 -loglevel2");
                Console.WriteLine();
                Console.WriteLine("Please see Readme.txt for information about this application.");
                Console.WriteLine();
                Console.WriteLine();
                Console.WriteLine("Please install Dokan if not already done or this application won't work.");
                Console.WriteLine();
                Console.WriteLine();
                Console.WriteLine("BEWARE! This application has development quality!");
                Console.WriteLine("It's all your fault if it bricks your iPhone, iPod touch, iPad or your PC!");
                Console.WriteLine("I give no warranty for your device. Only continue if you really know what you");
                Console.WriteLine("are doing. You have been warned.");
                Console.WriteLine();
                Console.WriteLine("Note: You can skip this message by calling iPhile with parameter -skipinfo");
                Console.WriteLine();
                Console.Write("Please press any key to start iPhile!");
                Console.ReadKey();
            }
            Console.Clear();

            Debugger.Log_Clear();

            Console.WriteLine("iPhile started\r\n");
            Console.WriteLine("Waiting for iPhones to connect...\r\n");
            
            PhoneListener = new MultiPhone();
            PhoneListener.Connect += new ConnectEventHandler(Connect_iPhone);
            PhoneListener.Disconnect += new ConnectEventHandler(Connect_iPhone);

            Console.ReadKey();
            Console.WriteLine();
            foreach (iPhone iDevice in iDevices)
            {
                Disconnect_FS(iDevice);
            }

            Debugger.Close();
        }

        /// <summary>
        /// Get the lowest available Drive Letter
        /// Function assumes all drive letters from C to Z are free.
        /// (A and B are ignored because Windows won't let us map iPhone Filesystem to them.)
        /// Then every occupied letter is removed from the list and the lowest will be given back.
        /// </summary>
        /// <returns>char containing the lowest available drive letter or '0' (zero char) if no letter available</returns>
        static char DriveLetter()
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

        static void Connect_iPhone(object sender, ConnectEventArgs args)
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
        /// iPhone connected, so connect filesystem
        /// </summary>
        static void Connect_FS(object iDevice)
        {
            iPhone Device = (iPhone)iDevice;
            Device.DriveLetter =  DriveLetter();

            if (Device.DriveLetter == '0')
            {
                Console.WriteLine("No drive letter available. iPhile stop.");
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
        static void Disconnect_FS(iPhone iDevice)
        {
            Console.WriteLine("Disconnecting filesystem under " + iDevice.DriveLetter.ToString().ToUpper() + ":\\>...\r\n");
            Debugger.Log(string.Format("EVENT: Disconnecting device under {0}:\\>", iDevice.DriveLetter.ToString().ToUpper()), Debugger.LogLevel.Event);
            DokanNet.DokanUnmount(iDevice.DriveLetter);
        }
    }

}
