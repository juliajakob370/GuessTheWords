/*
 * FILE             : ClientStateBag.cs
 * PROJECT          : A02-TCPIP > Client-GuessTheWords
 * PROGRAMMER       : Bibi Murwared
 * FIRST VERSION    : 2026-02-16
 * DESCRIPTION      : Stores the current client session state; The token, name, game and time.
 */
using System;
using System.Collections.Generic;
using System.Text;

namespace Client_GuessTheWords.Network
{
    /// <summary>
    /// Name: ClientStateBag
    /// PURPOSE: Holds the game info that teh client must remember between disocnnecteed request
    /// </summary>
    internal class ClientStateBag
    {

        internal string PlayerName { get; set; }
        internal string Token { get; set; }
        internal string Game { get; set; }
        internal int TotalWords { get; set; }
        internal int TimeLimit { get; set; }
        internal DateTime GameStartUtc { get; set; }

        internal ClientStateBag()
        {
            PlayerName = "";
            Token = "";
            Game = "";
            TotalWords = 0;
            TimeLimit = 0;
            GameStartUtc = DateTime.MinValue;
            return;

        }
        internal void NewGame(string playerName, string token, string game, int totalWords, int timeLimit)
        {
            PlayerName = playerName;
            Token = token;
            Game = game;
            TotalWords = totalWords;
            TimeLimit = timeLimit;
            GameStartUtc = DateTime.UtcNow;
            return;
        }
    
    internal int GetRemainingTime()
        {
            //UTC = Coordinated Universal Time

            int remainingTime = 0; //how many time left in the game
            TimeSpan timeUsed = TimeSpan.Zero; //how much time used since game started
            int elapsedSeconds = 0;

            if (GameStartUtc != DateTime.MinValue) // check if game started so min value not set yet
            {
                //check how long the game is running by subtratcting current time from 
                timeUsed = DateTime.UtcNow - GameStartUtc;
               
                //convert the elapsed time to total second
                elapsedSeconds = (int)timeUsed.TotalSeconds;  

                //calculate how much time is left
                remainingTime = TimeLimit - elapsedSeconds;

                //make sure the left time is not going below zero
                if (remainingTime < 0)
                {
                    remainingTime = 0;
                }
            }

            return remainingTime;
        }
    }
}
