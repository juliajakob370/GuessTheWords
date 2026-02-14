 /*
 * FILE             : GameSessionManager.cs
 * PROJECT          : GuessTheWords-A02 > Server
 * PROGRAMMER       : Mohammad Mehdi Ebrahimzadeh, Julia Jakob
 * FIRST VERSION    : 2026-02-16
 * DESCRIPTION      : This class manages all active game sessions using in-memory storage.
 *                    Handles session creation, retrieval, cleanup, and graceful shutdown.
 */

using System;
using System.Collections.Generic;
using Server_WordGuessingGame.Helper;

namespace Server_WordGuessingGame.Game
{
    /*
     * NAME    : GameSessionManager
     * PURPOSE : Manages game sessions with in-memory storage.
     *           Provides thread-safe operations for session management.
     */
    internal class GameSessionManager
    {
        private readonly List<GameData> gameDataFiles;
        private readonly Random random;
        private readonly int defaultTimeLimit;
        private readonly Dictionary<string, GameSession> localSessions;
        private readonly object sessionLock;

        private const int TOKEN_SEGMENT_LENGTH = 8; // length of each token segment for the fixed session tokens

        internal GameSessionManager(List<GameData> gameFiles, int timeLimit)
        {
            gameDataFiles = gameFiles;
            defaultTimeLimit = timeLimit;
            random = new Random();
            localSessions = new Dictionary<string, GameSession>(StringComparer.OrdinalIgnoreCase); // make the dictionary case insensitive for guess look ups
            sessionLock = new object();

            return;
        }

        /// <summary>
        /// Creates a new game session for a player
        /// </summary>
        /// <param name="playerName">Name of the player</param>
        /// <returns>New GameSession object</returns>
        internal GameSession CreateSession(string playerName)
        {
            bool noGameFiles = false; // to check if there are game files
            GameSession session = null;
            string token = "";
            GameData selectedGame = null;
            int randomIndex = 0;

            if (gameDataFiles == null || gameDataFiles.Count == 0)
            {
                ServerLogger.Log("No game data available for new session");
                noGameFiles = true;
            }

            else if (!noGameFiles)
            {
                token = GenerateToken();
                randomIndex = random.Next(0, gameDataFiles.Count);
                selectedGame = gameDataFiles[randomIndex];

                session = new GameSession(token, playerName, selectedGame, defaultTimeLimit);

                SaveSession(session);

                ServerLogger.Log("Session created - Player: " + playerName + ", Token: " + token +
                    ", Game: " + selectedGame.FileName + ", Words: " + selectedGame.TotalWords);
            }
                
            return session;
        }

        /// <summary>
        /// Retrieves a game session by token
        /// </summary>
        /// <param name="token">Session token</param>
        /// <returns>GameSession if found, null otherwise</returns>
        internal GameSession GetSession(string token)
        {
            GameSession session = null;

            if (!string.IsNullOrWhiteSpace(token))
            {
                lock (sessionLock)
                {
                    if (localSessions.ContainsKey(token))
                    {
                        session = localSessions[token];
                    }
                }
            }
            return session;
        }

        /// <summary>
        /// Saves or updates a game session
        /// </summary>
        /// <param name="session">Session to save</param>
        internal void SaveSession(GameSession session)
        {
            if (session != null && !string.IsNullOrWhiteSpace(session.Token))
            {
                lock (sessionLock)
                {
                    if (localSessions.ContainsKey(session.Token))
                    {
                        localSessions[session.Token] = session;
                    }
                    else
                    {
                        localSessions.Add(session.Token, session);
                    }
                }
            }
            return;
        }

        /// <summary>
        /// Removes a game session
        /// </summary>
        /// <param name="token">Session token to remove</param>
        internal void RemoveSession(string token)
        {
            if (!string.IsNullOrWhiteSpace(token))
            {
                lock (sessionLock)
                {
                    if (localSessions.ContainsKey(token))
                    {
                        localSessions.Remove(token);
                        ServerLogger.Log("Session removed: " + token);
                    }
                }
            }

            return;
        }

        /// <summary>
        /// Generates a unique session token
        /// </summary>
        /// <returns>Unique token string</returns>
        private string GenerateToken()
        {
            string token = "";
            string guid = "";
            string timestamp = "";

            guid = Guid.NewGuid().ToString("N"); // generate guid for session token with no dashes or seperators
            timestamp = DateTime.UtcNow.Ticks.ToString(); // get timestamp for token
            token = guid.Substring(0, TOKEN_SEGMENT_LENGTH) + timestamp.Substring(timestamp.Length - TOKEN_SEGMENT_LENGTH); // create a unique token for the client

            return token.ToUpper();
        }

        /// <summary>
        /// Cleans up all sessions
        /// </summary>
        internal void Shutdown()
        {
            int sessionCount = 0;
            
            lock (sessionLock)
            {
                sessionCount = localSessions.Count;
            }
            
            ServerLogger.LogAndDisplay("Shutting down session manager...");
            
            if (sessionCount > 0)
            {
                ServerLogger.LogAndDisplay("Removing " + sessionCount + " active session(s)");
            }

            lock (sessionLock)
            {
                localSessions.Clear();
            }

            ServerLogger.LogAndDisplay("Session manager shutdown complete");

            return;
        }

        /// <summary>
        /// Gets the count of active sessions
        /// </summary>
        /// <returns>Number of active sessions</returns>
        internal int GetActiveSessionCount()
        {
            int count = 0;

            lock (sessionLock)
            {
                count = localSessions.Count;
            }

            return count;
        }

        /// <summary>
        /// Gets all active sessions
        /// </summary>
        /// <returns>List of active GameSession objects</returns>
        internal List<GameSession> GetAllActiveSessions()
        {
            List<GameSession> sessions = new List<GameSession>();

            lock (sessionLock)
            {
                foreach (GameSession session in localSessions.Values)
                {
                    sessions.Add(session);
                }
            }

            return sessions;
        }
    }
}
