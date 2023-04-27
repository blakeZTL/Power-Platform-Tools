
using System.Security;
/// <summary>
/// Pauses the console application and displays a message to the user until the user hits the Enter key.
/// </summary>
/// <param name="message">The message to display to the user while waiting for input.</param>

namespace Deployment_Settings_File
{
    internal class Helpers
    {
        public static void PauseForUser(string message)
        {
            // Play a beep sound to alert the user that the program is waiting for input.
            Console.Beep();

            // Change the console text color to yellow and display the message prompting the user to press Enter.
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("\nPress the <Enter> key to continue. ({0})", message);

            // Wait for the user to press Enter and store the input.
            Console.ReadLine();

            // Clear the console screen and change the text color to green.
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Green;
        }

        public static SecureString GetPassword()
        {
            var pwd = new SecureString();
            while (true)
            {
                ConsoleKeyInfo i = Console.ReadKey(true);
                if (i.Key == ConsoleKey.Enter)
                {
                    break;
                }
                else if (i.Key == ConsoleKey.Backspace)
                {
                    if (pwd.Length > 0)
                    {
                        pwd.RemoveAt(pwd.Length - 1);
                        Console.Write("\b \b");
                    }
                }
                else if (i.KeyChar != '\u0000') // KeyChar == '\u0000' if the key pressed does not correspond to a printable character, e.g. F1, Pause-Break, etc
                {
                    pwd.AppendChar(i.KeyChar);
                    Console.Write("*");
                }
            }
            return pwd;
        }
    }
}