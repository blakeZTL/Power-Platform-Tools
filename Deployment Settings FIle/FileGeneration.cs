using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Xml;

namespace Deployment_Settings_File
{
    internal class FileGeneration
    {
        public static string CreateDeploymentSettings(string solutionPath, string solutionName)
        {
            Console.Title = "Create Deployment Settings";

            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine("\nAnalyzing solution...");
            Console.ForegroundColor = ConsoleColor.Green;

            string deploymentSettingsPath = $@"C:\Users\{Environment.UserName}\Downloads\{solutionName}_deploymentSettings.json";

            string solutionUnpackPath = solutionPath.Replace(".zip", "");

            // create a new json object
            JObject deploymentSettingsJson = new();

            // search for a folder called environmentvariabledefinitions
            string[] envVarDefPath = Directory.GetDirectories(solutionUnpackPath, "environmentvariabledefinitions", SearchOption.AllDirectories);

            // if the path exists then iterate through the xml files in all the sub folders
            if (envVarDefPath.Length > 0)
            {
                Console.WriteLine("\nEnvironment variable definitions found.\nCompiling definitions...");
                // get the folder names in the environmentvariabledefinitions folder
                string[] envVarDefFolders = Directory.GetDirectories(envVarDefPath[0]);

                // add the EnvironmentVariables property which of type array
                JArray environmentVariables = new();
                deploymentSettingsJson.Add("EnvironmentVariables", environmentVariables);

                // for each of the folders in the environmentvariabledefinitions folder, add the name of the folder as the value of a property SchemaName
                Console.ForegroundColor = ConsoleColor.White;
                foreach (string envVarDefFolder in envVarDefFolders)
                {
                    JObject envVarDef = new();

                    // get the name of the folder
                    string envVarDefFolderName = Path.GetFileName(envVarDefFolder);

                    // add the SchemaName property with the name of the folder as the value

                    envVarDef.Add("SchemaName", envVarDefFolderName);
                    envVarDef.Add("Value", "");

                    environmentVariables.Add(envVarDef);

                    Console.WriteLine($"\nAdded environmental variable:");
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.WriteLine(envVarDef);
                    Console.ForegroundColor = ConsoleColor.White;
                }
                Console.ForegroundColor = ConsoleColor.Green;
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.DarkMagenta;
                Console.WriteLine("\nNo environment variable definitions found.");
                Console.ForegroundColor = ConsoleColor.Green;
            }

            // look for a file called customizations.xml
            string[] customizationsPath = Directory.GetFiles(solutionUnpackPath, "customizations.xml", SearchOption.AllDirectories);

            //if the file exists then read the contents
            if (customizationsPath.Length > 0)
            {
                Console.WriteLine("\nCustomizations file found.\nCompiling customizations...");
                // read the contents of the file
                string customizationsXml = File.ReadAllText(customizationsPath[0]);
                // create a new xml document
                XmlDocument customizationsDoc = new();
                // load the xml into the document
                customizationsDoc.LoadXml(customizationsXml);

                // get the connectionReferences node if it exists
                XmlNode connectionReferencesNode = customizationsDoc.SelectSingleNode("//connectionreferences");

                JArray connectionReferences = new();
                deploymentSettingsJson.Add("ConnectionReferences", connectionReferences);

                // iterate through the connectionReferences
                Console.ForegroundColor = ConsoleColor.White;
                foreach (XmlNode connectionReference in connectionReferencesNode.ChildNodes)
                {
                    JObject connectionRef = new();

                    XmlNode logicalNameNode = connectionReference.SelectSingleNode("@connectionreferencelogicalname");

                    if (logicalNameNode != null)
                    {
                        string logicalName = logicalNameNode.InnerText;
                        connectionRef.Add("LogicalName", logicalName);
                    }

                    connectionRef.Add("ConnectionId", "");

                    XmlNode connectorIdNode = connectionReference.SelectSingleNode("connectorid");

                    if (connectorIdNode != null)
                    {
                        string connectorId = connectorIdNode.InnerText;
                        connectionRef.Add("ConnectorId", connectorId);
                    }

                    connectionReferences.Add(connectionRef);
                    Console.WriteLine($"\nAdded connection reference:");
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.WriteLine(connectionRef);
                    Console.ForegroundColor = ConsoleColor.White;
                }
                Console.ForegroundColor = ConsoleColor.Green;
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.DarkMagenta;
                Console.WriteLine("\nNo customizations file found.");
                Console.ForegroundColor = ConsoleColor.Green;
            }

            // look for folder called Workflows            
            string[] workflowsPath = Directory.GetDirectories(solutionUnpackPath, "Workflows", SearchOption.AllDirectories);

            if (workflowsPath.Length > 0)
            {
                Console.WriteLine($"\nWorkflows found.\nCompiling workflows...");

                JArray workflows = new();
                deploymentSettingsJson.Add("SolutionComponentOwnershipConfiguration", workflows);

                // for each file in the Workflows folder, get the file names
                string[] workflowFiles = Directory.GetFiles($@"{solutionUnpackPath}\Workflows", "*.json");

                string? workflowOwner = "";
                string? setWorkflowOwner;
                do
                {
                    Console.WriteLine("\nWould you like to set your workflows to the same owner? (y/n");
                    Console.ForegroundColor = ConsoleColor.White;
                    setWorkflowOwner = Console.ReadLine();
                }
                while (setWorkflowOwner != "y" && setWorkflowOwner != "n");

                if (setWorkflowOwner == "y")
                {
                    Console.WriteLine("\nPlease enter the email address of the owner:");
                    Console.ForegroundColor = ConsoleColor.White;
                    workflowOwner = Console.ReadLine();
                }
                
                foreach (string workflow in workflowFiles)
                {
                    JObject workflowRef = new();

                    // get file name of workflow
                    string fileName = Path.GetFileName(workflow).Replace(".json", "");

                    int startIndex = Math.Max(0, fileName.Length - 36);
                    string workflowId = fileName[startIndex..];

                    workflowRef.Add("solutionComponentType", 29);
                    workflowRef.Add("solutionComponentUniqueName", workflowId);
                    workflowRef.Add("ownerEmail", workflowOwner);

                    workflows.Add(workflowRef);
                    Console.WriteLine($"\nAdded workflow:");
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.WriteLine(workflowRef);
                    Console.ForegroundColor = ConsoleColor.White;
                }
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.DarkMagenta;
                Console.WriteLine("\nNo workflows found.");
                Console.ForegroundColor = ConsoleColor.Green;
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("\nCreating deployment settings file...");

            string jsonString = JsonConvert.SerializeObject(deploymentSettingsJson, Newtonsoft.Json.Formatting.Indented);

            File.WriteAllText(deploymentSettingsPath, jsonString);

            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine($"\nFile created successfully at:\n{deploymentSettingsPath}");
            Console.ForegroundColor = ConsoleColor.Green;

            Helpers.PauseForUser("Autofill");

            return deploymentSettingsPath;
        }

        public static void AutofilllSettingsFile(ServiceClient svc, string deploymentSettingsPath)
        {
            Console.Clear();
            Console.Title = "Autofill Connection References";
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine("\nAttempting to autofill connection references");

            using (svc)
            {
                // query the dataverse connector table
                QueryExpression connectorsQuery = new("connectionreference");
                connectorsQuery.ColumnSet.AllColumns = true;

                RetrieveMultipleRequest getConnectors = new()
                {
                    Query = connectorsQuery
                };

                string[] foundConnections = Array.Empty<string>();

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("\nQuerying Dataverse for connectors...");
                EntityCollection connectors = svc.RetrieveMultiple(connectorsQuery);

                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine($"\n{connectors.Entities.Count} records found.");

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("\nParsing connector names...");

                foreach (Entity connector in connectors.Entities)
                {
                    string connectorNameWithId = connector.GetAttributeValue<string>("connectorid");
                    string connectorNameNoId;

                    int lastDash = connectorNameWithId.LastIndexOf("-");
                    if (lastDash != -1)
                    {
                        connectorNameNoId = connectorNameWithId[..connectorNameWithId.LastIndexOf("-")];
                    }
                    else
                    {
                        connectorNameNoId = connectorNameWithId;
                    }

                    int lastForwardSlash = connectorNameNoId.LastIndexOf("/");
                    string connectorName;

                    if (lastForwardSlash != -1)
                    {
                        connectorName = connectorNameNoId[(lastForwardSlash + 1)..];
                    }
                    else
                    {
                        connectorName = connectorNameNoId;
                    }

                    Array.Resize(ref foundConnections, foundConnections.Length + 1);
                    foundConnections[^1] = connectorName;
                }

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("\nFound connections:");
                Console.ForegroundColor = ConsoleColor.DarkGray;
                foreach (string foundCon in foundConnections)
                {
                    Console.WriteLine("\n" + foundCon);
                }

                Console.ForegroundColor = ConsoleColor.Blue;
                Console.WriteLine("\nReading deployment settings file for comparison...");

                // read the json from the deploymentSettinsPath
                string dsfJson = File.ReadAllText(deploymentSettingsPath);
                JObject settingsJson = JObject.Parse(dsfJson);

                JArray conRefs = settingsJson["ConnectionReferences"].ToObject<JArray>();

                foreach (var conRef in conRefs)
                {
                    string connectorId = (string)conRef["ConnectorId"];
                    //string connectorLogicalName = (string)conRef["LogicalName"];

                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("\nId: " + connectorId);

                    string connectorInternalId = connectorId.Substring(connectorId.LastIndexOf('/') + 1);

                    int lastIndexOfHyphen = connectorInternalId.LastIndexOf("-");
                    string connectorWithoutId;

                    if (lastIndexOfHyphen != -1)
                    {
                        connectorWithoutId = connectorInternalId[..connectorInternalId.LastIndexOf("-")];
                    }
                    else
                    {
                        connectorWithoutId = connectorInternalId;
                    }
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine("\nLooking for: " + connectorWithoutId);

                    if (foundConnections.Contains(connectorWithoutId))
                    {
                        IEnumerable<JToken> targetConnections = conRefs.Where(tc =>
                        {
                            string tcConnectorId = tc["ConnectorId"]?.Value<string>();

                            int tcLastForwardSlash = tcConnectorId.LastIndexOf("/");
                            string tcConnectorWithoutId = tcLastForwardSlash == -1 ? tcConnectorId : tcConnectorId[(tcLastForwardSlash + 1)..];

                            int tcLastHyphen = tcConnectorWithoutId.LastIndexOf("-");
                            string tcConnectorClean = tcLastHyphen == -1 ? tcConnectorWithoutId : tcConnectorWithoutId[..tcLastHyphen];

                            return tcConnectorClean == connectorWithoutId;
                        });

                        if (!targetConnections.Any())
                        {
                            break;
                        }

                        foreach (JToken targetConnection in targetConnections)
                        {
                            Console.ForegroundColor = ConsoleColor.DarkGreen;
                            Console.WriteLine("Found record:");
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine(targetConnection);

                            if (targetConnection != null)
                            {
                                Console.ForegroundColor = ConsoleColor.Gray;
                                Console.Write($"Getting matching record from Dataverse...\n");                              

                                Entity matchingRecord = connectors.Entities.FirstOrDefault(e =>
                                {
                                    string mrConnectionId = e.GetAttributeValue<string>("connectionid");

                                    // skip any records where mrConnectorId is null
                                    if (mrConnectionId != null && !mrConnectionId.Contains('-'))
                                    {
                                        string mrConnectorId = e.GetAttributeValue<string>("connectorid");

                                        Console.ForegroundColor = ConsoleColor.DarkGreen;
                                        Console.WriteLine($"Cleaning ConnectorId {mrConnectorId} for comparassions to {connectorWithoutId}");
                                        Console.ForegroundColor = ConsoleColor.DarkGray;

                                        int mrLastHyphen = mrConnectorId.LastIndexOf("-");
                                        string mrConnectorWithoutId = mrLastHyphen == -1 ? mrConnectorId : mrConnectorId[..mrLastHyphen];

                                        int mrLastForwardSlash = mrConnectorWithoutId.LastIndexOf("/");
                                        string mrConnectorClean = mrLastForwardSlash == -1 ? mrConnectorWithoutId : mrConnectorWithoutId[(mrLastForwardSlash + 1)..];

                                        Console.WriteLine($"Comparing {mrConnectorClean} and {connectorWithoutId}");

                                        bool match = mrConnectorClean == connectorWithoutId;

                                        if (match)
                                        {
                                            Console.ForegroundColor = ConsoleColor.Magenta;
                                        }

                                        Console.WriteLine(match ? "Positive match!" : "No match.");

                                        return match;
                                    }
                                    return false;
                                });

                                string matchedId = matchingRecord.GetAttributeValue<string>("connectionid");

                                Console.ForegroundColor = ConsoleColor.DarkCyan;
                                matchedId = matchedId.Length < 1 || matchedId == null ? "None found" : matchedId;
                                Console.WriteLine($"Match: {connectorId} with ConnectionId of {matchedId}");
                                Console.ForegroundColor = ConsoleColor.DarkYellow;

                                Console.WriteLine($"Adding ConnectionId: {matchedId}\n");
                                targetConnection["ConnectionId"] = matchedId;
                            }
                        }
                    }
                    else
                    {
                        {
                            Console.ForegroundColor = ConsoleColor.DarkMagenta;
                            Console.WriteLine("\nNo match found.");
                            Console.ForegroundColor = ConsoleColor.Green;
                        }
                    }
                }
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("\nSaving changes to deployment settings file...");

                // Serialize the modified JSON back to the file
                settingsJson["ConnectionReferences"] = conRefs;
                string updatedJson = settingsJson.ToString();
                File.WriteAllText(deploymentSettingsPath, updatedJson);

                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("\nChanges saved to deployment settings file.");

                Helpers.PauseForUser("Finish and close.");
            }
        }
    }
}

