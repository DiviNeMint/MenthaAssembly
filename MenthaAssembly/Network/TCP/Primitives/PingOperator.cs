using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using static MenthaAssembly.Network.Primitives.TcpSocketBase;

namespace MenthaAssembly.Network.Primitives
{
    public class PingOperator
    {
        private readonly TcpServer Server;
        private Timer Timer;

        public IPingProvider PingProvider { set; get; }

        private int _Interval = 180000;
        /// <summary>
        /// Interval Mileseconds
        /// </summary>
        public int Interval
        {
            get => _Interval;
            set
            {
                if (Timer is null)
                {
                    _Interval = value;
                    return;
                }

                int Period = Math.Max(Interval >> 3, 1000);
                if (Timer.Change(0, Period))
                {
                    MaxCount = Math.Max(Interval / Period, 5);
                    _Interval = value;
                }
            }
        }

        private bool _Enable;
        public bool Enable
        {
            get => _Enable;
            set
            {
                _Enable = value && Server.Clients.Count > 0;
                if (_Enable)
                {
                    int Period = Math.Max(Interval >> 3, 1000);
                    MaxCount = Math.Max(Interval / Period, 5);

                    Timer = new Timer(OnPingProcess, null, 0, Period);
                }
                else
                {
                    Timer?.Dispose();
                    Timer = null;
                }
            }
        }

        internal PingOperator(TcpServer Server, IPingProvider PingProvider)
        {
            this.Server = Server;
            Server.Connected += OnServerConnected;
            Server.Disconnected += OnServerDisconnected;
            this.PingProvider = PingProvider;
        }

        private void OnServerConnected(object sender, IPEndPoint e)
        {
            if (!Enable)
                Enable = true;
        }

        private void OnServerDisconnected(object sender, IPEndPoint e)
        {
            if (Server.Clients.Count == 0)
                Enable = false;
        }

        private async void OnPingProcess(object state)
        {
            SocketToken[] Clients = Scan().ToArray();
            if (Clients.Length > 0)
                await Ping(Clients, PingProvider.Provide());
        }

        private int MaxCount;
        private IEnumerable<SocketToken> Scan()
        {
            for (int i = Server.Clients.Count - 1; i >= 0; i--)
            {
                SocketToken Token = Server.ClientTokens[i];
                if (++Token.PingCounter < MaxCount)
                    continue;

                Token.PingCounter = 0;
                yield return Token;
            }
        }

        private async Task Ping(IEnumerable<SocketToken> Clients, IMessage Request)
            => await Task.WhenAll(Clients.Select(i => Server.PingAsync(i, Request, Interval)));

    }
}
