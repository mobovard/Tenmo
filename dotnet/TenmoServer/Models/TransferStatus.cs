using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace TenmoServer.Models
{
    
    public enum TransferStatus
    {
        PENDING = 1, APPROVED = 2, REJECTED = 3
    }
}
