using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Assets.Scripts.UpdateService
{
    public static class Utils
    {
        public static string ToAsciiString(this byte[] bs)
        {
            if (bs == null || bs.Length == 0)
            {
                return null;
            }

            //var sb = new StringBuilder(bs.Length);
            //foreach (var b in bs)
            //    sb.Append(b.ToString("x2"));

            //return sb.ToString();
            return Encoding.ASCII.GetString(bs);
        }

        #region 计算MD5
        // https://stackoverflow.com/questions/10520048/calculate-md5-checksum-for-a-file
        // https://stackoverflow.com/questions/11454004/calculate-a-md5-hash-from-a-string
        // https://docs.microsoft.com/en-us/dotnet/api/system.security.cryptography.md5?redirectedfrom=MSDN&view=netframework-4.7.2
        private static StringBuilder hashsb = new StringBuilder(32);
        public static string CalcStreamMD5(Stream s)
        {
            hashsb.Clear();

            var md5 = MD5.Create();
            var hash = md5.ComputeHash(s);
            foreach (var bt in hash)
                hashsb.Append(bt.ToString("x2"));

            return hashsb.ToString();
        }

        public static string CalcStreamMD5Another(Stream s)
        {
            using (var md5 = MD5.Create())
            {
                var hash = md5.ComputeHash(s);
                return BitConverter.ToString(hash).Replace("-", string.Empty).ToLowerInvariant();
            }
        }
        #endregion
    }
}
