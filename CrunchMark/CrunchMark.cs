using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace CrunchMark
{
    using static Program;

    public class CrunchMark
    {
        public static List<int> HashVolume { get; set; } = new List<int>();

        private static List<CrunchMark> threads = new List<CrunchMark>();
        private static int currentThread = 0;
        private int threadId;
        private static bool toStop = false;
        private RandomNumberGenerator randomPool = RandomNumberGenerator.Create();
        private HashAlgorithm hasher;
        private byte[] source;

        public CrunchMark()
        {
            HashVolume.Add(0);
            threadId = currentThread++;
            threads.Add(this);
            hasher = SHA512.Create();
            source = new byte[8192];
            OnLoadProgress();
        }

        public void Start()
        {
            OnLoadProgress();   
            Task.Run(() =>
            {
                while (!toStop)
                {
                    randomPool.GetBytes(source);
                    var hash = GetHash(hasher, source);
                    lock (HashVolume)
                    {
                        var sum = hash.Sum(x => x);
                        HashVolume[threadId] += sum / (sum * 2) + 1;
                    }
                }
            });
        }

        static string GetHash(HashAlgorithm hasher, byte[] input)
        {
            byte[] data = hasher.ComputeHash(input);
            StringBuilder sBuilder = new StringBuilder();
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }
            return sBuilder.ToString();
        }

        public static void StartAll()
        {
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
