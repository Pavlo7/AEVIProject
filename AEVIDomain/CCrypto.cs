using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace AEVIDomain
{
    public static class CCrypto
    {
        public static string Bin2Hex(byte[] buf, int offset, int len)
        {
            StringBuilder bld = new StringBuilder();
            for (int i = offset; i < len; i++) bld.Append(buf[i + offset].ToString("X2"));
            return bld.ToString();
        }
        public static string Bin2Hex(byte[] buf) { return Bin2Hex(buf, 0, buf.Length); }

        public static int ToInt(char c)
        {
            if (c >= '0' && c <= '9') return c - '0';
            if (c >= 'a' && c <= 'f') return c - 'a' + 10;
            if (c >= 'A' && c <= 'F') return c - 'A' + 10;
            return 0;
        }
        public static void Hex2Bin(string str, byte[] dest)
        {
            for (int i = 0; i < str.Length; i += 2) dest[i / 2] = (byte)(ToInt(str[i]) * 16 + ToInt(str[i + 1]));
        }
        public static byte[] Hex2Bin(string str)
        {
            byte[] dest = new byte[str.Length / 2];
            Hex2Bin(str, dest);
            return dest;
        }

        static string PEncKey = "CCCCE60D0E97880803B96E2EB6CA2167";
        static string PEncIV = "0000000500000000";
        
        public static string EncryptPAN(string PAN)
        {
            System.Security.Cryptography.SymmetricAlgorithm alg = System.Security.Cryptography.TripleDES.Create();
            alg.KeySize = 128;
            alg.Key = Hex2Bin(PEncKey);
            alg.IV = Hex2Bin(PEncIV);
            alg.Padding = System.Security.Cryptography.PaddingMode.None;
            alg.Mode = System.Security.Cryptography.CipherMode.CBC;

            try
            {
                MemoryStream outs = new MemoryStream();
                System.Security.Cryptography.CryptoStream encStream = new System.Security.Cryptography.CryptoStream(outs, alg.CreateEncryptor(), System.Security.Cryptography.CryptoStreamMode.Write);
                encStream.Write(Hex2Bin(PAN.PadRight(32, 'A')), 0, 16);
                encStream.FlushFinalBlock();
                byte[] buf = new byte[16];
                Buffer.BlockCopy(outs.GetBuffer(), 0, buf, 0, 16);
                encStream.Close();
                return Bin2Hex(buf);
            }
            catch { }
            return null;
        }

        public static string DecryptPAN(string encryptedPAN)
        {
          //  Log log = new Log(LogPath);
            System.Security.Cryptography.SymmetricAlgorithm alg = System.Security.Cryptography.TripleDES.Create();
            alg.KeySize = 128;
            alg.Key = Hex2Bin(PEncKey);
            alg.IV = Hex2Bin(PEncIV);
            alg.Padding = System.Security.Cryptography.PaddingMode.None;
            alg.Mode = System.Security.Cryptography.CipherMode.CBC;

            byte[] buf = new byte[16];
            Hex2Bin(encryptedPAN, buf);
            try
            {
                MemoryStream outs = new MemoryStream();
                System.Security.Cryptography.CryptoStream encStream = new System.Security.Cryptography.CryptoStream(outs, alg.CreateDecryptor(), System.Security.Cryptography.CryptoStreamMode.Write);
                encStream.Write(buf, 0, 16);
                encStream.FlushFinalBlock();
                Buffer.BlockCopy(outs.GetBuffer(), 0, buf, 0, 16);
                encStream.Close();
                return Bin2Hex(buf).Trim('A');
            }
            catch { }
            return null;
        }
    }
}
