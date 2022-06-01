using Newtonsoft.Json;
using Serilog.Events;
using Serilog.Parsing;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Tencent.Cls.Sdk;

namespace Tencent.Cls.Test
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello, World!");
            Test1();
        }

        static async void Test1()
        {
            var client = JsonConvert.DeserializeObject<TxClsClient>(File.ReadAllText("config.json"));

            var plist = new List<LogEventProperty>();
            var tokens = new List<MessageTemplateToken>();

            var log = new LogEvent(DateTime.Now, LogEventLevel.Debug, null, new MessageTemplate("123", tokens), plist);

            try
            {
                var rsp = await client.EmitBatchAsync(log);
                Console.WriteLine(rsp);
            }
            catch (Exception ex)
            {
                Console.Write(ex);
            }

            Debugger.Break();
        }
    }
}