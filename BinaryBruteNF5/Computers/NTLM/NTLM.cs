using System;
using System.Threading;

namespace BinaryBrute
{
    public class NTLM : BruteToolsBase
    {

        /// <summary>
        /// Principal process that initialize all threads for calculate NTLM hashes
        /// </summary>
        /// <param name="hashes"></param>
        [STAThread]
        public static void Run(byte[][] hashes, Mode byteMode)
        {

            coresCount = Environment.ProcessorCount;
            //coresCount = 2;   //  Debug
            hashesToFind = hashes;
            bool newProcess = inputs4Core == null;

            Thread[] threads = new Thread[coresCount];

            if(newProcess) inputs4Core = new byte[coresCount][];

            Working = true;
            SetTimer();

            algorithm = Algorithm.NTLM;
            mode = byteMode;



            for (int i = 0; i < coresCount; i++)
            {
                int index = i;
                if (newProcess)
                {
                    inputs4Core[index] = new byte[1];
                    InstantiateNextByte4Core(index);
                }

                threads[index] = new Thread(new ParameterizedThreadStart(RunCore));
                threads[index].Priority = ThreadPriority.Highest;
                threads[index].Start(index);
            }

            threads[0].Join();

            Console.ReadKey();
        }




        private static void RunCore(object obj)
        {
            byte[] inputs;
            int CoreID = (int)obj;
            byte[] hash;
            NTLMProcessor ntlm = new NTLMProcessor();

            NextByte nextByte;


            if (mode == Mode.AllChars) nextByte = CalculateNextChar4Core;
            else if (mode == Mode.FullByte) nextByte = CalculateNextByte4Core;  //  TODO: PENDING OPTIMIZE
            else if (mode == Mode.OnlyNumbers) nextByte = CalculateNextNumber4Core; //  TODO: PENDING OPTIMIZE

            else nextByte = CalculateNextByte4Core;

            //  Core process
            while (Working)
            {
                inputs = nextByte(CoreID);

                hash = ntlm.ComputeHash(inputs);

                ExistHash(hashesToFind, hash, inputs);

                countHashes++;
            }
        }

        /// <summary>
        /// synchronous process to calculate hashes from a wordlist
        /// </summary>
        /// <param name="hashes">hashes to find</param>
        /// <param name="WordList"></param>
        public static void RunWithWordList(byte[][] hashes, byte[][] WordList)
        {
            hashesToFind = hashes;
            wordList = WordList;
            Thread[] threads = new Thread[coresCount];


            coresCount = 1;
            RunCoreWordList(0);

            Console.WriteLine("- Finished.");
            Console.WriteLine("- Count hashes calculated: " + countHashes + "/" + wordList.Length);

            Console.ReadKey();

        }


        private static void RunCoreWordList(object obj)
        {
            Console.WriteLine($"core {(int)obj} initializing");

            int i = (int)obj;   //  Index
            int length = wordList.Length;
            byte[] hash = new byte[0];

            NTLMProcessor ntlm = new NTLMProcessor();


            for (; i < length; i+= coresCount)
            {
                try
                {
                    hash = ntlm.ComputeHash(wordList[i]);
                    countHashes++;
                }
                catch(Exception e) { Console.WriteLine($"Error index({i}): {e.Message}"); }

                ExistHash(hashesToFind, hash, wordList[i]);
            }
        }
    }
}
