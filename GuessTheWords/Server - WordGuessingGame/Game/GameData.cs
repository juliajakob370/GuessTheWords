/*
 * FILE             : GameData.cs
 * PROJECT          : GuessTheWords-A02 > Server
 * PROGRAMMER       : Mohammad Mehdi Ebrahimzadeh
 * FIRST VERSION    : 2026-02-16
 * DESCRIPTION      : This class represents a single game data file containing the puzzle string
 *                    and the list of valid words that can be found in the puzzle.
 */

using System;
using System.Collections.Generic;

namespace Server_WordGuessingGame.Game
{
    /*
     * NAME    : GameData
     * PURPOSE : Holds the data for one game puzzle including the 30-character string
     *           and all valid words that appear in that string.
     */
    internal class GameData
    {
        internal string PuzzleString { get; set; }
        internal int TotalWords { get; set; }
        internal List<string> ValidWords { get; set; }
        internal string FileName { get; set; }

        internal GameData()
        {
            PuzzleString = "";
            TotalWords = 0;
            ValidWords = new List<string>();
            FileName = "";
            return;
        }

        /// <summary>
        /// Checks if a given word is valid for this puzzle
        /// </summary>
        /// <param name="word">Word to check</param>
        /// <returns>True if word is in the valid words list</returns>
        internal bool IsValidWord(string word)
        {
            bool isValid = false;
            int i = 0;

            if (ValidWords != null && !string.IsNullOrWhiteSpace(word))
            {
                for (i = 0; i < ValidWords.Count; i++)
                {
                    if (string.Equals(ValidWords[i], word, StringComparison.OrdinalIgnoreCase))
                    {
                        isValid = true;
                        break;
                    }
                }
            }

            return isValid;
        }

        /// <summary>
        /// Checks if a word appears in the puzzle string (forward or backward)
        /// </summary>
        /// <param name="word">Word to search for</param>
        /// <returns>True if word appears in puzzle string</returns>
        internal bool WordAppearsInPuzzle(string word)
        {
            bool appears = false;
            string upperWord = "";
            string reversedWord = "";
            string upperPuzzle = "";

            if (!string.IsNullOrWhiteSpace(word) && !string.IsNullOrWhiteSpace(PuzzleString))
            {
                upperWord = word.ToUpper();
                upperPuzzle = PuzzleString.ToUpper();

                if (upperPuzzle.Contains(upperWord))
                {
                    appears = true;
                }
                else
                {
                    reversedWord = ReverseString(upperWord);
                    if (upperPuzzle.Contains(reversedWord))
                    {
                        appears = true;
                    }
                }
            }

            return appears;
        }

        /// <summary>
        /// Reverses a string
        /// </summary>
        /// <param name="input">String to reverse</param>
        /// <returns>Reversed string</returns>
        private string ReverseString(string input)
        {
            string result = "";
            char[] charArray;

            if (!string.IsNullOrEmpty(input))
            {
                charArray = input.ToCharArray();
                Array.Reverse(charArray);
                result = new string(charArray);
            }

            return result;
        }
    }
}
