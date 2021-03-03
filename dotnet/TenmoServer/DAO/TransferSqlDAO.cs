using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using TenmoServer.Models;

namespace TenmoServer.DAO
{
    public class TransferSqlDAO: ITransferDAO
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
                    cmd.Parameters.AddWithValue("@amount",addTransfer.Amount);
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
    }
}
