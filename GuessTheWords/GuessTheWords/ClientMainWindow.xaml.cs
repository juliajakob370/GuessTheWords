/*
 * FILE             : ClientMainWindow.xaml.cs
 * PROJECT          : A02-TCPIP > Client_GuessTheWords
 * PROGRAMMER       : Bibi Murwared, Julia Jakob
 * FIRST VERSION    : 2026-02-16
 * DESCRIPTION      : This file connects the main window UI to the client logic.
 */

using Client_GuessTheWords;
using System.Windows;

namespace GuessTheWords
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

        /// <summary>
        /// When the user clicks on the help button open the help / how to play box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void help_Click(object sender, RoutedEventArgs e)
        {
            HowToPlay helpBox = new HowToPlay();
            helpBox.Owner = this;
            helpBox.ShowDialog();
        }

        private void submitGuess_Click(object sender, RoutedEventArgs e)
        {

        }

    }
}