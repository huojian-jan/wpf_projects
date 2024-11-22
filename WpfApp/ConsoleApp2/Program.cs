using System.Diagnostics;
using System.Numerics;
using System.Text;

namespace ConsoleApp2
{
    internal class Program
    {
        //static List<int> jobs = new List<int>() { 1, 8, 3, 7 };
        static void Main(string[] args)
        {
            var watch = new Stopwatch();
            watch.Start();

            ParallelDo();

            watch.Stop();

            Console.WriteLine("=======================================================");
            Console.WriteLine(watch.Elapsed.Seconds);

            Console.ReadKey();
        }


        static void ParallelDo()
        {
            var jobs = new List<int>() { 1, 9, 3, 7, 8, 2 };
            var opt = new ParallelOptions();
            opt.MaxDegreeOfParallelism = 3;
            Parallel.ForEach(jobs, opt, x =>
            {
                Console.WriteLine($"job:{x}\tthreadId:{Thread.CurrentThread.ManagedThreadId}");
                DoWork(1000 * x);
            });
        }

        static void DoWork(int job)
        {
            Thread.Sleep(job);
        }

        static void Do()
        {
            var jobs = new List<int>() { 1, 2, 3, 7, 8, 9 };
            foreach (var job in jobs)
            {
                DoWork(job * 1000);
                Console.WriteLine(job);
            }
        }

    }
}
