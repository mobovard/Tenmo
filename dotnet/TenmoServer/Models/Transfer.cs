using Newtonsoft.Json.Converters;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace TenmoServer.Models
{

    public class Transfer
    {
        public int TransferId { get; set; }
        [Required(ErrorMessage = "Transfer type ID is required")]
        public TransferTypes TransferTypeId { get; set; }
        [Required(ErrorMessage = "Transfer status ID is required")]
        public TransferStatus TransferStatusId { get; set; }

        [JsonIgnore]
        public int AccountFrom { get; set; }
        public int FromUserId { get; set; }
        public string FromUsername { get; set; }

        [JsonIgnore]
        public int AccountTo { get; set; }
        [Required(ErrorMessage = "To user ID is required")]
        public int ToUserId { get; set; }
        public string ToUsername { get; set; }
        [Required(ErrorMessage = "Amount is required")]
        public decimal Amount { get; set; }
    }
}
