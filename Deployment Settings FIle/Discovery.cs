using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.PowerPlatform.Dataverse.Client.Auth;
using Microsoft.PowerPlatform.Dataverse.Client.Model;
using Microsoft.Xrm.Sdk.Discovery;
using System;
using System.ComponentModel;
using System.DirectoryServices.AccountManagement;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Deployment_Settings_FIle
{
    internal class Discovery
    {
        //These sample application registration values are available for all online instances.
        public static string clientId = "51f81489-12ee-4a9e-aaae-a2591f45987d";
        public static string redirectUrl = "http://localhost";

        public static async Task ListEnvironments()
        {
            string username = "aUserName@anOrgName.onmicrosoft.com";
            string password = "aPassword";

            //Get all environments for the selected data center.
            DiscoverOrganizationsResult orgs = await GetAllOrganizations();

            if (orgs.OrganizationDetailCollection.Count.Equals(0))
            {
                Console.WriteLine("No valid environments returned for these credentials.");
                return;
            }

            Console.WriteLine("Type the number of the environments you want to use and press Enter.");

            int number = 0;

            //Display organizations so they can be selected
            foreach (OrganizationDetail organization in orgs.OrganizationDetailCollection)
            {
                number++;

                //Get the Organization URL
                string webAppUrl = organization.Endpoints[EndpointType.WebApplication];

                Console.WriteLine($"{number} Name: {organization.FriendlyName} URL: {webAppUrl}");
            }

            string typedValue = string.Empty;
            try
            {
                typedValue = Console.ReadLine();

                int selected = int.Parse(typedValue);

                if (selected <= number)
                {
                    OrganizationDetail org = orgs.OrganizationDetailCollection[selected - 1];
                    Console.WriteLine($"You selected '{org.FriendlyName}'");

                    //Use the selected org with ServiceClient to get the UserId
                    ShowUserId(org, username, password);
                }
                else
                {
                    throw new ArgumentOutOfRangeException("The selected value is not valid.");
                }
            }
            catch (ArgumentOutOfRangeException aex)
            {
                Console.WriteLine(aex.Message);
            }
            catch (Exception)
            {
                Console.WriteLine("Unable to process value: {0}", typedValue);
            }
        }
        public static async Task<DiscoverOrganizationsResult> GetAllOrganizations()
        {
            try
            {
                // Set up user credentials
                string cloudRegionUrl = "https://globaldisco.crm.dynamics.com";
                var creds = new System.ServiceModel.Description.ClientCredentials();
                // Fake creds
                creds.UserName.UserName = "aUserName@anOrgName.onmicrosoft.com";
                creds.UserName.Password = "aPassword";

                try
                {
                    //Call DiscoverOnlineOrganizationsAsync
                    DiscoverOrganizationsResult organizationsResult = await ServiceClient.DiscoverOnlineOrganizationsAsync(
                           discoveryServiceUri: new Uri($"{cloudRegionUrl}/api/discovery/v2.0/Instances"),
                           clientCredentials: creds,
                           clientId: clientId,
                           redirectUri: new Uri(redirectUrl),
                           isOnPrem: false,
                           authority: "https://login.microsoftonline.com/organizations/",
                           promptBehavior: PromptBehavior.Auto);

                    return organizationsResult;
                }
                catch (Exception)
                {

                    throw;
                }
            }
            catch (Exception)
            {

                throw;
            }
        }
        /// <summary>
        /// Show the user's UserId for the selected organization
        /// </summary>
        /// <param name="org">The selected organization</param>
        /// <param name="username">The user's username</param>
        /// <param name="password">The user's password</param>
        private static void ShowUserId(OrganizationDetail org, string username, string password)
        {
            try
            {
                string conn = $@"AuthType=OAuth;
                         Url={org.Endpoints[EndpointType.OrganizationService]};
                         UserName={username};
                         Password={password};
                         ClientId={clientId};
                         RedirectUri={redirectUrl};
                         Prompt=Auto;
                         RequireNewInstance=True";
                ServiceClient svc = new(conn);

                if (svc.IsReady)
                {
                    try
                    {
                        var response = (WhoAmIResponse)svc.Execute(new WhoAmIRequest());

                        Console.WriteLine($"Your UserId for {org.FriendlyName} is: {response.UserId}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
                else
                {
                    Console.WriteLine(svc.LastError);
                }
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
