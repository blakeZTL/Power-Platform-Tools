using System.Diagnostics;
using System.DirectoryServices.AccountManagement;
using System.Security;

namespace Deployment_Settings_File
{
    internal class Helpers
    {
        /// <summary>
        /// Pauses the console application and displays a message to the user until the user hits the Enter key.
        /// </summary>
        /// <param name="message">The message to display to the user while waiting for input.</param>
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

        /// <summary>
        /// Prompts the user for connection credentials.
        /// </summary>
        /// <returns>The input password as a secure string.</returns>
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

        /// <summary>
        /// A helper method to get the current user's username in the format of an email address.
        /// </summary>
        /// <returns>An email address sting</returns>
        public static string GetUserName()
        {
            
#pragma warning disable CA1416 // Validate platform compatibility
            string email = UserPrincipal.Current.EmailAddress; 
#pragma warning restore CA1416 // Validate platform compatibility
            
            string userName;
            if (!string.IsNullOrWhiteSpace(email))
            {
                userName = email!;

                return userName;
            }
            else
            {
                do
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("\nPlease enter your email address:");
                    Console.ForegroundColor = ConsoleColor.White;
                    email = Console.ReadLine()?.Trim() ?? "";

                    if (string.IsNullOrWhiteSpace(email))
                    {
                        Console.ForegroundColor = ConsoleColor.DarkMagenta;
                        Console.WriteLine("\nInvalid input. Please try again.");
                        continue;
                    }
                    else
                    {
                        userName = email!;
                        return userName;
                    }
                }
                while (true);
            }
        }

        /// <summary>
        /// A method to constuct a connection string from a URL for the Dataverse environment.
        /// </summary>
        /// <param name="url"></param>
        /// <returns>Populated connection string</returns>
        public static string ConstructConnectionString(string url)
        {
            //string baseString = System.Configuration.ConfigurationManager
            //    .ConnectionStrings["default"].ToString();
            //Debug.WriteLine(baseString);
            //string connectionString = baseString.Replace("{{url}}", url)
            //    .Replace("{{userName}}", GetUserName());
            string connectionString = $@"AuthType=OAuth;
            Url={url};
            Username={GetUserName()};
            Integrated Security=True;
            AppId=c1f1b3d2-3fd9-4fd8-b3b0-71f8e3f1e687;
            RedirectUri=https://power-apis-usgov001-public.consent.azure-apihub.us/redirect;
            TokenCacheStorePath=c:\\MyTokenCache\\msal_cache.data;
            LoginPrompt=Auto";

            Debug.WriteLine(connectionString);

            return connectionString;

        }
    }
}