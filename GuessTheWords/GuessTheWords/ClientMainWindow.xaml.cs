/*
 * FILE             : ClientMainWindow.xaml.cs
 * PROJECT          : A02-TCPIP > Client_GuessTheWords
 * PROGRAMMER       : Bibi Murwared, Julia Jakob
 * FIRST VERSION    : 2026-02-16
 * DESCRIPTION      : This file connects the main window UI to the client logic.
 */

using Client_GuessTheWords.Game;
using Client_GuessTheWords.Helper;
using Client_GuessTheWords.Network;
using GuessTheWords;
using System.Configuration;
using System.Windows;
using System;
using System.Collections.Generic;
using System.Windows.Controls;

namespace Client_GuessTheWords
{
    /// <summary>
    /// Interaction logic for ClientMainWindow.xaml
    /// </summary>
    public partial class ClientMainWindow : Window
    {
        //stores client game state token, time and puzzle
        private ClientStateBag state;

        //the protocol build and parse messages
        private GameProtocol protocol;

        //makes the connectionro server 
        private ClientConnection connection;

        // config values
        private string serverIp;
        private int serverPort;
        private int bufferSize;
        private int maxNameLength;

        //validation range constants
        private const int MIN_PORT = 1024;
        private const int MAX_PORT = 65535;
        private const int MIN_BUFFER = 64;
        private const int MAX_BUFFER = 65536;
        private const int MIN_NAME_LEN = 1;
        private const int MAX_NAME_LEN = 50;

        //protocol keys
        private const string STATUS_KEY = "STATUS";
        private const string TOKEN_KEY = "TOKEN";
        private const string PUZZLE_KEY = "PUZZLE";
        private const string TOTAL_WORDS_KEY = "TOTALWORDS";
        private const string TIME_LIMIT_KEY = "TIMELIMIT";
        private const string STATUS_OK = "OK";

        // Feedback Message Constants
        private const string EMPTY_GUESS_MESSAGE = "Please enter a guess!";

        public ClientMainWindow()
        {
            InitializeComponent();

            // create state bag
            state = new ClientStateBag();

            //load config when window starts
            LoadClientConfig();

            return;
        }

        // JULIA's FUNCTIONS
        /// <summary>
        /// When the user clicks on the help button open the help / how to play box
        /// </summary>
        /// <param name="sender">Control that triggered the event</param>
        /// <param name="e">Event arguments</param>
        private void Help_Click(object sender, RoutedEventArgs e)
        {
            HowToPlay helpBox = new HowToPlay();
            helpBox.Owner = this;
            helpBox.ShowDialog();
        }

        /// <summary>
        /// When the user submits a guess the Client should connect to the server and send the guess for validation
        /// </summary>
        /// <param name="sender">Control that triggered the event</param>
        /// <param name="e">Event arguments</param>
        private async void SubmitGuess_Click(object sender, RoutedEventArgs e)
        {
            bool empty = false;
            string guess = GuessTextBox.Text.Trim(); // get guess from user and trim off whitespace

            // make sure the user has actually entered a guess
            if (string.IsNullOrWhiteSpace(guess))
            {
                GuessFeedback.Foreground = System.Windows.Media.Brushes.Red; // change feedback color to red
                GuessFeedback.Text = EMPTY_GUESS_MESSAGE;
                empty = true;
            }

            else if (!empty)
            {
                // if the user entered a guess attempt to connect to server and send guess for validation
                try
                {
                    // build a request using the protocol, the token for the client (provided by the server and stored in the state) and the user's guess
                    string request = protocol.BuildGuessRequest(state.Token, guess);
                }
            }
            return;

                                              // use token from server and send guess to server for validation
                                              // get server results back
                                              // give user feedback based on result - update found word count / list box if the word has been found already

        }

        // BIBI's FUNCTIONS

        /// <summary>
        /// Loads config file and Validates it 
        /// </summary>
        private void LoadClientConfig()
        {
            try
            {
                //read values from appconfig
                string ipText = ConfigurationManager.AppSettings["ServerIP"];
                string portText = ConfigurationManager.AppSettings["ServerPort"];
                string versionText = ConfigurationManager.AppSettings["ProtocolVersion"];
                string endLineText = ConfigurationManager.AppSettings["EndLine"];
                string bufferText = ConfigurationManager.AppSettings["ReadBufferSize"];
                string maxNameText = ConfigurationManager.AppSettings["MaxNameLength"];

                //create validation object
                ClientValidation validator = new ClientValidation(MIN_PORT, MAX_PORT, MIN_BUFFER, MAX_BUFFER, MIN_NAME_LEN, MAX_NAME_LEN);

                //validate config values
                string configError = validator.ValidateAllConfig(ipText, portText, bufferText, maxNameText);

                if (!string.IsNullOrEmpty(configError))
                {
                    MessageBox.Show(configError);//any error happens show the message to client
                    ClientLogger.Log("config error: " + configError);
                    protocol = null;
                    connection = null;
                }
                else
                {
                    //parse and store correct values
                    serverIp = ipText.Trim();
                    serverPort = int.Parse(portText.Trim());
                    bufferSize = int.Parse(bufferText.Trim());
                    maxNameLength = int.Parse(maxNameText.Trim());

                    protocol = new GameProtocol(versionText.Trim(), endLineText.Trim());
                    connection = new ClientConnection(serverIp, serverPort, bufferSize, protocol);

                    ClientLogger.Log("config loaded successfully");
                }
            }
            catch (Exception ex)
            {
                ClientLogger.Log("config load error: " + ex.Message);
                MessageBox.Show("could not load config");
                protocol = null;
                connection = null;
            }

            return;
        }

        /// <summary>
        /// NAME: Start_Click
        /// PURPOSE: Handles the start button click event. Validates the player name and sends a start request to the server,
        /// receives the response, initializes the game state, and switches the UI to the Game page.
        /// </summary>
        /// <param name="sender"> control that triggered the event</param>
        /// <param name="e">event data</param>
        private async void Start_Click(object sender, RoutedEventArgs e)
        {

            bool success = true;
            string playerName = "";
            string nameError = "";
            string request = "";
            string responseText = "";
            string status = "";
            string token = "";
            string puzzle = "";
            int totalWords = 0;
            int timeLimit = 0;

            try
            {
                // check if config loaded properly
                if (protocol == null || connection == null)
                {
                    MessageBox.Show("client config is not loaded. check config file.");
                    ClientLogger.Log("start blocked: protocol/connection is null.");
                    success = false;
                }

                if (success)
                {
                    //create validation object using constants
                    ClientValidation validator = new ClientValidation(MIN_PORT, MAX_PORT,MIN_BUFFER, MAX_BUFFER,MIN_NAME_LEN, MAX_NAME_LEN);

                    //validate player name
                    nameError = validator.ValidatePlayerName(NameTextBox.Text, maxNameLength);

                    if (!string.IsNullOrEmpty(nameError))
                    {
                        NameErrorText.Text = nameError;
                        NameErrorText.Visibility = Visibility.Visible;
                        success = false;
                    }
                    else
                    {
                        NameErrorText.Visibility = Visibility.Hidden;
                    }
                }

                if (success)
                {
                    // clean player name
                    playerName = NameTextBox.Text.Trim();

                    //build start request
                    request = protocol!.BuildStartRequest(playerName);

                    //send request async
                    responseText = await connection!.SendRequestAsync(request);

                    if (string.IsNullOrWhiteSpace(responseText))
                    {
                        MessageBox.Show("Server is not available.");
                        ClientLogger.Log("Server returned empty response.");
                        success = false;
                    }
                }
                //if we get a reesponse
                if (success)
                {
                    //parse response
                    Dictionary<string, string> map = protocol.ParseResponse(responseText);

                    //read status
                    status = GetValue(map, STATUS_KEY);

                    if (!string.Equals(status, STATUS_OK, StringComparison.OrdinalIgnoreCase))
                    {
                        MessageBox.Show("Server rejected request.");
                        ClientLogger.Log("Server returned non-OK status.");
                        success = false;
                    }
                    else
                    {
                        // read values from response
                        token = GetValue(map, TOKEN_KEY);
                        puzzle = GetValue(map, PUZZLE_KEY);

                        int.TryParse(GetValue(map, TOTAL_WORDS_KEY), out totalWords);
                        int.TryParse(GetValue(map, TIME_LIMIT_KEY), out timeLimit);

                        // store in state bag
                        state.NewGame(playerName, token, puzzle, totalWords, timeLimit);

                        //update UI
                        StringSpace.Text = puzzle;

                        StartPage.Visibility = Visibility.Hidden;
                        GamePage.Visibility = Visibility.Visible;

                        ClientLogger.Log("Game started successfully.");
                    }
                }
            }
            catch (Exception ex)
            {
                ClientLogger.Log("Start_Click error: " + ex.Message);
                MessageBox.Show("Start failed.");
            }

            return;
        }

        /// <summary>
        /// NAME: GetValue
        /// PURPOSE: Safely retrieves a value from a dictionary using a key./ Prevents runtime errors if the key is not exist. 
        /// </summary>
        /// <param name="map">parsed response data</param>
        /// <param name="key">key to search for</param>
        /// <returns>value if found or empty string</returns>
        private string GetValue(Dictionary<string, string> map, string key)
        {
            string value = "";

            if (map != null && map.ContainsKey(key))
            {
                value = map[key];
            }

            return value;
        }

        private void Quit_Click(object sender, RoutedEventArgs e)
        {

        }

        private void PlayAgain_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}