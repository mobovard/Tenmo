using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using TenmoServer.Models;

namespace TenmoServer.DAO
{
    public class TransferSqlDAO : ITransferDAO
    {
        private readonly string connectionString;

        public TransferSqlDAO(string dbConnectionString)
        {
            connectionString = dbConnectionString;
        }

        public Transfer AddTransfer(Transfer addTransfer)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    SqlCommand cmd = new SqlCommand("INSERT INTO transfers (transfer_type_id, transfer_status_id, account_from, account_to, amount) VALUES (@transferTypeID, @transferStatusID, @accountFrom, @accountTo, @amount); SELECT @@IDENTITY;", conn);
                    cmd.Parameters.AddWithValue("@transferTypeID", addTransfer.TransferTypeId);
                    cmd.Parameters.AddWithValue("@transferStatusID", addTransfer.TransferStatusId);
                    cmd.Parameters.AddWithValue("@accountFrom", addTransfer.AccountFrom);
                    cmd.Parameters.AddWithValue("@accountTo", addTransfer.AccountTo);
                    cmd.Parameters.AddWithValue("@amount", addTransfer.Amount);
                    int transferID = (int)(decimal)cmd.ExecuteScalar(); //don't know why it complained when trying to cast this as an int directly, but going through decimal works...

                    addTransfer.TransferId = transferID;
                }
            }
            catch (SqlException)
            {
                throw;
            }

            return addTransfer;
        }

        public List<Transfer> GetTransfersForUser(int userId)
        {
            List<Transfer> returnTransfers = new List<Transfer>();
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    SqlCommand cmd = new SqlCommand("SELECT transfer_id, transfer_type_id, transfer_status_id, account_from, account_to, amount FROM transfers JOIN accounts af ON af.account_id = transfers.account_from JOIN accounts at ON at.account_id = transfers.account_to WHERE @userId IN (af.user_id, at.user_id);", conn);
                    cmd.Parameters.AddWithValue("@userId", userId);

                    SqlDataReader reader = cmd.ExecuteReader();


                    while (reader.Read())
                    {
                        Transfer t = GetTransfersFromReader(reader);

                        returnTransfers.Add(t);
                    }
                }
            }
            catch (Exception)
            {

                throw;
            }

            return returnTransfers;
        }

        public Transfer GetTransferById(int transferId)
        {
            Transfer returnTransfer = null;
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    SqlCommand cmd = new SqlCommand("SELECT transfer_id, transfer_type_id, transfer_status_id, account_from, account_to, amount FROM transfers WHERE transfer_id = @transferId;", conn);
                    cmd.Parameters.AddWithValue("@transferId", transferId);

                    SqlDataReader reader = cmd.ExecuteReader();
                    if (reader.Read())
                    {
                        returnTransfer = GetTransfersFromReader(reader);
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }

            return returnTransfer;
        }
        private static Transfer GetTransfersFromReader(SqlDataReader reader)
        {
            return new Transfer()
            {
                TransferId = Convert.ToInt32(reader["transfer_id"]),
                TransferTypeId = Convert.ToInt32(reader["transfer_type_id"]),
                TransferStatusId = Convert.ToInt32(reader["transfer_status_id"]),
                AccountFrom = Convert.ToInt32(reader["account_from"]),
                AccountTo = Convert.ToInt32(reader["account_to"]),
                Amount = Convert.ToDecimal(reader["amount"])
            };
        }
    }
}
