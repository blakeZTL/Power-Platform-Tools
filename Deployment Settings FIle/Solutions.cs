using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using System.IO;
using System.IO.Compression;
using System.Text.RegularExpressions;

namespace Deployment_Settings_File
{
    public class Solutions
    {
        #region Export Methods
        /// <summary>
        /// Exports a solution with a specific unique name by connecting to dataverse with the given connection string.
        /// </summary>
        /// <param name="svc">The client to Dataverse.</param>
        /// <param name="solutionName">The unique solution name.</param>        
        /// <returns>A file path to the solution zip file unless it is unsuccessful.</returns>
        public static string ExportSolution(ServiceClient svc, string solutionName)
        {
            // Set the console title.
            Console.Title = "Export Solution";

            // Prompt the user to specify whether the solution is managed or unmanaged.
            bool managed;
            string managedString = "";
            do
            {
                Console.WriteLine("\nManaged or unmanaged? (m/u):");
                Console.ForegroundColor = ConsoleColor.White;
                string? managedStringInput = Console.ReadLine();
                if (managedStringInput != "m" && managedStringInput != "u")
                {
                    Console.WriteLine("Please enter m or u.");
                    continue;
                }
                else
                {
                    managedString = managedStringInput;
                }
            }
            while (managedString != "m" && managedString != "u");
            if (managedString == "m")
            {
                managed = true;
            }
            else
            {
                managed = false;
            }

            using (svc)
            {
                Console.ForegroundColor = ConsoleColor.Blue;

                // Export the solution file.
                Console.WriteLine("\nExporting and Saving file...");
                string solutionPath = $@"C:\Users\{Environment.UserName}\Downloads\{solutionName}_{managedString}.zip";

                // Export or package a solution
                ExportSolutionRequest exportSolutionRequest = new()
                {
                    Managed = managed,
                    SolutionName = solutionName
                };
                ExportSolutionResponse exportSolutionResponse = (ExportSolutionResponse)svc.Execute(exportSolutionRequest);
                byte[] exportXml = exportSolutionResponse.ExportSolutionFile;
                File.WriteAllBytes(solutionPath, exportXml);

                // Display success message and return the file path.
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"\nSolution exported to {solutionPath}.");
                Helpers.PauseForUser("Unpack Solution");

                return solutionPath;
            }
        }

        /// <summary>
        /// Unpacks a solution file and extracts its contents to a new directory.
        /// </summary>
        /// <param name="solutionPath">The path to the solution file.</param>
        public static void UnpackSolution(string solutionPath)
        {
            // Set the console window title
            Console.Title = "Unpacking Solution";

            // Set the console text color to blue and display a message
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine("\nUnpacking solution file");

            // Remove the .zip extension from the solution file path to get the directory name
            string unpackPath = solutionPath.Replace(".zip", "");

            // Set the console text color to white and display a message
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("\nDirectory will be {0}", unpackPath);

            // Set the console text color to green
            Console.ForegroundColor = ConsoleColor.Green;

            // Check if the destination directory already exists
            if (Directory.Exists(unpackPath))
            {
                // Set the console text color to dark magenta and display a message
                Console.ForegroundColor = ConsoleColor.DarkMagenta;
                Console.WriteLine("\nDirectory already exists. Deleting...");

                // Delete the destination directory and all its contents
                Directory.Delete(unpackPath, true);
            }

            // Set the console text color to blue and display a message
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine("\nCreating directory...");

            // Create a new directory with the same name as the old directory
            Directory.CreateDirectory(unpackPath);

            // Extract the contents of the solution file to the destination directory
            ZipFile.ExtractToDirectory(solutionPath, unpackPath);

            // Set the console text color to green and display a message
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("\nUnpacked files are ready.");

            // Pause the program and wait for user input to continue
            Helpers.PauseForUser("Generate Deployment Settings File");
        }
        #endregion

        #region Get Solution Information
        /// <summary>
        /// Prompts the user to enter the logical name of the solution they want to export, and checks that the name is valid (does not contain spaces).
        /// </summary>
        /// <returns>The logical name of the solution to export.</returns>
        public static string GetSolutionName()
        {
            string pattern;
            string solutionName = "";
            // Loop until the user enters a valid solution name (does not contain spaces)
            do
            {
                // Set the console text color to blue and display a message
                Console.ForegroundColor = ConsoleColor.Blue;
                Console.WriteLine("\n\nExporting Solution.");

                // Set the console text color to green and prompt the user to enter the solution name
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("\nPlease enter solution logical name (ex: EagleEye):");

                // Set the console text color to white and read the user's input
                Console.ForegroundColor = ConsoleColor.White;
                string? solutionNameInput = Console.ReadLine()?.Trim();

                // Set the console text color to green
                Console.ForegroundColor = ConsoleColor.Green;

                // Set the pattern for the solution name (cannot contain spaces)
                pattern = "\\s+";

                if (solutionNameInput != null)
                {
                    // Check if the solution name contains spaces. If it does, display an error message and continue the loop
                    if (Regex.IsMatch(solutionNameInput, pattern))
                    {
                        Console.WriteLine("Solution name cannot contain spaces."); 
                        continue;
                    }
                    else
                    {
                        solutionName = solutionNameInput;
                        break;
                    }
                }
                else
                {                    
                    continue;
                }
            } while (Regex.IsMatch(solutionName, pattern) && string.IsNullOrWhiteSpace(solutionName));

            // If the solution name is valid, return it
            return solutionName;
        }

        /// <summary>
        /// A method that prompts the user to input the solution file's full path including extension.
        /// </summary>
        /// <param name="solutionPath"></param>
        /// <param name="solutionExported"></param>
        public static void GetSolutionPath(out string solutionPath, out bool solutionExported)
        {
            string solutionPathInput;
            do
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write("\nPlease input the solution file's full path including extension.\n");
                Console.ForegroundColor = ConsoleColor.White;
                solutionPathInput = Console.ReadLine()?.Trim() ?? "";

                if (!string.IsNullOrWhiteSpace(solutionPathInput))
                {
                    if (!File.Exists(solutionPathInput))
                    {
                        Console.ForegroundColor = ConsoleColor.DarkMagenta;
                        Console.WriteLine("\nFile not found. Please try again.");
                        Console.ForegroundColor = ConsoleColor.Green;
                        solutionPathInput = "";
                    }
                    else
                    {
                        break;
                    }
                }
            }
            while (!string.IsNullOrWhiteSpace(solutionPathInput));

            solutionPath = solutionPathInput!;
            solutionExported = false;
        }

        #endregion

        /// <summary>
        /// Prompts the user to delete the generated solution files (.zip and unpacked directory) and deletes them if requested.
        /// </summary>
        /// <param name="solutionZipPath">The path to the solution zip file.</param>
        /// <param name="solutionUnpackPath">The path to the directory where the solution was unpacked.</param>
        public static void CleanUpSolutionFiles(string solutionZipPath, string solutionUnpackPath)
        {
            // Set the console window title
            Console.Title = "Clean up";

            string cleanUp = "";

            // Loop until the user enters a valid input (y or n)
            do
            {
                // Set the console text color to green and prompt the user
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("\nWould you like to remove the generated solution files (.zip and unpacked directory)? (y/n)");

                // Set the console text color to white and read the user's input
                Console.ForegroundColor = ConsoleColor.White;
                string? cleanUpInput = Console.ReadLine();

                // If the input is invalid, set the console text color to magenta and display an error message
                if (cleanUpInput != "y" && cleanUpInput != "n")
                {
                    Console.ForegroundColor = ConsoleColor.Magenta;
                    Console.WriteLine("Invalid input. Please try again");
                }
                else
                {
                    cleanUp = cleanUpInput;
                }
            }
            while (cleanUp != "y" && cleanUp != "n");

            // If the user chose to delete the files
            if (cleanUp == "y")
            {
                // Set the console text color to dark gray and display a message
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine($"Deleting file at {solutionZipPath}");

                // Delete the solution zip file
                File.Delete(solutionZipPath);

                // Set the console text color to dark red and display a message
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine("File deleted.");

                // Set the console text color to dark gray and display a message
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine($"Deleting directory at {solutionUnpackPath}");

                // Delete the directory where the solution was unpacked, along with all its contents
                Directory.Delete(solutionUnpackPath, true);

                // Set the console text color to dark red and display a message
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine("Directory deleted.");
            }
        }
    }
}