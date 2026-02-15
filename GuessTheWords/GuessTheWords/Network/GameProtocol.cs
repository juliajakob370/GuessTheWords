/*
 * FILE             : GameProtocol.cs
 * PROJECT          : GuessTheWords-A02 > Client
 * PROGRAMMER       : Bibi Murwared
 * FIRST VERSION    : 2026-02-16
 * DESCRIPTION      : This class builds requests and reads responses using the game protocol.
 * https://learn.microsoft.com/en-us/dotnet/api/system.text.stringbuilder?view=net-10.0
 * https://stackoverflow.com/questions/13230414/case-insensitive-access-for-generic-dictionary
 * https://stackoverflow.com/questions/1508203/best-way-to-split-string-into-lines
 */
using System;
using System.Collections.Generic;
using System.Text;

namespace Client_GuessTheWords.Game
{
    /// <summary>
    /// NAME : GameProtocol
    /// Purpose : Creats request strings and parses server responses string using a text protocol
    /// </summary>
    internal class GameProtocol
    {
        private readonly string endLine;
        private readonly string version;

        /// <summary>
        /// Initializes a new instance of the GameProtocol with version and end of message marker.
        /// </summary>
        /// <param name="_version">Protocol version string to include in messages.</param>
        /// <param name="_endLine">End of message marker used to detect complete messages.</param>
        internal GameProtocol(string _version, string _endLine)
        {
            version = _version;
            endLine = _endLine;
        }
        /// <summary>
        /// build the start game request that is sent to the server
        /// </summary>
        /// <param name="playerName">name entered by the player</param>
        /// <returns>string of the request message for the server to start the game</returns>
        internal string BuildStartRequest(string playerName)
        {
            // stringbuilder is used to build the request line by line
            StringBuilder request = new StringBuilder();

            request.AppendLine(version);
            request.AppendLine("CMD:START");
            request.AppendLine("NAME:" + playerName);
            request.AppendLine(endLine);


            return request.ToString();

        }

        /// <summary>
        /// Builds a start game request that includes the client listener port.
        /// </summary>
        /// <param name="playerName">Name entered by the player.</param>
        /// <param name="listenerPort">Port number of the client notification listener.</param>
        /// <returns>Request message string for starting a game.</returns>
        internal string BuildStartRequestWithPort(string playerName, int listenerPort)
        {
            StringBuilder request = new StringBuilder();

            request.AppendLine(version);
            request.AppendLine("CMD:START");
            request.AppendLine("NAME:" + playerName);
            request.AppendLine("LISTENERPORT:" + listenerPort);
            request.AppendLine(endLine);

            return request.ToString();
        }

        /// <summary>
        /// Builds the request message to send to the server 
        /// </summary>
        /// <param name="token">token recieved from server and used to identify the client instance</param>
        /// <param name="guess">guess entered by user</param>
        /// <returns>string of the request message to send to the server for the guess validation</returns>
        internal string BuildGuessRequest(string token, string guess)
        {
            // stringbuilder is used to build the request line by line
            StringBuilder request = new StringBuilder();

            request.AppendLine(version);
            request.AppendLine("CMD:GUESS");
            request.AppendLine("TOKEN:" + token);
            request.AppendLine("WORD:" + guess);
            request.AppendLine(endLine);

            return request.ToString();
        }

        /// <summary>
        /// Builds a quit request message to end the current game session.
        /// </summary>
        /// <param name="token">Session token identifying the client game.</param>
        /// <returns>Formatted quit request message string.</returns>
        internal string BuildQuitRequest(string token)
        {
            StringBuilder request = new StringBuilder();

            request.AppendLine(version);
            request.AppendLine("CMD:QUIT");
            request.AppendLine("TOKEN:" + token);
            request.AppendLine(endLine);

            return request.ToString();
        }

        /// <summary>
        /// checks what server sends is a full message
        /// </summary>
        /// <param name="bufferText">text read from the server</param>
        /// <returns>true if END is found and false otherwise</returns>
        internal bool IsCompleteMessage(string bufferText)
        {
            //keeps if the message is complete
            bool complete = false;

            //check if the buffer is not empty
            if (!string.IsNullOrEmpty(bufferText))
            {
                // check for END marker 
                complete = bufferText.Contains(endLine);
            }

            //full message is found yay!!
            return complete;
        }

        /// <summary>
        /// parses the server response into key value pairs :)))))))))))))))))))))))))
        /// </summary>
        /// <param name="responseText">full response text from server</param>
        /// <returns> parsed response data in dictionary</returns>
        internal Dictionary<string, string> ParseResponse(string responseText)
        {
            // create a dictionary to store key value pairs from what server 
            Dictionary<string, string> map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            //go ahead if response is not empty or whitespace
            if (!string.IsNullOrWhiteSpace(responseText))
            {
                //split response into lines using newline characters
                string[] lines = responseText.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

                int i = 0;

                // loop through each line in the response
                for (i = 0; i < lines.Length; i++)
                {
                    // remove extra spaces from the line
                    string line = lines[i].Trim();

                    // if we reach the END  stop reading the message
                    if (line == endLine)
                    {
                        i = lines.Length;
                    }
                    else
                    {
                        // check if the line contains a key value format
                        if (line.Contains(":"))
                        {
                            // split the line into two parts at the first colon only
                            string[] parts = line.Split(new char[] { ':' }, 2);

                            //left side is the key 
                            string key = parts[0].Trim();

                            //right side is the value 
                            string value = parts[1].Trim();

                            // add the key only if it does not already exist
                            if (!map.ContainsKey(key))
                            {
                                map.Add(key, value);
                            }
                        }
                        else
                        {
                            //if there is no colon this line is the protocol version
                            if (!map.ContainsKey("VERSION"))
                            {
                                map.Add("VERSION", line);
                            }
                        }
                    }
                }
            }

            //return the parsed data
            return map;
        }
    }
}
