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
        private const string BASE_QUERY = @"SELECT transfer_id, transfer_type_id, transfer_status_id, account_from, uf.user_id as from_user_id, uf.username as from_username, account_to, ut.user_id as to_user_id, ut.username as to_username, amount FROM transfers t
                                            JOIN accounts af ON t.account_from = af.account_id
                                            JOIN users uf ON af.user_id = uf.user_id
                                            JOIN accounts at ON t.account_to = at.account_id
                                            JOIN users ut ON at.user_id = ut.user_id ";

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

        public List<Transfer> GetTransfersForUser(int userId, TransferStatus? status)
        {
            List<Transfer> returnTransfers = new List<Transfer>();
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    string commandText = BASE_QUERY + "WHERE @userId IN(af.user_id, at.user_id)";

                    SqlCommand cmd = new SqlCommand();
                    cmd.Connection = conn;
                    if (status == null)
                    {
                        cmd.CommandText = commandText;
                    }
                    else
                    {
                        cmd.CommandText = commandText + " AND transfer_status_id = @statusId;";
                        cmd.Parameters.AddWithValue("@statusId", (int)status);
                    }
                    cmd.Parameters.AddWithValue("@userId", userId);

                    SqlDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        Transfer t = GetTransferFromReader(reader);
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

                    SqlCommand cmd = new SqlCommand(BASE_QUERY + "WHERE transfer_id = @transferId;", conn);
                    cmd.Parameters.AddWithValue("@transferId", transferId);

                    SqlDataReader reader = cmd.ExecuteReader();
                    if (reader.Read())
                    {
                        returnTransfer = GetTransferFromReader(reader);
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }

            return returnTransfer;
        }

        public Transfer UpdateTransferStatus(Transfer transfer)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    SqlCommand cmd = new SqlCommand("UPDATE transfers SET transfer_status_id = @statusId WHERE transfer_id = @transferId;", conn);
                    cmd.Parameters.AddWithValue("@transferId", transfer.TransferId);
                    cmd.Parameters.AddWithValue("@statusId", (int)transfer.TransferStatusId);

                    int numberOfRows = cmd.ExecuteNonQuery();
                    if (numberOfRows == 1)
                    {
                        return transfer;
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }

            return null;
        }
        private static Transfer GetTransferFromReader(SqlDataReader reader)
        {
            return new Transfer()
            {
                TransferId = Convert.ToInt32(reader["transfer_id"]),
                TransferTypeId = (TransferTypes)Convert.ToInt32(reader["transfer_type_id"]),
                TransferStatusId = (TransferStatus)Convert.ToInt32(reader["transfer_status_id"]),
                AccountFrom = Convert.ToInt32(reader["account_from"]),
                FromUserId = Convert.ToInt32(reader["from_user_id"]),
                FromUsername = Convert.ToString(reader["from_username"]),
                AccountTo = Convert.ToInt32(reader["account_to"]),
                ToUserId = Convert.ToInt32(reader["to_user_id"]),
                ToUsername = Convert.ToString(reader["to_username"]),
                Amount = Convert.ToDecimal(reader["amount"])
            };
        }
    }
}
