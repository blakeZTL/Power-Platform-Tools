﻿using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Data;
using System.Text.RegularExpressions;
using System.Xml;

namespace Deployment_Settings_File
{
    internal class FileGeneration
    {
        #region File Generation
        /// <summary>
        /// Creates a deployment settings file based on the solution provided.
        /// </summary>
        /// <param name="solutionPath"></param>
        /// <param name="solutionName"></param>
        /// <returns>The path to the created file</returns>
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

            #region Environment Variables
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
            #endregion


            // look for a file called customizations.xml
            string[] customizationsPath = Directory.GetFiles(solutionUnpackPath, "customizations.xml", SearchOption.AllDirectories);

            //if the file exists then read the contents
            if (customizationsPath.Length > 0)
            {
                #region Connection References
                Console.WriteLine("\nCustomizations file found.\nCompiling customizations...");
                // read the contents of the file
                string customizationsXml = File.ReadAllText(customizationsPath[0]);
                // create a new xml document
                XmlDocument customizationsDoc = new();
                // load the xml into the document
                customizationsDoc.LoadXml(customizationsXml);

                // get the connectionReferences node if it exists
                XmlNode? connectionReferencesNode = customizationsDoc.SelectSingleNode("//connectionreferences");

                JArray connectionReferences = new();
                deploymentSettingsJson.Add("ConnectionReferences", connectionReferences);

                if (connectionReferencesNode?.ChildNodes.Count > 0)
                {
                    // iterate through the connectionReferences
                    Console.ForegroundColor = ConsoleColor.White;
                    foreach (XmlNode connectionReference in connectionReferencesNode.ChildNodes)
                    {
                        JObject connectionRef = new();

                        XmlNode? logicalNameNode = connectionReference.SelectSingleNode("@connectionreferencelogicalname");

                        if (logicalNameNode != null)
                        {
                            string logicalName = logicalNameNode.InnerText;
                            connectionRef.Add("LogicalName", logicalName);
                        }

                        connectionRef.Add("ConnectionId", "");

                        XmlNode? connectorIdNode = connectionReference.SelectSingleNode("connectorid");

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
                    Console.WriteLine("\nNo connection references found.");
                    Console.ForegroundColor = ConsoleColor.Green;
                }
                #endregion
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


                #region Workflow Owner
                string? workflowOwner = "";
                string? setWorkflowOwner;
                do
                {
                    Console.WriteLine("\nWould you like to set your workflows to the same owner? (y/n)");
                    Console.ForegroundColor = ConsoleColor.White;
                    setWorkflowOwner = Console.ReadLine();
                }
                while (setWorkflowOwner != "y" && setWorkflowOwner != "n");

                if (setWorkflowOwner == "y")
                {
                    do
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("\nPlease enter the email address of the owner:");
                        Console.ForegroundColor = ConsoleColor.White;
                        string workflowOwnerInput = Console.ReadLine() ?? "";

                        string emailPattern = @"[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?";

                        if (!string.IsNullOrWhiteSpace(workflowOwnerInput) && Regex.IsMatch(workflowOwnerInput, emailPattern))
                        {
                            workflowOwner = workflowOwnerInput;
                        }
                        else
                        {
                            Console.ForegroundColor = ConsoleColor.Magenta;
                            Console.WriteLine("\nPlease enter a valid email address.");
                            Console.ForegroundColor = ConsoleColor.White;
                        }
                    }
                    while (string.IsNullOrWhiteSpace(workflowOwner));
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
            #endregion
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


        /// <summary>
        /// A method to get the path of the deployment settings file from the user.
        /// </summary>
        /// <returns>A path that has been verified to exist.</returns>
        public static string GetDeploymentSettingsFilePath()
        {
            string deploymentSettingsFileInput;
            string deploymentSettingsFile = "";
            do
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("\nPlease input the full path of your deployment settings file including extension.\n");
                Console.ForegroundColor = ConsoleColor.White;
                deploymentSettingsFileInput = Console.ReadLine()?.Trim() ?? "";

                if (!string.IsNullOrWhiteSpace(deploymentSettingsFileInput))
                {
                    if (!File.Exists(deploymentSettingsFileInput))
                    {
                        Console.ForegroundColor = ConsoleColor.DarkMagenta;
                        Console.WriteLine("\nFile not found. Please try again.");
                        continue;
                    }
                    else
                    {
                        deploymentSettingsFile = deploymentSettingsFileInput;
                        break;
                    }
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.DarkMagenta;
                    Console.WriteLine("\nInvalid input. Please try again.");
                }
            }
            while (string.IsNullOrWhiteSpace(deploymentSettingsFile));

            return deploymentSettingsFile;
        }
        #endregion

        #region Autofill Methods
        /// <summary>
        /// A method to autofill connection references in the deployment settings file
        /// </summary>
        /// <param name="svc"></param>
        /// <param name="deploymentSettingsPath"></param>
        public static void AutofillConnections(ServiceClient svc, string deploymentSettingsPath)
        {
            Console.Clear();
            Console.Title = "Autofill Connection References";
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine("\nAttempting to autofill connection references");

            using (svc)
            {
                #region Get Connections from Dataverse
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
                #endregion


                Console.ForegroundColor = ConsoleColor.Blue;
                Console.WriteLine("\nReading deployment settings file for comparison...");

                // read the json from the deploymentSettinsPath
                string dsfJson = File.ReadAllText(deploymentSettingsPath);

                JObject settingsJson = JObject.Parse(dsfJson);

                JArray? conRefs = settingsJson["ConnectionReferences"]?.ToObject<JArray>();

                if (conRefs != null)
                {
                    foreach (var conRef in conRefs)
                    {
                        string connectorId = (string)conRef["ConnectorId"]! ?? "";
                        //string connectorLogicalName = (string)conRef["LogicalName"];

                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("\nId: " + connectorId);

                        string connectorInternalId = connectorId[(connectorId.LastIndexOf('/') + 1)..];

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
                                string tcConnectorId = tc["ConnectorId"]?.Value<string>() ?? "";

                                int tcLastForwardSlash = tcConnectorId.LastIndexOf("/");
                                string tcConnectorWithoutId = tcLastForwardSlash == -1 ? tcConnectorId : tcConnectorId[(tcLastForwardSlash + 1)..];

                                int tcLastHyphen = tcConnectorWithoutId.LastIndexOf("-");
                                string tcConnectorClean = tcLastHyphen == -1 ? tcConnectorWithoutId : tcConnectorWithoutId[..tcLastHyphen];

                                return tcConnectorClean == connectorWithoutId;
                            });

                            #region Get Matching Record from Dataverse
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

                                    Entity? matchingRecord = connectors.Entities.FirstOrDefault(e =>
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

                                    string matchedId = matchingRecord?.GetAttributeValue<string>("connectionid") ?? "";

                                    Console.ForegroundColor = ConsoleColor.DarkCyan;
                                    matchedId = matchedId.Length < 1 || matchedId == null ? "None found" : matchedId;
                                    Console.WriteLine($"Match: {connectorId} with ConnectionId of {matchedId}");
                                    Console.ForegroundColor = ConsoleColor.DarkYellow;

                                    Console.WriteLine($"Adding ConnectionId: {matchedId}\n");
                                    targetConnection["ConnectionId"] = matchedId;
                                }
                            }
                            #endregion
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
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Magenta;
                    Console.WriteLine("\nNo ConnectionReferences found in deployment settings file.");
                    Console.ForegroundColor = ConsoleColor.Green;
                }
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("\nSaving changes to deployment settings file...");

                // Serialize the modified JSON back to the file
                settingsJson["ConnectionReferences"] = conRefs;
                string updatedJson = settingsJson.ToString();
                File.WriteAllText(deploymentSettingsPath, updatedJson);

                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("\nChanges saved to deployment settings file.");

                Helpers.PauseForUser("Autofill Environmental Variables");

                FileGeneration.AutofillEnvVars(svc, deploymentSettingsPath);
            }
        }

        /// <summary>
        /// A method to autofill the environment variables in the deployment settings file
        /// </summary>
        /// <param name="svc"></param>
        /// <param name="deploymentSettingsPath"></param>
        public static void AutofillEnvVars(ServiceClient svc, string deploymentSettingsPath)
        {

            Console.Clear();
            Console.Title = "Autofill Environment Variables";

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Getting variables from deployment settings file...");

            // read the json from the deploymentSettinsPath
            string dsfJson = File.ReadAllText(deploymentSettingsPath);
            JObject settingsJson = JObject.Parse(dsfJson);

            JArray envVars = settingsJson["EnvironmentVariables"]!.ToObject<JArray>() ?? new JArray();

            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("\nFound variables:");
            Console.ForegroundColor = ConsoleColor.Red;
            foreach (var envVar in envVars)
            {
                Console.WriteLine("\n" + envVar);
            }

            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine("\nReading environment variables from Dataverse...");

            QueryExpression getEnvVarValues = new("environmentvariablevalue")
            {
                ColumnSet = new ColumnSet("schemaname", "value")
            };
            getEnvVarValues.Criteria.AddCondition("schemaname", ConditionOperator.In, envVars.Select(e => e["SchemaName"]!.Value<string>()).ToArray());

            EntityCollection envVarValues = svc.RetrieveMultiple(getEnvVarValues);

            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("\nFound values:");
            Console.ForegroundColor = ConsoleColor.Red;
            foreach (var envVarValue in envVarValues.Entities)
            {
                Console.WriteLine("\n" + envVarValue);
            }
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine("Matching records...");

            foreach (var envVar in envVars)
            {
                string schemaName = envVar["SchemaName"]!.Value<string>() ?? "";
                Entity? matchingRecord = envVarValues.Entities.FirstOrDefault(e => e.GetAttributeValue<string>("schemaname") == schemaName);
                if (matchingRecord != null)
                {
                    Console.ForegroundColor = ConsoleColor.DarkCyan;
                    Console.WriteLine($"Match: {schemaName} with value of {matchingRecord.GetAttributeValue<string>("value")}");
                    Console.ForegroundColor = ConsoleColor.DarkYellow;
                    Console.WriteLine($"Adding value: {matchingRecord.GetAttributeValue<string>("value")}\n");
                    envVar["Value"] = matchingRecord.GetAttributeValue<string>("value");
                }
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("\nSaving changes to deployment settings file...");

            // Serialize the modified JSON back to the file
            settingsJson["EnvironmentVariables"] = envVars;
            string updatedJson = settingsJson.ToString();
            File.WriteAllText(deploymentSettingsPath, updatedJson);

            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("\nChanges saved to deployment settings file.");

            Helpers.PauseForUser("Finish and exit.");
        }
        #endregion
    }
}