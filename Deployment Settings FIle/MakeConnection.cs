using Microsoft.PowerPlatform.Dataverse.Client;
using Newtonsoft.Json.Linq;
using System.Configuration;
using System.DirectoryServices.AccountManagement;
using System.Security;

namespace Deployment_Settings_File
{
    internal class MakeConnection
    {
        /// <summary>
        /// Prompts the user for connection credentials and returns them as an array of strings.
        /// </summary>
        /// <param name="env">The environment URL to use for the connection.</param>
        /// <param name="oldClientId">The old client ID, if it exists and should be reused.</param>
        /// <param name="oldClientSecret">The old client secret, if it exists and should be reused.</param>
        /// <returns>An array of strings containing the connection string, client ID, and client secret.</returns>
        public static ServiceClient? ServicePrincipal()
        {
            // Set the console title to indicate that the connection is being prepared.
            Console.Title = "Preparing Connection";

            string? clientId;
            SecureString? clientSecretSec;
            string? url;
            string[]? environments = GetEnvironments();

            /// <TODO> Make these 3 values match regex and not be blank </TODO>

            //Prompt the user for the environment URL                
            Console.WriteLine("\nEnter environment url (for example: https://{environmentName}.crm9.dynamics.com):");
            Console.ForegroundColor = ConsoleColor.White;
            url = Console.ReadLine();
            Console.ForegroundColor = ConsoleColor.Green;



            // Prompt the user for the client ID              
            Console.WriteLine("\nEnter client id:");
            Console.ForegroundColor = ConsoleColor.White;
            clientId = Console.ReadLine();
            Console.ForegroundColor = ConsoleColor.Green;

            // Prompt the user for the client secret as a secure string
            Console.WriteLine("\nEnter client secret:");
            Console.ForegroundColor = ConsoleColor.White;
            clientSecretSec = Helpers.GetPassword();

            // Convert the secure string to a string
            string? clientSecret = new System.Net.NetworkCredential(string.Empty, clientSecretSec).Password;

            // Create the service connection string using the info provided above.
            string? connectionString = @$"
            AuthType=ClientSecret;
            Url={url};
            ClientId={clientId};
            ClientSecret={clientSecret}";

            ServiceClient svc;
            try
            {
                Console.ForegroundColor = ConsoleColor.Blue;
                Console.WriteLine("\nMaking connection...");
                svc = new(connectionString);

                if (svc.IsReady)
                {
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.WriteLine($"\nConnection Made to {url}");
                    // Remove the client secret from memory
                    clientSecretSec.Dispose();

                    return svc;
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Magenta;
                    Console.WriteLine("Connection Failed");
                    return null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return null;
            }
        }

        /// <summary>
        /// A method that that connects the user to Dataverse using the OAuth authentication method.
        /// </summary>
        /// <returns>An active client.</returns>
        public static ServiceClient OAuth()
        {
            Console.Title = "Preparing Connection";
            
            string? url;

            // Get current users email address
            string? email = UserPrincipal.Current.EmailAddress;

            //Prompt the user for the environment URL
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("\nEnter environment url (for example: https://{environmentName}.crm9.dynamics.com):");
            Console.ForegroundColor = ConsoleColor.White;
            url = Console.ReadLine();
            Console.ForegroundColor = ConsoleColor.Green;

            /// < TODO > AppId and redirect url needs to be customized to your own credntials in appsettings.json </ TODO >
            // Create the service connection string using the info provided above.
            string? connectionString = @$"
            AuthType=OAuth;
            Username={email};
            Integrated Security=True;
            Url={url};            
            AppId=c1f1b3d2-3fd9-4fd8-b3b0-71f8e3f1e687;
            RedirectUri=https://power-apis-usgov001-public.consent.azure-apihub.us/redirect;
            TokenCacheStorePath=c:\MyTokenCache\msal_cache.data;
            LoginPrompt=Auto
            ";


            ServiceClient svc;
            try
            {
                Console.ForegroundColor = ConsoleColor.Blue;
                Console.WriteLine("\nMaking connection...");

                svc = new(connectionString);

                if (svc.IsReady)
                {
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.WriteLine($"\nConnection Made to {url}");

                    return svc;
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Magenta;
                    Console.WriteLine("Connection Failed");
                    return null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return null;
            }
        }
        
        /// <summary>
        /// A method that gets the environments from the appsettings.json file.
        /// </summary>
        /// <returns>An array of saved environments.</returns>
        public static string[] GetEnvironments()
        {
            // Get the environments from the appsettings.json file
            string? environments = ConfigurationManager.AppSettings["Environments"];
            // Convert the environments to a string array
            string[] environmentsArray = environments.Split(',');
            return environmentsArray;
        }

        /// <summary>
        /// A method that gets the environment URL from the appsettings.json file.
        /// </summary>
        /// <param name="environmentName"></param>
        /// <returns>The url of the input environment name</returns>
        public static string GetEnvironmentUrl(string environmentName)
        {
            // Get the environments from the appsettings.json file
            string? environments = ConfigurationManager.AppSettings["Environments"];
            // Convert the environments to a string array
            string[] environmentsArray = environments.Split(',');
            // Get the index of the environment name
            int index = Array.IndexOf(environmentsArray, environmentName);
            // Get the environment URL from the appsettings.json file
            string? environmentUrls = ConfigurationManager.AppSettings["EnvironmentUrls"];
            // Convert the environment URLs to a string array
            string[] environmentUrlsArray = environmentUrls.Split(',');
            // Get the environment URL from the array
            string environmentUrl = environmentUrlsArray[index];
            return environmentUrl;
        }

        /// <summary>
        /// A method that saves the environment to the appsettings.json file.
        /// </summary>
        /// <param name="environmentName"></param>
        /// <param name="environmentUrl"></param>
        public static void SaveEnvironment(string environmentName, string environmentUrl)
        {
            // Get the environments from the appsettings.json file
            string? environments = ConfigurationManager.AppSettings["Environments"];
            // Convert the environments to a string array
            string[] environmentsArray = environments.Split(',');
            // Get the environment URL from the appsettings.json file
            string? environmentUrls = ConfigurationManager.AppSettings["EnvironmentUrls"];
            // Convert the environment URLs to a string array
            string[] environmentUrlsArray = environmentUrls.Split(',');
            // Add the new environment to the arrays
            environmentsArray.Append(environmentName);
            environmentUrlsArray.Append(environmentUrl);
            // Convert the arrays to strings
            string environmentsString = string.Join(",", environmentsArray);
            string environmentUrlsString = string.Join(",", environmentUrlsArray);
            // Save the new environments to the appsettings.json file
            ConfigurationManager.AppSettings["Environments"] = environmentsString;
            ConfigurationManager.AppSettings["EnvironmentUrls"] = environmentUrlsString;
        }
    }
}
