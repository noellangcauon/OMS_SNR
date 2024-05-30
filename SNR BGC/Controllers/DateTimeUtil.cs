using System;

namespace SNR_BGC.Controllers
{
    public static class DateTimeUtil
    {
        public static long ToTimestamp(this DateTime value)
        {
            return ((DateTimeOffset)value).ToUnixTimeSeconds();
        }
    }
}
