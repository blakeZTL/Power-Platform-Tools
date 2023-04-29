using Microsoft.Extensions.Configuration;
using Microsoft.PowerPlatform.Dataverse.Client;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Configuration;
using System.DirectoryServices.AccountManagement;
using System.Security;

namespace Deployment_Settings_File
{
    internal class MakeConnection
    {
        #region JSON Classes
        public class Environment
        {
            public string? Name { get; set; }
            public string? Url { get; set; }
        }
        #endregion

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
            Environment[]? environments = GetEnvironments();

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

            string? envUrl = PromptForEnvironment();

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
        public static Environment[] GetEnvironments()
        {
            // Get the environments from the appsettings.json file
            IConfiguration config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .Build();

            Environment[] environments = config.GetSection("Environments")
                .GetChildren()
                .Select(x => new Environment
                {
                    Name = x.GetValue<string>("name"),
                    Url = x.GetValue<string>("url")
                })
                .ToArray();

            return environments;
        }


        /// <summary>
        /// A method that gets the environment URL from the appsettings.json file.
        /// </summary>
        /// <param name="environmentName"></param>
        /// <returns>The url of the input environment name</returns>
        public static string GetEnvironmentUrl(string environmentName)
        {
            Environment[] environments = GetEnvironments();

            // Get the environment URL from the environment object where the name matches the input environment name
            string environmentUrl = environments.Where(x => x.Name == environmentName)
                                                .Select(x => x.Url)
                                                .FirstOrDefault()??
                                                throw new ArgumentException($"Environment '{environmentName}' not found in appsettings.json");

            return environmentUrl;
        }


        /// <summary>
        /// A method that saves the environment to the appsettings.json file.
        /// </summary>
        /// <param name="environmentName"></param>
        /// <param name="environmentUrl"></param>
        public static void SaveEnvironment(string environmentName, string environmentUrl)
        {
            // Get the configuration from the appsettings.json file
            IConfiguration config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            // Get the existing environments array
            Environment[] environments = GetEnvironments();

            // Check if an environment with the same name already exists
            if (environments.Any(x => x.Name == environmentName))
            {
                throw new ArgumentException($"Environment '{environmentName}' already exists in appsettings.json");
            }

            // Add the new environment to the array
            environments.Append(new Environment { Name = environmentName, Url = environmentUrl });

            // Save the updated configuration to the appsettings.json file
            var json = JsonConvert.SerializeObject(config.GetSection("ConnectionStrings"), Formatting.Indented)
                     + "\n" // Or use "\r\n" depending on the line ending used in the appsettings.json file
                     + JsonConvert.SerializeObject(new { Environments = environments }, Formatting.Indented);
            File.WriteAllText("appsettings.json", json);
        }


        /// <summary>
        /// A method that prompts the user for the environment
        /// and saves it to the appsettings.json file if not already there.
        /// </summary>
        /// <returns>The selected or input environment.</returns>
        public static string PromptForEnvironment()
        {
            // Get the environments from the appsettings.json file
            Environment[]? environments = GetEnvironments();
            Console.WriteLine("\n" + environments);

            // Make an array of the environment names with an index value for each starting at 1
            string[] environmentNames = environments.Select((x, i) => $"{i + 1}. {x.Name}").ToArray();            
            Console.WriteLine("\n" + environmentNames);

            string? environmentChoice;
            do
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Choose an environment by number or type new to enter your own:");
                Console.ForegroundColor = ConsoleColor.White;
                environmentChoice = Console.ReadLine();
            }
            while (environmentChoice != "new");

            
            // Prompt the user for the environment name
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("\nEnter environment name:");
            Console.ForegroundColor = ConsoleColor.White;
            string environmentName = Console.ReadLine();
            Console.ForegroundColor = ConsoleColor.Green;
            
            return environmentName;
        }
    }
}
