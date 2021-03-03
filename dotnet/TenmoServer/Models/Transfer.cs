using System.ComponentModel.DataAnnotations;

namespace TenmoServer.Models
{
    public class Transfer
    {
        public int TransferId { get; set; }
        public int TransferTypeId { get; set; }
        public int TransferStatusId { get; set; }
        public int AccountFrom { get; set; }

        [Required(ErrorMessage = "Account To should not be blank.")]
        public int AccountTo { get; set; }

        [Required(ErrorMessage = "Ammount should not be blank.")]
        public decimal Amount { get; set; }
    }
}
