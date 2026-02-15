/*
 * FILE             : HowToPlay.xaml.cs
 * PROJECT          : GuessTheWords-A02 > Client
 * PROGRAMMER       : Bibi Murwared
 * FIRST VERSION    : 2026-02-15
 * DESCRIPTION      : Code behind for the How To Play help window in the client application.
 */
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace GuessTheWords

{
    /// <summary>
    /// Interaction logic for HowToPlay.xaml, Displays instructions on how to play the game
    /// </summary>
    public partial class HowToPlay : Window
    {
        /// <summary>
        /// Initializes a new instance of the HowToPlay help window
        /// </summary>
        public HowToPlay()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Handles the Close button click and closes the help window.
        /// </summary>
        /// <param name="sender">Control that triggered the event.</param>
        /// <param name="e">Event arguments for the click event.</param>
        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
            return;

        }
    }
}
