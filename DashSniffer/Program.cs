using System;
using System.Configuration;
using System.Linq;
using SharpPcap;
using SharpPcap.LibPcap;

namespace DashSniffer
{
    class Program
    {
        static void Main(string[] args)
        {
            var device = GetDevice();

            Console.WriteLine($"Listening on {device.Name}");

            device.OnPacketArrival += DashEventHandler.Resolve;
            device.Open(DeviceMode.Promiscuous, 1000);
            device.StartCapture();

            Console.ReadLine();

            device.StopCapture();
            device.Close();

        }

        private static LibPcapLiveDevice GetDevice()
        {
            var devices = LibPcapLiveDeviceList.Instance;

            if (devices == null || devices.Count < 1)
                throw new Exception("No devices available to capture on.");

            var preConfiguredDevice = devices.FirstOrDefault(m => string.Equals(m.Name, ConfigurationManager.AppSettings["DeviceName"], StringComparison.OrdinalIgnoreCase));

            if (preConfiguredDevice != null)
                return preConfiguredDevice;

            var i = 0;

            foreach (var dev in devices)
            {
                Console.WriteLine("{0}) {1} {2}", i, dev.Name, dev.Description);
                i++;
            }

            Console.WriteLine();
            Console.Write("listening device number: ");
            i = int.Parse(Console.ReadLine());

            var device = devices[i];
            return device;
        }
    }
}
