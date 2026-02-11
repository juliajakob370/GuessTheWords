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
        //mark the end 
        private const string END_LINE = "END";
        /// <summary>
        /// builde the start game request that is sent to the server
        /// </summary>
        /// <param name="protocolVersion">what type of request is this</param>
        /// <param name="playerName">name entered by the player</param>
        /// <returns></returns>
        internal string BuildStartRequest(string protocolVersion, string playerName)
        {
            // stringbuilder is used to build the request line by line
            StringBuilder request = new StringBuilder();

            request.AppendLine(protocolVersion);
            request.AppendLine("CMD:START");
            request.AppendLine("NAME:" + playerName);
            request.AppendLine(END_LINE);


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
                complete = bufferText.Contains("END");

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
                string[] lines = responseText.Split(new char[] {'\r','\n'},StringSplitOptions.RemoveEmptyEntries);

                int i = 0;

                // loop through each line in the response
                for (i = 0; i < lines.Length; i++)
                {
                    // remove extra spaces from the line
                    string line = lines[i].Trim();

                    // if we reach the END  stop reading the message
                    if (line == END_LINE)
                    {
                        break;
                    }
                    //remind me seans validation i swear while we validate that whole data files
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

            //return the parsed data
            return map;
        }
    }
}
