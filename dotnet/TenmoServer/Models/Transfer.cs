using System.ComponentModel.DataAnnotations;

namespace TenmoServer.Models
{
    public class Transfer
    {
        public int TransferId { get; set; }
        public TransferTypes TransferTypeId { get; set; }
        public TransferStatus TransferStatusId { get; set; }
        public int AccountFrom { get; set; }
        public int FromUserId { get; set; }
        public string FromUsername { get; set; }
        public int AccountTo { get; set; }
        public int ToUserId { get; set; }
        public string ToUsername { get; set; }
        public decimal Amount { get; set; }
    }
}
