/*
 * FILE             : GameProtocol.cs
 * PROJECT          : GuessTheWords-A02 > Client
 * PROGRAMMER       : Bibi Murwared
 * FIRST VERSION    : 2026-02-16
 * DESCRIPTION      : This class builds requests and reads responses using the game protocol.
 */
using System;
using System.Collections.Generic;
using System.Text;

namespace Client_GuessTheWords.Game
{
    internal class GameProtocol
    {
        private readonly string endLine;
        private readonly string version;

        internal GameProtocol(string _version, string _endLine)
        {
            version = _version;
            endLine = _endLine;
        }
        /// <summary>
        /// build the start game request that is sent to the server
        /// </summary>
        /// <param name="playerName">name entered by the player</param>
        /// <returns></returns>
        internal string BuildStartRequest( string playerName)
        {
            // stringbuilder is used to build the request line by line
            StringBuilder request = new StringBuilder();

            request.AppendLine(version);
            request.AppendLine("CMD:START");
            request.AppendLine("NAME:" + playerName);
            request.AppendLine(endLine);


            return request.ToString();

        }

Parses response into simple fields
 */