using BinaryBrute.Computers;
using System;
using System.Threading;

namespace BinaryBrute
{
    public class MD5 : BruteToolsBase
    {

        /// <summary>
        /// Principal process that initialize all threads for calculate MD5 hashes
        /// </summary>
        /// <param name="hashes"></param>
        [STAThread]
        public static void Run(byte[][] hashes, Mode byteMode)
        {

            coresCount = Environment.ProcessorCount;
            hashesToFind = hashes;
            bool newProcess = inputs4Core == null;

            Thread[] threads = new Thread[coresCount];


            algorithm = Algorithm.MD5;
            mode = byteMode;

            if(newProcess) inputs4Core = new byte[coresCount][];

            Working = true;
            SetTimer();



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
            //RunCore(0); //  debug

            Console.ReadKey();
        }




        private static void RunCore(object obj)
        {
            byte[] inputs;
            int CoreID = (int)obj;
            byte[] hash;
            MD5Managed md5Manag = new MD5Managed();

            NextByte nextByte;


            if (mode == Mode.AllChars) nextByte = CalculateNextChar4Core;
            else if (mode == Mode.OnlyNumbers) nextByte = CalculateNextNumber4Core;

            else nextByte = CalculateNextByte4Core;

            while (Working)
            {
                inputs = nextByte(CoreID);

                hash = md5Manag.ComputeHash(inputs);

                ExistHash(hashesToFind, hash, inputs);

                countHashes++;
            }
        }



    }
}
