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

namespace iPhile
{
    /// <summary>
    /// Small debugger/logger class.
    /// Logs to debug window if compiled as DEBUG, else logs to log.txt
    /// </summary>
    static class Debugger
    {
        public static LogLevel LLevel = LogLevel.Event;

        public enum LogLevel
        {
            Error = 0,
            Event,
            Information
        }

        public static bool LogToFile = true;

        static StreamWriter Writer = null;

        public static void Log_Clear()
        {
            if (LogToFile)
            {
                if (Writer != null)
                    Writer.Close();
                File.Delete("log.txt");
                Writer = new StreamWriter("log.txt", true);
            }
        }

        public static void Log(string LogString)
        {
            if (LogToFile)
                Writer.WriteLine(LogString);
            else
                System.Diagnostics.Debug.WriteLine(LogString);
        }

        public static void Log(string LogString, LogLevel logLevel)
        {
            if ((int)LLevel >= (int)logLevel)
                if (LogToFile)
                    Writer.WriteLine(LogString);
                else
                    System.Diagnostics.Debug.WriteLine(LogString);
        }

        public static void Close()
        {
            if (LogToFile)
                Writer.Close();
        }
    }
}