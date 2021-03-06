﻿using System;
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

        public delegate void LoadProgressHandler(LoadProgressEventArg e);
        public static event LoadProgressHandler LoadProgressEvent = new LoadProgressHandler((loadEvent) => { });

        public static void Main()
        {
            LoadProgressEvent += HandleLoadProgress;
            OnLoadProgress();

            for (int i = 0; i < Environment.ProcessorCount; i++)
            {
                new CrunchMark();
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
                                    Console.Clear();
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

        public static void OnLoadProgress()
        {
            LoadProgressEvent(new LoadProgressEventArg(currentLoadProgress++, loadTasks));
        }

        public static void HandleLoadProgress(LoadProgressEventArg e)
        {
            if (runcount <= 2)
            {
                Console.Clear();
                Console.WriteLine($"Initialising {string.Format("{0:p0}", e.ProgressPercent)}\t Algortihm: SHA-512 8192 bytes payload");
            }
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
