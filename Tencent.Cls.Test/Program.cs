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
            Test1();
        }

        static async void Test1()
        {
            var client = JsonConvert.DeserializeObject<TxClsClient>(File.ReadAllText("config.json"));

            try
            {
                client.Info(new Dictionary<string, string>()
                {
                    { "msg", "test1" },
                    { "uuid", Guid.NewGuid().ToString() },
                });

                Console.ReadLine();
                Debugger.Break();
            }
            catch (Exception ex)
            {
                Console.Write(ex);
                Debugger.Break();
            }
        }
    }
}