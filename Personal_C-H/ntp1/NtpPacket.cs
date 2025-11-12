using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;

namespace ntp1
{
    internal class NtpPacket
    {

        internal static DateTime ToDateTime(byte[] timeMessage)
        {
            ulong intPart = (ulong)timeMessage[40] << 24 | (ulong)timeMessage[41] << 16 | (ulong)timeMessage[42] << 8 | (ulong)timeMessage[43];
            ulong fractPart = (ulong)timeMessage[44] << 24 | (ulong)timeMessage[45] << 16 | (ulong)timeMessage[46] << 8 | (ulong)timeMessage[47];

            var milliseconds = (intPart * 1000) + ((fractPart * 1000) / 0x100000000L);
            var networkDateTime = (new DateTime(1900, 1, 1)).AddMilliseconds((long)milliseconds);
            return networkDateTime;
        }
    }
}
