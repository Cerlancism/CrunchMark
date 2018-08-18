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

        public CrunchMark()
        {
            randomSimplePool = new Random();
            randomPool = RandomNumberGenerator.Create();
            System.Threading.Thread.Sleep(100);
            HashVolume.Add(0);
            threadId = currentThread++;
            threads.Add(this);
            hasher = MD5.Create();
            source = new byte[(int)Math.Pow(2, 17)];
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
                    string hash = GetHash(hasher, source);

                    if (BurnDisk)
                    {
                        var directory = Directory.GetCurrentDirectory() + @"\BurnFiles";

                        File.WriteAllBytes(directory + $@"\{hash}.dat", source);
                    }

                    lock (HashVolume)
                    {
                        HashVolume[threadId]++;
                    }
                }
            });
        }

        string GetHash(HashAlgorithm hasher, byte[] input)
        {
            byte[] data = hasher.ComputeHash(input);
            return GetStringHex(data);
        }

        string GetStringHex(byte[] input)
        {
            StringBuilder sBuilder = new StringBuilder();
            for (int i = 0; i < input.Length; i++)
            {
                sBuilder.Append(input[i].ToString("x2"));
            }
            return sBuilder.ToString();
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
