using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace TenmoServer.Models
{
    public enum TransferTypes
    {
        REQUEST = 1, SEND = 2
    }
}
