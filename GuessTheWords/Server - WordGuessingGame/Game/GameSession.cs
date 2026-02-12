/*
 * FILE             : GameSession.cs
 * PROJECT          : GuessTheWords-A02 > Server
 * PROGRAMMER       : Mohammad Mehdi Ebrahimzadeh
 * FIRST VERSION    : 2026-02-16
 * DESCRIPTION      : This class represents a single client game session with all the state
 *                    information including player name, token, game data, and found words.
 */

using System;
using System.Collections.Generic;

namespace Server_WordGuessingGame.Game
{
    /*
     * NAME    : GameSession
     * PURPOSE : Manages the state of a single game session for one client.
     *           Tracks which words have been found and validates guesses.
     */
    internal class GameSession
    {
        internal string Token { get; set; }
        internal string PlayerName { get; set; }
        internal GameData GameData { get; set; }
        internal List<string> FoundWords { get; set; }
        internal DateTime CreatedUtc { get; set; }
        internal int TimeLimit { get; set; }
        internal string ClientIp { get; set; }
        internal int ClientListenerPort { get; set; }

        internal GameSession()
        {
            Token = "";
            PlayerName = "";
            GameData = null;
            FoundWords = new List<string>();
            CreatedUtc = DateTime.UtcNow;
            TimeLimit = 0;
            ClientIp = "";
            ClientListenerPort = 0;
            return;
        }

        internal GameSession(string token, string playerName, GameData gameData, int timeLimit)
        {
            Token = token;
            PlayerName = playerName;
            GameData = gameData;
            TimeLimit = timeLimit;
            FoundWords = new List<string>();
            CreatedUtc = DateTime.UtcNow;
            ClientIp = "";
            ClientListenerPort = 0;
            return;
        }

        /// <summary>
        /// Validates a word guess and returns the result
        /// </summary>
        /// <param name="guess">The word being guessed</param>
        /// <returns>Result code: F=Found, A=Already Found, N=Not Found</returns>
        internal string ValidateGuess(string guess)
        {
            string result = "";
            bool isValid = false;
            bool alreadyFound = false;

            if (GameData == null || string.IsNullOrWhiteSpace(guess))
            {
                result = "N";
                return result;
            }

            isValid = GameData.IsValidWord(guess);

            if (!isValid)
            {
                result = "N";
                return result;
            }

            alreadyFound = IsWordAlreadyFound(guess);

            if (alreadyFound)
            {
                result = "A";
            }
            else
            {
                FoundWords.Add(guess.ToUpper());
                result = "F";
            }

            return result;
        }

        /// <summary>
        /// Checks if a word has already been found in this session
        /// </summary>
        /// <param name="word">Word to check</param>
        /// <returns>True if word was already found</returns>
        private bool IsWordAlreadyFound(string word)
        {
            bool found = false;
            int i = 0;

            if (FoundWords != null && !string.IsNullOrWhiteSpace(word))
            {
                for (i = 0; i < FoundWords.Count; i++)
                {
                    if (string.Equals(FoundWords[i], word, StringComparison.OrdinalIgnoreCase))
                    {
                        found = true;
                        break;
                    }
                }
            }

            return found;
        }

        /// <summary>
        /// Gets the number of words remaining to be found
        /// </summary>
        /// <returns>Number of words not yet found</returns>
        internal int GetRemainingWords()
        {
            int remaining = 0;

            if (GameData != null)
            {
                remaining = GameData.TotalWords - FoundWords.Count;
                if (remaining < 0)
                {
                    remaining = 0;
                }
            }

            return remaining;
        }

        /// <summary>
        /// Checks if all words have been found
        /// </summary>
        /// <returns>True if all words found</returns>
        internal bool IsGameComplete()
        {
            bool complete = false;

            if (GameData != null && FoundWords.Count >= GameData.TotalWords)
            {
                complete = true;
            }

            return complete;
        }

        /// <summary>
        /// Checks if the game session has expired based on time limit
        /// </summary>
        /// <returns>True if session has expired</returns>
        internal bool IsExpired()
        {
            bool expired = false;
            TimeSpan elapsed = TimeSpan.Zero;
            int elapsedSeconds = 0;

            elapsed = DateTime.UtcNow - CreatedUtc;
            elapsedSeconds = (int)elapsed.TotalSeconds;

            if (elapsedSeconds > TimeLimit)
            {
                expired = true;
            }

            return expired;
        }
    }
}
