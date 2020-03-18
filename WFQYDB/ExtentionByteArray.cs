using System;
using System.Collections.Generic;
using System.Text;

namespace WFQYDB
{
    public static class ExtentionByteArray
    {
        //public static string ToHexString(this Span<byte> bytes)
        //{
        //    string r = "";
        //    foreach (var b in bytes)
        //        r += " 0x" + b.ToString("X2");
        //    return r.Trim();
        //}

        public static byte[] Take(this byte[] bytes, int start = 0, int length = -1)
        {
            //int len = length == -1 ? bytes.Length : length;
            int end = start + length;
            end = end < bytes.Length ? end : (bytes.Length - 1);
            int len = end - start;
            var ret = new byte[len];
            
            for (var i = 0; i < len; i++)
                ret[i] = bytes[i + start];
            return ret;
        }

        public static string ToHexString(this byte[] bytes, int start = 0, int length = -1)
        {
            string r = "";

            if ((start >= bytes.Length) || (length < -1))
                return r;

            int len = ((length == -1) || ((start + length) > bytes.Length)) ? bytes.Length - start : length;

            for (var i = 0; i < len; i++)
                r += " 0x" + bytes[start + i].ToString("X2");

            return r.Trim();
        }

        public static string ToMinHexString(this byte[] bytes, int start = 0, int length = -1)
        {
            string r = "";

            if ((start >= bytes.Length)||(length < -1))
                return r;

            int len = ((length == -1) || ((start + length) > bytes.Length)) ? bytes.Length - start : length;
                        
            for (var i = 0; i < len; i++)
                r += " " + bytes[start + i].ToString("X2");

            return r.Trim();
        }
    }
}
