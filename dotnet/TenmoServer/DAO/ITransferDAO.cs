using System.Collections.Generic;
using TenmoServer.Models;

namespace TenmoServer.DAO
{
    public interface ITransferDAO
    {
        public Transfer AddTransfer(Transfer addTransfer);
        public List<Transfer> GetTransfersForUser(int userId, TransferStatus? status);
        public Transfer GetTransferById(int transferId);
        public Transfer UpdateTransferStatus(Transfer transfer);
    }
}
