using System.Collections.Concurrent;
using System.Diagnostics;

namespace FileSize
{

    internal class Program
    {
        static ConcurrentBag<long> OtherAccountData = new ConcurrentBag<long>();
        //private static TimeSpan _reserveTime = DateTime.Now - DateTime.Now.AddYears(-1);
        private static TimeSpan _reserveTime = DateTime.Now - DateTime.Now.AddYears(-1);
        static void Main(string[] args)
        {
            // var candidateDirs = Directory.EnumerateDirectories(@"C:\Users\33008\AppData\Local\ShadowBot\users").Where(x => !x.Contains("Standalone") && !x.Contains("Assistant"));
            // ComputeOtherAccountCache();

            
            
            
            Console.ReadKey();
        }

        private static void ParallelTest()
        {
            Parallel.ForEach(Enumerable.Range(0, 10), x =>
            {
                LongMethod();
            });
            
            Console.WriteLine("Parallel Test");    
        }



        private static void LongMethod()
        {
            for (int i = 0; i < 100000000; i++)
            {
                if (i % 999 == 0)
                {
                    Console.Write("**************");
                }
            }
        }





        private static void ComputeOtherAccountCache()
        {
            var candidateDirs = Directory.EnumerateDirectories(@"C:\Users\33008\AppData\Local\ShadowBot\users").Where(x => !x.Contains("Standalone") && !x.Contains("Assistant"));
            //排除调度，单机版
            var size = 0l;
            var tasks = new List<Task<long>>();
            var watch = new Stopwatch();
            watch.Start();
            foreach (var user in candidateDirs)
            {
                DirSize(new DirectoryInfo(user));
            }
            watch.Stop();
            foreach (var itemSize in OtherAccountData)
            {
                size += itemSize;
            }

            var mb = size / (1024d * 1024);
            mb = Math.Round(mb, 3);
            var time = watch.ElapsedMilliseconds / 1000d;
            time = Math.Round(time, 3);

            Console.WriteLine($"size={mb} MB\ttime={time}");


        }

        private static long DirSize(DirectoryInfo dirInfo, int layer = 0)
        {
            if (!dirInfo.Exists) return 0;
            if (layer == 2)
            {
                Parallel.ForEach(dirInfo.EnumerateDirectories(), dir =>
                {
                    DirSize(dir, layer++);
                });
            }
            else
            {
                foreach (var dir in dirInfo.EnumerateDirectories())
                {
                    DirSize(dir, layer++);
                }
            }

            foreach (var file in dirInfo.EnumerateFiles())
            {
                if (DateTime.Now - file.LastWriteTime > _reserveTime)
                {
                    OtherAccountData.Add(file.Length);
                }
            }

            return 0;
        }

    }
}
