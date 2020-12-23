using MenthaAssembly.Network.Primitives;
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
    public class TcpServer : TcpSocketBase
    {
        public event EventHandler<IPEndPoint> Connected;

        protected readonly ConcurrentObservableCollection<IPEndPoint> ClientKeys = new ConcurrentObservableCollection<IPEndPoint>();
        internal protected readonly ConcurrentObservableCollection<SocketToken> ClientTokens = new ConcurrentObservableCollection<SocketToken>();
        internal protected readonly ConcurrentDictionary<IPEndPoint, SocketToken> PrepareClients = new ConcurrentDictionary<IPEndPoint, SocketToken>();

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

        public IPEndPoint IPEndPoint { protected set; get; }

        public IConnectionValidator ConnectionValidator { get; }

        public PingOperator PingOperator { get; }

        public TcpServer(IMessageHandler Handler) : this(null, Handler) { }
        public TcpServer(IMessageHandler Handler, bool EnableAutoPing) : this(Handler, null, EnableAutoPing) { }
        public TcpServer(IMessageHandler Handler, IConnectionValidator Validator) : this(null, Handler, Validator, null) { }
        public TcpServer(IMessageHandler Handler, IConnectionValidator Validator, bool EnableAutoPing) : this(null, Handler, Validator, EnableAutoPing ? CommonPingProvider.Instance : null) { }
        public TcpServer(IProtocolHandler Protocol, IMessageHandler Handler) : this(Protocol, Handler, null, null) { }
        public TcpServer(IProtocolHandler Protocol, IMessageHandler Handler, IPingProvider PingProvider) : this(Protocol, Handler, null, PingProvider) { }
        public TcpServer(IProtocolHandler Protocol, IMessageHandler Handler, IConnectionValidator Validator) : this(Protocol, Handler, Validator, null) { }
        public TcpServer(IProtocolHandler Protocol, IMessageHandler Handler, IConnectionValidator Validator, IPingProvider PingProvider) : this(Protocol, Handler, Validator, PingProvider, 8192) { }
        public TcpServer(IProtocolHandler Protocol, IMessageHandler Handler, IConnectionValidator Validator, IPingProvider PingProvider, int BufferSize) : base(Protocol ?? CommonProtocolHandler.Instance, Handler, BufferSize)
        {
            this.ConnectionValidator = Validator;

            if (PingProvider != null)
                PingOperator = new PingOperator(this, PingProvider);
        }

        protected Socket Listener;
        public void Start(string Address, int Port)
        {
            if (!IPAddress.TryParse(Address, out IPAddress TempIP))
                throw new Exception($"{this.GetType().Name} Start Error\nAddress may not be correct format.");

            this.Start(new IPEndPoint(TempIP, Port));
        }
        public void Start(IPAddress IPAddress, int Port)
            => this.Start(new IPEndPoint(IPAddress, Port));
        public void Start(IPEndPoint IPEndPoint)
        {
            try
            {
                // Reset
                this.Dispose();
                IsDisposed = false;

                // Create New Listener
                Listener = new Socket(SocketType.Stream, ProtocolType.Tcp);
                Listener.Bind(IPEndPoint);
                Listener.Listen(MaxListenCount);

                Debug.WriteLine($"[Info]{this.GetType().Name} Start at [{IPEndPoint.Address}:{IPEndPoint.Port}].");

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
                    this.OnAcceptProcess(e);
            }
            catch (Exception Ex)
            {
                if (IsDisposing)
                    return;

                Debug.WriteLine($"[Error]{this.GetType().Name}.{nameof(Listen)} {Ex.Message}");
            }
        }

        public async Task<IMessage> SendAsync(IPEndPoint Client, IMessage Request)
            => await SendAsync(Client, Request, 5000);
        public async Task<IMessage> SendAsync(IPEndPoint Client, IMessage Request, int TimeoutMileseconds)
        {
            if (TryGetToken(Client, out SocketToken Token))
                return await base.SendAsync(Token, Request, TimeoutMileseconds);

            return ErrorMessage.ClientNotFound;
        }

        public async Task<T> SendAsync<T>(IPEndPoint Client, IMessage Request)
            where T : IMessage
            => await SendAsync<T>(Client, Request, 3000);
        public async Task<T> SendAsync<T>(IPEndPoint Client, IMessage Request, int TimeoutMileseconds)
            where T : IMessage
        {
            if (TryGetToken(Client, out SocketToken Token))
            {
                IMessage Response = await base.SendAsync(Token, Request, TimeoutMileseconds);
                if (ErrorMessage.Timeout.Equals(Response))
                    throw new TimeoutException();

                if (ErrorMessage.NotSupport.Equals(Response))
                    throw new NotSupportedException();

                return (T)await base.SendAsync(Token, Request, TimeoutMileseconds);
            }

            throw new ClientNotFoundException();
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
            => Send(Client, Request, 5000);
        public IMessage Send(IPEndPoint Client, IMessage Request, int TimeoutMileseconds)
        {
            if (TryGetToken(Client, out SocketToken Token))
                return base.Send(Token, Request, TimeoutMileseconds);

            return ErrorMessage.ClientNotFound;
        }

        public T Send<T>(IPEndPoint Client, IMessage Request)
            where T : IMessage
            => Send<T>(Client, Request, 3000);
        public T Send<T>(IPEndPoint Client, IMessage Request, int TimeoutMileseconds)
            where T : IMessage
        {
            if (TryGetToken(Client, out SocketToken Token))
            {
                IMessage Response = base.Send(Token, Request, TimeoutMileseconds);
                if (ErrorMessage.Timeout.Equals(Response))
                    throw new TimeoutException();

                if (ErrorMessage.NotSupport.Equals(Response))
                    throw new NotSupportedException();

                return (T)base.Send(Token, Request, TimeoutMileseconds);
            }

            throw new ClientNotFoundException();
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

        protected bool TryGetToken(IPEndPoint Key, out SocketToken Token)
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
                SocketAsyncEventArgs e2 = Dequeue();
                SocketToken Token = new SocketToken(s);
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

                    bool IsValidated = ConnectionValidator.Validate(ClientAddress);

                    PrepareClients.TryRemove(ClientAddress, out _);

                    if (IsValidated)
                        AddClient(ClientAddress, Token);
                    else
                        Token.Dispose();
                }
            }

            // Loop Listen
            if (!IsDisposed)
                Listen(e);
        }

        protected override void OnDisconnected(SocketToken Token)
        {
            // Remove Client
            RemoveClient(Token.Address, Token);

            // Trigger Disconnected Event.
            base.OnDisconnected(Token);
        }

        protected void AddClient(IPEndPoint Key, SocketToken Token)
        {
            ClientKeys.Add(Key);
            ClientTokens.Add(Token);

            // Trigger Connected Event.
            Connected?.Invoke(this, Key);
        }
        protected void RemoveClient(IPEndPoint Key, SocketToken Token)
        {
            ClientKeys.Remove(Key);
            ClientTokens.Remove(Token);
        }

        private bool IsDisposing = false;
        public override void Dispose()
        {
            if (IsDisposed)
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
                foreach (SocketToken item in PrepareClients.Values.ToArray())
                    item.Dispose();

                PrepareClients.Clear();
            }
            finally
            {
                IsDisposing = false;
                IsDisposed = true;
            }
        }

        ~TcpServer()
        {
            Dispose();
        }

    }
}
