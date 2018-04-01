using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace CrunchMark
{
    public class CrunchMark
    {
        public static List<int> HasheResults { get; set; } = new List<int>();

        private static List<CrunchMark> threads = new List<CrunchMark>();
        private static int currentThread = 0;
        private int threadId;
        private static bool toStop = false;
        private RandomNumberGenerator randomPool = RandomNumberGenerator.Create();
        private HashAlgorithm hasher;
        private byte[] source;

        public CrunchMark()
        {
            HasheResults.Add(0);
            threadId = currentThread++;
            threads.Add(this);
            hasher = SHA512.Create();
            source = new byte[8192];
            Program.LoadProgress();
        }

        public void Start()
        {
            Program.LoadProgress();
            Task.Run(() =>
            {
                while (!toStop)
                {
                    randomPool.GetBytes(source);
                    GetMd5Hash(hasher, source);
                    lock (HasheResults)
                    {
                        HasheResults[threadId]++;
                    }
                }
            });
        }

        string GetMd5Hash(HashAlgorithm hasher, byte[] input)
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
            HasheResults = HasheResults.ConvertAll(x => x = 0);
        }
    }
}
