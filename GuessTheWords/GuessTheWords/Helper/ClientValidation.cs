/*
 * FILE             : ClientValidation.cs
 * PROJECT          : GuessTheWords-A02 > Client
 * PROGRAMMER       : Bibi Murwared
 * FIRST VERSION    : 2026-02-16
 * DESCRIPTION      : Validates client configuration input (IP, port, buffer size, name length)
 *                    and player name using configuration-defined ranges.
 */

using A02_TCPIP;
using System;
using System.Net;

namespace ClientsWordGame.Helper
{
    /*
     * NAME    : ClientValidation
     * PURPOSE : Handle validation of client input (configuration values and player name)
     *           using the configuration ranges for Server IP, Port, buffer size and
     *           maximum name length.
     */
    internal class ClientValidation
    {
        private int _minPort;
        private int _maxPort;
        private int _minBuffer;
        private int _maxBuffer;
        private int _minNameLength;
        private int _maxNameLength;

        /// <summary>
        /// Constructor to initialize validation with configuration ranges.
        /// </summary>
        /// <param name="minPort">Minimum valid port number</param>
        /// <param name="maxPort">Maximum valid port number</param>
        /// <param name="minBuffer">Minimum buffer size</param>
        /// <param name="maxBuffer">Maximum buffer size</param>
        /// <param name="minNameLength">Minimum player name length</param>
        /// <param name="maxNameLength">Maximum player name length</param>
        public ClientValidation(int minPort,int maxPort,int minBuffer,int maxBuffer,int minNameLength,int maxNameLength)
        {
            _minPort = minPort;
            _maxPort = maxPort;
            _minBuffer = minBuffer;
            _maxBuffer = maxBuffer;
            _minNameLength = minNameLength;
            _maxNameLength = maxNameLength;
        }

        /// <summary>
        /// Validates all config values read from App.config before starting the client.
        /// </summary>
        /// <param name="serverIp">server io address as text from config</param>
        /// <param name="serverPort">server port as text from config</param>
        /// <param name="bufferSize">read buffer size as text from config</param>
        /// <param name="maxNameLen">MaxNameLength as text from config</param>
        /// <returns>Empty string if all values are valid otherwise an error string for the UI.</returns>
        public string ValidateAllConfig(string serverIp,string serverPort,string bufferSize,string maxNameLen)
        {
            string result = "";

            bool validIP = ValidateIP(serverIp);
            bool validPort = ValidatePort(serverPort);
            bool validBuffer = ValidateBufferSize(bufferSize);
            bool validNameLen = ValidateMaxNameLength(maxNameLen);

            if (!validIP)
            {
                result = result + "\nInvalid Server IP Address in config!";
            }
            else if (!validPort)
            {
                result = result + "\nInvalid Server Port in config!";
            }
            else if (!validBuffer)
            {
                result = result + "\nInvalid ReadBufferSize in config!";
            }
            else if (!validNameLen)
            {
                result = result + "\nInvalid MaxNameLength in config!";
            }

            return result;
        }

        /// <summary>
        /// Validates a single IP address text.
        /// </summary>
        internal bool ValidateIP(string ip)
        {
            IPAddress parsed = null;
            bool goodIP = IPAddress.TryParse(ip, out parsed);

            if (!goodIP)
            {
                ClientLogger.Log("Config/IP validation error: value for IP address could not be parsed [" + ip + "].");
            }

            return goodIP;
        }

        /// <summary>
        /// Validates a single port text against the configured range.
        /// </summary>
        internal bool ValidatePort(string port)
        {
            int checkPort = 0;

            if (int.TryParse(port, out checkPort))
            {
                if (checkPort >= _minPort && checkPort <= _maxPort)
                {
                    return true;
                }

                ClientLogger.Log("Config/port validation error: port not in valid range " + _minPort + "-" + _maxPort + " [" + checkPort + "].");
            }
            else
            {
                ClientLogger.Log("Config/port validation error: value could not be parsed to int [" + port + "].");
            }

            return false;
        }

        /// <summary>
        /// Validates a buffer size string against the configured range.
        /// </summary>
        internal bool ValidateBufferSize(string bufferSize)
        {
            int checkSize = 0;

            if (int.TryParse(bufferSize, out checkSize))
            {
                if (checkSize >= _minBuffer && checkSize <= _maxBuffer)
                {
                    return true;
                }

                ClientLogger.Log("Config/buffer validation error: buffer size not in valid range " + _minBuffer + "-" + _maxBuffer + " [" + checkSize + "].");
            }
            else
            {
                ClientLogger.Log("Config/buffer validation error: value could not be parsed to int [" + bufferSize + "].");
            }

            return false;
        }

        /// <summary>
        /// Validates MaxNameLength string against the configured allowed range.
        /// </summary>
        internal bool ValidateMaxNameLength(string maxNameLen)
        {
            int checkLen = 0;

            if (int.TryParse(maxNameLen, out checkLen))
            {
                if (checkLen >= _minNameLength && checkLen <= _maxNameLength)
                {
                    return true;
                }

                ClientLogger.Log("Config/name length validation error: MaxNameLength not in valid range " + _minNameLength + "-" + _maxNameLength + " [" + checkLen + "].");
            }
            else
            {
                ClientLogger.Log("Config/name length validation error: value could not be parsed to int [" + maxNameLen + "].");
            }

            return false;
        }

        /// <summary>
        /// Validates the player name entered in the UI.
        /// </summary>
        /// <param name="playerName">name from textbox</param>
        /// <param name="configuredMaxNameLength">MaxNameLength value from config (already parsed)</param>
        /// <returns>Empty string if OK, or user-friendly error message for the UI.</returns>
        public string ValidatePlayerName(string playerName, int configuredMaxNameLength)
        {
            string result = "";

            if (string.IsNullOrWhiteSpace(playerName))
            {
                result = "Please enter your Name.";
                ClientLogger.Log("Player name validation error: name is empty or whitespace.");
            }
            else
            {
                string trimmed = playerName.Trim();
                int length = trimmed.Length;

                if (length < _minNameLength)
                {
                    result = "Name is too short.";
                    ClientLogger.Log("Player name validation error: shorter than minimum allowed length.");
                }
                else if (length > configuredMaxNameLength)
                {
                    result = "Name is too long. max " + configuredMaxNameLength + " characters.";
                    ClientLogger.Log("Player name validation error: exceeds MaxNameLength limit.");
                }
            }

            return result;
        }
    }
}
