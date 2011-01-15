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
using Dokan;
using Manzana;

namespace iPhile
{
    class iPhoneFS : DokanOperations
    {
        //iDevice mapped to drive letter
        private iPhone iDevice;
        //For jailbreak devices only: Get size of media partition
        private iPhone_ForceAFC iDeviceMedia;

        private string LetterString;

        //We won't windows allow to create certain files on our UNIX system.
        //Those file names are case insensitive, even if windows tried to create AuToRuN.iNf it would fail.
        private string[] FileCreationFilters = new string[] { "autorun.inf", "thumbs.db", "desktop.ini" };

        //Proof-of-concept. Delegate using lambda expression.
        private delegate string ConvertPathDelegate(string Path);
        ConvertPathDelegate ConvertPath = (Path) => Path.Replace('\\', '/');

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="iphone">iDevice to map to drive letter</param>
        public iPhoneFS(iPhone iphone)
        {
            iDevice = iphone;

            LetterString = iDevice.DriveLetter.ToString().ToUpper() + ":> ";
            Debugger.Log(LetterString + "EVENT: FileSystem initialized.", Debugger.LogLevel.Event);

            if (iDevice.IsJailbreak)
            {
                iDeviceMedia = new iPhone_ForceAFC(iDevice);
                Debugger.Log(LetterString + "EVENT: iDevice jailbroken. Connecting to afc service.", Debugger.LogLevel.Event);
            }
        }

        /// <summary>
        /// What could we clean up?
        /// </summary>
        public int Cleanup(string filename, DokanFileInfo info)
        {
            return DokanNet.DOKAN_SUCCESS;
        }

        /// <summary>
        /// Hope this works as intended.
        /// </summary>
        public int CloseFile(string filename, DokanFileInfo info)
        {
            Debugger.Log(LetterString + "FS: CloseFile: " + filename, Debugger.LogLevel.Information);
            if (info.Context != null)
            {
                info.Context.Close();
                info.Context = null;
            }
            return DokanNet.DOKAN_SUCCESS;
        }

        /// <summary>
        /// The name suggests it already: This function creates a directory.
        /// </summary>
        public int CreateDirectory(string filename, DokanFileInfo info)
        {
            //If directory already exists throw error "already existing".
            if (iDevice.Exists(ConvertPath(filename)))
                return -DokanNet.ERROR_ALREADY_EXISTS;
            Debugger.Log(LetterString + "FS: CreateDirectory: " + filename, Debugger.LogLevel.Information);
            return (iDevice.CreateDirectory(ConvertPath(filename)) ? DokanNet.DOKAN_SUCCESS : DokanNet.DOKAN_ERROR);
        }

        /// <summary>
        /// This creates a file.
        /// Dunno if this works as expected. But should now.
        /// </summary>
        public int CreateFile(
            string filename,
            System.IO.FileAccess access,
            System.IO.FileShare share,
            System.IO.FileMode mode,
            System.IO.FileOptions options,
            DokanFileInfo info)
        {
            //We don't want windows to create desktop.ini, thumbs.db or autorun.inf or some other bullshit.
            foreach (string FilterString in FileCreationFilters)
                if (filename.ToLower().EndsWith(FilterString))
                    return -DokanNet.ERROR_ACCESS_DENIED;
            Debugger.Log(LetterString + "FS: CreateFile: " + filename + " Mode: " + mode.ToString(), Debugger.LogLevel.Information);

            try //and pray it works...
            {
                if (iDevice.IsDirectory(ConvertPath(filename)))
                {
                    info.IsDirectory = true;
                    info.Context = null;
                    return DokanNet.DOKAN_SUCCESS;
                }
                switch (mode)
                {
                    case System.IO.FileMode.CreateNew: //Creates a new file if not already existing, else drop error
                        if (iDevice.Exists(ConvertPath(filename)))
                            return -DokanNet.ERROR_ALREADY_EXISTS;
                        info.Context = iPhoneFile.OpenWrite(iDevice, ConvertPath(filename));
                        break;
                    case System.IO.FileMode.Create: //Create a new file even if already existing
                        info.Context = iPhoneFile.OpenWrite(iDevice, ConvertPath(filename));
                        break;
                    case System.IO.FileMode.Open: //If file not existing drop error
                        if (!iDevice.Exists(ConvertPath(filename)))
                            return -DokanNet.ERROR_FILE_NOT_FOUND;
                        info.Context = iPhoneFile.OpenRead(iDevice, ConvertPath(filename));
                        break;
                    case System.IO.FileMode.Append: //Appends to file or creates a new file if not existing
                        if (!iDevice.Exists(ConvertPath(filename)))
                            info.Context = iPhoneFile.OpenWrite(iDevice, ConvertPath(filename));
                        break;
                    case System.IO.FileMode.OpenOrCreate: //Open file if exists; Create a file if not existing
                        if (!iDevice.Exists(ConvertPath(filename)))
                            info.Context = iPhoneFile.OpenWrite(iDevice, ConvertPath(filename));
                        else
                            info.Context = iPhoneFile.OpenRead(iDevice, ConvertPath(filename));
                        break;
                    case System.IO.FileMode.Truncate: //Open file and delete its contents
                        info.Context = iPhoneFile.OpenWrite(iDevice, ConvertPath(filename));
                        break;
                }
            }
            catch (Exception ex)
            {
                Debugger.Log(LetterString + "ERROR: FS: CreateFile: " + ex.Message + " Filename: " + filename, Debugger.LogLevel.Error);
                return DokanNet.DOKAN_ERROR;
            }
            return DokanNet.DOKAN_SUCCESS;
        }

        /// <summary>
        /// Deletes a directory.
        /// </summary>
        public int DeleteDirectory(string filename, DokanFileInfo info)
        {
            Debugger.Log(LetterString + "FS: DeleteDirectory: " + filename, Debugger.LogLevel.Information);
            iDevice.DeleteDirectory(ConvertPath(filename));
            return DokanNet.DOKAN_SUCCESS;
        }

        /// <summary>
        /// Deletes a file.
        /// </summary>
        public int DeleteFile(string filename, DokanFileInfo info)
        {
            Debugger.Log(LetterString + "FS: DeleteFile: " + filename, Debugger.LogLevel.Information);
            iDevice.DeleteFile(ConvertPath(filename));
            return DokanNet.DOKAN_SUCCESS;
        }

        /// <summary>
        /// Hope this works.
        /// info.Context has a Flush() method so we will call it.
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="info"></param>
        /// <returns></returns>
        public int FlushFileBuffers(
            string filename,
            DokanFileInfo info)
        {
            if (info.Context != null)
            {
                Debugger.Log(LetterString + "FS: FlushFileBuffers: " + filename, Debugger.LogLevel.Information);
                info.Context.Flush();
            }
            return DokanNet.DOKAN_SUCCESS;
        }

        /// <summary>
        /// AKA directory listing
        /// </summary>
        public int FindFiles(
            string filename,
            System.Collections.ArrayList files,
            DokanFileInfo info)
        {
            Debugger.Log(LetterString + "FS: FindFiles: " + filename, Debugger.LogLevel.Information);
            string Filename = ConvertPath(filename);

            if (!iDevice.Exists(Filename))
                return -DokanNet.ERROR_PATH_NOT_FOUND;
            
            //Parse directories...
            foreach (string Directory in iDevice.GetDirectories(Filename))
            {
                Dictionary<string, string> PhoneFileInfo = iDevice.GetFileInfo(Filename + (Filename.EndsWith("/") ? "" : "/") + Directory);

                FileInformation finfo = new FileInformation();
                finfo.FileName = Directory;
                finfo.Attributes = System.IO.FileAttributes.Directory;
                if (Directory.StartsWith("."))
                    finfo.Attributes |= System.IO.FileAttributes.Hidden;

                //LastAccessTime setting.
                //sometimes causes problems when no information is available, causing ugly errors on
                //iPhile's console.
                if (PhoneFileInfo.ContainsKey("st_mtime"))
                {
                    //seconds since 01.01.1970 00:00:00
                    long unix_timestamp = long.Parse(PhoneFileInfo["st_mtime"].Substring(0, 10));
                    //Add unix_timestamp to LastAccessTime later.
                    DateTime LastAccessTime = new DateTime(1970, 1, 1, 0, 0, 0, 0);
                    //Add seconds since 1970 to 1970 so we get our Windows date :-)
                    LastAccessTime = LastAccessTime.AddSeconds(unix_timestamp);

                    finfo.LastAccessTime = LastAccessTime;
                    finfo.LastWriteTime = LastAccessTime;
                    finfo.CreationTime = LastAccessTime;
                }
                else
                {
                    finfo.LastAccessTime = DateTime.Now;
                    finfo.LastWriteTime = DateTime.Now;
                    finfo.CreationTime = DateTime.Now;
                }
                files.Add(finfo);
            }
            //... then files.
            foreach (string File in iDevice.GetFiles(Filename))
            {
                Dictionary<string, string> PhoneFileInfo = iDevice.GetFileInfo(Filename + (Filename.EndsWith("/") ? "" : "/") + File);

                FileInformation finfo = new FileInformation();
                finfo.FileName = File;
                finfo.Attributes = System.IO.FileAttributes.Normal;
                if (File.StartsWith("."))
                    finfo.Attributes |= System.IO.FileAttributes.Hidden;

                //LastAccessTime setting.
                //sometimes causes problems when no information is available, causing ugly errors on
                //iPhile's console.
                if (PhoneFileInfo.ContainsKey("st_mtime"))
                {
                    //seconds since 01.01.1970 00:00:00
                    long unix_timestamp = long.Parse(PhoneFileInfo["st_mtime"].Substring(0, 10));
                    //Add unix_timestamp to LastAccessTime later.
                    DateTime LastAccessTime = new DateTime(1970, 1, 1, 0, 0, 0, 0);
                    //Add seconds since 1970 to 1970 so we get our Windows date :-)
                    LastAccessTime = LastAccessTime.AddSeconds(unix_timestamp);

                    finfo.LastAccessTime = LastAccessTime;
                    finfo.LastWriteTime = LastAccessTime;
                    finfo.CreationTime = LastAccessTime;
                }
                else
                {
                    finfo.LastAccessTime = DateTime.Now;
                    finfo.LastWriteTime = DateTime.Now;
                    finfo.CreationTime = DateTime.Now;
                }

                finfo.Length = (long)iDevice.FileSize(Filename + (Filename.EndsWith("/") ? "" : "/") + File);
                files.Add(finfo);
            }
            return DokanNet.DOKAN_SUCCESS;
        }

        /// <summary>
        /// Get information about a specific file or folder
        /// </summary>
        public int GetFileInformation(
            string filename,
            FileInformation fileinfo,
            DokanFileInfo info)
        {
            try
            {
                Debugger.Log(LetterString + "FS: GetFileInformation: " + filename, Debugger.LogLevel.Information);
                string Filename = ConvertPath(filename);

                if (!iDevice.Exists(Filename))
                    return -DokanNet.ERROR_FILE_NOT_FOUND;

                Dictionary<string, string> PhoneFileInfo = iDevice.GetFileInfo(Filename);

                string FileName = (Filename == "/" ? "/" : Filename.Split('/')[Filename.Split('/').Length - 1]);

                //Is this directory or file?
                fileinfo.Attributes = (iDevice.IsFile(Filename) ? System.IO.FileAttributes.Normal : System.IO.FileAttributes.Directory);
                if (FileName.StartsWith("."))
                    fileinfo.Attributes |= System.IO.FileAttributes.Hidden;
                fileinfo.FileName = FileName;

                //LastAccessTime setting.
                //sometimes causes problems when no information is available, causing ugly errors on
                //iPhile's console.
                if (PhoneFileInfo.ContainsKey("st_mtime"))
                {
                    //seconds since 01.01.1970 00:00:00
                    long unix_timestamp = long.Parse(PhoneFileInfo["st_mtime"].Substring(0, 10));
                    //Add unix_timestamp to LastAccessTime later.
                    DateTime LastAccessTime = new DateTime(1970, 1, 1, 0, 0, 0, 0);
                    //Add seconds since 1970 to 1970 so we get our Windows date :-)
                    LastAccessTime = LastAccessTime.AddSeconds(unix_timestamp);

                    fileinfo.LastAccessTime = LastAccessTime;
                    fileinfo.LastWriteTime = LastAccessTime;
                    fileinfo.CreationTime = LastAccessTime;
                }
                else
                {
                    fileinfo.LastAccessTime = DateTime.Now;
                    fileinfo.LastWriteTime = DateTime.Now;
                    fileinfo.CreationTime = DateTime.Now;
                }

                fileinfo.Length = (long)iDevice.FileSize(Filename);

                return DokanNet.DOKAN_SUCCESS;
            }
            catch (Exception ex)
            {
                Debugger.Log(LetterString + "ERROR: FS: GetFileInformation: " + ex.Message + " Filename: " + filename, Debugger.LogLevel.Error);
                return DokanNet.DOKAN_ERROR;
            }
        }

        /// <summary>
        /// Locking does not seem to be implemented by Apple
        /// </summary>
        public int LockFile(
            string filename,
            long offset,
            long length,
            DokanFileInfo info)
        {
            return DokanNet.DOKAN_SUCCESS;
        }

        /// <summary>
        /// Renames or moves a file
        /// </summary>
        public int MoveFile(
            string filename,
            string newname,
            bool replace,
            DokanFileInfo info)
        {
            Debugger.Log(LetterString + "FS: MoveFile: " + filename + " -> " + newname, Debugger.LogLevel.Information);
            try
            {
                //Manzana.dll 'rename' is actually a move command
                iDevice.Rename(ConvertPath(filename), ConvertPath(newname));
            }
            catch (Exception ex)
            {
                
                Debugger.Log(LetterString + "ERROR: FS: MoveFile: " + ex.Message + " " + filename + " -> " + newname, Debugger.LogLevel.Error);
                
                return DokanNet.DOKAN_ERROR;
            }
            return DokanNet.DOKAN_SUCCESS;
        }

        /// <summary>
        /// Well, we don't need no code to open specified directories, but we
        /// could tell Dokan if the desired filename is actually a directory :-)
        /// </summary>
        public int OpenDirectory(string filename, DokanFileInfo info)
        {
            Debugger.Log(LetterString + "FS: OpenDirectory: " + filename, Debugger.LogLevel.Information);
            info.IsDirectory = (iDevice.IsDirectory(ConvertPath(filename)) || ConvertPath(filename) == "/");
            return DokanNet.DOKAN_SUCCESS;
        }

        /// <summary>
        /// Reads a file.
        /// Should be working now. Critical error in MobileDevice.cs which caused Seek() to do _nothing_ fixed.
        /// </summary>
        public int ReadFile(
            string filename,
            byte[] buffer,
            ref uint readBytes,
            long offset,
            DokanFileInfo info)
        {
            Debugger.Log(LetterString + "INFO: FS: ReadFile: " + filename + " Offset " + offset.ToString() + " ReadBytes " + buffer.Length.ToString(), Debugger.LogLevel.Information);

            //Stop if file does not exist
            if (!iDevice.Exists(ConvertPath(filename)))
                return -DokanNet.ERROR_FILE_NOT_FOUND;

            if (!iDevice.IsFile(ConvertPath(filename)))
                return -DokanNet.ERROR_FILE_NOT_FOUND;

            try
            {
                if (info.Context == null)
                    info.Context = iPhoneFile.Open(iDevice, ConvertPath(filename), System.IO.FileAccess.Read);

                info.Context.Position = offset; //Simply set the correct offset
                readBytes = (uint)info.Context.Read(buffer, 0, buffer.Length); //And read it into our buffer
            }
            catch (Exception ex)
            {
                Debugger.Log(LetterString + "ERROR: FS: ReadFile: " + ex.Message + " Filename: " + filename, Debugger.LogLevel.Error);
                return DokanNet.DOKAN_ERROR; //DOKAN_ERROR = -1 (already negative)
            }
            return DokanNet.DOKAN_SUCCESS;
        }

        /// <summary>
        /// Don't know what that's good for
        /// So no implementation. But works fine :D
        /// </summary>
        public int SetEndOfFile(string filename, long length, DokanFileInfo info)
        {
            return DokanNet.DOKAN_SUCCESS;
        }

        /// <summary>
        /// See above.
        /// </summary>
        public int SetAllocationSize(string filename, long length, DokanFileInfo info)
        {
            return DokanNet.DOKAN_SUCCESS;
        }

        /// <summary>
        /// Sets file attributes
        /// Not supportet through iTunesMobileDevice.dll (or is a hidden feature?)
        /// Use SSH instead :P
        /// </summary>
        public int SetFileAttributes(
            string filename,
            System.IO.FileAttributes attr,
            DokanFileInfo info)
        {
            return DokanNet.DOKAN_SUCCESS;
        }

        /// <summary>
        /// Sets file time
        /// Not supportet through iTunesMobileDevice.dll (or is a hidden feature?)
        /// </summary>
        public int SetFileTime(
            string filename,
            DateTime ctime,
            DateTime atime,
            DateTime mtime,
            DokanFileInfo info)
        {
            return DokanNet.DOKAN_SUCCESS;
        }

        /// <summary>
        /// Unlocks a file.
        /// Since we have no lock support we have no unlock support, either.
        /// </summary>
        public int UnlockFile(string filename, long offset, long length, DokanFileInfo info)
        {
            return DokanNet.DOKAN_SUCCESS;
        }

        /// <summary>
        /// Doesn't seem to be neccessary before unmounting the iDevice.
        /// </summary>
        public int Unmount(DokanFileInfo info)
        {
            return DokanNet.DOKAN_SUCCESS;
        }

        /// <summary>
        /// Disk space on iPhone.
        /// Uses root partition size if jailbroken (afc2)
        /// or media partition size if not jailbroken. (no afc2 connection)
        /// 
        /// Manzana does not get correct values so we'll use something big enough to copy files onto the device here.
        /// </summary>
        public int GetDiskFreeSpace(
           ref ulong freeBytesAvailable,
           ref ulong totalBytes,
           ref ulong totalFreeBytes,
           DokanFileInfo info)
        {
            //We have only access to the root partition's space information using afc2.
            //So we'll have to create a new connection using afc.
            if (iDevice.IsJailbreak)
            {
                iDeviceMedia.RefreshFileSystemInfo();
                iDevice.RefreshFileSystemInfo();
                freeBytesAvailable = (ulong)iDeviceMedia.FileSystemFreeBytes + (ulong)iDevice.FileSystemFreeBytes;
                totalBytes = (ulong)iDeviceMedia.FileSystemTotalBytes + (ulong)iDevice.FileSystemTotalBytes;
                totalFreeBytes = freeBytesAvailable;
            }
            else
            {
                iDevice.RefreshFileSystemInfo();
                freeBytesAvailable = (ulong)iDevice.FileSystemFreeBytes;
                totalBytes = (ulong)iDevice.FileSystemTotalBytes;
                totalFreeBytes = freeBytesAvailable;
            }
            return DokanNet.DOKAN_SUCCESS;
        }

        /// <summary>
        /// Writes a file.
        /// info.Context is set in CreateFile.
        /// Hope this works now, as setting file offsets is now fixed :-)
        /// </summary>
        public int WriteFile(
            string filename,
            byte[] buffer,
            ref uint writtenBytes,
            long offset,
            DokanFileInfo info)
        {
            try
            {
                if (info.Context == null) //should not happen. But who knows.
                    info.Context = iPhoneFile.OpenWrite(iDevice, ConvertPath(filename));
                long InitialOffset = offset; //Set initial offset
                info.Context.Position = offset; //Set offset, this is working now :-)
                info.Context.Write(buffer, 0, buffer.Length);
                writtenBytes = (uint)(info.Context.Position - InitialOffset); //tell Dokan how many bytes were written.
            }
            catch (Exception ex)
            {
                Debugger.Log(LetterString + "ERROR: FS: WriteFile: " + ex.Message + " Filename: " + filename, Debugger.LogLevel.Error);
                return DokanNet.DOKAN_ERROR;
            }
            return DokanNet.DOKAN_SUCCESS;
        }
    }

}
