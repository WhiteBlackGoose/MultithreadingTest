// See https://aka.ms/new-console-template for more information
using System.Diagnostics;

RunTests(1, 2, 3, 4, 6, 8, 10, 12, 16, 20, 24, 28, 32);
// int TIME_PER_TEST_MS = 10_000;

void RunTests(params int[] threadCount)
{
    var table = new Table("Thread Count", "Time elapsed", "Perf MFLOPS", "Perf per thread MFLOPS");
    foreach (var tc in threadCount)
    {
        table.Print(tc);
        var (time, counts) = RunTest(tc);
        var countsDec = (decimal)(counts * 1_000 / time / 100_000) / 10m;
        var countsDecThread = (decimal)(counts * 1_000 / time / 100_000 / tc) / 10m;
        table.Print(time, countsDec, countsDecThread);
    }
}

(long EllapsedAverage, long TotalCount) RunTest(int threadCount)
{
    var stats = new Stats(threadCount);
    for (int i = 0; i < threadCount; i++)
    {
        var jobId = i;
        new Thread(() => Job(stats, jobId)).Start();
    }
    Thread.Sleep(2500);
    stats.CancelRequested = true;
    while (!stats.Cancelled.All(c => c))
        Thread.Sleep(100);
    return (stats.EllapsedTime.Sum() / threadCount, stats.Iters.Sum());
}

Console.WriteLine("Done");

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
            g /= 2F;
        if (stats.CancelRequested)
            break;
        stats.Iters[jobId] = stats.Iters[jobId] + ITER_COUNT * 24;
    }
    stats.EllapsedTime[jobId] = sw.ElapsedMilliseconds;
    stats.Cancelled[jobId] = true;
    return g;
}

class Stats
{
    public long[] Iters { get; }
    public long[] EllapsedTime { get; }
    public bool CancelRequested { get; set; }
    public bool[] Cancelled { get; set; }
    public Stats(int cores)
    {
        Iters = new long[cores];
        CancelRequested = false;
        Cancelled = new bool[cores];
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
    public Table(params object[] columns)
    {
        lengths = columns.Select(c => c.ToString()!.Length).ToArray();
        currentCol = 0;
        Print(columns);
    }
    public void Print(params object[] args)
    {
        foreach (var arg in args)
            PrintOne(arg.ToString()!);
        void PrintOne(string s)
        {
            var colId = PopNext();
            if (colId is 0)
                Console.Write("| ");
            Console.Write(s);
            Console.Write(new string(' ', lengths[colId] - s.Length));
            Console.Write(" | ");
            if (colId == lengths.Length - 1)
                Console.WriteLine();
        }
    }
}
