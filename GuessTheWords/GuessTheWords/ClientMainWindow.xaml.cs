/*
 * FILE             : ClientMainWindow.xaml.cs
 * PROJECT          : A02-TCPIP > Client_GuessTheWords
 * PROGRAMMER       : Bibi Murwared, Julia Jakob
 * FIRST VERSION    : 2026-02-16
 * DESCRIPTION      : This file connects the main window UI to the client logic.
 */

using GuessTheWords;
using System.Windows;

namespace Client_GuessTheWords
{
    /// <summary>
    /// Interaction logic for ClientMainWindow.xaml
    /// </summary>
    public partial class ClientMainWindow : Window
    {
        public ClientMainWindow()
        {
            InitializeComponent();
        }

        // JULIA's FUNCTIONS!~~~~~~~~~~~ I will write these later i dont have enough brain power rn -----------------------
        /// <summary>
        /// When the user clicks on the help button open the help / how to play box
        /// </summary>
        /// <param name="sender">Control that triggered the event</param>
        /// <param name="e">Event arguments</param>
        private void help_Click(object sender, RoutedEventArgs e)
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
        private void submitGuess_Click(object sender, RoutedEventArgs e)
        {
            // this is where the guess connection to server will go
            string guess = GuessTextBox.Text; // get guess from user
                                              // use token from server and send guess to server for validation
                                              // get server results back
                                              // give user feedback based on result - update found word count / list box if the word has been found already

        }
        // BIBI's FUNCTIONS
        private void Start_Click(object sender, RoutedEventArgs e)
        {
            // UI only for now: switch pages
            StartPage.Visibility = Visibility.Hidden;
            GamePage.Visibility = Visibility.Visible;
            return;
        }
        // BIBI's FUNCTIONS
        private void Start_Click(object sender, RoutedEventArgs e)
        {
            // UI only for now: switch pages
            StartPage.Visibility = Visibility.Hidden;
            GamePage.Visibility = Visibility.Visible;
            return;
        }

    }
}