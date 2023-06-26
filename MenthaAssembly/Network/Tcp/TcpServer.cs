using MenthaAssembly.Network.Primitives;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace MenthaAssembly.Network
{
    public class TcpServer : TcpBase
    {
        public event EventHandler<EndPoint> Connected;

        private readonly ConcurrentDictionary<EndPoint, Session> Sessions = new();

        private EndPoint _Address;
        public EndPoint Address
            => _Address;

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

                foreach (Session Session in Sessions.Values.ToArray())
                    SetKeepAlive(Session.Socket, value, _CheckKeepAliveInterval);
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
                    foreach (Session Session in Sessions.Values.ToArray())
                        SetKeepAlive(Session.Socket, true, value);
            }
        }

        public TcpServer(ISessionHandler SessionHandler) : base(SessionHandler)
        {
        }

        private IOCPSocket Listener;
        public void Start(string Address, int Port)
        {
            if (!IPAddress.TryParse(Address, out IPAddress TempIP))
                throw new Exception($"{GetType().Name} Start Error\nAddress may not be correct format.");

            Start(new IPEndPoint(TempIP, Port));
        }
        public void Start(IPAddress IPAddress, int Port)
            => Start(new IPEndPoint(IPAddress, Port));
        public void Start(EndPoint Address)
        {
            // Checks Dispose
            if (IsDisposed)
                return;

            // Reset
            Stop();

            // Start
            try
            {
                OnStart(Address);

                // Create Listener
                Listener = new(SocketType.Stream, ProtocolType.Tcp);
                Listener.Bind(Address);
                Listener.Listen();
                Debug.WriteLine($"[Info][{GetType().Name}]Start at [{(Address is IPEndPoint IPAddress ? $"{IPAddress.Address}:{IPAddress.Port}" : Address)}].");

                // Start Listen
                Listener.Accepted += OnListenerAccepted;
                Listener.Accept();
            }
            finally
            {
                _Address = Address;
            }
        }

        public void Stop()
        {
            OnStop();

            // Address
            _Address = null;

            // Listener
            if (Listener != null)
            {
                Listener.Dispose();
                Listener = null;
            }

            // Sessions
            foreach (Session Session in Sessions.Values.ToArray())
                Session.Socket.Dispose();

            Sessions.Clear();
        }

        public IMessage Send(EndPoint Client, IMessage Request)
            => Send(Client, Request, DefaultSendTimeout);
        public IMessage Send(EndPoint Client, IMessage Request, int TimeoutMileseconds)
        {
            if (!Sessions.TryGetValue(Client, out Session Session))
                return ErrorMessage.NotConnected;

            Task<IMessage> Action;
            lock (Session)
            {
                Action = SendAsync(Session, Request, TimeoutMileseconds);
            }

            Action.Wait();
            return Action.Result;
        }

        public T Send<T>(EndPoint Client, IMessage Request) where T : IMessage
            => Send<T>(Client, Request, DefaultSendTimeout);
        public T Send<T>(EndPoint Client, IMessage Request, int TimeoutMileseconds)
            where T : IMessage
        {
            if (!Sessions.TryGetValue(Client, out Session Session))
                throw new NotConnectedException();

            Task<IMessage> Action;
            lock (Session)
            {
                Action = SendAsync(Session, Request, TimeoutMileseconds);
            }

            Action.Wait();

            IMessage Response = Action.Result;
            return ErrorMessage.Timeout.Equals(Response) ? throw new TimeoutException() :
                   ErrorMessage.NotSupport.Equals(Response) ? throw new NotSupportedException() :
                   (T)Response;
        }

        public Dictionary<EndPoint, IMessage> Send(IMessage Request)
            => Send(Sessions.Values.ToArray(), Request, DefaultSendTimeout);
        public Dictionary<EndPoint, IMessage> Send(IMessage Request, int TimeoutMileseconds)
            => Send(Sessions.Values.ToArray(), Request, TimeoutMileseconds);
        public Dictionary<EndPoint, IMessage> Send(IEnumerable<EndPoint> Clients, IMessage Request)
            => Send(Clients, Request, DefaultSendTimeout);
        public Dictionary<EndPoint, IMessage> Send(IEnumerable<EndPoint> Clients, IMessage Request, int TimeoutMileseconds)
            => Send(Clients.Select(i => Sessions.TryGetValue(i, out Session Session) ? Session : null).Where(i => i != null), Request, TimeoutMileseconds);
        protected Dictionary<EndPoint, IMessage> Send(IEnumerable<Session> Clients, IMessage Request, int TimeoutMileseconds)
        {
            Dictionary<EndPoint, Task<IMessage>> Result = Clients.ToDictionary(i => i.Address,
                                                                               i =>
                                                                               {
                                                                                   lock (i)
                                                                                   {
                                                                                       return SendAsync(i, Request, TimeoutMileseconds);
                                                                                   }
                                                                               });

            Task.WhenAll(Result.Values).Wait();

            return Result.ToDictionary(i => i.Key, i => Result[i.Key].Result);
        }

        public async Task<IMessage> SendAsync(EndPoint Client, IMessage Request)
            => await SendAsync(Client, Request, DefaultSendTimeout);
        public async Task<IMessage> SendAsync(EndPoint Client, IMessage Request, int TimeoutMileseconds)
        {
            if (!Sessions.TryGetValue(Client, out Session Session))
                return ErrorMessage.NotConnected;

            Task<IMessage> Action;
            lock (Session)
            {
                Action = SendAsync(Session, Request, TimeoutMileseconds);
            }

            return await Action;
        }

        public async Task<T> SendAsync<T>(EndPoint Client, IMessage Request) where T : IMessage
            => await SendAsync<T>(Client, Request, DefaultSendTimeout);
        public async Task<T> SendAsync<T>(EndPoint Client, IMessage Request, int TimeoutMileseconds)
            where T : IMessage
        {
            if (!Sessions.TryGetValue(Client, out Session Session))
                throw new NotConnectedException();

            Task<IMessage> Action;
            lock (Session)
            {
                Action = SendAsync(Session, Request, TimeoutMileseconds);
            }

            IMessage Response = await Action;
            return ErrorMessage.Timeout.Equals(Response) ? throw new TimeoutException() :
                   ErrorMessage.NotSupport.Equals(Response) ? throw new NotSupportedException() :
                   (T)Response;
        }

        public async Task<Dictionary<EndPoint, IMessage>> SendAsync(IMessage Request)
            => await SendAsync(Sessions.Values.ToArray(), Request, DefaultSendTimeout);
        public async Task<Dictionary<EndPoint, IMessage>> SendAsync(IMessage Request, int TimeoutMileseconds)
            => await SendAsync(Sessions.Values.ToArray(), Request, TimeoutMileseconds);
        public async Task<Dictionary<EndPoint, IMessage>> SendAsync(IEnumerable<EndPoint> Clients, IMessage Request)
            => await SendAsync(Clients, Request, DefaultSendTimeout);
        public async Task<Dictionary<EndPoint, IMessage>> SendAsync(IEnumerable<EndPoint> Clients, IMessage Request, int TimeoutMileseconds)
            => await SendAsync(Clients.Select(i => Sessions.TryGetValue(i, out Session Session) ? Session : null).Where(i => i != null), Request, TimeoutMileseconds);
        protected async Task<Dictionary<EndPoint, IMessage>> SendAsync(IEnumerable<Session> Clients, IMessage Request, int TimeoutMileseconds)
        {
            Dictionary<EndPoint, Task<IMessage>> Result = Clients.ToDictionary(i => i.Address,
                                                                               i =>
                                                                               {
                                                                                   lock (i)
                                                                                   {
                                                                                       return SendAsync(i, Request, TimeoutMileseconds);
                                                                                   }
                                                                               });

            await Task.WhenAll(Result.Values);

            return Result.ToDictionary(i => i.Key, i => Result[i.Key].Result);
        }

        private void OnListenerAccepted(object sender, IOCPSocket e)
        {
            // Session
            Session Session = new(e);
            EndPoint Address = Session.Address;
            Sessions.AddOrUpdate(Address, Session, (k, v) => Session);

            // Init
            e.Received += (s, e) => OnReceived(Session, e);
            e.Disconnected += (s, e) =>
            {
                // Release Session
                Sessions.TryRemove(Address, out _);

                //if (ClientDatas.TryRemove(Address, out Session Session))
                //{
                //    foreach (TaskCompletionSource<IMessage> Operator in Session.ResponseOperators.Values.ToArray().Select(i=>i.Item1))
                //        Operator.TrySetResult(ErrorMessage.Disconnected);

                //    Session.ResponseOperators.Clear();
                //}


                Debug.WriteLine($"[Info][{GetType().Name}]Client[{Address}] is disconnected.");
                OnDisconnected(Address);
            };
            e.Receive();

            // Verify
            if (!VerifyConnection(Address))
            {
                Sessions.TryRemove(Address, out _);
                e.Dispose();
                return;
            }

            // Keep Alive
            if (_EnableCheckKeepAlive)
                SetKeepAlive(e, true, _CheckKeepAliveInterval);

            // Connected Event
            Debug.WriteLine($"[Info][{GetType().Name}]Accept client[{(Address is IPEndPoint IPAddress ? $"{IPAddress.Address}:{IPAddress.Port}" : Address)}].");
            OnConnected(Address);

            // Loop Accept
            Listener.Accept();
        }

        protected virtual void OnStart(EndPoint Address)
        {
        }

        protected virtual void OnStop()
        {
        }

        protected virtual bool VerifyConnection(EndPoint Address)
            => true;

        protected virtual void OnConnected(EndPoint Address)
            => Connected?.Invoke(this, Address);

        private void OnReceived(Session Session, Stream Stream)
        {
            IMessage Message;
            int UID;
            bool Reply;
            try
            {
                // Header
                SessionHandler.DecodeHeader(Stream, out UID, out Reply);

                // Message
                Message = SessionHandler.DecodeMessage(Stream);
                Debug.WriteLine($"[Info][{GetType().Name}]Receive {Message?.GetType().Name ?? "Unknown Message"} from [{Session.Address}].");
            }
            catch (Exception Ex)
            {
                if (!(Ex is IOException IOEx &&
                     IOEx.InnerException is ObjectDisposedException ODEx &&
                     ODEx.ObjectName == typeof(Socket).FullName) &&
                     Ex is not SocketException)
                    Debug.WriteLine($"[Error][{GetType().Name}]Decode exception.");

                Session.Socket.Dispose();
                return;
            }

            // Received Event
            if (Reply)
                Task.Run(() => OnReceived(new(this, Session, UID, Message)));

            // Reply Message
            else if (Session.ResponseOperators.TryRemove(UID, out Tuple<TaskCompletionSource<IMessage>, CancellationTokenSource> Operator))
                Operator.Item1.TrySetResult(Message);

            // Loop Receive
            Session.Socket.Receive();
        }

        private unsafe void SetKeepAlive(IOCPSocket Socket, bool Enable, uint Interval)
        {
            if (Socket is null)
                return;

            byte[] Data = new byte[sizeof(TcpKeepAlive)];
            fixed (byte* pData = &Data[0])
            {
                *(TcpKeepAlive*)pData = new TcpKeepAlive
                {
                    Enable = Enable,
                    Time = Interval,
                    Interval = MathHelper.Clamp(Interval / 8U, 100U, 1000U)
                };
            }

            Socket.IOControl(IOControlCode.KeepAliveValues, Data, null);
        }

        private bool IsDisposed = false;
        public override void Dispose()
        {
            if (IsDisposed)
                return;

            IsDisposed = true;
            Stop();
        }

    }
}