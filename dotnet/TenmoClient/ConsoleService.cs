using RestSharp;
using RestSharp.Authenticators;
using System;
using System.Collections.Generic;
using TenmoClient.Data;

namespace TenmoClient
{

    public class ConsoleService
    {
        private readonly static string API_BASE_URL = "https://localhost:44315/";
        private readonly IRestClient client = new RestClient();
        private const string DASHES = "-----------------------------------------";

        /// <summary>
        /// Prompts for transfer ID to view, approve, or reject
        /// </summary>
        /// <param name="action">String to print in prompt. Expected values are "Approve" or "Reject" or "View"</param>
        /// <returns>ID of transfers to view, approve, or reject</returns>
        public int PromptForTransferID(string action)
        {
            Console.WriteLine("");
            Console.Write("Please enter transfer ID to " + action + " (0 to cancel): ");
            if (!int.TryParse(Console.ReadLine(), out int auctionId))
            {
                Console.WriteLine("Invalid input. Only input a number.");
                return 0;
            }
            else
            {
                return auctionId;
            }
        }

        public LoginUser PromptForLogin()
        {
            Console.Write("Username: ");
            string username = Console.ReadLine();
            string password = GetPasswordFromConsole("Password: ");

            LoginUser loginUser = new LoginUser
            {
                Username = username,
                Password = password
            };
            return loginUser;
        }

        private string GetPasswordFromConsole(string displayMessage)
        {
            string pass = "";
            Console.Write(displayMessage);
            ConsoleKeyInfo key;

            do
            {
                key = Console.ReadKey(true);

                // Backspace Should Not Work
                if (!char.IsControl(key.KeyChar))
                {
                    pass += key.KeyChar;
                    Console.Write("*");
                }
                else
                {
                    if (key.Key == ConsoleKey.Backspace && pass.Length > 0)
                    {
                        pass = pass.Remove(pass.Length - 1);
                        Console.Write("\b \b");
                    }
                }
            }
            // Stops Receving Keys Once Enter is Pressed
            while (key.Key != ConsoleKey.Enter);
            Console.WriteLine("");
            return pass;
        }

        public void DisplayBalance()
        {
            RestRequest request = new RestRequest(API_BASE_URL + "user/account");
            client.Authenticator = new JwtAuthenticator(UserService.GetToken());
            IRestResponse<API_Account> response = client.Get<API_Account>(request);

            if (response.ResponseStatus != ResponseStatus.Completed)
            {
                Console.WriteLine("An error occurred communicating with the server.");
            }
            else if (!response.IsSuccessful)
            {
                Console.WriteLine("An error response was received from the server. The status code is " + (int)response.StatusCode);
            }
            else
            {
                Console.WriteLine($"Your current account balance is: {response.Data.Balance:C2}");
            }
        }

        public void DisplayUsers()
        {
            Console.WriteLine(DASHES);
            Console.WriteLine("Users");
            Console.WriteLine(String.Format("{0,-12}", "ID") + "Name");
            Console.WriteLine(DASHES);

            RestRequest request = new RestRequest(API_BASE_URL + "user");
            client.Authenticator = new JwtAuthenticator(UserService.GetToken());
            IRestResponse<List<API_User>> response = client.Get<List<API_User>>(request);

            if (response.ResponseStatus != ResponseStatus.Completed)
            {
                Console.WriteLine("An error occurred communicating with the server.");
            }
            else if (!response.IsSuccessful)
            {
                Console.WriteLine("An error response was received from the server. The status code is " + (int)response.StatusCode);
            }
            else
            {
                foreach (API_User user in response.Data)
                {
                    Console.WriteLine(String.Format("{0,-12}", user.UserId) + user.Username);
                }
                Console.WriteLine(DASHES);
            }
        }

        public void CreateTransfer(API_Transfer transfer)
        {
            RestRequest request = new RestRequest(API_BASE_URL + "user/transfer");
            request.AddJsonBody(transfer);
            client.Authenticator = new JwtAuthenticator(UserService.GetToken());
            IRestResponse<API_Transfer> response = client.Post<API_Transfer>(request);

            if (!response.IsSuccessful)
            {
                Console.WriteLine($"Error {(int)response.StatusCode}: {response.Content}");
            }
            else
            {
                Console.WriteLine("Transfer executed successfully.");
            }
        }

        public void DisplayTransfers()
        {
            RestRequest request = new RestRequest(API_BASE_URL + "user/transfer");
            client.Authenticator = new JwtAuthenticator(UserService.GetToken());
            IRestResponse<List<API_Transfer>> response = client.Get<List<API_Transfer>>(request);

            if (!response.IsSuccessful)
            {
                Console.WriteLine($"Error {(int)response.StatusCode}: {response.Content}");
            }
            else
            {
                Console.WriteLine(DASHES);
                Console.WriteLine("Transfers");
                Console.WriteLine(String.Format("{0,-8} {1,-20} {2,0}", "ID", "From/To", "Amount"));
                Console.WriteLine(DASHES);

                foreach (API_Transfer transfer in response.Data)
                {
                    string tofrom = "";
                    if (UserService.GetUserId() == transfer.FromUserId)
                    {
                        tofrom = $"To: {transfer.ToUsername}";
                    }
                    else
                    {
                        tofrom = $"From: {transfer.FromUsername}";
                    }

                    Console.WriteLine(String.Format("{0,-8} {1,-20} {2,0}", transfer.TransferId, tofrom, $"{transfer.Amount:C2}"));

                }
                Console.WriteLine(DASHES);
            }
        }

        public void DisplayPendingTransfers()
        {
            RestRequest request = new RestRequest(API_BASE_URL + "user/transfer?status=pending");
            client.Authenticator = new JwtAuthenticator(UserService.GetToken());
            IRestResponse<List<API_Transfer>> response = client.Get<List<API_Transfer>>(request);

            if (!response.IsSuccessful)
            {
                Console.WriteLine($"Error {(int)response.StatusCode}: {response.Content}");
            }
            else
            {
                Console.WriteLine(DASHES);
                Console.WriteLine("Transfers");
                Console.WriteLine(String.Format("{0,-8} {1,-20} {2,0}", "ID", "To", "Amount"));
                Console.WriteLine(DASHES);

                foreach (API_Transfer transfer in response.Data)
                {
                    if (transfer.ToUserId == UserService.GetUserId())
                    {
                        Console.WriteLine(String.Format("{0,-8} {1,-20} {2,0}", transfer.TransferId, transfer.FromUsername, $"{transfer.Amount:C2}"));
                    }

                }
                Console.WriteLine(DASHES);
            }
        }
        public void DisplayTransfer(int transferID)
        {
            RestRequest request = new RestRequest(API_BASE_URL + $"user/transfer/{transferID}");
            client.Authenticator = new JwtAuthenticator(UserService.GetToken());
            IRestResponse<API_Transfer> response = client.Get<API_Transfer>(request);

            if (!response.IsSuccessful)
            {
                Console.WriteLine($"Error {(int)response.StatusCode}: {response.Content}");
            }
            else
            {
                Console.WriteLine(DASHES);
                Console.WriteLine("Transfer Details");
                Console.WriteLine(DASHES);
                Console.WriteLine($"Id: {response.Data.TransferId}");
                Console.WriteLine($"From: {response.Data.FromUsername} ({response.Data.FromUserId})");
                Console.WriteLine($"To: {response.Data.ToUsername} ({response.Data.ToUserId})");
                Console.WriteLine($"Type: {response.Data.TransferTypeId}");
                Console.WriteLine($"Status: {response.Data.TransferStatusId}");
                Console.WriteLine($"Amount: {response.Data.Amount:C2}");
                Console.WriteLine(DASHES);
            }
        }

        public int GetInteger(string message)
        {
            string userInput = string.Empty;
            int intValue = 0;
            int numberOfAttempts = 0;

            do
            {
                if (numberOfAttempts > 0)
                {
                    Console.WriteLine("Invalid input format. Please try again");
                }

                Console.Write(message + " ");
                userInput = Console.ReadLine();
                numberOfAttempts++;
            }
            while (!int.TryParse(userInput, out intValue));

            return intValue;
        }

        public decimal GetDecimal(string message)
        {
            string userInput = string.Empty;
            decimal decimalValue = 0.0M;
            int numberOfAttempts = 0;

            do
            {
                if (numberOfAttempts > 0)
                {
                    Console.WriteLine("Invalid input format. Please try again");
                }

                Console.Write(message + " ");
                userInput = Console.ReadLine();
                numberOfAttempts++;
            }
            while (!decimal.TryParse(userInput, out decimalValue));

            return decimalValue;
        }
    }
}
