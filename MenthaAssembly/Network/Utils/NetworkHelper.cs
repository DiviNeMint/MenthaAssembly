using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace System.Net
{
    public class NetworkHelper
    {
        public static IEnumerable<IPAddress> GetInterNetworkIPAddresses()
        {
            foreach (IPAddress Address in GetAllInterNetworkAddresses())
            {
                PingReply Reply = Ping(Address, 1000);
                if (Reply.Status == IPStatus.Success)
                    yield return Reply.Address;
            }

            //ConcurrentCollection<IPAddress> Results = new ConcurrentCollection<IPAddress>();

            //Task GetPingTask(IPAddress IP)
            //    => Task.Run(async () =>
            //    {
            //        PingReply Reply = await Ping(IP, 1000);
            //        if (Reply.Status == IPStatus.Success)
            //            Results.Add(Reply.Address);
            //    });

            //Task[] Tasks = GetAllInterNetworkAddresses().Select(i => GetPingTask(i))
            //                                            .ToArray();
            //Task.WaitAll(Tasks);

            //return Results;
        }

        public static async IAsyncEnumerable<IPAddress> GetInterNetworkIPAddressesAsync()
        {
            foreach (IPAddress Address in GetAllInterNetworkAddresses())
            {
                PingReply Reply = await PingAsync(Address, 1000);
                if (Reply.Status == IPStatus.Success)
                    yield return Reply.Address;
            }
        }

        public static IEnumerable<IPAddress> GetAllInterNetworkAddresses()
        {
            Dictionary<byte[], List<byte>> Datas = new();
            foreach (byte[] LocalBytes in GetLocalhostInterNetworkAddresses().Select(i => i.GetAddressBytes()))
            {
                byte TempByte = LocalBytes[LocalBytes.Length - 1];
                if (Datas.Keys.FirstOrDefault(i => AddressBaseEquals(i, LocalBytes)) is byte[] Key)
                {
                    Datas[Key].Add(TempByte);
                    continue;
                }

                Datas.Add(LocalBytes, new List<byte> { TempByte });
            }

            foreach (KeyValuePair<byte[], List<byte>> Data in Datas)
            {
                byte[] AddressBytes = Data.Key;
                for (int i = 1; i <= byte.MaxValue; i++)
                {
                    byte TempByte = (byte)i;
                    if (Data.Value.Contains(TempByte))
                        continue;

                    AddressBytes[3] = TempByte;
                    yield return new IPAddress(AddressBytes);
                }
            }
        }
        private static bool AddressBaseEquals(byte[] Address1, byte[] Address2)
        {
            if (Address1.Length != Address2.Length)
                return false;

            for (int i = 0; i < Address1.Length - 1; i++)
                if (Address1[i] != Address2[i])
                    return false;

            return true;
        }

        public static IEnumerable<IPAddress> GetLocalhostInterNetworkAddresses()
            => Dns.GetHostAddresses(Dns.GetHostName())
                  .Where(i => i.AddressFamily == AddressFamily.InterNetwork);
        //                    i.AddressFamily == AddressFamily.InterNetworkV6);

        public static IEnumerable<int> GetNetorkMTUv4()
            => NetworkInterface.GetAllNetworkInterfaces()
                               .Where(i => i.NetworkInterfaceType != NetworkInterfaceType.Loopback && i.NetworkInterfaceType != NetworkInterfaceType.Tunnel)
                               .Select(i => i.GetIPProperties().GetIPv4Properties().Mtu);
        public static IEnumerable<int> GetNetorkMTUv6()
            => NetworkInterface.GetAllNetworkInterfaces()
                               .Where(i => i.NetworkInterfaceType != NetworkInterfaceType.Loopback && i.NetworkInterfaceType != NetworkInterfaceType.Tunnel)
                               .Select(i => i.GetIPProperties().GetIPv6Properties().Mtu);

        private static readonly byte[] PingData = new byte[1];
        public static PingReply Ping(IPAddress IPAddress, int Timeout)
        {
            using Ping p = new Ping();
            return p.Send(IPAddress, Timeout, PingData, null);
        }
        public static async Task<PingReply> PingAsync(IPAddress IPAddress, int Timeout)
        {
            using Ping p = new();
            return await p.SendPingAsync(IPAddress, Timeout, PingData, null);
        }

        public static IEnumerable<int> GetActiveTcpPorts()
            => IPGlobalProperties.GetIPGlobalProperties()
                                 .GetActiveTcpListeners()
                                 .Select(i => i.Port);

        public bool CheckAvailableServerPort(int Port)
            => GetActiveTcpPorts().Contains(Port);

    }
}
