// See https://aka.ms/new-console-template for more information
using System.Diagnostics;

int THREAD_COUNT = int.Parse(args[0]);
// int TIME_PER_TEST_MS = 10_000;

var stats = new Stats();
// Console.WriteLine(Job(stats, 2));
// return;
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

static float Job(Stats stats, int jobId)
{
    var g = (float)jobId + 0.1F;
    const float C = 1.00001F;
    const int ITER_COUNT = 10000;
    while (true)
    {
        for (int i = 0; i < ITER_COUNT; i++)
        {
            g *= C;g *= C; g *= C; g *= C; 
            g *= C;g *= C; g *= C; g *= C; 
            g *= C;g *= C; g *= C; g *= C; 
            g *= C;g *= C; g *= C; g *= C; 
            g *= C;g *= C; g *= C; g *= C; 
            g *= C;g *= C; g *= C; g *= C; 
        }
        while (g > 1000F)
            g /= 1000F;
        stats.Iters[jobId] = stats.Iters[jobId] + ITER_COUNT * 24;
    }
    return g;
}

class Stats
{
    public long[] Iters { get; } = new long[32];
    public long TotalPerf { set; get; } = 0;
}
