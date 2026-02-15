/*
 * FILE             : ServerLogger.cs
 * PROJECT          : GuessTheWords-A02 > Server
 * PROGRAMMER       : Mohammad Mehdi Ebrahimzadeh
 * FIRST VERSION    : 2026-02-16
 * DESCRIPTION      : This class handles logging functionality for the server application.
 *                    Writes timestamped messages to a log file with thread-safe operations.
 */

using System;
using System.IO;

namespace Server_WordGuessingGame.Helper
{
    /*
     * NAME    : ServerLogger
     * PURPOSE : Provides logging capabilities for the server application. Ensures thread-safe
     *           file operations and creates timestamped log entries.
     */
    internal class ServerLogger
    {
        private static string logFileName = "";
        private const string DefaultLogDirectory = "Logs";
        private const string DefaultLogFileName = "server.log";
        private static readonly object logLock = new object();
        private static bool initialized = false;

        /// <summary>
        /// Initializes the logger with a unique log file name
        /// </summary>
        /// <param name="baseLogFileName">The base name of the log file</param>
        internal static void Initialize(string baseLogFileName)
        {
            lock (logLock)
            {
                if (!initialized)
                {
                    logFileName = GenerateUniqueLogFileName(baseLogFileName);
                    initialized = true;

                    try
                    {
                        string directory = Path.GetDirectoryName(logFileName);
                        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                        {
                            Directory.CreateDirectory(directory);
                        }

                        File.WriteAllText(logFileName, "-----------------------------------------------n");
                        File.AppendAllText(logFileName, "SERVER LOG - SESSION STARTED\n");
                        File.AppendAllText(logFileName, "-----------------------------------------------n");
                        File.AppendAllText(logFileName, "Session: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "\n");
                        File.AppendAllText(logFileName, "-----------------------------------------------n\n");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Logger initialization error: " + ex.Message);
                        logFileName = "";
                        initialized = false;
                    }
                }
            }

            return;
        }

        /// <summary>
        /// Generates a unique log file name using timestamp
        /// </summary>
        /// <param name="baseLogFileName">Base log file name</param>
        /// <returns>Unique log file name with timestamp</returns>
        private static string GenerateUniqueLogFileName(string baseLogFileName)
        {
            string result = "";
            string fileNameWithoutExt = "";
            string extension = "";
            string timestamp = "";
            string uniqueFileName = "";

            fileNameWithoutExt = Path.GetFileNameWithoutExtension(baseLogFileName);
            extension = Path.GetExtension(baseLogFileName);

            if (string.IsNullOrEmpty(extension))
            {
                extension = ".log";
            }

            timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
            uniqueFileName = fileNameWithoutExt + "_" + timestamp + extension;
            result = Path.Combine(DefaultLogDirectory, uniqueFileName);

            return result;
        }

        /// <summary>
        /// Logs a message to the log file
        /// </summary>
        /// <param name="message">Message to log</param>
        internal static void Log(string message)
        {
            string logMessage = "";

            if (!initialized || string.IsNullOrEmpty(logFileName))
            {
                Initialize(DefaultLogFileName);
            }

            if (!string.IsNullOrEmpty(logFileName))
            {
                logMessage = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " - " + message;

                lock (logLock)
                {
                    try
                    {
                        File.AppendAllText(logFileName, logMessage + Environment.NewLine);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Logger write error: " + ex.Message);
                    }
                }
            }

            return;
        }

        /// <summary>
        /// Logs a message and also displays it in the console
        /// </summary>
        /// <param name="message">Message to log and display</param>
        internal static void LogAndDisplay(string message)
        {
            Console.WriteLine(message);
            Log(message);
            return;
        }

        /// <summary>
        /// Gets the current log file name
        /// </summary>
        /// <returns>Current log file name</returns>
        internal static string GetLogFileName()
        {
            return logFileName;
        }
    }
}
