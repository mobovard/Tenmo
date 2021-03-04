using System.Collections.Generic;
using TenmoServer.Models;

namespace TenmoServer.DAO
{
    public interface ITransferDAO
    {
        public Transfer AddTransfer(Transfer addTransfer);
        public List<Transfer> GetTransfersForUser(int userId);
        public Transfer GetTransferById(int transferId);
    }
}
