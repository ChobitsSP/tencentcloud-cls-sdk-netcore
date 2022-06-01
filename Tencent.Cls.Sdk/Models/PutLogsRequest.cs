using Newtonsoft.Json;
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
        [JsonProperty("filename")]
        public string Filename { get; set; }

        [JsonProperty("source")]
        public string Source { get; set; }

        [JsonProperty("contextFlow")]
        public string ContextFlow { get; set; }

        [JsonProperty("logs")]
        public List<LogItem> Logs { get; set; }

        [JsonProperty("logTags")]
        public List<LogTag> LogTags { get; set; }

        public byte[] GetBytes()
        {
            return new byte[0];
        }
    }

    public class LogGroupList
    {
        [JsonProperty("logGroupList")]
        public List<LogGroup> List { get; set; }
    }

    public class LogItem
    {
        [JsonProperty("time")]
        public long? Time { get; set; }
        public List<LogContent> Contents { get; set; }
    }

    public class LogTag
    {
        public string Key { get; set; }
        public string Value { get; set; }
    }

    public class LogContent
    {
        public string Key { get; set; }
        public string Value { get; set; }
    }
}