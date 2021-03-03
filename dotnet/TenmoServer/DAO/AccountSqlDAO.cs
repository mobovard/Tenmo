using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using TenmoServer.Models;

namespace TenmoServer.DAO
{
    public class AccountSqlDAO : IAccountDAO
    {
        private readonly string connectionString;

        public AccountSqlDAO(string dbConnectionString)
        {
            connectionString = dbConnectionString;
        }

        public Account GetAccount(int userId)
        {
            Account returnAccount = null;

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    SqlCommand cmd = new SqlCommand("SELECT account_id, user_id, balance FROM accounts WHERE user_id = @userId;", conn);
                    cmd.Parameters.AddWithValue("@userId", userId);
                    SqlDataReader reader = cmd.ExecuteReader();

                    if (reader.Read())
                    {
                        returnAccount = GetAccountFromReader(reader);
                    }
                }
            }
            catch (SqlException)
            {
                throw;
            }

            return returnAccount;
        }

        public void UpdateBalance(int accountId, decimal difference)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    // get initial balance
                    SqlCommand getBalCmd = new SqlCommand("SELECT balance FROM accounts WHERE account_id = @accountId;", conn);
                    getBalCmd.Parameters.AddWithValue("@accountId", accountId);
                    SqlDataReader reader = getBalCmd.ExecuteReader();
                    reader.Read();
                    decimal balance = Convert.ToDecimal(reader["balance"]);
                    reader.Close();

                    // update balance
                    SqlCommand updateCmd = new SqlCommand("UPDATE accounts SET balance = @balance WHERE account_id = @accountId;", conn);
                    updateCmd.Parameters.AddWithValue("@balance", balance + difference);
                    updateCmd.Parameters.AddWithValue("@accountId", accountId);
                    updateCmd.ExecuteNonQuery();
                }
            }
            catch (SqlException)
            {
                throw;
            }
        }

        private Account GetAccountFromReader(SqlDataReader reader)
        {
            Account a = new Account()
            {
                AccountId = Convert.ToInt32(reader["account_id"]),
                UserId = Convert.ToInt32(reader["user_id"]),
                Balance = Convert.ToDecimal(reader["balance"]),
            };

            return a;
        }
    }
}
