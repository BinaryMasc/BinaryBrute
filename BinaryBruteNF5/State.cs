using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BinaryBrute
{
    public class State
    {
        public string[] Hashes4Core { get; set; }
        public ulong CountHashes { get; set; }
        public Algorithm Algorithm { get; set; }
        public Mode Mode { get; set; }


    }



    public enum Algorithm
    {
        MD5,
        NTLM,

        AES
    }


    public enum Mode
    {
        FullByte,
        AllChars,
        OnlyNumbers,
        OnlyLetters,
        AlphaNumeric,
        LowercaseUppercase,
        WordList
    }
}
