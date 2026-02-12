/*
 * FILE             : Program.cs
 * PROJECT          : GuessTheWords-A02 > Server
 * PROGRAMMER       : Mohammad Mehdi Ebrahimzadeh
 * FIRST VERSION    : 2026-02-16
 * DESCRIPTION      : Main entry point for the Word Guessing Game server.
 *                    Implements TCP listener with multi-client support using Tasks,
 *                    in-memory session management, and graceful shutdown handling.
 * REFERENCES       : https://learn.microsoft.com/en-us/dotnet/api/system.net.sockets.tcplistener
 *                    https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task
 */

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Server_WordGuessingGame.Game;
using Server_WordGuessingGame.Helper;
using Server_WordGuessingGame.Protocol;

namespace Server_WordGuessingGame
{
    internal class Program
    {
        private static TcpListener listener = null;
        private static GameSessionManager sessionManager = null;
        private static ServerProtocol protocol = null;
        private static CancellationTokenSource cancellationSource = null;
        private static int serverPort = 0;
        private static int bufferSize = 0;
        private static bool isRunning = false;
        private static readonly object runLock = new object();

        // Configuration Constants
        private const int DEFAULT_SERVER_PORT = 12345;
        private const int DEFAULT_BUFFER_SIZE = 1024;
        private const int DEFAULT_TIME_LIMIT = 180;
        private const int MIN_SERVER_PORT = 1024;
        private const int MAX_SERVER_PORT = 65535;
        private const int MIN_BUFFER_SIZE = 64;
        private const int SHUTDOWN_WAIT_MS = 500;
        private const int COLON_SPLIT_IP_INDEX = 0;
        private const int EMPTY_VALUE = 0;

        // Default Configuration Values
        private const string DEFAULT_END_LINE = "END";
        private const string DEFAULT_GAME_DATA_FOLDER = "GameData";
        private const string LOG_FILE_NAME = "server.log";

        private static void Main(string[] args)
        {
            bool configLoaded = false;

            Console.WriteLine("-----------------------------------------------");
            Console.WriteLine("   WORD GUESSING GAME - SERVER");
            Console.WriteLine("-----------------------------------------------");
            Console.WriteLine();

            ServerLogger.Initialize(LOG_FILE_NAME);

            configLoaded = LoadConfiguration();

            if (!configLoaded)
            {
                Console.WriteLine("Failed to load configuration. Press any key to exit.");
                Console.ReadKey();
                return;
            }

            cancellationSource = new CancellationTokenSource();

            Console.CancelKeyPress += HandleCancelKeyPress;

            StartServer();

            Console.WriteLine();
            Console.WriteLine("Press Ctrl+C to stop the server...");
            Console.WriteLine();

            WaitForShutdown();

            StopServer();

            Console.WriteLine("Server stopped. Press any key to exit.");
            Console.ReadKey();

            return;
        }

        private static bool LoadConfiguration()
        {
            bool success = true;
            string portText = "";
            string bufferText = "";
            string versionText = "";
            string endLineText = "";
            string gameDataFolder = "";
            string timeLimitText = "";
            int timeLimit = 0;
            List<GameData> gameFiles = null;
            GameDataLoader loader = null;

            try
            {
                ServerLogger.Log("Loading configuration...");

                portText = ConfigurationManager.AppSettings["ServerPort"];
                bufferText = ConfigurationManager.AppSettings["ReadBufferSize"];
                versionText = ConfigurationManager.AppSettings["ProtocolVersion"];
                endLineText = ConfigurationManager.AppSettings["EndLine"];
                gameDataFolder = ConfigurationManager.AppSettings["GameDataFolder"];
                timeLimitText = ConfigurationManager.AppSettings["DefaultTimeLimit"];

                if (string.IsNullOrWhiteSpace(portText))
                {
                    Console.WriteLine("Error: ServerPort not found in config. Using default " + DEFAULT_SERVER_PORT);
                    ServerLogger.Log("Error: ServerPort not found in config. Using default " + DEFAULT_SERVER_PORT);
                    portText = DEFAULT_SERVER_PORT.ToString();
                }

                if (string.IsNullOrWhiteSpace(bufferText))
                {
                    Console.WriteLine("Warning: ReadBufferSize not found in config. Using default " + DEFAULT_BUFFER_SIZE);
                    ServerLogger.Log("Warning: ReadBufferSize not found in config. Using default " + DEFAULT_BUFFER_SIZE);
                    bufferText = DEFAULT_BUFFER_SIZE.ToString();
                }

                if (string.IsNullOrWhiteSpace(versionText))
                {
                    versionText = "GTW/1.0";
                }

                if (string.IsNullOrWhiteSpace(endLineText))
                {
                    endLineText = DEFAULT_END_LINE;
                }

                if (string.IsNullOrWhiteSpace(gameDataFolder))
                {
                    gameDataFolder = DEFAULT_GAME_DATA_FOLDER;
                }

                if (string.IsNullOrWhiteSpace(timeLimitText))
                {
                    timeLimitText = DEFAULT_TIME_LIMIT.ToString();
                }

                if (!int.TryParse(portText, out serverPort) || serverPort < MIN_SERVER_PORT || serverPort > MAX_SERVER_PORT)
                {
                    Console.WriteLine("Error: Invalid server port: " + portText);
                    Console.WriteLine("Port must be between " + MIN_SERVER_PORT + " and " + MAX_SERVER_PORT);
                    ServerLogger.Log("Error: Invalid server port: " + portText);
                    success = false;
                    return success;
                }

                if (!int.TryParse(bufferText, out bufferSize) || bufferSize < MIN_BUFFER_SIZE)
                {
                    Console.WriteLine("Error: Invalid buffer size");
                    ServerLogger.Log("Error: Invalid buffer size");
                    success = false;
                    return success;
                }

                if (!int.TryParse(timeLimitText, out timeLimit) || timeLimit <= EMPTY_VALUE)
                {
                    timeLimit = DEFAULT_TIME_LIMIT;
                }

                protocol = new ServerProtocol(versionText, endLineText);

                loader = new GameDataLoader(gameDataFolder);
                gameFiles = loader.LoadAllGameFiles();

                if (gameFiles.Count == EMPTY_VALUE)
                {
                    Console.WriteLine("Error: No valid game data files loaded");
                    ServerLogger.Log("Error: No valid game data files loaded");
                    success = false;
                    return success;
                }

                sessionManager = new GameSessionManager(gameFiles, timeLimit);

                Console.WriteLine("Configuration loaded successfully");
                Console.WriteLine("Server Port: " + serverPort);
                Console.WriteLine("Buffer Size: " + bufferSize);
                Console.WriteLine("Time Limit: " + timeLimit + " seconds");
                Console.WriteLine("Game Files: " + gameFiles.Count);
                
                ServerLogger.Log("Configuration loaded - Port: " + serverPort + ", Buffer: " + bufferSize + ", TimeLimit: " + timeLimit + ", GameFiles: " + gameFiles.Count);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Configuration error: " + ex.Message);
                ServerLogger.Log("Configuration error: " + ex.Message);
                success = false;
            }

            return success;
        }

        private static void StartServer()
        {
            string hostName = "";
            IPHostEntry hostEntry = null;
            int i = 0;

            try
            {
                listener = new TcpListener(IPAddress.Any, serverPort);
                listener.Start();

                lock (runLock)
                {
                    isRunning = true;
                }

                Console.WriteLine("Server started on port " + serverPort);
                Console.WriteLine("Listening on all network interfaces (0.0.0.0)");
                Console.WriteLine();
                
                ServerLogger.Log("Server started on port " + serverPort);

                try
                {
                    hostName = Dns.GetHostName();
                    hostEntry = Dns.GetHostEntry(hostName);

                    Console.WriteLine("Server IP Addresses:");
                    Console.WriteLine("  Localhost: 127.0.0.1:" + serverPort);

                    ServerLogger.Log("Server IP: 127.0.0.1:" + serverPort);

                    for (i = 0; i < hostEntry.AddressList.Length; i++)
                    {
                        if (hostEntry.AddressList[i].AddressFamily == AddressFamily.InterNetwork)
                        {
                            Console.WriteLine("  Network: " + hostEntry.AddressList[i].ToString() + ":" + serverPort);
                            ServerLogger.Log("Network IP: " + hostEntry.AddressList[i].ToString() + ":" + serverPort);
                        }
                    }

                    Console.WriteLine();
                    Console.WriteLine("Clients on other computers should use the Network IP address");
                }
                catch (Exception ex)
                {
                    ServerLogger.Log("Could not determine IP addresses: " + ex.Message);
                }

                Task.Run(() => AcceptClientsAsync(cancellationSource.Token));
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error starting server: " + ex.Message);
                ServerLogger.Log("Error starting server: " + ex.Message);
            }

            return;
        }

        private static async Task AcceptClientsAsync(CancellationToken cancellationToken)
        {
            ServerLogger.Log("Listener task started - ready to accept multiple clients");

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    TcpClient client = await listener.AcceptTcpClientAsync();
                    
                    string clientEndpoint = client.Client.RemoteEndPoint.ToString();
                    ServerLogger.Log("Client connected from: " + clientEndpoint);

                    Task.Run(() => HandleClientAsync(client, cancellationToken), cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (ObjectDisposedException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    if (!cancellationToken.IsCancellationRequested)
                    {
                        ServerLogger.Log("Error accepting client: " + ex.Message);
                    }
                }
            }

            ServerLogger.Log("Listener task stopped");

            return;
        }

        private static async Task HandleClientAsync(TcpClient client, CancellationToken cancellationToken)
        {
            NetworkStream stream = null;
            string requestText = "";
            string responseText = "";
            string clientEndpoint = "";
            string sessionToken = "";
            bool normalDisconnect = false;

            try
            {
                clientEndpoint = client.Client.RemoteEndPoint.ToString();
                stream = client.GetStream();

                requestText = await ReadRequestAsync(stream, cancellationToken);

                if (!string.IsNullOrWhiteSpace(requestText))
                {
                    responseText = ProcessRequest(requestText, clientEndpoint, out sessionToken);

                    await SendResponseAsync(stream, responseText, cancellationToken);
                    normalDisconnect = true;
                }
            }
            catch (OperationCanceledException)
            {
                ServerLogger.Log("Client handler cancelled for: " + clientEndpoint);
            }
            catch (Exception ex)
            {
                ServerLogger.Log("Error handling client " + clientEndpoint + ": " + ex.Message);
            }
            finally
            {
                try
                {
                    if (stream != null)
                    {
                        stream.Close();
                    }
                    client.Close();
                    
                    if (normalDisconnect)
                    {
                        ServerLogger.Log("Client disconnected: " + clientEndpoint);
                    }
                    else
                    {
                        ServerLogger.Log("Client connection closed (timeout/error): " + clientEndpoint);
                    }
                }
                catch (Exception ex)
                {
                    ServerLogger.Log("Error closing client connection: " + ex.Message);
                }
            }

            return;
        }

        private static async Task<string> ReadRequestAsync(NetworkStream stream, CancellationToken cancellationToken)
        {
            string requestText = "";
            StringBuilder builder = new StringBuilder();
            byte[] buffer = new byte[bufferSize];
            bool done = false;
            int bytesRead = 0;
            string chunk = "";

            try
            {
                while (!done && !cancellationToken.IsCancellationRequested)
                {
                    bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, cancellationToken);

                    if (bytesRead == 0)
                    {
                        done = true;
                    }
                    else
                    {
                        chunk = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                        builder.Append(chunk);

                        if (protocol.IsCompleteMessage(builder.ToString()))
                        {
                            done = true;
                        }
                    }
                }

                requestText = builder.ToString();
                ServerLogger.Log("Request received: " + requestText.Replace("\r\n", " | "));
            }
            catch (Exception ex)
            {
                ServerLogger.Log("Error reading request: " + ex.Message);
            }

            return requestText;
        }

        private static async Task SendResponseAsync(NetworkStream stream, string responseText, 
            CancellationToken cancellationToken)
        {
            byte[] responseBytes;

            try
            {
                responseBytes = Encoding.UTF8.GetBytes(responseText);
                await stream.WriteAsync(responseBytes, 0, responseBytes.Length, cancellationToken);
                await stream.FlushAsync(cancellationToken);

                ServerLogger.Log("Response sent: " + responseText.Replace("\r\n", " | "));
            }
            catch (Exception ex)
            {
                ServerLogger.Log("Error sending response: " + ex.Message);
            }

            return;
        }

        private static string ProcessRequest(string requestText, string clientEndpoint, out string sessionToken)
        {
            string responseText = "";
            Dictionary<string, string> requestData = null;
            string command = "";
            string playerName = "";
            string guess = "";
            GameSession session = null;
            string result = "";
            int remaining = 0;
            int listenerPort = 0;
            string clientIp = "";

            sessionToken = "";

            try
            {
                requestData = protocol.ParseRequest(requestText);

                if (!requestData.ContainsKey("CMD"))
                {
                    responseText = protocol.BuildErrorResponse("Invalid request format");
                    return responseText;
                }

                command = requestData["CMD"];

                if (string.Equals(command, "START", StringComparison.OrdinalIgnoreCase))
                {
                    if (!requestData.ContainsKey("NAME"))
                    {
                        responseText = protocol.BuildErrorResponse("Player name required");
                        return responseText;
                    }

                    playerName = requestData["NAME"];
                    session = sessionManager.CreateSession(playerName);

                    if (session == null)
                    {
                        responseText = protocol.BuildErrorResponse("Failed to create game session");
                        return responseText;
                    }

                    sessionToken = session.Token;
                    
                    if (requestData.ContainsKey("LISTENERPORT") && int.TryParse(requestData["LISTENERPORT"], out listenerPort))
                    {
                        clientIp = clientEndpoint.Split(':')[COLON_SPLIT_IP_INDEX];
                        session.ClientIp = clientIp;
                        session.ClientListenerPort = listenerPort;
                        sessionManager.SaveSession(session);
                        ServerLogger.Log("Client listener registered - IP: " + clientIp + ", Port: " + listenerPort);
                    }
                    
                    ServerLogger.Log("New game session started for: " + playerName + " (Token: " + sessionToken + ")");
                    UpdateConnectedClientsDisplay();

                    responseText = protocol.BuildStartResponse(
                        session.Token,
                        session.GameData.PuzzleString,
                        session.GameData.TotalWords,
                        session.TimeLimit);
                }
                else if (string.Equals(command, "GUESS", StringComparison.OrdinalIgnoreCase))
                {
                    if (!requestData.ContainsKey("TOKEN") || !requestData.ContainsKey("WORD"))
                    {
                        responseText = protocol.BuildErrorResponse("Token and word required");
                        return responseText;
                    }

                    sessionToken = requestData["TOKEN"];
                    guess = requestData["WORD"];

                    session = sessionManager.GetSession(sessionToken);

                    if (session == null)
                    {
                        responseText = protocol.BuildErrorResponse("Invalid or expired session");
                        ServerLogger.Log("Invalid/expired session attempt with token: " + sessionToken);
                        return responseText;
                    }

                    result = session.ValidateGuess(guess);
                    sessionManager.SaveSession(session);

                    remaining = session.GetRemainingWords();

                    responseText = protocol.BuildGuessResponse(result, remaining);

                    if (session.IsGameComplete())
                    {
                        ServerLogger.Log("All words found by: " + session.PlayerName + " (Token: " + sessionToken + ")");
                        sessionManager.RemoveSession(sessionToken);
                        UpdateConnectedClientsDisplay();
                    }
                }
                else if (string.Equals(command, "QUIT", StringComparison.OrdinalIgnoreCase))
                {
                    if (!requestData.ContainsKey("TOKEN"))
                    {
                        responseText = protocol.BuildErrorResponse("Token required");
                        return responseText;
                    }

                    sessionToken = requestData["TOKEN"];
                    session = sessionManager.GetSession(sessionToken);

                    if (session != null)
                    {
                        ServerLogger.Log("Client quit - Player: " + session.PlayerName + ", Token: " + sessionToken);
                        sessionManager.RemoveSession(sessionToken);
                        UpdateConnectedClientsDisplay();
                    }

                    responseText = protocol.BuildStartResponse("", "", EMPTY_VALUE, EMPTY_VALUE);
                }
                else
                {
                    responseText = protocol.BuildErrorResponse("Unknown command: " + command);
                }
            }
            catch (Exception ex)
            {
                ServerLogger.Log("Error processing request: " + ex.Message);
                responseText = protocol.BuildErrorResponse("Internal server error");
            }

            return responseText;
        }

        private static void UpdateConnectedClientsDisplay()
        {
            int activeSessions = 0;
            
            if (sessionManager != null)
            {
                activeSessions = sessionManager.GetActiveSessionCount();
            }
            
            Console.SetCursorPosition(EMPTY_VALUE, Console.CursorTop);
            Console.Write("                                                                ");
            Console.SetCursorPosition(EMPTY_VALUE, Console.CursorTop);
            Console.Write("Active Players: " + activeSessions);
            
            Console.Title = "Word Guessing Game Server - Active Players: " + activeSessions;
            ServerLogger.Log("Active Players: " + activeSessions);
        }

        private static void HandleCancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            e.Cancel = true;
            Console.WriteLine();
            Console.WriteLine("Shutdown signal received...");
            ServerLogger.Log("Shutdown signal received");
            
            NotifyClientsOfShutdown();
            
            cancellationSource.Cancel();

            return;
        }

        private static void NotifyClientsOfShutdown()
        {
            List<GameSession> sessions = null;
            string shutdownMessage = "";
            byte[] messageBytes = null;
            int notifiedCount = 0;
            int i = 0;
            TcpClient client = null;
            NetworkStream stream = null;

            if (sessionManager == null)
            {
                return;
            }

            sessions = sessionManager.GetAllActiveSessions();

            if (sessions.Count == EMPTY_VALUE)
            {
                ServerLogger.Log("No active sessions to notify");
                return;
            }

            shutdownMessage = protocol.BuildShutdownMessage();
            messageBytes = Encoding.UTF8.GetBytes(shutdownMessage);

            ServerLogger.Log("Notifying " + sessions.Count + " active player(s) of shutdown");

            for (i = 0; i < sessions.Count; i++)
            {
                try
                {
                    if (!string.IsNullOrWhiteSpace(sessions[i].ClientIp) && sessions[i].ClientListenerPort > EMPTY_VALUE)
                    {
                        client = new TcpClient();
                        client.Connect(sessions[i].ClientIp, sessions[i].ClientListenerPort);
                        stream = client.GetStream();
                        stream.Write(messageBytes, 0, messageBytes.Length);
                        stream.Flush();
                        stream.Close();
                        client.Close();
                        notifiedCount++;
                        ServerLogger.Log("Notified " + sessions[i].PlayerName + " at " + sessions[i].ClientIp + ":" + sessions[i].ClientListenerPort);
                    }
                }
                catch (Exception ex)
                {
                    ServerLogger.Log("Failed to notify " + sessions[i].PlayerName + ": " + ex.Message);
                }
            }

            ServerLogger.Log("Successfully notified " + notifiedCount + " player(s)");

            return;
        }

        private static void WaitForShutdown()
        {
            while (!cancellationSource.Token.IsCancellationRequested)
            {
                Thread.Sleep(SHUTDOWN_WAIT_MS);
            }

            return;
        }

        private static void StopServer()
        {
            bool running = false;

            lock (runLock)
            {
                running = isRunning;
                isRunning = false;
            }

            if (running)
            {
                Console.WriteLine("Stopping server...");
                ServerLogger.Log("Stopping server");

                Thread.Sleep(SHUTDOWN_WAIT_MS);

                try
                {
                    if (listener != null)
                    {
                        listener.Stop();
                        Console.WriteLine("Listener stopped");
                        ServerLogger.Log("Listener stopped");
                    }
                }
                catch (Exception ex)
                {
                    ServerLogger.Log("Error stopping listener: " + ex.Message);
                }

                if (sessionManager != null)
                {
                    int remainingSessions = sessionManager.GetActiveSessionCount();
                    if (remainingSessions > EMPTY_VALUE)
                    {
                        ServerLogger.Log("Cleaning up " + remainingSessions + " active session(s)");
                    }
                    sessionManager.Shutdown();
                }

                Console.WriteLine("Server shutdown complete");
                ServerLogger.Log("Server shutdown complete");
            }

            return;
        }
    }
}