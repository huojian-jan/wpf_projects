using System;
using System.Diagnostics;

class Program
{
    static void Main()
    {
        var pageFaultsCounter = new PerformanceCounter("Memory", "Page Faults/sec");

        while (true)
        {
            float pageFaults = pageFaultsCounter.NextValue();
            var path = @"D:\Desktop\temp\page.txt";
            var msg = $"{DateTime.Now.ToShortDateString()}\t当前缺页速率: {pageFaults} 次/秒";
            Console.WriteLine(msg);
            File.AppendAllText(path,msg+"\n");
            System.Threading.Thread.Sleep(1000);
        }
    }
}