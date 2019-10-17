using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.IO;

namespace CrunchMark
{
    using static Program;

    public class CrunchMark
    {
        public static List<int> HashVolume { get; private set; } = new List<int>();

        private static List<CrunchMark> threads = new List<CrunchMark>();
        private static int currentThread = 0;
        private int threadId;
        private static bool toStop = false;
        private RandomNumberGenerator randomPool;
        private Random randomSimplePool;
        private HashAlgorithm hasher;
        private byte[] source;

        public CrunchMark(int size)
        {
            randomSimplePool = new Random();
            randomPool = RandomNumberGenerator.Create();
            System.Threading.Thread.Sleep(100);
            HashVolume.Add(0);
            threadId = currentThread++;
            threads.Add(this);
            hasher = MD5.Create();

            source = new byte[size];

            OnLoadProgress();
        }

        public void Start()
        {
            OnLoadProgress();   
            Task.Run(() =>
            {
                while (true)
                {
                    randomPool.GetBytes(source);
                    string hash = ComputeHash(hasher, source);

                    if (BurnDisk)
                    {
                        var directory = Directory.GetCurrentDirectory() + @"\BurnFiles";

                        File.WriteAllText(directory + $@"\{hash}.dat", BytesToHexString(source));
                    }

                    lock (HashVolume)
                    {
                        HashVolume[threadId]++;
                    }
                }
            });
        }

        string ComputeHash(HashAlgorithm hasher, byte[] input)
        {
            byte[] data = hasher.ComputeHash(input);
            return BytesToHexString(data);
        }

        string BytesToHexString(byte[] input)
        {
            StringBuilder sBuilder = new StringBuilder();
            for (int i = 0; i < input.Length; i++)
            {
                sBuilder.Append(input[i].ToString("x2"));
            }
            return sBuilder.ToString();
            //return string.Concat(input.Select(b => b.ToString("x2")));
        }

        public static void StartAll()
        {
            Directory.CreateDirectory(Directory.GetCurrentDirectory() + @"\BurnFiles");
            toStop = false;
            ClearAll();
            threads.ForEach(x => x.Start());
        }

        public static void ClearAll()
        {
            HashVolume = HashVolume.ConvertAll(x => x = 0);
        }
    }
}
