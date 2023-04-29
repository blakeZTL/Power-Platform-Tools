using Microsoft.Extensions.Configuration;
using Microsoft.PowerPlatform.Dataverse.Client;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics;
using System.DirectoryServices.AccountManagement;
using Deployment_Settings_FIle.Properties;
using System.Security;
using System.Resources;

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

        #region ServiceClient Methods
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

            url = PromptForEnvironment();

            /// < TODO > AppId and redirect url needs to be customized to your own credntials in appsettings.json </ TODO >
            // Create the service connection string using the info provided above.

            string? connectionString = Helpers.ConstructConnectionString(url);

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

        #endregion

        #region Environment Methods
        /// <summary>
        /// A method that gets the environments from the appsettings.json file.
        /// </summary>
        /// <returns>An array of saved environments.</returns>
        public static Environment[] GetEnvironments()
        {
            // Get the configuration from the appsettings.json file
            IConfiguration config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .Build();

            // Get the "Environments" section and its children
            IConfigurationSection environmentsSection = config.GetSection("Environments");
            IEnumerable<IConfigurationSection> environmentSections = environmentsSection.GetChildren();

            // Create an array to hold the mapped environments
            Environment[] environments = new Environment[environmentSections.Count()];

            // Map each IConfigurationSection to an Environment object
            int i = 0;
            foreach (IConfigurationSection environmentSection in environmentSections)
            {
                Environment environment = new()
                {
                    Name = environmentSection["name"],
                    Url = environmentSection["url"]
                };

                environments[i] = environment;
                i++;
            }

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
            string environmentUrl = environments.Where(x => x.Name == environmentName[3..])
                                                .Select(x => x.Url)
                                                .FirstOrDefault() ??
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
            // read the json from the appsettings.json file
            string dsfJson = File.ReadAllText("appsettings.json");
            JObject? settingsJson = JObject.Parse(dsfJson);
            Debug.WriteLine(settingsJson);

            JArray? envs = settingsJson["Environments"]?.ToObject<JArray>();

            // Check if the environment already exists in the appsettings.json file
            bool environmentExists = envs.Any(x => x["name"]?.ToString() == environmentName);
            if (environmentExists)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"\nEnvironment '{environmentName}' already saved.");
            }
            else
            {
                // Add the new environment to the appsettings.json file
                JObject? newEnv = new()
                {
                    { "name", environmentName },
                    { "url", environmentUrl }
                };
                Debug.WriteLine(newEnv);
                envs.Add(newEnv);
                Debug.WriteLine(envs);

                //update changed property
                settingsJson["Environments"] = envs;

                // Write the new json to the appsettings.json file
                string output = JsonConvert.SerializeObject(settingsJson, Formatting.Indented);
                Debug.WriteLine(output);
                File.WriteAllText("appsettings.json", output);
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine($"\nEnvironment '{environmentName}' saved.");                
            }
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

            // Make an array of the environment names with an index value for each starting at 1
            string[] environmentNames = environments.Select((x, i) => $"{i + 1}. {x.Name}").ToArray();
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("\nSaved Environments:");
            Console.ForegroundColor = ConsoleColor.Red;
            foreach (string name in environmentNames)
            {
                Console.WriteLine(name);
            }

            string? environmentChoice;
            do
            {
                environmentChoice = null;
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"\nChoose an environment by number or type new to enter your own: (new/1-{environmentNames.Length})");
                Console.ForegroundColor = ConsoleColor.White;
                environmentChoice = Console.ReadLine();
            }
            while (environmentChoice != "new" && //equal to a digit
                   (!int.TryParse(environmentChoice, out int result) || //equal to new
                   result < 1 || //less than 1
                   result > environmentNames.Length)); //greater than the number of environments

            string? environmentName;
            if (environmentChoice == "new")
            {
                // Prompt the user for the environment name
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("\nEnter environment name:");
                Console.ForegroundColor = ConsoleColor.White;
                environmentName = Console.ReadLine();

                // Prompt the user for the environment URL
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("\nEnter environment url (for example: https://{environmentName}.crm9.dynamics.com):");
                Console.ForegroundColor = ConsoleColor.White;
                string? environmentUrl = Console.ReadLine();

                SaveEnvironment(environmentName, environmentUrl);

                return environmentUrl;
            }
            else
            {
                // Get the environment name from the environmentNames array where the index matches the input environmentChoice
                environmentName = environmentNames[int.Parse(environmentChoice) - 1];
                // Get the environment URL from the appsettings.json file
                string? environmentUrl = GetEnvironmentUrl(environmentName);
                return environmentUrl;
            }
        }
        #endregion
    }
}
