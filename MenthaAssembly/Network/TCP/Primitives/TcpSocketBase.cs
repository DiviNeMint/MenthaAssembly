using MenthaAssembly.Utils;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace MenthaAssembly.Network.Primitives
{
    public abstract class TcpSocketBase : IDisposable
    {
        public event EventHandler<IPEndPoint> Disconnected;

        private static readonly ConcurrentQueue<SocketAsyncEventArgs> Pool = new ConcurrentQueue<SocketAsyncEventArgs>();
        private static readonly ConcurrentQueue<byte[]> BufferPool = new ConcurrentQueue<byte[]>();

        public int BufferSize { get; }

        public IProtocolHandler ProtocolHandler { get; }

        public IMessageHandler MessageHandler { get; }

        protected TcpSocketBase(IProtocolHandler Protocol, IMessageHandler MessageHandler, int BufferSize)
        {
            if (MessageHandler is null)
                throw new ArgumentNullException(nameof(MessageHandler));

            this.ProtocolHandler = Protocol;
            this.MessageHandler = MessageHandler;
            this.BufferSize = BufferSize;
        }

        internal protected void Enqueue(ref SocketAsyncEventArgs e)
        {
            e.SocketError = SocketError.Success;
            e.AcceptSocket = null;
            e.UserToken = null;

            // Enqueue Buffer
            BufferPool.Enqueue(e.Buffer);
            e.SetBuffer(null, 0, 0);

            Pool.Enqueue(e);
        }
        internal protected SocketAsyncEventArgs Dequeue()
        {
            if (!Pool.TryDequeue(out SocketAsyncEventArgs e))
            {
                e = new SocketAsyncEventArgs();
                e.Completed += OnIOCompleted;
            }

            byte[] Buffer = DequeueBuffer();
            e.SetBuffer(Buffer, 0, Buffer.Length);

            return e;
        }
        private byte[] DequeueBuffer()
            => BufferPool.TryDequeue(out byte[] Buffer) ? Buffer : new byte[BufferSize];


        internal protected IMessage Send(SocketToken Token, IMessage Request, int TimeoutMileseconds)
        {
            TaskCompletionSource<IMessage> TaskToken = new TaskCompletionSource<IMessage>();
            Token.ResponseTaskSource = TaskToken;

            // Timeout
            CancellationTokenSource CancelToken = new CancellationTokenSource(TimeoutMileseconds);
            CancelToken.Token.Register(() => TaskToken.TrySetResult(ErrorMessage.Timeout), false);
            Token.ResponseCancelToken = CancelToken;

            Token.Lock.Wait(CancelToken.Token);
            if (!CancelToken.IsCancellationRequested)
            {
                if (Request is IIdentityMessage IdentityMessage)
                {
                    Token.LastRequsetUID += 2;
                    IdentityMessage.UID = Token.LastRequsetUID;
                }

                // Encode Message
                Stream MessageStream = ProtocolHandler.Encode(Request);
                Token.MessageEncodeStream = MessageStream;

                if (MessageStream is null)
                {
                    TaskToken.TrySetResult(ErrorMessage.NotSupport);
                    Token.Lock.Release();
                }
                else
                {
                    // Set SendDatas
                    SocketAsyncEventArgs e = Dequeue();
                    e.UserToken = Token;
                    MessageStream.Read(e.Buffer, 0, e.Count);

                    if (!Token.Socket.SendAsync(e))
                        OnSendProcess(e);
                }
            }

            TaskToken.Task.Wait();

            return TaskToken.Task.Result;
        }
        internal protected void Reply(SocketToken Token, IMessage Request, int TimeoutMileseconds)
        {
            // Timeout
            CancellationTokenSource CancelToken = new CancellationTokenSource(TimeoutMileseconds);

            Token.Lock.Wait(CancelToken.Token);
            if (!CancelToken.IsCancellationRequested)
            {
                // Encode Message
                Stream MessageStream = ProtocolHandler.Encode(Request);
                Token.MessageEncodeStream = MessageStream;

                if (MessageStream is null)
                {
                    Token.Lock.Release();
                    return;
                }

                // Set SendDatas
                SocketAsyncEventArgs e = Dequeue();
                e.UserToken = Token;
                MessageStream.Read(e.Buffer, 0, e.Count);

                if (!Token.Socket.SendAsync(e))
                    OnSendProcess(e);
            }
        }

        internal protected async Task<IMessage> SendAsync(SocketToken Token, IMessage Request, int TimeoutMileseconds)
        {
            TaskCompletionSource<IMessage> TaskToken = new TaskCompletionSource<IMessage>();
            Token.ResponseTaskSource = TaskToken;

            // Timeout
            CancellationTokenSource CancelToken = new CancellationTokenSource(TimeoutMileseconds);
            CancelToken.Token.Register(() => TaskToken.TrySetResult(ErrorMessage.Timeout), false);
            Token.ResponseCancelToken = CancelToken;

            await Token.Lock.WaitAsync(CancelToken.Token);
            if (!CancelToken.IsCancellationRequested)
            {
                if (Request is IIdentityMessage IdentityMessage)
                {
                    Token.LastRequsetUID += 2;
                    IdentityMessage.UID = Token.LastRequsetUID;
                }

                // Encode Message
                Stream MessageStream = ProtocolHandler.Encode(Request);
                Token.MessageEncodeStream = MessageStream;

                if (MessageStream is null)
                {
                    TaskToken.TrySetResult(ErrorMessage.NotSupport);
                    Token.Lock.Release();
                }
                else
                {
                    // Set SendDatas
                    SocketAsyncEventArgs e = Dequeue();
                    e.UserToken = Token;
                    MessageStream.Read(e.Buffer, 0, e.Count);

                    if (!Token.Socket.SendAsync(e))
                        OnSendProcess(e);
                }
            }

            return await TaskToken.Task;
        }
        internal protected async Task ReplyAsync(SocketToken Token, IMessage Request, int TimeoutMileseconds)
        {
            // Timeout
            CancellationTokenSource CancelToken = new CancellationTokenSource(TimeoutMileseconds);

            await Token.Lock.WaitAsync(CancelToken.Token);
            if (!CancelToken.IsCancellationRequested)
            {
                // Encode Message
                Stream MessageStream = ProtocolHandler.Encode(Request);
                Token.MessageEncodeStream = MessageStream;

                if (MessageStream is null)
                {
                    Token.Lock.Release();
                    return;
                }

                // Set SendDatas
                SocketAsyncEventArgs e = Dequeue();
                e.UserToken = Token;
                MessageStream.Read(e.Buffer, 0, e.Count);

                if (!Token.Socket.SendAsync(e))
                    OnSendProcess(e);
            }
        }

        private void OnIOCompleted(object sender, SocketAsyncEventArgs e)
        {
            switch (e.LastOperation)
            {
                case SocketAsyncOperation.Receive:
                    OnReceiveProcess(e);
                    break;
                case SocketAsyncOperation.Send:
                    OnSendProcess(e);
                    break;
                default:
                    throw new ArgumentException("The last operation completed on the socket was not a Accept or Receive");
            }
        }

        protected virtual void OnSendProcess(SocketAsyncEventArgs e)
        {
            if (e.UserToken is SocketToken Token)
            {
                if (e.SocketError == SocketError.Success)
                {
                    // Reset Auto Ping Counter.
                    Token.PingCounter = 0;

                    Stream EncodeStream = Token.MessageEncodeStream;

                    // Fill Datas
                    int Length = EncodeStream.Read(e.Buffer, 0, e.Count);
                    if (Length > 0)
                    {
                        Stream Stream = new NetworkStream(Token.Socket);
                        do
                        {
                            Stream.Write(e.Buffer, 0, Length);
                            Length = EncodeStream.Read(e.Buffer, 0, e.Count);
                        }
                        while (Length > 0);

                        Stream.Dispose();
                    }

                    EncodeStream.Dispose();
                    Token.MessageEncodeStream = null;

                    Token.Lock.Release();
                }
                else
                {
                    // Dispose socket so that OnReceiveProcess will trigger OnDisconnect.
                    Token.Socket?.Dispose();
                    Token.Socket = null;
                }
            }

            Enqueue(ref e);
        }

        private Action<SocketToken, IMessage> ReplyHandler;
        protected virtual void OnReceiveProcess(SocketAsyncEventArgs e)
        {
            if (e.UserToken is SocketToken Token)
            {
                // Check Client's Connection Status
                if (e.SocketError == SocketError.Success &&
                    e.BytesTransferred > 0)
                {
                    // Reset Auto Ping Counter.
                    Token.PingCounter = 0;

                    IMessage ReceiveMessage;
                    try
                    {
                        Token.Lock.Wait();

                        // Decode Message
                        ConcatStream s = new ConcatStream(e.Buffer, 0, e.BytesTransferred, new NetworkStream(Token.Socket));
                        ReceiveMessage = ProtocolHandler.Decode(s);
                        s.Dispose();
                    }
                    finally
                    {
                        Token.Lock.Release();
                    }

                    if (ReceiveMessage != null)
                    {
                        if (this.ReplyHandler is null)
                            this.ReplyHandler = OnReplyProcess;

                        ReplyHandler.BeginInvoke(Token, ReceiveMessage, (ar) => ReplyHandler.EndInvoke(ar), null);
                    }

                    // Loop Receive
                    if (!Token.Socket.ReceiveAsync(e))
                        OnReceiveProcess(e);

                    return;
                }

                // Trigger Disconnected Event.
                OnDisconnected(Token);
            }

            // Push Resource to pool.
            Enqueue(ref e);
        }

        private void OnReplyProcess(SocketToken Token, IMessage ReceiveMessage)
        {
            bool IsResponse = true;
            int ReceiveUID = -1;

            if (ReceiveMessage is IIdentityMessage IdentityMessage)
            {
                IsResponse = Token.LastRequsetUID == IdentityMessage.UID;
                ReceiveUID = IdentityMessage.UID;
            }

            if (IsResponse &&
                Token.ResponseTaskSource != null)
            {
                try
                {
                    // Set Response
                    Token.ResponseTaskSource.TrySetResult(ReceiveMessage);
                }
                finally
                {
                    Token.ResponseTaskSource = null;

                    // Release CancelToken
                    Token.ResponseCancelToken?.Dispose();
                    Token.ResponseCancelToken = null;
                }
            }
            else
            {
                // Handle Received Message
                IMessage Response = MessageHandler.HandleMessage(Token.Address, ReceiveMessage);

                // Check Response
                if (Response is null)
                    Response = ErrorMessage.ReceivingNotSupport;

                if (Response is IIdentityMessage IdentityResponse)
                    IdentityResponse.UID = ReceiveUID;

                Stream MessageStream = ProtocolHandler.Encode(Response);
                if (MessageStream is null)
                {
                    Console.WriteLine($"[Warn]{this.ProtocolHandler.GetType().Name} not support {Response.GetType().Name}.");

                    ErrorMessage Error = ErrorMessage.ReceivingNotSupport;
                    Error._UID = ReceiveUID;

                    MessageStream = ErrorMessage.Encode(Error);
                }

                // Replay
                Token.Lock.Wait();

                Token.MessageEncodeStream = MessageStream;

                SocketAsyncEventArgs e = Dequeue();
                e.UserToken = Token;

                // Set SendDatas
                MessageStream.Read(e.Buffer, 0, e.Count);

                if (!Token.Socket.SendAsync(e))
                    OnSendProcess(e);
            }
        }

        protected virtual void OnDisconnected(SocketToken Token)
        {
            if (!IsDisposed &&
                !Token.IsDisposed)
                Disconnected?.Invoke(this, Token.Address);

            Token.Dispose();
        }

        protected bool IsDisposed = false;
        public abstract void Dispose();

        internal protected class SocketToken : IDisposable
        {
            public SemaphoreSlim Lock { get; private set; } = new SemaphoreSlim(1);

            public Socket Socket { get; internal set; }

            public IPEndPoint Address { get; private set; }

            public SocketAsyncEventArgs AsyncArgs { get; private set; }

            public Stream MessageEncodeStream { set; get; }

            /// <summary>
            /// Client : odd
            /// <para/>
            /// Server : even
            /// </summary>
            public int LastRequsetUID { set; get; }

            public TaskCompletionSource<IMessage> ResponseTaskSource { set; get; }

            public CancellationTokenSource ResponseCancelToken { set; get; }

            public int PingCounter { set; get; }

            public SocketToken(Socket Socket, SocketAsyncEventArgs e)
            {
                this.Socket = Socket;
                this.Address = (IPEndPoint)Socket.RemoteEndPoint;
                this.AsyncArgs = e;
                e.UserToken = this;
            }

            internal bool IsDisposed = false;
            public void Dispose()
            {
                if (IsDisposed)
                    return;

                try
                {
                    // Dispose Socket.
                    Socket?.Dispose();
                    Socket = null;
                    Address = null;
                    AsyncArgs = null;

                    // Dispose ResponseTask
                    ResponseTaskSource?.TrySetResult(ErrorMessage.Disconnected);
                    ResponseTaskSource = null;
                    ResponseCancelToken?.Dispose();
                    ResponseCancelToken = null;

                    // Dispose Stream
                    MessageEncodeStream?.Dispose();
                    MessageEncodeStream = null;

                    // Dispose Lock
                    Lock?.Dispose();
                    Lock = null;
                }
                finally
                {
                    IsDisposed = true;
                }
            }

            ~SocketToken()
            {
                Dispose();
            }

        }

    }

}
