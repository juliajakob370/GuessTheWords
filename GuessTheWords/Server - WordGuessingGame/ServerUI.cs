/*
 * FILE             : ServerUI.cs
 * PROJECT          : GuessTheWords-A02 > Server
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

        /// <summary>
        /// Updates Server UI display of active players count
        /// </summary>
        /// <param name="count">count of active players</param>
        public void SetActivePlayers(int count)
        {
            Console.SetCursorPosition(0, Console.CursorTop); 
            Console.Write(new string(' ', 64)); 
            Console.SetCursorPosition(0, Console.CursorTop); 
            Console.Write("Active Players: " + count); 
            Console.Title = "Word Guessing Game Server - Active Players: " + count;
        }

    }


}
}
