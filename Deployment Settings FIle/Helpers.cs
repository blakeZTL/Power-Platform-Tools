using Microsoft.PowerPlatform.Dataverse.Client;
using System.DirectoryServices.AccountManagement;
using System.Net.NetworkInformation;
using System.Security;
using System.Security.Policy;

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

        //public static string[] GetEnvironments()
        //{
        //    // Get current users email address
        //    string? email = UserPrincipal.Current.EmailAddress;

        //    /// <TODO> AppId and redirect url needs to be customized to your own credntials </TODO>
        //    // Create the service connection string using the info provided above.
        //    string? connectionString = @$"
        //    AuthType=OAuth;
        //    Username={email};
        //    Integrated Security=True;                        
        //    AppId=c1f1b3d2-3fd9-4fd8-b3b0-71f8e3f1e687;
        //    RedirectUri=https://power-apis-usgov001-public.consent.azure-apihub.us/redirect;
        //    TokenCacheStorePath=c:\MyTokenCache\msal_cache.data;
        //    LoginPrompt=Auto
        //    ";

        //    ServiceClient svc = new(connectionString);

        //    if (svc.IsReady)
        //    {
        //        Console.ForegroundColor = ConsoleColor.DarkGray;
        //        Console.WriteLine($"\nConnection Made to {svc.ConnectedOrgFriendlyName}");
        //        var environments = svc.ExecuteOrganizationRequest();
        //        string[] environmentNames = new string[environments.Count];
        //        int i = 0;
        //        foreach (var environment in environments)
        //        {
        //            environmentNames[i] = environment.;
        //            i++;
        //        }
        //        return environmentNames;
        //    }
        //    else
        //    {
        //        Console.ForegroundColor = ConsoleColor.Magenta;
        //        Console.WriteLine("Connection Failed");
        //        return null;
        //    }
        //}
    }
}