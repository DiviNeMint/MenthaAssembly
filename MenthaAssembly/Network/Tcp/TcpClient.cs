using MenthaAssembly.Network.Primitives;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace MenthaAssembly.Network
{
    public class TcpClient : TcpBase
    {
        private const int DefaultConnectTimeout = 3000;
        public event EventHandler<EndPoint> Connected;

        private Session ServerSession;
        public EndPoint Address
            => ServerSession?.Address;

        public TcpClient(ISessionHandler SessionHandler) : base(SessionHandler)
        {
        }

        public bool TryConnect(string Address, int Port)
            => IPAddress.TryParse(Address, out IPAddress IP) &&
               TryConnect(new IPEndPoint(IP, Port), DefaultConnectTimeout);
        public bool TryConnect(IPAddress IPAddress, int Port)
            => TryConnect(new IPEndPoint(IPAddress, Port), DefaultConnectTimeout);
        public bool TryConnect(IPEndPoint Address)
            => TryConnect(Address, DefaultConnectTimeout);
        public virtual bool TryConnect(IPEndPoint Address, int TimeoutMileseconds)
        {
            // Checks Dispose
            if (IsDisposed)
                return false;

            // Reset Last Connecting
            Close();

            // Connect Server
            IOCPSocket e = new(SocketType.Stream, ProtocolType.Tcp);
            if (!e.TryConnect(Address, TimeoutMileseconds))
            {
                e.Dispose();
                return false;
            }

            // Session
            Session Session = new(e);
            ServerSession = Session;

            // Init
            e.Received += (s, e) => OnReceived(Session, e);
            e.Disconnect += (s, e) =>
            {
                // Release Session
                ServerSession = null;

                Debug.WriteLine($"[Info][{GetType().Name}]Disconnected.");
                OnDisconnected(Address);
            };
            e.Receive();

            //// Keep Alive
            //if (_EnableCheckKeepAlive)
            //    SetKeepAlive(e, true, _CheckKeepAliveInterval);

            // Connected Event
            Debug.WriteLine($"[Info][{GetType().Name}]Connect to [{(Address is IPEndPoint IPAddress ? $"{IPAddress.Address}:{IPAddress.Port}" : Address)}].");
            OnConnected(Address);

            return true;
        }

        private void Close()
        {
            // Session
            if (ServerSession != null)
            {
                ServerSession.Socket.Dispose();
                ServerSession = null;
            }
        }

        public IMessage Send(IMessage Request)
            => Send(Request, DefaultSendTimeout);
        public IMessage Send(IMessage Request, int TimeoutMileseconds)
        {
            if (ServerSession is not Session Session)
                return ErrorMessage.NotConnected;

            Task<IMessage> Action;
            lock (Session)
            {
                Action = SendAsync(Session, Request, TimeoutMileseconds);
            }

            Action.Wait();
            return Action.Result;
        }

        public T Send<T>(IMessage Request) where T : IMessage
            => Send<T>(Request, DefaultSendTimeout);
        public T Send<T>(IMessage Request, int TimeoutMileseconds)
            where T : IMessage
        {
            if (ServerSession is not Session Session)
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

        public async Task<IMessage> SendAsync(IMessage Request)
            => await SendAsync(Request, DefaultSendTimeout);
        public async Task<IMessage> SendAsync(IMessage Request, int TimeoutMileseconds)
        {
            if (ServerSession is not Session Session)
                return ErrorMessage.NotConnected;

            Task<IMessage> Action;
            lock (Session)
            {
                Action = SendAsync(Session, Request, TimeoutMileseconds);
            }

            return await Action;
        }

        public async Task<T> SendAsync<T>(IMessage Request) where T : IMessage
            => await SendAsync<T>(Request, 3000);
        public async Task<T> SendAsync<T>(IMessage Request, int TimeoutMileseconds)
            where T : IMessage
        {
            if (ServerSession is not Session Session)
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

        private bool IsDisposed = false;
        public override void Dispose()
        {
            if (IsDisposed)
                return;

            IsDisposed = true;
            Close();
        }

    }
}