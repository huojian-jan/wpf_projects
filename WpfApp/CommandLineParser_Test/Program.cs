using System.Globalization;
using System.Runtime.InteropServices;
using Windows.Win32;
using Windows.Win32.Foundation;
using Microsoft.Win32.SafeHandles;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CommandLineParser_Test
{
    internal class Program
    {
        static void Main(string[] args)
        {

            //Test1();
            JsonFormat();


            Console.ReadKey();

        }

        static void JsonFormat()
        {
            //var obj = new
            //{
            //    hello = "hello",
            //    world = "world",
            //    info = new
            //    {
            //        name = "huojian",
            //        age = 20
            //    }
            //};

            var json = JsonConvert.SerializeObject(new List<string>() { "1", "2", "3" });
            var obj = JToken.Parse(json);

            var msg= obj.ToString(Formatting.Indented);

            Console.WriteLine(msg);

        }

        static Task Test1()
        {
            Task.Run(() =>
            {
                for (int i = 0; i < 10; i++)
                {
                    Console.WriteLine(i);
                }
            });

            return Task.CompletedTask;
        }

    }
}
