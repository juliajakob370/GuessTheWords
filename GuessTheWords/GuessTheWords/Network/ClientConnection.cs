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

namespace Client_GuessTheWords.Game
{
    internal class ClientConnection
    {
        private readonly string serverIp;//server ip from config
        private readonly int serverPort;//server port from config
        private readonly int bufferSize;//how big the read buffer is

        private readonly GameProtocol protocol;// protocol object builds and check message

        //recieevs everything from ourside amd saves them here 
        internal ClientConnection(string serverIpValue, int serverPortValue, int bufferSizeValue, GameProtocol protocolValue)
        {
            serverIp = serverIpValue;
            serverPort = serverPortValue;
            bufferSize = bufferSizeValue;
            protocol = protocolValue;
            return;
        }

        internal async Task<string> SendRequestAsync(string requestText)
        {
            string responseText = "";

            TcpClient client = null;
            NetworkStream stream = null;

            try
            {
                //creat new tcp client
                client = new TcpClient();

                ClientLogger.Log("connecting to server " + serverIp + ":" + serverPort);
                await client.ConnectAsync(serverIp, serverPort); // connect to servre

                stream = client.GetStream();//get stream after connection

                byte[] requestBytes = Encoding.UTF8.GetBytes(requestText); //convert the request message
                await stream.WriteAsync(requestBytes, 0, requestBytes.Length); //send it to servre
                await stream.FlushAsync(); //make sure everything is sended

                ClientLogger.Log("request sent, reading response...");

                StringBuilder builder = new StringBuilder();
                byte[] buffer = new byte[bufferSize];

                bool done = false;
                //read untill full message recived
                while (!done)
                {
                    int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);//read it from server

                    if (bytesRead == 0)
                    {
                        done = true; //server closed :(
                    }
                    else
                    {
                        string chunk = Encoding.UTF8.GetString(buffer, 0, bytesRead);//convert recieved bytes 

                        builder.Append(chunk); //add chucnk to builder

                        // check if message contains END line
                        if (protocol.IsCompleteMessage(builder.ToString()))
                        {
                            done = true;
                        }
                    }
                }
                // final response after everything 
                responseText = builder.ToString();
                ClientLogger.Log("response received (" + responseText.Length + " chars)");
            }
            catch (Exception ex)
            {
                // log any network error
                ClientLogger.Log("network error in SendRequestAsync: " + ex.Message);
                responseText = "";
            }
            finally
            {
                try
                {
                    //close stream safely
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
                    //close the client
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
            // return what we took from server
            return responseText;
        }
    }
}
