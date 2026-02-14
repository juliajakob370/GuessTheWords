/*
 * FILE             : GameDataLoader.cs
 * PROJECT          : GuessTheWords-A02 > Server
 * PROGRAMMER       : Mohammad Mehdi Ebrahimzadeh, Julia Jakob
 * FIRST VERSION    : 2026-02-16
 * DESCRIPTION      : This class loads game data files from the GameData folder and validates
 *                    the content according to the game requirements.
 */

using System;
using System.Collections.Generic;
using System.IO;
using Server_WordGuessingGame.Helper;

namespace Server_WordGuessingGame.Game
{
    /*
     * NAME    : GameDataLoader
     * PURPOSE : Loads and validates game data files. Ensures each file has correct format
     *           with a 30-character puzzle string, word count, and valid word list.
     */
    internal class GameDataLoader
    {
        private readonly string gameDataFolder;
        private const int MIN_LINES = 3;
        private const int PUZZLE_LENGTH = 30;
        internal GameDataLoader(string folderPath)
        {
            gameDataFolder = folderPath;
        }

        /// <summary>
        /// Loads all valid game data files from the GameData folder
        /// </summary>
        /// <returns>List of GameData objects loaded from files</returns>
        internal List<GameData> LoadAllGameFiles()
        {
            List<GameData> gameFiles = new List<GameData>();
            string[] files;
            int i = 0;
            GameData gameData = null;
            string fullPath = "";
            string currentDir = "";

            try
            {
                currentDir = Directory.GetCurrentDirectory();
                ServerLogger.Log("Current working directory: " + currentDir);

                fullPath = Path.GetFullPath(gameDataFolder);
                ServerLogger.Log("Looking for game data at: " + fullPath);

                if (!Directory.Exists(gameDataFolder))
                {
                    ServerLogger.LogAndDisplay("Error: GameData folder not found at " + fullPath);
                    ServerLogger.LogAndDisplay("Current directory: " + currentDir);
                }
                else
                {
                    files = Directory.GetFiles(gameDataFolder, "*.txt");

                    if (files.Length == 0)
                    {
                        ServerLogger.LogAndDisplay("Error: No game data files found in " + fullPath);
                    }

                    else
                    {
                        ServerLogger.LogAndDisplay("Loading game data files from " + gameDataFolder);

                        for (i = 0; i < files.Length; i++)
                        {
                            gameData = LoadGameFile(files[i]);
                            if (gameData != null)
                            {
                                gameFiles.Add(gameData);
                                ServerLogger.LogAndDisplay("Loaded: " + Path.GetFileName(files[i]) +
                                    " (" + gameData.TotalWords + " words)");
                            }
                        }
                    }
                }

                ServerLogger.LogAndDisplay("Total game files loaded: " + gameFiles.Count);
            }
            catch (Exception ex)
            {
                ServerLogger.LogAndDisplay("Error loading game files: " + ex.Message);
            }

            return gameFiles;
        }

        /// <summary>
        /// Loads a single game data file
        /// </summary>
        /// <param name="filePath">Path to the game data file</param>
        /// <returns>GameData object if successful, null otherwise</returns>
        private GameData LoadGameFile(string filePath)
        {
            GameData gameData = null;
            string[] lines = null;
            string puzzleString = "";
            int totalWords = 0;
            string errorMessage = "";
            bool isValid = true;

            try
            {
                if (!File.Exists(filePath))
                {
                    ServerLogger.Log("File not found: " + filePath);
                    isValid = false;
                }
                else
                {
                    lines = File.ReadAllLines(filePath);

                    if (lines.Length < MIN_LINES)
                    {
                        ServerLogger.Log("Invalid file format (too few lines): " + filePath);
                        isValid = false;
                    }
                    else
                    {
                        puzzleString = lines[0].Trim();
                        if (puzzleString.Length != PUZZLE_LENGTH)
                        {
                            ServerLogger.Log("Invalid puzzle string length in " + filePath +
                                " (expected "+ PUZZLE_LENGTH.ToString() + " , got " + puzzleString.Length.ToString() + ")");
                            isValid = false;
                        }
                        else if (!int.TryParse(lines[1].Trim(), out totalWords))
                        {
                            ServerLogger.Log("Invalid word count in " + filePath);
                            isValid = false;
                        }
                        else if (lines.Length < (2 + totalWords))
                        {
                            ServerLogger.Log("File does not contain expected number of words: " + filePath);
                            isValid = false;
                        }
                        else
                        {
                            // All pre-checks passed - create and validate words
                            gameData = new GameData();
                            gameData.FileName = Path.GetFileName(filePath);
                            gameData.PuzzleString = puzzleString;
                            gameData.TotalWords = totalWords;
                            gameData.ValidWords = new List<string>();

                            for (int i = 2; i < lines.Length && gameData.ValidWords.Count < totalWords; i++)
                            {
                                string word = lines[i].Trim();
                                if (!string.IsNullOrWhiteSpace(word))
                                {
                                    if (!gameData.WordAppearsInPuzzle(word))
                                    {
                                        errorMessage = "Word '" + word + "' does not appear in puzzle in " + filePath;
                                        ServerLogger.Log(errorMessage);
                                        isValid = false;
                                        break;  // Exit loop early
                                    }
                                    gameData.ValidWords.Add(word);
                                }
                            }

                            if (isValid && gameData.ValidWords.Count != totalWords)
                            {
                                ServerLogger.Log("Word count mismatch in " + filePath);
                                isValid = false;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ServerLogger.Log("Error loading file " + filePath + ": " + ex.Message);
                isValid = false;
            }

            return isValid ? gameData : null;  // single return - if valid return gameData else return null
        }

    }
}
