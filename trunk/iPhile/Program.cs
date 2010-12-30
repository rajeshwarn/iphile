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
using System.Threading;
using System.Windows.Forms;

namespace iPhile
{
    static class Program
    {
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

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            bool isSingleInstance = false;

            using (Mutex mtx = new Mutex(true, "iPhileSingleInstanceMutex", out isSingleInstance))
            {
                if (isSingleInstance)
                {
                    iPhile iPhileInstance = new iPhile(SkipInfo);
                    iPhileInstance.notifyIcon.Visible = true;
                    Application.Run();
                    iPhileInstance.notifyIcon.Dispose();
                }
                else
                {
                    MessageBox.Show("iPhile is already running.", "iPhile", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    Environment.Exit(0);
                }
            } //releases our Mutex
        }
    }
}
