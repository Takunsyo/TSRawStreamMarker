using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TSRawStreamMarker.TransportStream.Packets
{
    public class Helper
    {
        public static List<int> NetworkPIDS = new List<int>();
        public static List<int> PATPIDS = new List<int>();

        public static IPSISection TryGetPSISection(TSPacket packet)
        {
            switch (packet.PID)
            {
                case 0x0000:
                    var tmp = new PATPacket(new BitPacket(packet.Payload), packet.IsPayloadEntry);
                    foreach(var i in tmp.Programs)
                    {
                        if (i.ProgramNumber == 0x0000)
                        {
                            if (!NetworkPIDS.Contains(i.PID)) NetworkPIDS.Add(i.PID);
                        }
                        else
                        {
                            if (!PATPIDS.Contains(i.PID)) PATPIDS.Add(i.PID);
                        }
                    }
                    return tmp;
                case 0x0001:
                    var cat = new CATPacket(new BitPacket(packet.Payload), packet.IsPayloadEntry);
                    return cat;
                case 0x0002:
                    var dsp =new DescriptionPacket(new BitPacket(packet.Payload), packet.IsPayloadEntry);
                    return dsp;
                case int x when x >= 0x00010 && x <= 0x1ffe:
                    if (NetworkPIDS.Contains(x))
                    {
                        var a = new PrivatePacket(new BitPacket(packet.Payload), packet.IsPayloadEntry);
                        return a;
                    }
                    if (PATPIDS.Contains(x))
                    {
                        var a = new PMTPacket(new BitPacket(packet.Payload), packet.IsPayloadEntry);
                        return a;
                    }
                    var pkg = new PSIPacket(new BitPacket(packet.Payload), packet.IsPayloadEntry);
                    return pkg;
                    
                default:
                    return null;
                    //Not allawed.
            }
        }
    }
}
