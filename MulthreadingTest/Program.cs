// See https://aka.ms/new-console-template for more information
using System.Diagnostics;

int THREAD_COUNT = int.Parse(args[0]);
// int TIME_PER_TEST_MS = 10_000;

void RunTests(int[] threadCount)
{
    foreach (var tc in threadCount)
    {
        Console.WriteLine($"Running test: thread Count: {tc}");
        var (time, counts) = RunTest(tc);
        Console.WriteLine($"Time: {time}");
    }
}

(long EllapsedAverage, long TotalCount) RunTest(int threadCount)
{
    var stats = new Stats(threadCount);
    for (int i = 0; i <= threadCount; i++)
    {
        if (i == threadCount)
             new Thread(() => Watch(stats)).Start();
        else
        {
            var jobId = i;
            new Thread(() => Job(stats, jobId)).Start();
        }
    }
    Thread.Sleep(1000);
    return (stats.EllapsedTime.Sum() / threadCount, stats.TotalPerf);
}

Console.WriteLine("Done");

void Watch(Stats stats)
{
    Thread.Sleep(10000);
}

static float Job(Stats stats, int jobId)
{
    var g = (float)jobId + 0.1F;
    const float C = 1.00001F;
    const int ITER_COUNT = 10000;
    var sw = Stopwatch.StartNew();
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
        if (stats.Cancelled)
            break;
        stats.Iters[jobId] = stats.Iters[jobId] + ITER_COUNT * 24;
    }
    stats.EllapsedTime[jobId] = sw.ElapsedMilliseconds;
    return g;
}

class Stats
{
    public long[] Iters { get; }
    public long[] EllapsedTime { get; }
    public long TotalPerf { set; get; }
    public bool Cancelled { get; set; }
    public Stats(int cores)
    {
        Iters = new long[cores];
        TotalPerf = 0;
        Cancelled = false;
        EllapsedTime = new long[cores];
    }
}

class Table
{
    private int[] lengths;
    private int currentCol;
    private int PopNext()
    {
        if (currentCol == lengths.Length)
            currentCol = 0;
        return currentCol++;
    }
    public Table(params string[] columns)
    {
        lengths = columns.Select(c => c.Length).ToArray();
        currentCol = 0;
        Print(columns);
    }
    public void Print(params string[] args)
    {
        foreach (var arg in args)
            PrintOne(arg);
        void PrintOne(string s)
        {
            var colId = PopNext();
            if (colId is 0)
                Console.Write("| ");
            Console.Write(s);
            Console.Write(new string(' ', lengths[colId] - s.Length));
            Console.Write(" |");
            if (colId == lengths.Length - 1)
                Console.WriteLine();
        }
    }
}
