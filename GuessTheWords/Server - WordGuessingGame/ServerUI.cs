/*
 * FILE             : ServerUI.cs
 * PROJECT          : A02 - TCPIP > Server - GuessTheWords
 * PROGRAMMER       : Julia Jakob
 * FIRST VERSION    : 2026-02-13
 * DESCRIPTION      : This file contains helper methods for all server console output shown to the user
 */
namespace Server___WordGuessingGame
{
    /*
     * NAME    : ServerUI
     * PURPOSE : This class handles console output for the server
     */
    internal class ServerUI
    {
        /// <summary>
        /// Displays the string passed to it in the console.
        /// </summary>
        /// <param name="displayString">The string to display to the user</param>
        public void Display(string displayString)
        {
            Console.WriteLine(displayString);
            return;
        }

        /// <summary>
        /// Waits for the user to press any key before continuing execution
        /// </summary>
        public void PressAnyKey()
        {
            Console.ReadKey();
            return;
        }
    }
}
