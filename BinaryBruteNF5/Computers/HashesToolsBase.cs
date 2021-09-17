using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Timers;
using Timer = System.Timers.Timer;

namespace BinaryBrute
{
    /// <summary>
    /// Class from which classes for main processes will inherit
    /// </summary>
    public class BruteToolsBase
    {

        //  Here we use variables instead fields for performance
        protected static ulong countHashes;
        private static ulong hashesPerMinute;

        protected static byte[][] hashesToFind;
        protected static byte[][] inputs4Core;
        protected static byte[][] wordList;

        protected static bool Working;
        protected static int coresCount;


        protected static Algorithm algorithm;
        protected static Mode mode;

        protected static Timer aTimer;

        protected delegate byte[] NextByte(int core);
        //---


        //  Timer function
        protected static void OnTimedEvent(object source, ElapsedEventArgs e)
        {

            hashesPerMinute = countHashes - hashesPerMinute;
            Save();

            Console.WriteLine("\n\n" + DateTime.Now.ToString("HH:mm:ss") +
                              "\tHashes: " + countHashes +
                              "\tHashes in the last min: " + hashesPerMinute + " (" + hashesPerMinute / 60 + " h/s)" +
                              "\tAlgorithm:" + algorithm);

            for (int i = 0; i < coresCount; i++)
            {
                Console.Write("Thread " + i + "\t");
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write(ByteArrayToHexStringSep(inputs4Core[i]) + "\n");
                Console.ResetColor();
            }


            hashesPerMinute = countHashes;

        }

        protected static void SetTimer()
        {
            // Create a timer with one minute interval.
            aTimer = new System.Timers.Timer(60000);
            // Hook up the Elapsed event for the timer. 
            aTimer.Elapsed += OnTimedEvent;
            aTimer.AutoReset = true;
            aTimer.Enabled = true;
        }


        /// <summary>
        /// Save data in "saved.txt" for retrieve later
        /// </summary>
        private static void Save()
        {
            string fileName = "saved.txt";
            string text = string.Empty;

            foreach (byte[] bytes in inputs4Core) text += ByteArrayToHexStringSep(bytes) + "\n";

            text += "," + algorithm + "," + countHashes + "," + mode;

            File.WriteAllText(fileName, text);

        }



        //---

        /// <summary>
        /// Evaluate if exists the hash calculated in the hashes to find
        /// </summary>
        /// <param name="hashesToFind">Array with the hashes to find</param>
        /// <param name="hash">Hash calculated</param>
        /// <param name="value">Value that represent the hash</param>
        protected static void ExistHash(byte[][] hashesToFind, byte[] hash, byte[] value)
        {

            for (int i = 0; i < hashesToFind.Length; i++)
            {
                if (CompareBytes(hashesToFind[i], hash))
                    HashFound(hash, value);
            }
        }

        /// <summary>
        /// Execute when a hash has found for save the value and show on console
        /// </summary>
        /// <param name="hash">hash calculated</param>
        /// <param name="value">brute value</param>
        protected static void HashFound(byte[] hash, byte[] value)
        {
            string txt; //  Report
            string valueStr = ByteArrayToHexStringSep(value); //    Value represented in Hex
            string hashStr = Program.ByteArrayToHexString(hash); //    Hash represented in Hex
            string valuePlainText = Encoding.UTF8.GetString(value); //  Value represented in plain text

            bool error = false;

            do
            {
                try
                {
                    if (File.Exists("found.txt"))
                    {
                        txt = File.ReadAllText("found.txt") + "\n";
                        txt += hashStr + ": " + valueStr + "  - \"" + valuePlainText + "\".";

                        File.WriteAllText("found.txt", txt);
                    }
                    else
                    {
                        txt = hashStr + ": " + valueStr + "  - \"" + valuePlainText + "\".";

                        File.WriteAllText("found.txt", txt);
                    }
                }

                catch (Exception) { error = true; Thread.Sleep(10); }   //  oops
            } while (error);

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Hash found!\n" + hashStr + ": " + valueStr + "  - \"" + valuePlainText + "\".");
            Console.ResetColor();
        }

        /// <summary>
        /// Compare 2 byte[] arrays with the same size
        /// </summary>
        protected static bool CompareBytes(byte[] byte1, byte[] byte2)
        {
            if (byte1[0] != byte2[0]) return false; //  For discart

            for (int i = 1; i < byte1.Length; i++) if (byte1[i] != byte2[i]) return false;

            return true;
        }


        /// <summary>
        /// Compare 2 byte[] arrays with the same size
        /// </summary>
        protected static bool CompareBytes1(byte[] byte1, byte[] byte2)
        {
            for (int i = 0; i < byte1.Length; i++) if (byte1[i] != byte2[i]) return false;

            return true;
        }

        /// <summary>
        /// Compare 2 byte[] arrays 
        /// </summary>
        protected static bool CompareBytes2(byte[] byte1, byte[] byte2)
        {
            //  If compare 2 arrays with different length
            if (byte1.Length != byte2.Length) return false;

            for (int i = 0; i < byte1.Length; i++) if (!byte1[i].Equals(byte2[i])) return false;

            return true;
        }




        //---


        //  From 0 to 255 (256)

        protected static byte[] CalculateNextByte4Core(int coreID)
        {
            for (byte c = 0; c < coresCount; c++)
            {
                //  For each byte
                for (int i = 0; i < inputs4Core[coreID].Length; i++)
                {
                    if (++inputs4Core[coreID][i] > 0) break;
                    else if (i == inputs4Core[coreID].Length - 1)
                        inputs4Core[coreID] = inputs4Core[coreID].Append((byte)0).ToArray();
                }
            }

            return inputs4Core[coreID];
        }





        //  From 32 to 126 (94) (full chars)
        protected static byte[] CalculateNextChar4Core1(int coreID)
        {
            for (byte c = 0; c < coresCount; c++)
            {
                //  For each byte
                for (int i = 0; i < inputs4Core[coreID].Length; i++)
                {
                    if (++inputs4Core[coreID][i] < 126) break;

                    else
                    {
                        inputs4Core[coreID][i] = 32;

                        if (i == inputs4Core[coreID].Length - 1)
                            inputs4Core[coreID] = inputs4Core[coreID].Append((byte)32).ToArray();
                    }

                }
            }

            return inputs4Core[coreID];
        }

        //  From 32 to 126 (94) (full chars)
        protected static byte[] CalculateNextChar4Core0(int coreID)
        {
            bool writed = false;
            byte aux;
            //  For each byte
            for (int i = 0; i < inputs4Core[coreID].Length; i++)
            {
                if (writed) aux = 1;
                else
                {
                    writed = true;
                    aux = (byte)coresCount;
                }

                inputs4Core[coreID][i] += aux;
                if (inputs4Core[coreID][i] < 126) break;

                else
                {
                    inputs4Core[coreID][i] = (byte)(32 + 126 - inputs4Core[coreID][i]);

                    if (i == inputs4Core[coreID].Length - 1)
                        inputs4Core[coreID] = inputs4Core[coreID].Append((byte)32).ToArray();
                }

                writed = true;

            }

            return inputs4Core[coreID];
        }

        //  From 32 to 126 (94) (full chars)
        protected static byte[] CalculateNextChar4Core(int coreID)
        {
            for (int i = 0; i < inputs4Core[coreID].Length; i++)
            {
                
                inputs4Core[coreID][i] += (byte)(i > 0 ? 1 : coresCount );// 8 is padding (in test)
                if (inputs4Core[coreID][i] < 126) break;

                else
                {
                    inputs4Core[coreID][i] = (byte)(32 + 126 - inputs4Core[coreID][i]);

                    if (i == inputs4Core[coreID].Length - 1)
                        inputs4Core[coreID] = inputs4Core[coreID].Append((byte)32).ToArray();
                }

            }

            return inputs4Core[coreID];
        }

        //  From 60 to 71 (10) (only numbers)
        protected static byte[] CalculateNextNumber4Core(int coreID)
        {
            for (byte c = 0; c < coresCount; c++)
            {
                //  For each byte
                for (int i = 0; i < inputs4Core[coreID].Length; i++)
                {
                    if (++inputs4Core[coreID][i] < 72) break;

                    else
                    {
                        inputs4Core[coreID][i] = 60;

                        if (i == inputs4Core[coreID].Length - 1)
                            inputs4Core[coreID] = inputs4Core[coreID].Append((byte)60).ToArray();
                    }

                }
            }

            return inputs4Core[coreID];
        }


        protected static byte[] InstantiateNextByte4Core(int coreID)
        {
            for (byte c = 0; c < coreID; c++)
            {
                //  For each byte
                for (int i = 0; i < inputs4Core[coreID].Length; i++)
                {
                    if (++inputs4Core[coreID][i] != 0) break;
                    else if (i.Equals(inputs4Core[coreID].Length - 1))
                        inputs4Core[coreID] = inputs4Core[coreID].Append((byte)0).ToArray();
                }
            }

            return inputs4Core[coreID];
        }





        protected static byte[] CalculateNextByte(byte[] bytes)
        {
            int i;

            //  For each byte
            for (i = 0; i < bytes.Length; i++)
            {
                if (++bytes[i] != 0) break;
                else if (i == bytes.Length - 1) bytes = bytes.Append((byte)0).ToArray();
            }

            return bytes;
        }



        public static string ByteArrayToHexStringSep(byte[] Bytes)
        {
            StringBuilder Result = new StringBuilder(Bytes.Length * 2);
            string HexAlphabet = "0123456789ABCDEF";

            foreach (byte B in Bytes)
            {
                Result.Append(HexAlphabet[(int)(B >> 4)]);
                Result.Append(HexAlphabet[(int)(B & 0xF)]);

                Result.Append(" ");
            }

            return Result.ToString();
        }



        public static void SetActualInputsArray(byte[][] arrays) 
        {
            inputs4Core = arrays;
        }

        public static void SetCountHashes(ulong count)
        {
            countHashes = count;
        }

        public static byte[][] GetInputsArray() { return inputs4Core; }

    }
}
