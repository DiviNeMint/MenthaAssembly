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

        internal protected async Task<IMessage> Send(SocketToken Token, IMessage Request, int TimeoutMileseconds)
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
        internal protected async Task Reply(SocketToken Token, IMessage Request, int TimeoutMileseconds)
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
                    while (EncodeStream.Read(e.Buffer, 0, e.Count) > 0)
                    {
                        // Loop Send
                        if (!Token.Socket.SendAsync(e))
                        {
                            OnSendProcess(e);
                            return;
                        }
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

        private Action<SocketToken, IMessage> ReplyAction;
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

                    Token.Lock.Wait();

                    // Decode Message
                    ConcatStream s = new ConcatStream(e.Buffer, 0, e.BytesTransferred, new NetworkStream(Token.Socket));
                    IMessage ReceiveMessage = ProtocolHandler.Decode(s);
                    s.Dispose();

                    if (Token.ResponseTaskSource != null)
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
                            Token.ResponseCancelToken.Dispose();
                            Token.ResponseCancelToken = null;
                        }
                    }
                    else
                    {
                        if (MessageHandler.HandleMessage(Token.Address, ReceiveMessage) is IMessage Response)
                        {
                            if (this.ReplyAction is null)
                                this.ReplyAction = OnReplyProcess;

                            ReplyAction.BeginInvoke(Token, Response, (ar) => ReplyAction.EndInvoke(ar), null);
                        }
                    }

                    Token.Lock.Release();

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

        protected virtual void OnDisconnected(SocketToken Token)
        {
            if (!IsDisposed &&
                !Token.IsDisposed)
                Disconnected?.Invoke(this, Token.Address);

            Token.Dispose();
        }

        private void OnReplyProcess(SocketToken Token, IMessage Response)
        {
            // Check Reply
            if (Response != null &&
                Token.Lock != null)
            {
                Stream MessageStream = ProtocolHandler.Encode(Response);
                if (MessageStream is null)
                {
                    Console.WriteLine($"ProtocolHandler not support {Response.GetType().Name}.");
                    return;
                }

                Token.Lock.Wait();

                Token.MessageEncodeStream = MessageStream;

                SocketAsyncEventArgs e2 = Dequeue();
                e2.UserToken = Token;
                // Set SendDatas
                MessageStream.Read(e2.Buffer, 0, e2.Count);

                if (!Token.Socket.SendAsync(e2))
                    OnSendProcess(e2);
            }
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
