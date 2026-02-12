/*
 * FILE             : ClientLogger.cs
 * PROJECT          : GuessTheWords-A02 > Client
 * PROGRAMMER       : Bibi Murwared
 * FIRST VERSION    : 2026-02-16
 * DESCRIPTION      : This class writes client log messages to a log file from the config file.
 */

using System;
using System.Configuration;
using System.IO;

namespace Client_GuessTheWords
{
    /*
     * NAME    : ClientLogger
     * PURPOSE : Handles logging functionality for the Client application. Writes timestamped messages
     *           to a unique log file. Each client instance gets a unique log file using timestamps.
     */
    internal class ClientLogger
    {
        private static string _logFileName = "";
        private static readonly object _logLock = new object();
        private static bool _initialized = false;

        /// <summary>
        /// Initializes the logger with a unique log file name for this instance
        /// </summary>
        /// <param name="baseLogFileName">The base name of the log file from config</param>
        internal static void Initialize(string baseLogFileName)
        {
            lock (_logLock)
            {
                if (!_initialized)
                {
                    _logFileName = GenerateUniqueLogFileName(baseLogFileName);
                    _initialized = true;

                    try
                    {
                        string directory = Path.GetDirectoryName(_logFileName);
                        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                        {
                            Directory.CreateDirectory(directory);
                        }

                        File.WriteAllText(_logFileName, "===========================================\n");
                        File.AppendAllText(_logFileName, "CLIENT LOG - SESSION STARTED\n");
                        File.AppendAllText(_logFileName, "===========================================\n");
                        File.AppendAllText(_logFileName, "Session: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "\n");
                        File.AppendAllText(_logFileName, "===========================================\n\n");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Logger initialization error: " + ex.Message);
                        _logFileName = "";
                        _initialized = false;
                    }
                }
            }

            return;
        }

        /// <summary>
        /// Generates a unique log file name using timestamp
        /// </summary>
        /// <param name="baseLogFileName">Base log file name from config</param>
        /// <returns>Unique log file name</returns>
        private static string GenerateUniqueLogFileName(string baseLogFileName)
        {
            string fileNameWithoutExt = Path.GetFileNameWithoutExtension(baseLogFileName);
            string extension = Path.GetExtension(baseLogFileName);

            if (string.IsNullOrEmpty(extension))
            {
                extension = ".log";
            }

            string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
            string uniqueFileName = fileNameWithoutExt + "_" + timestamp + extension;

            return Path.Combine("Logs", uniqueFileName);
        }

        /// <summary>
        /// Logs a message to the log file
        /// </summary>
        /// <param name="message">Message to log</param>
        internal static void Log(string message)
        {
            if (!_initialized || string.IsNullOrEmpty(_logFileName))
            {
                string baseLogFileName = LoadLogFileNameFromConfig();
                Initialize(baseLogFileName);
            }

            if (!string.IsNullOrEmpty(_logFileName))
            {
                string logMessage = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " - " + message;

                lock (_logLock)
                {
                    try
                    {
                        File.AppendAllText(_logFileName, logMessage + Environment.NewLine);
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
        /// Loads the log file name from configuration
        /// </summary>
        /// <returns>Log file name</returns>
        private static string LoadLogFileNameFromConfig()
        {
            string logFileName = "client.log";

            try
            {
                string configValue = ConfigurationManager.AppSettings["LogFileName"];
                if (!string.IsNullOrEmpty(configValue))
                {
                    logFileName = configValue;
                }
            }
            catch (Exception ex)
            {
                // Use default if config read fails
            }

            return logFileName;
        }

        /// <summary>
        /// Gets the current log file name
        /// </summary>
        /// <returns>Current log file name</returns>
        internal static string GetLogFileName()
        {
            return _logFileName;
        }
    }
}
