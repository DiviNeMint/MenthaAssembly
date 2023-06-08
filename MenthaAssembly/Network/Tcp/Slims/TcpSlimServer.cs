using MenthaAssembly.Network.Primitives;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace MenthaAssembly.Network
{
    public class TcpSlimServer : TcpSlimBase
    {
        public event EventHandler<IPEndPoint> Connected;

        private ConcurrentObservableCollection<IPEndPoint> ClientKeys = new();
        private ConcurrentObservableCollection<TcpSlimToken> ClientTokens = new();

        private ReadOnlyCollection<IPEndPoint> _Clients;
        public ReadOnlyCollection<IPEndPoint> Clients
        {
            get
            {
                if (_Clients is null)
                    _Clients = new ReadOnlyConcurrentObservableCollection<IPEndPoint>(ClientKeys);

                return _Clients;
            }
        }

        protected IPEndPoint _Address;
        public IPEndPoint Address
            => _Address;

        public int MaxListenCount { set; get; } = 20;

        private bool _EnableCheckKeepAlive = true;
        private uint _CheckKeepAliveInterval = 180000U;

        /// <summary>
        /// 是否定期檢查連線狀況
        /// </summary>
        public bool EnableCheckKeepAlive
        {
            get => _EnableCheckKeepAlive;
            set
            {
                _EnableCheckKeepAlive = value;

                foreach (TcpSlimToken Token in ClientTokens.ToArray())
                    Token.Stream.SetKeepAlive(value, _CheckKeepAliveInterval);
            }
        }

        /// <summary>
        /// 檢查連線狀況間隔（單位：毫秒）
        /// </summary>
        public uint CheckKeepAliveInterval
        {
            get => _CheckKeepAliveInterval;
            set
            {
                _CheckKeepAliveInterval = value;
                if (_EnableCheckKeepAlive)
                    foreach (TcpSlimToken Token in ClientTokens.ToArray())
                        Token.Stream.SetKeepAlive(true, value);
            }
        }

        public TcpSlimServer() : base(CommonProtocolCoder.Instance)
        {
        }
        public TcpSlimServer(IProtocolCoder Protocol) : base(Protocol) 
        {
        }

        protected Socket Listener;
        public void Start(string Address, int Port)
        {
            if (!IPAddress.TryParse(Address, out IPAddress TempIP))
                throw new Exception($"{GetType().Name} Start Error\nAddress may not be correct format.");

            Start(new IPEndPoint(TempIP, Port));
        }
        public void Start(IPAddress IPAddress, int Port)
            => Start(new IPEndPoint(IPAddress, Port));
        public virtual void Start(IPEndPoint IPEndPoint)
        {
            try
            {
                // Reset
                Stop();
                _IsDisposed = false;

                // Create New Listener
                Listener = new Socket(SocketType.Stream, ProtocolType.Tcp);
                Listener.Bind(IPEndPoint);
                Listener.Listen(MaxListenCount);

                Debug.WriteLine($"[Info][{GetType().Name}]Start at [{IPEndPoint.Address}:{IPEndPoint.Port}].");
                
                // Start Listen
                Listen();
            }
            finally
            {
                _Address = IPEndPoint;
            }
        }

        public virtual void Stop()
        {
            // Listener
            Listener?.Close();
            Listener?.Dispose();
            Listener = null;

            // Clients
            ClientKeys.Clear();
            ClientTokens.ForEach(i => EnqueueToken(i));
            ClientTokens.Clear();
        }

        private void Listen(SocketAsyncEventArgs e = null)
        {
            if (e is null)
            {
                e = new SocketAsyncEventArgs();
                e.Completed += OnAcceptCompleted;
            }

            // Clear LastSocket
            else if (e.AcceptSocket != null)
                e.AcceptSocket = null;

            if (IsDisposing)
                return;

            try
            {
                if (!Listener.AcceptAsync(e))
                    OnAcceptProcess(e);
            }
            catch (Exception Ex)
            {
                if (IsDisposing)
                    return;

                Debug.WriteLine($"[Error][{GetType().Name}]{nameof(Listen)} {Ex.Message}");
            }
        }
        private void OnAcceptCompleted(object sender, SocketAsyncEventArgs e)
        {
            if (e.LastOperation != SocketAsyncOperation.Accept)
                throw new ArgumentException("The last operation completed on the socket was not Accepting.");

            OnAcceptProcess(e);
        }
        private void OnAcceptProcess(SocketAsyncEventArgs e)
        {
            if (e.AcceptSocket is Socket s &&
                s.Connected)
            {
                TcpStream Stream = new TcpStream(s, Pool);
                Stream.SetKeepAlive(_EnableCheckKeepAlive, _CheckKeepAliveInterval);

                // Build Token
                TcpSlimToken Token = DequeueToken();
                PrepareToken(Token, Stream);

                // Events
                void OnStreamDisconnected(IPEndPoint sender, Stream e)
                {
                    RemoveClient(Token.Address);
                    Stream.ReceiveCompleted -= OnStreamReceiveCompleted;
                    Stream.Disconnected -= OnStreamDisconnected;
                }

                void OnStreamReceiveCompleted(IPEndPoint sender, Stream e)
                    => OnReceived(Token, e);

                Stream.ReceiveCompleted += OnStreamReceiveCompleted;
                Stream.Disconnected += OnStreamDisconnected;

                AddClient(Token.Address, Token);

                // Start Receive
                Stream.Receive();
            }

            // Loop Listen
            if (!_IsDisposed)
                Listen(e);
        }

        protected bool TryGetClient(IPEndPoint Address, out TcpSlimToken Token)
        {
            int Index = ClientKeys.IndexOf(Address);
            if (Index > -1)
            {
                Token = ClientTokens[Index];
                return true;
            }

            Token = default;
            return false;
        }
        protected void AddClient(IPEndPoint Address, TcpSlimToken Token)
        {
            ClientKeys.Add(Address);
            ClientTokens.Add(Token);
            OnConnected(Address, Token);
        }
        public void RemoveClient(IPEndPoint Address)
        {
            int Index = ClientKeys.IndexOf(Address);
            if (Index > -1)
            {
                TcpSlimToken Token = ClientTokens[Index];

                ClientKeys.RemoveAt(Index);
                ClientTokens.RemoveAt(Index);

                EnqueueToken(Token);
                OnDisconnected(Address);
            }
        }

        protected override TcpSlimToken CreateToken()
            => new TcpSlimToken(false);
        protected override void PrepareToken(TcpSlimToken Token, TcpStream Stream)
            => Token.Prepare(Stream);
        protected override void ResetToken(TcpSlimToken Token)
            => Token.Clear();

        public async Task<IMessage> SendAsync(IPEndPoint Client, IMessage Request)
            => await SendAsync(Client, Request, 3000);
        public async Task<IMessage> SendAsync(IPEndPoint Client, IMessage Request, int TimeoutMileseconds)
            => TryGetClient(Client, out TcpSlimToken Token) ? await base.SendAsync(Token, Request, TimeoutMileseconds) :
                                                          ErrorMessage.NotConnected;

        public async Task<T> SendAsync<T>(IPEndPoint Client, IMessage Request)
            where T : IMessage
            => await SendAsync<T>(Client, Request, 3000);
        public async Task<T> SendAsync<T>(IPEndPoint Client, IMessage Request, int TimeoutMileseconds)
            where T : IMessage
        {
            if (TryGetClient(Client, out TcpSlimToken Token))
            {
                IMessage Response = await base.SendAsync(Token, Request, TimeoutMileseconds);
                if (ErrorMessage.Timeout.Equals(Response))
                    throw new TimeoutException();

                if (ErrorMessage.NotSupport.Equals(Response))
                    throw new NotSupportedException();

                return (T)await base.SendAsync(Token, Request, TimeoutMileseconds);
            }

            throw new NotConnectedException();
        }

        public async Task<Dictionary<IPEndPoint, IMessage>> SendAsync(IMessage Request)
            => await SendAsync(Clients.ToArray(), Request, 3000);
        public async Task<Dictionary<IPEndPoint, IMessage>> SendAsync(IMessage Request, int TimeoutMileseconds)
            => await SendAsync(Clients.ToArray(), Request, TimeoutMileseconds);
        public async Task<Dictionary<IPEndPoint, IMessage>> SendAsync(IEnumerable<IPEndPoint> Clients, IMessage Request)
            => await SendAsync(Clients, Request, 3000);
        public async Task<Dictionary<IPEndPoint, IMessage>> SendAsync(IEnumerable<IPEndPoint> Clients, IMessage Request, int TimeoutMileseconds)
        {
            Dictionary<IPEndPoint, Task<IMessage>> Result = Clients.ToDictionary(i => i, i => SendAsync(i, Request, TimeoutMileseconds));

            await Task.WhenAll(Result.Values);

            return Result.ToDictionary(i => i.Key, i => Result[i.Key].Result);
        }

        public IMessage Send(IPEndPoint Client, IMessage Request)
            => Send(Client, Request, 3000);
        public IMessage Send(IPEndPoint Client, IMessage Request, int TimeoutMileseconds)
            => TryGetClient(Client, out TcpSlimToken Token) ? base.Send(Token, Request, TimeoutMileseconds) :
                                                          ErrorMessage.NotConnected;

        public T Send<T>(IPEndPoint Client, IMessage Request)
            where T : IMessage
            => Send<T>(Client, Request, 3000);
        public T Send<T>(IPEndPoint Client, IMessage Request, int TimeoutMileseconds)
            where T : IMessage
        {
            if (TryGetClient(Client, out TcpSlimToken Token))
            {
                IMessage Response = base.Send(Token, Request, TimeoutMileseconds);
                if (ErrorMessage.Timeout.Equals(Response))
                    throw new TimeoutException();

                if (ErrorMessage.NotSupport.Equals(Response))
                    throw new NotSupportedException();

                return (T)base.Send(Token, Request, TimeoutMileseconds);
            }

            throw new NotConnectedException();
        }

        public Dictionary<IPEndPoint, IMessage> Send(IMessage Request)
            => Send(Clients.ToArray(), Request, 3000);
        public Dictionary<IPEndPoint, IMessage> Send(IMessage Request, int TimeoutMileseconds)
            => Send(Clients.ToArray(), Request, TimeoutMileseconds);
        public Dictionary<IPEndPoint, IMessage> Send(IEnumerable<IPEndPoint> Clients, IMessage Request)
            => Send(Clients, Request, 3000);
        public Dictionary<IPEndPoint, IMessage> Send(IEnumerable<IPEndPoint> Clients, IMessage Request, int TimeoutMileseconds)
        {
            Dictionary<IPEndPoint, Task<IMessage>> Result = Clients.ToDictionary(i => i, i => SendAsync(i, Request, TimeoutMileseconds));

            Task.WhenAll(Result.Values).Wait();

            return Result.ToDictionary(i => i.Key, i => Result[i.Key].Result);
        }

        protected virtual void OnConnected(IPEndPoint Address, TcpSlimToken Token)
        {
            // Trigger Connected Event.
            Connected?.Invoke(this, Address);
            Debug.WriteLine($"[Info][{GetType().Name}]Accept client[{Address}].");
        }
        protected override void OnDisconnected(IPEndPoint Address)
        {
            // Trigger Disconnected Event.
            base.OnDisconnected(Address);
            Debug.WriteLine($"[Info][{GetType().Name}]Client[{Address}] is disconnected.");
        }

        private bool _IsDisposed = false;
        private bool IsDisposing = false;
        public override void Dispose()
        {
            if (_IsDisposed)
                return;

            IsDisposing = true;

            try
            {
                // Listener
                Listener?.Close();
                Listener?.Dispose();
                Listener = null;

                _Address = null;

                // Clients
                ClientKeys.Clear();
                ClientKeys = null;

                ClientTokens.ForEach(i => ResetToken(i));
                ClientTokens.Clear();
                ClientTokens = null;

                base.Dispose();
            }
            finally
            {
                IsDisposing = false;
                _IsDisposed = true;
            }
        }

    }
}
