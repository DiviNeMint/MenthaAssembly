﻿using MenthaAssembly.Network.Primitives;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace MenthaAssembly.Network
{
    public class TcpServer : TcpSocket
    {
        public event EventHandler<IPEndPoint> Connected;

        protected readonly ConcurrentObservableCollection<IPEndPoint> ClientKeys = new ConcurrentObservableCollection<IPEndPoint>();
        protected internal readonly ConcurrentObservableCollection<TcpToken> ClientTokens = new ConcurrentObservableCollection<TcpToken>();
        protected internal readonly ConcurrentDictionary<IPEndPoint, TcpToken> PrepareClients = new ConcurrentDictionary<IPEndPoint, TcpToken>();

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

        public int MaxListenCount { set; get; } = 20;

        public override bool IsDisposed => _IsDisposed;

        public IPEndPoint IPEndPoint { get; protected set; }

        public IConnectionValidator ConnectionValidator { set; get; }

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

                foreach (TcpToken Token in ClientTokens.Concat(PrepareClients.Values).ToArray())
                    Token.SetKeepAlive(value, _CheckKeepAliveInterval);
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
                    foreach (TcpToken Token in ClientTokens.Concat(PrepareClients.Values).ToArray())
                        Token.SetKeepAlive(true, value);
            }
        }

        public TcpServer() : this(null, null) { }
        public TcpServer(IConnectionValidator Validator) : this(null, Validator) { }
        public TcpServer(IProtocolCoder Protocol) : this(Protocol, null) { }
        public TcpServer(IProtocolCoder Protocol, IConnectionValidator Validator) : base(Protocol ?? CommonProtocolCoder.Instance)
        {
            ConnectionValidator = Validator;
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
        public void Start(IPEndPoint IPEndPoint)
        {
            try
            {
                // Reset
                Dispose();
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
                this.IPEndPoint = IPEndPoint;
            }
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

        public async Task<IMessage> SendAsync(IPEndPoint Client, IMessage Request)
            => await SendAsync(Client, Request, 3000);
        public async Task<IMessage> SendAsync(IPEndPoint Client, IMessage Request, int TimeoutMileseconds)
            => TryGetToken(Client, out TcpToken Token) ? await base.SendAsync(Token, Request, TimeoutMileseconds) :
                                                         ErrorMessage.NotConnected;

        public async Task<T> SendAsync<T>(IPEndPoint Client, IMessage Request)
            where T : IMessage
            => await SendAsync<T>(Client, Request, 3000);
        public async Task<T> SendAsync<T>(IPEndPoint Client, IMessage Request, int TimeoutMileseconds)
            where T : IMessage
        {
            if (TryGetToken(Client, out TcpToken Token))
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
            => TryGetToken(Client, out TcpToken Token) ? base.Send(Token, Request, TimeoutMileseconds) :
                                                         ErrorMessage.NotConnected;

        public T Send<T>(IPEndPoint Client, IMessage Request)
            where T : IMessage
            => Send<T>(Client, Request, 3000);
        public T Send<T>(IPEndPoint Client, IMessage Request, int TimeoutMileseconds)
            where T : IMessage
        {
            if (TryGetToken(Client, out TcpToken Token))
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

        protected bool TryGetToken(IPEndPoint Key, out TcpToken Token)
        {
            int Index = ClientKeys.IndexOf(Key);
            if (Index > -1)
            {
                Token = ClientTokens[Index];
                return true;
            }

            return PrepareClients.TryGetValue(Key, out Token);
        }

        protected virtual void OnAcceptCompleted(object sender, SocketAsyncEventArgs e)
        {
            if (e.LastOperation != SocketAsyncOperation.Accept)
                throw new ArgumentException("The last operation completed on the socket was not Accepting.");

            OnAcceptProcess(e);
        }

        protected virtual void OnAcceptProcess(SocketAsyncEventArgs e)
        {
            if (e.AcceptSocket is Socket s &&
                s.Connected)
            {
                TcpToken Token = new TcpToken(s, false);
                Token.SetKeepAlive(_EnableCheckKeepAlive, _CheckKeepAliveInterval);

                SocketAsyncEventArgs e2 = Dequeue(true);
                e2.UserToken = Token;

                IPEndPoint ClientAddress = Token.Address;
                if (ConnectionValidator is null)
                {
                    AddClient(ClientAddress, Token);

                    if (!s.ReceiveAsync(e2))
                        OnReceiveProcess(e2);
                }
                else
                {
                    PrepareClients.AddOrUpdate(ClientAddress, Token, (key, i) => Token);

                    if (!s.ReceiveAsync(e2))
                        OnReceiveProcess(e2);

                    bool IsValidated = ConnectionValidator.Validate(this, ClientAddress);

                    PrepareClients.TryRemove(ClientAddress, out _);

                    if (IsValidated)
                        AddClient(ClientAddress, Token);
                    else
                        Token.Dispose();
                }
            }

            // Loop Listen
            if (!_IsDisposed)
                Listen(e);
        }

        protected override void OnDisconnected(TcpToken Token)
        {
            // Remove Client
            RemoveClient(Token.Address, Token);

            // Trigger Disconnected Event.
            base.OnDisconnected(Token);
        }

        protected void AddClient(IPEndPoint Key, TcpToken Token)
        {
            ClientKeys.Add(Key);
            ClientTokens.Add(Token);

            // Trigger Connected Event.
            Connected?.Invoke(this, Key);

            Debug.WriteLine($"[Info][{GetType().Name}]Accept client[{Token.Address}].");
        }
        protected void RemoveClient(IPEndPoint Key, TcpToken Token)
        {
            ClientKeys.Remove(Key);
            ClientTokens.Remove(Token);
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

                // Clients
                ClientKeys.Clear();
                ClientTokens.ForEach(i => i.Dispose());
                ClientTokens.Clear();

                // PrepareClients
                foreach (TcpToken item in PrepareClients.Values.ToArray())
                    item.Dispose();

                PrepareClients.Clear();
            }
            finally
            {
                IsDisposing = false;
                _IsDisposed = true;
            }
        }

        ~TcpServer()
        {
            Dispose();
        }

    }
}
