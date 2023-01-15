// See https://aka.ms/new-console-template for more information
using System.Diagnostics;

int THREAD_COUNT = int.Parse(args[0]);
int TIME_PER_TEST_MS = 10_000;

var stats = new Stats();
for (int i = 0; i <= THREAD_COUNT; i++)
{
    if (i == THREAD_COUNT)
         new Thread(() => Print(stats)).Start();
    else
    {
        var jobId = i;
        new Thread(() => Job(stats, jobId)).Start();
    }
}

Console.WriteLine("Done");

void Print(Stats stats)
{
    var initial = stats.Iters.ToArray();
    var sw = new Stopwatch();
    sw.Start();
    while (true)
    {
        Thread.Sleep(400);
        Console.Clear();
        var totalDiff = 0L;
        for (int i = 0; i < THREAD_COUNT; i++)
        {
            Console.Write($"#{i}\t");
            Console.Write(stats.Iters[i] - initial[i]);
            Console.Write('\t');
            PrintPerf(stats.Iters[i] - initial[i], sw);
            totalDiff += stats.Iters[i] - initial[i];
        }
        PrintPerf(totalDiff, sw);
    }

    static void PrintPerf(long iter, Stopwatch sw)
    {
        var perf = iter * 1000 / sw.ElapsedMilliseconds;
        Console.Write(perf / 1_000_000);
        Console.WriteLine(" MFLOPS");
    }
}

static void Job(Stats stats, int jobId)
{
    var a = 1L; 
    var b = 1L;
    var i = 0L;
    while (true)
    {
        (b, a) = (a + b, b);
        i++;
        if (i % 1000 == 0)
            stats.Iters[jobId] = i;
    }
}

class Stats
{
    public long[] Iters { get; } = new long[32];
    public long TotalPerf { set; get; } = 0;
}
