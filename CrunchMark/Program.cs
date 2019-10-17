using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;

namespace CrunchMark
{
    public class Program
    {
        static int loadTasks = Environment.ProcessorCount * 3 + 6;
        static int currentLoadProgress = 0;
        static int runcount = 0;
        static Timer timer;
        public static bool BurnDisk = false;


        public static event Action<int, int> LoadProgressEvent;

        public static void Main()
        {
            Console.WriteLine("Burn Disk? Y/N");
            string input = Console.ReadLine();
            if (input.ToLower() == "y")
            {
                BurnDisk = true;
            }

            Console.WriteLine("Enter crunch size in the power of 2: ");
            var inputSize = Console.ReadLine();
            var crunchSize = 0;
            if (int.TryParse(inputSize, out int power) && power < 30)
            {
                crunchSize = (int)Math.Pow(2, power - 1);
                Console.WriteLine($"Using {crunchSize * 2} byte file size.");
            }
            else
            {
                crunchSize = (int)Math.Pow(2, 15);
                Console.WriteLine($"Invalid input, redirected to {crunchSize * 2} byte file size.");
            }

            Console.WriteLine();

            LoadProgressEvent += (current, totall) =>
            {
                if (runcount <= 2)
                {
                    Console.SetCursorPosition(0, Console.CursorTop - 1);
                    ClearCurrentConsoleLine();
                    Console.WriteLine($"Initialising {string.Format("{0:p0}", (float)current / totall)}\t Algortihm: SHA-512 8192 bytes payload");
                }
            };
            OnLoadProgress();

            for (int i = 0; i < Environment.ProcessorCount; i++)
            {
                new CrunchMark(crunchSize);
            }

            var previousTime = DateTime.Now;
            var threadTracker = Task.Run(() =>
            {
                CrunchMark.StartAll();
                timer = new Timer(1000);
                timer.Start();
                timer.Elapsed += (s, e) =>
                {
                    runcount++;
                    OnLoadProgress();
                    timer.Stop();

                    bool countFailed = false;
                    int errorcount = 0;

                    do
                    {
                        try
                        {
                            lock (CrunchMark.HashVolume)
                            {
                                int currentTotal = CrunchMark.HashVolume.Sum();
                                OnLoadProgress();
                                double deltaTime = (DateTime.Now - previousTime).TotalMilliseconds;
                                previousTime = DateTime.Now;
                                currentTotal = (int)((currentTotal * 1.0) / deltaTime * 1000);

                                if (runcount == 3)
                                {
                                    Console.SetCursorPosition(0, Console.CursorTop - 1);
                                    ClearCurrentConsoleLine();
                                    Console.WriteLine("Thread count: " + Environment.ProcessorCount);
                                    Console.ForegroundColor = ConsoleColor.Yellow;
                                    Console.WriteLine("Yellow line: time delta more than 1100 or encountered 1 threading error, which can cause inconsistency.");
                                    Console.ResetColor();
                                    Console.ForegroundColor = ConsoleColor.Red;
                                    Console.WriteLine("Red line: time delta more than 1500 or encountered more than 1 threading error, which can cause severe inconsistency.\n");
                                    Console.ResetColor();
                                }
                                if (deltaTime > 1100) Console.ForegroundColor = ConsoleColor.Yellow;
                                if (deltaTime > 1500) Console.ForegroundColor = ConsoleColor.Red;
                                if (runcount >= 3) Console.WriteLine($"Score: {string.Format("{0:n0}", currentTotal)}\tThreading errors: {errorcount} \tDelta time: {deltaTime} ms");
                                Console.ResetColor();
                                countFailed = false;

                                CrunchMark.ClearAll();
                            }
                        }
                        catch (Exception error)
                        {
                            errorcount++;
                            countFailed = true;
                            if (errorcount == 1) Console.ForegroundColor = ConsoleColor.Yellow;
                            if (errorcount > 1) Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine(error);
                            Console.WriteLine(error.StackTrace);
                        }
                    }
                    while (countFailed);

                    OnLoadProgress();
                    timer.Start();
                };
            });

            Console.ReadLine();
        }

        public static void ClearCurrentConsoleLine()
        {
            int currentLineCursor = Console.CursorTop;
            Console.SetCursorPosition(0, Console.CursorTop);
            Console.Write(new string(' ', Console.WindowWidth));
            Console.SetCursorPosition(0, currentLineCursor);
        }

        public static void OnLoadProgress()
        {
            LoadProgressEvent(currentLoadProgress++, loadTasks);
        }
    }

    public class LoadProgressEventArg : EventArgs
    {
        public float ProgressPercent { get => current / (float)total; }

        private int current;
        private int total;

        public LoadProgressEventArg(int current, int total)
        {
            this.current = current;
            this.total = total;
        }
    }
}
