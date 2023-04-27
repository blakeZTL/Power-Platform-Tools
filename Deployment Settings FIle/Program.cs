using Microsoft.PowerPlatform.Dataverse.Client;

namespace Deployment_Settings_File
{
    class Program
    {
        static void Main()
        {
            Console.Title = "Main";
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("A BURST tool for creating deployment settings files for Dataverse solutions.\n");
            Console.ForegroundColor = ConsoleColor.Green;

            string? solutionName = null;
            string? needToExport;

            do
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("\nDo you need to export a solution? (y/n)");
                Console.ForegroundColor = ConsoleColor.White;
                needToExport = Console.ReadLine();
                Console.ForegroundColor = ConsoleColor.Green;

                if (needToExport != "y" && needToExport != "n")
                {
                    Console.ForegroundColor = ConsoleColor.DarkMagenta;
                    Console.WriteLine("\nInvalid input. Please try again.");
                    Console.ForegroundColor = ConsoleColor.Green;
                }
            }
            while (needToExport != "y" && needToExport != "n");

            string? solutionPath;
            bool solutionExported;

            if (needToExport == "y")
            {
                string? connectionType;
                do
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("\nConnect via your credentials or a service principal? (c/s)");
                    Console.ForegroundColor = ConsoleColor.White;
                    connectionType = Console.ReadLine();

                    if (connectionType != "c" && connectionType != "s")
                    {
                        Console.ForegroundColor = ConsoleColor.DarkMagenta;
                        Console.WriteLine("\nInvalid input. Please try again.");
                    }
                }
                while (connectionType != "c" && connectionType != "s");

                ServiceClient? svc;
                if (connectionType == "c") 
                { 
                    svc = MakeConnection.OAuth();
                }
                else
                {
                    svc = MakeConnection.ServicePrincipal();
                }                    

                solutionName = Solutions.GetSolutionName();

                solutionPath = Solutions.ExportSolution(svc, solutionName);

                Solutions.UnpackSolution(solutionPath);

                solutionExported = true;
            }
            else
            {
                Console.Write("\nPlease input the solution file's full path including extension.\n");
                Console.ForegroundColor = ConsoleColor.White;
                solutionPath = Console.ReadLine();

                solutionExported = false;
            }

            string? needSettingsFile;
            string? deploymentSettingsFile = null;
            do
            {
                Console.Title = "Deployment Settings";
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("\nDo you need to create a deployment settings file? (y/n)\n");
                Console.ForegroundColor = ConsoleColor.White;
                needSettingsFile = Console.ReadLine();
                Console.ForegroundColor = ConsoleColor.Green;

                if (needSettingsFile != "y" && needSettingsFile != "n")
                {
                    Console.ForegroundColor = ConsoleColor.DarkMagenta;
                    Console.WriteLine("\nInvalid input. Please try again.");
                    Console.ForegroundColor = ConsoleColor.Green;
                }
            }
            while (needSettingsFile != "y" && needSettingsFile != "n");

            if (needSettingsFile == "y")
            {
                solutionName ??= Solutions.GetSolutionName();

                deploymentSettingsFile = FileGeneration.CreateDeploymentSettings(solutionPath, solutionName);

                if (solutionExported == true)
                {
                    string? unpackPath = solutionPath.Replace(".zip", "");
                    Solutions.CleanUpSolutionFiles(solutionPath, unpackPath);
                }
            }

            string? needToAutoFill;

            do
            {
                Console.Title = "Autofill";
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("\nDo you want to autofill your/a deployment settings file? (y/n)\n");
                Console.ForegroundColor = ConsoleColor.White;
                needToAutoFill = Console.ReadLine();
                Console.ForegroundColor = ConsoleColor.Green;

                if (needToAutoFill != "y" && needToAutoFill != "n")
                {
                    Console.ForegroundColor = ConsoleColor.DarkMagenta;
                    Console.WriteLine("Invalid input. Please try again.");
                    Console.ForegroundColor = ConsoleColor.Green;
                }
            }
            while (needToAutoFill != "y" && needToAutoFill != "n");

            if (needToAutoFill == "n")
            {
                Helpers.PauseForUser("Finish and exit");
            }
            else
            {
                if (deploymentSettingsFile == null)
                {
                    Console.WriteLine("\nPlease input the full path of your deployment settings file including extension.\n");
                    Console.ForegroundColor = ConsoleColor.White;
                    deploymentSettingsFile = Console.ReadLine();
                }

                string? connectionType;
                do
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("\nConnect via your credentials or a service principal? (c/s)");
                    Console.ForegroundColor = ConsoleColor.White;
                    connectionType = Console.ReadLine();

                    if (connectionType != "c" && connectionType != "s")
                    {
                        Console.ForegroundColor = ConsoleColor.DarkMagenta;
                        Console.WriteLine("\nInvalid input. Please try again.");
                    }
                }
                while (connectionType != "c" && connectionType != "s");

                ServiceClient? svc;
                if (connectionType == "c")
                {
                    svc = MakeConnection.OAuth();
                }
                else
                {
                    svc = MakeConnection.ServicePrincipal();
                }

                FileGeneration.AutofilllSettingsFile(svc, deploymentSettingsFile);
            }
        }
    }
}


