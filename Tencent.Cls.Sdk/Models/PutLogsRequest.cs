using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tencent.Cls.Sdk.Models
{
    public class PutLogsRequest : Dictionary<string, string>
    {
        public string Topic { get; set; }
        public string Source { get; set; }

        private LogGroup mlogItems;

        public byte[] getLogGroupBytes(string Source)
        {
            this.mlogItems.Source = Source;
            return this.mlogItems.GetBytes();
        }
    }

    public class LogGroup
    {
        public string FileName { get; set; }
        public string Source { get; set; }
        public List<LogItem> Logs { get; set; }

        public byte[] GetBytes()
        {
            return new byte[0];
        }
    }

    public class LogItem
    {

    }
}