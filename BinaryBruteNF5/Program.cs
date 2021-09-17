using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace BinaryBrute
{
    static class Program
    {
        static void Main(string[] args)
        {
            Console.Title = "BinaryBrute";

            Algorithm algorithm;
            Mode mode;

            string[] hashList;
            ulong countHahes;

            byte[][] inputs = null;
            bool stateLoaded = false;



            //  Check hashes to find
            if (!File.Exists("hashes.txt")) 
            {
                Console.WriteLine("(!) 'hashes.txt' not found.\n");
                Console.ReadKey();
                return;
            }

            else hashList = File.ReadAllLines("hashes.txt");


            //  Load State
            if (File.Exists("saved.txt") && LoadMenu())
            {
                stateLoaded = true;
                var saved = File.ReadAllLines("saved.txt");
                var header = saved.Last().Split(',');

                //  Set inputs saved
                inputs = (from str in saved
                          where !str.Contains(',')
                          select StringToByteArray(str.Replace(" ", ""))).ToArray();


                //  Set algorithm
                algorithm = SelectAlgorithm(header[1]);

                countHahes = (ulong)Convert.ToDecimal(header[2]);

                //  Set mode
                mode = SelectMode(header[3]);

                Console.Clear();
            }

            //  Configure new state
            else
            {
                algorithm = ChooseAlgorithm();
                Console.Clear();
                mode = ChooseMode();
                Console.Clear();
                countHahes = 0;
            }


            //  Select the hashes to find previously loaded
            byte[][] hashesArray = (from str in hashList select StringToByteArray(str)).ToArray();

            //  WordList
            List<byte[]> wordList = new List<byte[]>();

            


            if (stateLoaded)
            {
                if(mode != Mode.WordList)
                {
                    Console.WriteLine("- Data loaded");
                    BruteToolsBase.SetActualInputsArray(inputs);
                    BruteToolsBase.SetCountHashes(countHahes);

                    Console.WriteLine("Initializing with Byte arrays: \n");
                    Console.ForegroundColor = ConsoleColor.DarkYellow;
                    foreach (var hash in BruteToolsBase.GetInputsArray()) Console.WriteLine(ByteArrayToHexString(hash));
                    Console.ResetColor();
                }
            }


            Console.WriteLine("- Working");

            //  Load the wordlist
            if(mode == Mode.WordList)
            {
                Console.WriteLine($"- Reading wordlist.");
                var lines = File.ReadAllLines("wordlist.txt");

                Console.WriteLine("- Deserializing.");
                foreach (var line in lines) wordList.Add(Encoding.UTF8.GetBytes(line));
            }

            //  Run
            if (algorithm == Algorithm.MD5)
            {
                if(mode != Mode.WordList) MD5.Run(hashesArray, mode);

                //else MD5.RunWithWordList(hashesArray, wordList.ToArray());    //  Pending
            }

            else if (algorithm == Algorithm.NTLM)
            {
                if (mode != Mode.WordList) NTLM.Run(hashesArray, mode);
                else NTLM.RunWithWordList(hashesArray, wordList.ToArray());
            }
                
                
                



        }





        private static Algorithm ChooseAlgorithm()
        {
            char[] options = { '1', '2' };

            char opc;
            do
            {
                Console.Clear();

                Console.WriteLine("Select algorithm:\n");
                Console.WriteLine("1) NTLM");
                Console.WriteLine("2) MD5");

                Console.WriteLine("\nOption: ");

                opc = Console.ReadKey().KeyChar;
            } while (!options.Contains(opc));

            switch (opc)
            {
                case '1': return Algorithm.NTLM;
                case '2': return Algorithm.MD5;


                default: return Algorithm.MD5;
            }
        }



        private static Mode ChooseMode()
        {
            char[] options = { '1', '2' , '3', '4' , '5', '6' };

            char opc;
            do
            {
                Console.Clear();

                Console.WriteLine("Select Mode (range of byte def):\n");
                Console.WriteLine("1) Only Numbers (60 - 71)");
                Console.WriteLine("2) AlphaNumeric");
                Console.WriteLine("3) OnlyLetters");
                Console.WriteLine("4) All Chars (32 - 126)");
                Console.WriteLine("5) Full Byte (0 - 255)");
                Console.WriteLine("6) Wordlist");

                Console.WriteLine("\nOption: ");

                opc = Console.ReadKey().KeyChar;
            } while (!options.Contains(opc));
            
            switch (opc)
            {
                case '1': return Mode.OnlyNumbers;
                case '2': return Mode.AlphaNumeric;
                case '3': return Mode.OnlyLetters;

                case '4': return Mode.AllChars;
                case '5': return Mode.FullByte;
                case '6': return Mode.WordList;


                default: return Mode.FullByte;
            }
        }

        private static Mode SelectMode(string opc)
        {
            switch (opc)
            {
                case "OnlyNumbers": return Mode.OnlyNumbers;
                case "AlphaNumeric": return Mode.AlphaNumeric;
                case "OnlyLetters": return Mode.OnlyLetters;

                case "AllChars": return Mode.AllChars;
                case "FullByte": return Mode.FullByte;
                case "WordList": return Mode.WordList;


                default: return Mode.FullByte;
            }
        }

        private static Algorithm SelectAlgorithm(string opc)
        {
            switch (opc)
            {
                case "MD5": return Algorithm.MD5;
                case "NTLM": return Algorithm.NTLM;
                default: return Algorithm.MD5;
            }
        }



        private static bool LoadMenu()
        {
            try
            {
                var saved = File.ReadAllLines("saved.txt");
                var header = saved.Last().Split(',');

                var sAlgorithm = header[1];
                var sCountHashes = header[2];
                var sMode = header[3];

                Console.WriteLine("\nLoad state?\n");
                Console.WriteLine("Algorithm: " + sAlgorithm);
                Console.WriteLine("Mode: " + sMode);
                Console.WriteLine("Hashes calculated: " + sCountHashes);

                Console.WriteLine("\n(Y/N)");

                ConsoleKey opc;
                do
                {
                    opc = Console.ReadKey().Key;
                } while (opc != ConsoleKey.Y && opc != ConsoleKey.N);

                return opc == ConsoleKey.Y;
            }
            catch (Exception)
            {
                Console.Clear();
                Console.WriteLine("Error: save file is corrupt.");
                Console.WriteLine("- Press any key to continue...");
                Console.ReadKey();
                return false;
            }
        }






        public static byte[] StringToByteArray(string hex)
        {
            return Enumerable.Range(0, hex.Length)
                             .Where(x => x % 2 == 0)
                             .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                             .ToArray();
        }

        public static string ByteArrayToHexString(byte[] Bytes)
        {
            StringBuilder Result = new StringBuilder(Bytes.Length * 2);
            string HexAlphabet = "0123456789ABCDEF";

            foreach (byte B in Bytes)
            {
                Result.Append(HexAlphabet[(int)(B >> 4)]);
                Result.Append(HexAlphabet[(int)(B & 0xF)]);
            }

            return Result.ToString();
        }

    }
}
