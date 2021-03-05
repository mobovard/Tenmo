using System;
using System.Collections.Generic;
using System.Text;

namespace TenmoClient.Data
{
    public class API_Transfer
    {
        public int TransferId { get; set; }
        public API_TransferTypes TransferTypeId { get; set; }
        public API_TransferStatus TransferStatusId { get; set; }
        public int AccountFrom { get; set; }
        public int FromUserId { get; set; }
        public string FromUsername { get; set; }
        public int AccountTo { get; set; }
        public int ToUserId { get; set; }
        public string ToUsername { get; set; }
        public decimal Amount { get; set; }
    }
}
