/*
 * FILE             : ServerProtocol.cs
 * PROJECT          : GuessTheWords-A02 > Server
 * PROGRAMMER       : Mohammad Mehdi Ebrahimzadeh
 * FIRST VERSION    : 2026-02-16
 * DESCRIPTION      : This class handles parsing client requests and building server responses
 *                    according to the game protocol.
 */

using System;
using System.Collections.Generic;
using System.Text;

namespace Server_WordGuessingGame.Protocol
{
    /*
     * NAME    : ServerProtocol
     * PURPOSE : Manages the communication protocol between server and clients.
     *           Parses incoming requests and builds properly formatted responses.
     */
    internal class ServerProtocol
    {
        private readonly string version;
        private readonly string endLine;

        internal ServerProtocol(string protocolVersion, string endLineMarker)
        {
            version = protocolVersion;
            endLine = endLineMarker;
            return;
        }

        /// <summary>
        /// Parses a client request into a dictionary of key-value pairs
        /// </summary>
        /// <param name="requestText">The raw request text from client</param>
        /// <returns>Dictionary containing parsed request data</returns>
        internal Dictionary<string, string> ParseRequest(string requestText)
        {
            Dictionary<string, string> map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            string[] lines;
            int i = 0;

            if (!string.IsNullOrWhiteSpace(requestText))
            {
                lines = requestText.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

                for (i = 0; i < lines.Length; i++)
                {
                    string line = lines[i].Trim();

                    if (line == endLine)
                    {
                        i = lines.Length;
                    }
                    else
                    {


                        if (line.Contains(":"))
                        {
                            string[] parts = line.Split(new char[] { ':' }, 2);
                            string key = parts[0].Trim();
                            string value = parts[1].Trim();

                            if (!map.ContainsKey(key))
                            {
                                map.Add(key, value);
                            }
                        }
                        else
                        {
                            if (!map.ContainsKey("VERSION"))
                            {
                                map.Add("VERSION", line);
                            }
                        }
                    }
                }
            }

            return map;
        }

        /// <summary>
        /// Builds a response for a successful game start
        /// </summary>
        /// <param name="token">Session token for the client</param>
        /// <param name="puzzle">The 30-character puzzle string</param>
        /// <param name="totalWords">Total number of words in the puzzle</param>
        /// <param name="timeLimit">Time limit for the game in seconds</param>
        /// <returns>Formatted response string</returns>
        internal string BuildStartResponse(string token, string puzzle, int totalWords, int timeLimit)
        {
            StringBuilder response = new StringBuilder();

            response.AppendLine(version);
            response.AppendLine("STATUS:OK");
            response.AppendLine("TOKEN:" + token);
            response.AppendLine("PUZZLE:" + puzzle);
            response.AppendLine("TOTALWORDS:" + totalWords);
            response.AppendLine("TIMELIMIT:" + timeLimit);
            response.AppendLine(endLine);

            return response.ToString();
        }

        /// <summary>
        /// Builds a response for a guess validation
        /// </summary>
        /// <param name="result">Result code: F=Found, A=Already Found, N=Not Found</param>
        /// <param name="wordsRemaining">Number of words still to be found</param>
        /// <returns>Formatted response string</returns>
        internal string BuildGuessResponse(string result, int wordsRemaining)
        {
            StringBuilder response = new StringBuilder();

            response.AppendLine(version);
            response.AppendLine("STATUS:OK");
            response.AppendLine("RESULT:" + result);
            response.AppendLine("REMAINING:" + wordsRemaining);
            response.AppendLine(endLine);

            return response.ToString();
        }

        /// <summary>
        /// Builds an error response
        /// </summary>
        /// <param name="errorMessage">Error message to send to client</param>
        /// <returns>Formatted error response string</returns>
        internal string BuildErrorResponse(string errorMessage)
        {
            StringBuilder response = new StringBuilder();

            response.AppendLine(version);
            response.AppendLine("STATUS:ERROR");
            response.AppendLine("MESSAGE:" + errorMessage);
            response.AppendLine(endLine);

            return response.ToString();
        }

        /// <summary>
        /// Builds a response to notify clients of server shutdown
        /// </summary>
        /// <returns>Formatted shutdown message string</returns>
        internal string BuildShutdownMessage()
        {
            StringBuilder response = new StringBuilder();

            response.AppendLine(version);
            response.AppendLine("STATUS:SHUTDOWN");
            response.AppendLine("MESSAGE:Server is shutting down gracefully");
            response.AppendLine(endLine);

            return response.ToString();
        }

        /// <summary>
        /// Checks if a message is complete by looking for the end line marker
        /// </summary>
        /// <param name="bufferText">Text buffer to check</param>
        /// <returns>True if message is complete, false otherwise</returns>
        internal bool IsCompleteMessage(string bufferText)
        {
            bool complete = false;

            if (!string.IsNullOrEmpty(bufferText))
            {
                complete = bufferText.Contains(endLine);
            }

            return complete;
        }
    }
}
