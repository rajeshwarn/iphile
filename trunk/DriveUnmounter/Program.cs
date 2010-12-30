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
using Dokan;

namespace DriveUnmounter
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Clear();
            Console.WriteLine("DriveUnmounter Copyright (C) 2010");
            Console.WriteLine("This program comes with ABSOLUTELY NO WARRANTY.");
            Console.WriteLine("This is free software, and you are welcome to redistribute it");
            Console.WriteLine("under certain conditions; see \"COPYING.txt\".");
            Console.WriteLine();
            Console.WriteLine("Use this tool if one iPhone drive got stuck in Windows Explorer.");
            Console.WriteLine("This means it is removed but still shown in Windows Explorer.\r\n");

            Console.WriteLine("BEWARE: Do ONLY use this tool for drives created by iPhile that were not");
            Console.WriteLine("        properly unmounted!");
            Console.WriteLine("BEWARE: Do NOT use this tool for drives not created by iPhile!");
            Console.WriteLine("BEWARE: USE THIS TOOL AT YOUR OWN RISK!\r\n\r\n");

            Console.Write("Please type the drive letter (e.g. E) the device was mapped to: ");

            string DriveLetterString = Console.ReadLine().ToLower();
            if (DriveLetterString.Length > 1 || DriveLetterString.Length < 1 || !char.IsLetter(DriveLetterString, 0))
            {
                Console.WriteLine("Wrong input. DriveUnmounter stop.");
                Console.ReadKey();
                return;
            }

            char DriveLetter = DriveLetterString.ToCharArray()[0];
            bool DriveConnected = false;

            foreach (DriveInfo Drive in DriveInfo.GetDrives())
            {
                if (Drive.Name.ToLower().ToCharArray()[0] == DriveLetter)
                {
                    if (Drive.DriveType == DriveType.Removable)
                    {
                        DriveConnected = true;
                        break;
                    }
                    else
                    {
                        Console.WriteLine("Drive does not seem to be mounted by iPhile. DriveUnmounter stop.");
                        return;
                    }
                }
            }

            if (DriveConnected)
            {
                Console.WriteLine("Attempting to unmount drive " + DriveLetterString.ToUpper() + ":\\>...");
                DokanNet.DokanUnmount(DriveLetter);

                Console.WriteLine("The drive should now be successfully unmounted.");
            }
            else
            {
                Console.WriteLine("There is no drive with the specified drive letter. DriveUnmounter stop.");
            }
            Console.ReadKey();
        }
    }
}
