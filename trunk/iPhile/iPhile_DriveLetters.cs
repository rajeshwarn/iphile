using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace iPhile
{
    /// <summary>
    /// This partial class contains code to get available drive letters.
    /// </summary>
    public sealed partial class iPhile
    {
        /// <summary>
        /// Get the lowest available Drive Letter
        /// </summary>
        /// <returns>char containing the lowest available drive letter or '0' (zero char) if no letter available</returns>
        private char DriveLetter()
        {
            List<char> AvailableLetters = AvailableDriveLetters();

            return (AvailableLetters.Count == 0 ? '0' : AvailableLetters[0]);
        }

        /// <summary>
        /// Returns a list of every available drive letter
        /// Function assumes all drive letters from C to Z are free.
        /// (A and B are ignored because they are reserved for floppy drives and thus not available for mapping.)
        /// Then every occupied letter is removed from the list and the lowest will be given back.
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
    }
}
