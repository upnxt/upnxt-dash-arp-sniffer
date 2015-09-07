using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using PacketDotNet;
using SharpPcap;

namespace DashSniffer
{
    public class DashEventHandler
    {
        public static void Resolve(object sender, CaptureEventArgs e)
        {
            var result = Packet.ParsePacket(e.Packet.LinkLayerType, e.Packet.Data);
            var packet = result as EthernetPacket;

            if (packet?.Type == EthernetPacketType.Arp)
                PostDashEvent(packet);
        }

        private static void PostDashEvent(EthernetPacket packet)
        {
            if (((ARPPacket)packet.PayloadPacket).SenderProtocolAddress.ToString() != "0.0.0.0")
                return;

            var macAddress = packet.SourceHwAddress.ToString();
            var button = ConfigurationManager.AppSettings.AllKeys.SingleOrDefault(m => m.Contains(macAddress));

            if (button == null)
                return;

            var client = new HttpClient();
            var values = new Dictionary<string, string>
                {
                    {"Event", ConfigurationManager.AppSettings[button] },
                    {"MacAddress", macAddress },
                    {"CreatedOn", DateTime.Now.ToString() }
                };

            var data = new FormUrlEncodedContent(values);
            client.PostAsync("http://localhost:56719/your/api/url/here", data).ContinueWith(task =>
            {
                client.Dispose();
            });
        }
    }
}
