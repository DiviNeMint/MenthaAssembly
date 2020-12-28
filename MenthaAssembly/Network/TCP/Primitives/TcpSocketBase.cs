using MenthaAssembly.Utils;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
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
        internal protected SocketAsyncEventArgs Dequeue(bool AllocBuffer = true)
        {
            if (!Pool.TryDequeue(out SocketAsyncEventArgs e))
            {
                e = new SocketAsyncEventArgs();
                e.Completed += OnIOCompleted;
            }

            if (AllocBuffer)
            {
                byte[] Buffer = DequeueBuffer();
                e.SetBuffer(Buffer, 0, Buffer.Length);
            }

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

            try
            {
                Token.Lock.Wait(CancelToken.Token);
                if (!CancelToken.IsCancellationRequested)
                {
                    if (Request is IIdentityMessage IdentityMessage)
                    {
                        Token.LastRequsetUID += 2;
                        IdentityMessage.UID = Token.LastRequsetUID;
                    }

                    // Encode Message
                    Stream MessageStream;
                    try
                    {
                        MessageStream = ProtocolHandler.Encode(Request);
                    }
                    catch
                    {
                        Debug.WriteLine($"[Error]Encoding {Request.GetType().Name} hanppen exception.");

                        // Set Result
                        TaskToken.TrySetResult(ErrorMessage.EncodeException);
                        Token.ResponseTaskSource = null;

                        // Release CancelToken
                        Token.ResponseCancelToken?.Dispose();
                        Token.ResponseCancelToken = null;

                        TaskToken.Task.Wait();
                        return TaskToken.Task.Result;
                    }

                    byte[] Buffer = DequeueBuffer();
                    int Length = MessageStream?.Read(Buffer, 0, BufferSize) ?? 0;
                    if (Length == 0)
                    {
                        // Enqueue Buffer
                        BufferPool.Enqueue(Buffer);

                        // Set Result
                        TaskToken.TrySetResult(ErrorMessage.NotSupport);
                        Token.ResponseTaskSource = null;

                        // Release CancelToken
                        Token.ResponseCancelToken?.Dispose();
                        Token.ResponseCancelToken = null;

                        TaskToken.Task.Wait();
                        return TaskToken.Task.Result;
                    }

                    // Send Datas
                    try
                    {
                        do
                        {
                            SocketAsyncEventArgs e = Dequeue(false);
                            e.UserToken = Token;
                            e.SetBuffer(Buffer, 0, Length);

                            if (!Token.Socket.SendAsync(e))
                                OnSendProcess(e);

                            Buffer = DequeueBuffer();
                            Length = MessageStream.Read(Buffer, 0, BufferSize);

                        } while (Length > 0);
                    }
                    catch
                    {
                        // Disconnect
                        Token.Dispose();
                    }
                    finally
                    {
                        MessageStream.Dispose();

                        // Enqueue Last Empty Buffer
                        BufferPool.Enqueue(Buffer);
                    }
                }
            }
            finally
            {
                Token.Lock?.Release();
            }

            TaskToken.Task.Wait();
            return TaskToken.Task.Result;
        }
        internal protected void Reply(SocketToken Token, IMessage Request, int TimeoutMileseconds)
        {
            // Timeout
            CancellationTokenSource CancelToken = new CancellationTokenSource(TimeoutMileseconds);

            try
            {
                Token.Lock.Wait(CancelToken.Token);
                if (!CancelToken.IsCancellationRequested)
                {
                    // Encode Message
                    Stream MessageStream;
                    try
                    {
                        MessageStream = ProtocolHandler.Encode(Request);
                    }
                    catch
                    {
                        Debug.WriteLine($"[Error]Encoding {Request.GetType().Name} hanppen exception.");
                        CancelToken.Dispose();
                        return;
                    }

                    byte[] Buffer = DequeueBuffer();
                    int Length = MessageStream?.Read(Buffer, 0, BufferSize) ?? 0;
                    if (Length == 0)
                    {
                        // Enqueue Buffer
                        BufferPool.Enqueue(Buffer);
                        return;
                    }

                    // Send Datas
                    try
                    {
                        do
                        {
                            SocketAsyncEventArgs e = Dequeue(false);
                            e.UserToken = Token;
                            e.SetBuffer(Buffer, 0, Length);

                            if (!Token.Socket.SendAsync(e))
                                OnSendProcess(e);

                            Buffer = DequeueBuffer();
                            Length = MessageStream.Read(Buffer, 0, BufferSize);

                        } while (Length > 0);
                    }
                    catch
                    {
                        // Disconnect
                        Token.Dispose();
                    }
                    finally
                    {
                        MessageStream.Dispose();

                        // Enqueue Last Empty Buffer
                        BufferPool.Enqueue(Buffer);
                    }
                }
            }
            finally
            {
                Token.Lock?.Release();
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

            try
            {
                await Token.Lock.WaitAsync(CancelToken.Token);
                if (!CancelToken.IsCancellationRequested)
                {
                    if (Request is IIdentityMessage IdentityMessage)
                    {
                        Token.LastRequsetUID += 2;
                        IdentityMessage.UID = Token.LastRequsetUID;
                    }

                    // Encode Message
                    Stream MessageStream;
                    try
                    {
                        MessageStream = ProtocolHandler.Encode(Request);
                    }
                    catch
                    {
                        Debug.WriteLine($"[Error]Encoding {Request.GetType().Name} hanppen exception.");

                        // Set Result
                        TaskToken.TrySetResult(ErrorMessage.EncodeException);
                        Token.ResponseTaskSource = null;

                        // Release CancelToken
                        Token.ResponseCancelToken?.Dispose();
                        Token.ResponseCancelToken = null;

                        return await TaskToken.Task;
                    }

                    byte[] Buffer = DequeueBuffer();
                    int Length = MessageStream?.Read(Buffer, 0, BufferSize) ?? 0;
                    if (Length == 0)
                    {
                        // Enqueue Buffer
                        BufferPool.Enqueue(Buffer);

                        // Set Result
                        TaskToken.TrySetResult(ErrorMessage.NotSupport);
                        Token.ResponseTaskSource = null;

                        // Release CancelToken
                        Token.ResponseCancelToken?.Dispose();
                        Token.ResponseCancelToken = null;

                        return await TaskToken.Task;
                    }

                    // Send Datas
                    try
                    {
                        do
                        {
                            SocketAsyncEventArgs e = Dequeue(false);
                            e.UserToken = Token;
                            e.SetBuffer(Buffer, 0, Length);

                            if (!Token.Socket.SendAsync(e))
                                OnSendProcess(e);

                            Buffer = DequeueBuffer();
                            Length = MessageStream.Read(Buffer, 0, BufferSize);

                        } while (Length > 0);
                    }
                    catch
                    {
                        // Disconnect
                        Token.Dispose();
                    }
                    finally
                    {
                        MessageStream.Dispose();

                        // Enqueue Last Empty Buffer
                        BufferPool.Enqueue(Buffer);
                    }
                }
            }
            finally
            {
                Token.Lock?.Release();
            }

            return await TaskToken.Task;
        }
        internal protected async Task ReplyAsync(SocketToken Token, IMessage Request, int TimeoutMileseconds)
        {
            // Timeout
            CancellationTokenSource CancelToken = new CancellationTokenSource(TimeoutMileseconds);

            try
            {
                await Token.Lock.WaitAsync(CancelToken.Token);
                if (!CancelToken.IsCancellationRequested)
                {
                    // Encode Message
                    Stream MessageStream;
                    try
                    {
                        MessageStream = ProtocolHandler.Encode(Request);
                    }
                    catch
                    {
                        Debug.WriteLine($"[Error]Encoding {Request.GetType().Name} hanppen exception.");
                        CancelToken.Dispose();
                        return;
                    }

                    byte[] Buffer = DequeueBuffer();
                    int Length = MessageStream?.Read(Buffer, 0, BufferSize) ?? 0;
                    if (Length == 0)
                    {
                        // Enqueue Buffer
                        BufferPool.Enqueue(Buffer);
                        return;
                    }

                    // Send Datas
                    try
                    {
                        do
                        {
                            SocketAsyncEventArgs e = Dequeue(false);
                            e.UserToken = Token;
                            e.SetBuffer(Buffer, 0, Length);

                            if (!Token.Socket.SendAsync(e))
                                OnSendProcess(e);

                            Buffer = DequeueBuffer();
                            Length = MessageStream.Read(Buffer, 0, BufferSize);

                        } while (Length > 0);
                    }
                    catch
                    {
                        // Disconnect
                        Token.Dispose();
                    }
                    finally
                    {
                        MessageStream.Dispose();

                        // Enqueue Last Empty Buffer
                        BufferPool.Enqueue(Buffer);
                    }
                }
            }
            finally
            {
                Token.Lock?.Release();
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
                    catch (Exception Ex)
                    {
                        if (!(Ex is IOException IOEx &&
                             IOEx.InnerException is ObjectDisposedException ODEx &&
                             ODEx.ObjectName == typeof(Socket).FullName) &&
                             !(Ex is SocketException))
                            Debug.WriteLine($"[Error]Decode exception.");

                        // Trigger Disconnected Event.
                        OnDisconnected(Token);

                        // Push Resource to pool.
                        Enqueue(ref e);
                        return;
                    }
                    finally
                    {
                        Token.Lock?.Release();
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
                IMessage Response;
                try
                {
                    Response = MessageHandler.HandleMessage(Token.Address, ReceiveMessage);
                }
                catch
                {
                    Response = ErrorMessage.ReceivingHandleException;
                }

                // Check Response
                if (Response is null)
                    Response = ErrorMessage.ReceivingNotSupport;

                if (Response is IIdentityMessage IdentityResponse)
                    IdentityResponse.UID = ReceiveUID;

                try
                {
                    // Replay
                    Token.Lock?.Wait();

                    Stream MessageStream;
                    try
                    {
                        MessageStream = ProtocolHandler.Encode(Response);
                    }
                    catch
                    {
                        ErrorMessage Error = ErrorMessage.ReceivingEncodeException;
                        Error._UID = ReceiveUID;
                        MessageStream = ErrorMessage.Encode(Error);
                    }

                    byte[] Buffer = DequeueBuffer();
                    int Length = MessageStream?.Read(Buffer, 0, BufferSize) ?? 0;
                    if (Length == 0)
                    {
                        Console.WriteLine($"[Warn]{this.ProtocolHandler.GetType().Name} not support {Response.GetType().Name}.");

                        ErrorMessage Error = ErrorMessage.ReceivingNotSupport;
                        Error._UID = ReceiveUID;

                        MessageStream = ErrorMessage.Encode(Error);
                        Length = MessageStream.Read(Buffer, 0, BufferSize);
                    }

                    // Send Datas
                    try
                    {
                        do
                        {
                            SocketAsyncEventArgs e = Dequeue(false);
                            e.UserToken = Token;
                            e.SetBuffer(Buffer, 0, Length);

                            if (!Token.Socket.SendAsync(e))
                                OnSendProcess(e);

                            Buffer = DequeueBuffer();
                            Length = MessageStream.Read(Buffer, 0, BufferSize);
                        } while (Length > 0);
                    }
                    catch
                    {
                        // Disconnect
                        Token.Dispose();
                    }
                    finally
                    {
                        MessageStream.Dispose();

                        // Enqueue Last Empty Buffer
                        BufferPool.Enqueue(Buffer);
                    }
                }
                finally
                {
                    Token.Lock?.Release();
                }
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

            /// <summary>
            /// Client : odd
            /// <para/>
            /// Server : even
            /// </summary>
            public int LastRequsetUID { set; get; }

            public TaskCompletionSource<IMessage> ResponseTaskSource { set; get; }

            public CancellationTokenSource ResponseCancelToken { set; get; }

            public int PingCounter { set; get; }

            public SocketToken(Socket Socket)
            {
                this.Socket = Socket;
                this.Address = (IPEndPoint)Socket.RemoteEndPoint;
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

                    // Dispose ResponseTask
                    ResponseTaskSource?.TrySetResult(ErrorMessage.Disconnected);
                    ResponseTaskSource = null;
                    ResponseCancelToken?.Dispose();
                    ResponseCancelToken = null;

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
