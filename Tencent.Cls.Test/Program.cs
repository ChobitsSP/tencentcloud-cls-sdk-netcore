using Newtonsoft.Json;
using Serilog.Events;
using System;
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
            var client = JsonConvert.DeserializeObject<AsyncClient>(File.ReadAllText("config.json"));

            LogEvent log = null;

            var rsp = await client.EmitBatchAsync(log);

            Console.WriteLine(rsp);

            Debugger.Break();
        }
    }
}