using System;
using System.Collections.Generic;
using System.Text;

namespace System
{
    public static class ExtentionDateTime
    {
        public static Int32 ToUnixTimestamp(this DateTime dt)
        {
            // 		(new DateTime(1970, 1, 1)).Ticks	621355968000000000	long
            return (Int32)(dt.ToUniversalTime().Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
        }

        public static Int64 ToUnixTimestampMs(this DateTime dt)
        {
            // 		(new DateTime(1970, 1, 1)).Ticks	621355968000000000	long
            return (Int64)((dt.ToUniversalTime().Subtract(new DateTime(1970, 1, 1))).TotalSeconds*1_000);
        }
    }
}
