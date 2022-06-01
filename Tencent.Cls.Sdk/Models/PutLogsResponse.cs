using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Tencent.Cls.Sdk.Models
{
    public class PutLogsResponse
    {
        public Dictionary<string, string> Headers { get; set; }
        public HttpStatusCode StatusCode { get; set; }
        public string errorcode { get; set; }
        public string errormessage { get; set; }
    }
}
