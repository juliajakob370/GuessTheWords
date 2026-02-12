/*
 * FILE             : ClientConnection.cs
 * PROJECT          : GuessTheWords-A02 > Client
 * PROGRAMMER       : Bibi Murwared
 * FIRST VERSION    : 2026-02-16
 * DESCRIPTION      : This class handles TCP communication with the game server.
 */
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Client_GuessTheWords.Game
{
    internal class ClientConnection
    {
        private readonly string serverIp;
        private readonly int serverPort;
        private readonly int bufferSize;
        private readonly GameProtocol protocol;
        private TcpListener clientListener;
        private int listenerPort;
        private CancellationTokenSource listenerCancellation;
        private Action<string> onServerNotification;

        internal ClientConnection(string serverIpValue, int serverPortValue, int bufferSizeValue, GameProtocol protocolValue)
        {
            serverIp = serverIpValue;
            serverPort = serverPortValue;
            bufferSize = bufferSizeValue;
            protocol = protocolValue;
            listenerPort = 0;
            listenerCancellation = null;
            onServerNotification = null;
            return;
        }

        internal void StartListener(Action<string> notificationCallback)
        {
            onServerNotification = notificationCallback;
            listenerCancellation = new CancellationTokenSource();
            
            clientListener = new TcpListener(System.Net.IPAddress.Any, 0);
            clientListener.Start();
            listenerPort = ((System.Net.IPEndPoint)clientListener.LocalEndpoint).Port;
            
            ClientLogger.Log("Client listener started on port: " + listenerPort);
            
            Task.Run(() => ListenForServerNotificationsAsync(listenerCancellation.Token));
            
            return;
        }

        internal void StopListener()
        {
            try
            {
                if (listenerCancellation != null)
                {
                    listenerCancellation.Cancel();
                }
                
                if (clientListener != null)
                {
                    clientListener.Stop();
                    ClientLogger.Log("Client listener stopped");
                }
            }
            catch (Exception ex)
            {
                ClientLogger.Log("Error stopping listener: " + ex.Message);
            }
            
            return;
        }

        internal int GetListenerPort()
        {
            return listenerPort;
        }

        private async Task ListenForServerNotificationsAsync(CancellationToken cancellationToken)
        {
            ClientLogger.Log("Listening for server notifications...");
            
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    TcpClient client = await clientListener.AcceptTcpClientAsync();
                    Task.Run(() => HandleServerNotificationAsync(client, cancellationToken));
                }
                catch (ObjectDisposedException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    if (!cancellationToken.IsCancellationRequested)
                    {
                        ClientLogger.Log("Error accepting notification: " + ex.Message);
                    }
                }
            }
            
            ClientLogger.Log("Notification listener stopped");
            
            return;
        }

        private async Task HandleServerNotificationAsync(TcpClient client, CancellationToken cancellationToken)
        {
            NetworkStream stream = null;
            StringBuilder builder = null;
            byte[] buffer = null;
            bool done = false;
            int bytesRead = 0;
            string chunk = "";
            string message = "";
            
            try
            {
                stream = client.GetStream();
                builder = new StringBuilder();
                buffer = new byte[bufferSize];
                done = false;
                
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
                
                message = builder.ToString();
                ClientLogger.Log("Server notification received: " + message.Replace("\r\n", " | "));
                
                if (onServerNotification != null)
                {
                    onServerNotification(message);
                }
            }
            catch (Exception ex)
            {
                ClientLogger.Log("Error handling notification: " + ex.Message);
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
                }
                catch (Exception ex)
                {
                    ClientLogger.Log("Error closing notification connection: " + ex.Message);
                }
            }
            
            return;
        }

        internal async Task<string> SendRequestAsync(string requestText)
        {
            string responseText = "";
            TcpClient client = null;
            NetworkStream stream = null;

            try
            {
                client = new TcpClient();

                ClientLogger.Log("connecting to server " + serverIp + ":" + serverPort);
                await client.ConnectAsync(serverIp, serverPort);

                stream = client.GetStream();

                byte[] requestBytes = Encoding.UTF8.GetBytes(requestText);
                await stream.WriteAsync(requestBytes, 0, requestBytes.Length);
                await stream.FlushAsync();

                ClientLogger.Log("request sent, reading response...");

                StringBuilder builder = new StringBuilder();
                byte[] buffer = new byte[bufferSize];

                bool done = false;
                while (!done)
                {
                    int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);

                    if (bytesRead == 0)
                    {
                        done = true;
                    }
                    else
                    {
                        string chunk = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                        builder.Append(chunk);

                        if (protocol.IsCompleteMessage(builder.ToString()))
                        {
                            done = true;
                        }
                    }
                }
                
                responseText = builder.ToString();
                ClientLogger.Log("response received (" + responseText.Length + " chars)");
            }
            catch (Exception ex)
            {
                ClientLogger.Log("network error in SendRequestAsync: " + ex.Message);
                responseText = "";
            }
            finally
            {
                try
                {
                    if (stream != null)
                    {
                        stream.Close();
                    }
                }
                catch (Exception ex)
                {
                    ClientLogger.Log("stream close error: " + ex.Message);
                }

                try
                {
                    if (client != null)
                    {
                        client.Close();
                    }
                }
                catch (Exception ex)
                {
                    ClientLogger.Log("client close error: " + ex.Message);
                }
            }
            
            return responseText;
        }

        internal async Task SendQuitRequestAsync(string token)
        {
            string request = "";
            string response = "";
            
            try
            {
                ClientLogger.Log("Sending quit request to server for token: " + token);
                
                request = protocol.BuildQuitRequest(token);
                response = await SendRequestAsync(request);
                
                ClientLogger.Log("Quit request sent, response: " + response.Replace("\r\n", " | "));
            }
            catch (Exception ex)
            {
                ClientLogger.Log("Error sending quit request: " + ex.Message);
            }
            
            return;
        }
    }
}
