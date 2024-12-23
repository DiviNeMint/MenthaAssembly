using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace MenthaAssembly.Network
{
    public sealed class IOCPSocket : IDisposable
    {
        public event EventHandler<IOCPSocket> Accepted;

        public event EventHandler<Stream> Received;

        public event EventHandler Disconnected;

        public AddressFamily AddressFamily
            => Socket.AddressFamily;

        public SocketType SocketType
            => Socket.SocketType;

        public ProtocolType ProtocolType
            => Socket.ProtocolType;

        public EndPoint LocalEndPoint
            => Socket.LocalEndPoint;

        public EndPoint RemoteEndPoint
            => Socket.RemoteEndPoint;

        public int BufferSize { set; get; } = 4096;

        private readonly Socket Socket;
        private IOCPSocket(Socket Socket)
        {
            this.Socket = Socket;
        }
        public IOCPSocket(SocketType socketType, ProtocolType protocolType) :
            this(new Socket(socketType, protocolType))
        {

        }
        public IOCPSocket(AddressFamily addressFamily, SocketType socketType, ProtocolType protocolType) :
            this(new Socket(addressFamily, socketType, protocolType))
        {

        }

        public void Bind(EndPoint Address)
            => Socket.Bind(Address);

        public void Listen()
            => Socket.Listen(int.MaxValue);

        public void Accept()
        {
            SocketAsyncEventArgs e = DequeueOperator();
            InternalAccept(e);
        }
        private void InternalAccept(SocketAsyncEventArgs e)
        {
            if (IsDisposed)
            {
                EnqueueOperator(e);
                return;
            }

            e.AcceptSocket = null;
            if (!Socket.AcceptAsync(e))
                OnAcceptProcess(e);
        }


        public bool TryConnect(EndPoint Address)
            => TryConnect(Address, new CancellationTokenSource(5000).Token);
        public bool TryConnect(EndPoint Address, int Timeout)
            => TryConnect(Address, new CancellationTokenSource(Timeout).Token);
        public bool TryConnect(EndPoint Address, CancellationToken Token)
        {
            // Checks Dispose
            if (IsDisposed)
                return false;

            // Checks Connect Status
            if (Socket.Connected)
                return false;

            // Token
            TaskCompletionSource<bool> ConnectToken = new();

            // Operator
            SocketAsyncEventArgs e = DequeueOperator();
            e.UserToken = ConnectToken;
            e.RemoteEndPoint = Address;

            // Timeout
            CancellationTokenSource CancelToken = CancellationTokenSource.CreateLinkedTokenSource(Token);
            CancelToken.Token.Register(() =>
            {
                if (ConnectToken.TrySetResult(false))
                    Socket.CancelConnectAsync(e);
            }, false);

            try
            {
                // Connect
                if (Socket.ConnectAsync(e))
                    ConnectToken.Task.Wait();
                else
                    OnConnectProcess(e);

                return ConnectToken.Task.Result;
            }
            finally
            {
                // Release Operator
                EnqueueOperator(e);

                // Release Cancel Token
                CancelToken.Dispose();
            }
        }

        private IOCPToken Token;
        public void Receive()
        {
            // Checks Dispose
            if (IsDisposed)
                return;

            // Token
            IOCPToken Token = Socket.Connected ? this.Token : new();
            if (Token is null)
            {
                Token = new();
                this.Token = Token;
            }

            // Receives
            if (Token.Buffer is null)
            {
                // Operator
                SocketAsyncEventArgs e = DequeueOperator();
                e.UserToken = Token;

                // Buffer
                int Length = BufferSize;
                byte[] Buffer = ArrayPool<byte>.Shared.Rent(Length);
                e.SetBuffer(Buffer, 0, Length);

                // Receives
                if (!Socket.ReceiveAsync(e))
                    OnReceiveProcess(e);
            }

            // Last Receive
            else
            {
                OnReceived(null);
            }
        }

        public int Receive(byte[] Buffer, int Offset, int Length)
        {
            // Checks Dispose
            if (IsDisposed)
                return 0;

            if (Socket.Connected &&
                Token?.Buffer is byte[] ReceiveBuffer)
            {
                int ReceiveLength = Token.Length,
                    ReceiveOffset = Token.Offset,
                    ReadLength = Math.Min(ReceiveLength - ReceiveOffset, Length);
                if (ReadLength > 0)
                {
                    Array.Copy(ReceiveBuffer, ReceiveOffset, Buffer, Offset, ReadLength);

                    ReceiveOffset += ReadLength;
                    if (ReceiveLength <= ReceiveOffset)
                    {
                        ArrayPool<byte>.Shared.Return(ReceiveBuffer);
                        Token.Buffer = null;
                        Token = null;
                    }
                    else
                    {
                        Token.Offset = ReceiveOffset;
                    }
                }

                return ReadLength;
            }
            else
            {
                // Token
                TaskCompletionSource<int> TaskToken = new();

                // Operator
                SocketAsyncEventArgs e = DequeueOperator();
                e.UserToken = TaskToken;
                e.SetBuffer(Buffer, Offset, Length);

                try
                {
                    // Receives
                    if (Socket.ReceiveAsync(e))
                        TaskToken.Task.Wait();
                    else
                        OnReceiveProcess(e);

                    return TaskToken.Task.Result;
                }
                finally
                {
                    // Release Operator
                    e.SetBuffer(null, 0, 0);
                    EnqueueOperator(e);
                }
            }
        }

        public void ReceiveFrom(EndPoint Remote)
        {
            // Checks Dispose
            if (IsDisposed)
                return;

            // Operator
            SocketAsyncEventArgs e = DequeueOperator();
            e.RemoteEndPoint = Remote;
            e.UserToken = new IOCPToken() { Remote = Remote };

            // Buffer
            int Length = BufferSize;
            byte[] Buffer = ArrayPool<byte>.Shared.Rent(Length);
            e.SetBuffer(Buffer, 0, Length);

            // Receives
            if (!Socket.ReceiveFromAsync(e))
                OnReceiveProcess(e);
        }

        public int ReceiveFrom(byte[] Buffer, int Offset, int Length, EndPoint Remote)
            => InternalReceiveFrom(Buffer, Offset, Length, new(Remote));
        private int InternalReceiveFrom(byte[] Buffer, int Offset, int Length, IOCPToken Token)
        {
            // Checks Dispose
            if (IsDisposed)
                return 0;

            if (Token?.Buffer is byte[] ReceiveBuffer)
            {
                int ReceiveLength = Token.Length,
                    ReceiveOffset = Token.Offset,
                    ReadLength = Math.Min(ReceiveLength - ReceiveOffset, Length);
                if (ReadLength > 0)
                {
                    Array.Copy(ReceiveBuffer, ReceiveOffset, Buffer, Offset, ReadLength);

                    ReceiveOffset += ReadLength;
                    if (ReceiveLength <= ReceiveOffset)
                    {
                        ArrayPool<byte>.Shared.Return(ReceiveBuffer);
                        Token.Buffer = null;
                    }
                    else
                    {
                        Token.Offset = ReceiveOffset;
                    }
                }

                return ReadLength;
            }
            else
            {
                // Token
                TaskCompletionSource<int> TaskToken = new();

                // Operator
                SocketAsyncEventArgs e = DequeueOperator();
                e.UserToken = TaskToken;
                e.RemoteEndPoint = Token.Remote;
                e.SetBuffer(Buffer, Offset, Length);

                try
                {
                    // Receives
                    if (Socket.ReceiveFromAsync(e))
                        TaskToken.Task.Wait();
                    else
                        OnReceiveProcess(e);

                    return TaskToken.Task.Result;
                }
                finally
                {
                    // Release Operator
                    e.SetBuffer(null, 0, 0);
                    EnqueueOperator(e);
                }
            }
        }

        public void Send(byte[] Buffer, int Offset, int Length)
        {
            // Checks Dispose
            if (IsDisposed)
                return;

            // Checks Connect Status
            if (!Socket.Connected)
                return;

            // Token
            TaskCompletionSource<bool> Token = new();

            // Operator
            SocketAsyncEventArgs e = DequeueOperator();
            e.UserToken = Token;
            e.SetBuffer(Buffer, Offset, Length);

            try
            {
                // Send
                if (Socket.SendAsync(e))
                    Token.Task.Wait();
                else
                    OnSendProcess(e);
            }
            finally
            {
                // Release Operator
                e.SetBuffer(null, 0, 0);
                EnqueueOperator(e);
            }
        }

        public void SendTo(byte[] Buffer, int Offset, int Length, EndPoint Remote)
        {
            // Checks Dispose
            if (IsDisposed)
                return;

            // TaskToken
            TaskCompletionSource<bool> TaskToken = new();

            // Operator
            SocketAsyncEventArgs e = DequeueOperator();
            e.UserToken = TaskToken;
            e.RemoteEndPoint = Remote;
            e.SetBuffer(Buffer, Offset, Length);

            try
            {
                // Send
                if (Socket.SendToAsync(e))
                    TaskToken.Task.Wait();
                else
                    OnSendProcess(e);
            }
            finally
            {
                // Release Operator
                e.SetBuffer(null, 0, 0);
                EnqueueOperator(e);
            }
        }

        private void OnAccepted(IOCPSocket e)
        {
            if (Accepted != null)
                Task.Run(() => Accepted.Invoke(this, e));
        }

        private void OnReceived(IOCPToken Token)
        {
            if (Received != null)
            {
                Stream s = InternalGetStream(Token);
                Received.Invoke(this, s);
                s.Dispose();
            }
        }

        private void OnDisconnect()
        {
            if (IsDisposed)
                return;

            Disconnected?.Invoke(this, EventArgs.Empty);
            Dispose();
        }

        private void OnOperatorCompleted(object sender, SocketAsyncEventArgs e)
        {
            switch (e.LastOperation)
            {
                case SocketAsyncOperation.Accept:
                    {
                        OnAcceptProcess(e);
                        break;
                    }
                case SocketAsyncOperation.Connect:
                    {
                        OnConnectProcess(e);
                        break;
                    }
                case SocketAsyncOperation.Receive:
                case SocketAsyncOperation.ReceiveFrom:
                    {
                        OnReceiveProcess(e);
                        break;
                    }
                case SocketAsyncOperation.Send:
                case SocketAsyncOperation.SendTo:
                    {
                        OnSendProcess(e);
                        break;
                    }
                default:
                    {
                        Debug.WriteLine($"[Error][{nameof(IOCPSocket)}Not support the operation {e.LastOperation}.");
                        throw new ArgumentException("The last operation completed on the socket was not a Send or Receive");
                    }
            }
        }
        private void OnAcceptProcess(SocketAsyncEventArgs e)
        {
            if (e.SocketError == SocketError.Success &&
                e.AcceptSocket is Socket s &&
                s.Connected)
                OnAccepted(new IOCPSocket(s));
        }
        private void OnConnectProcess(SocketAsyncEventArgs e)
        {
            // Result
            if (e.UserToken is TaskCompletionSource<bool> Token)
                Token.TrySetResult(e.SocketError == SocketError.Success);
        }
        private void OnReceiveProcess(SocketAsyncEventArgs e)
        {
            if (e.UserToken is TaskCompletionSource<int> Task)
            {
                if (e.SocketError == SocketError.Success &&
                    e.BytesTransferred > 0)
                {
                    Task.TrySetResult(e.BytesTransferred);
                }
                else
                {
                    Task.TrySetResult(0);
                    OnDisconnect();
                }
            }
            else if (e.UserToken is IOCPToken Token)
            {
                if (e.SocketError == SocketError.Success &&
                    e.BytesTransferred > 0)
                {
                    Token.Buffer = e.Buffer;
                    Token.Offset = 0;
                    Token.Length = e.BytesTransferred;
                    OnReceived(e.LastOperation == SocketAsyncOperation.Receive ? null : Token);
                }
                else
                {
                    ArrayPool<byte>.Shared.Return(e.Buffer);
                    OnDisconnect();
                }

                // Release Operator
                EnqueueOperator(e);
            }
        }
        private void OnSendProcess(SocketAsyncEventArgs e)
        {
            // Result
            if (e.UserToken is TaskCompletionSource<bool> Token)
                Token.TrySetResult(e.SocketError == SocketError.Success);
        }

        public Stream GetStream()
            => InternalGetStream(null);
        public Stream GetStream(EndPoint Remote)
            => InternalGetStream(new(Remote));
        private Stream InternalGetStream(IOCPToken Token)
            => new IOCPStream(this, Token);

        private void EnqueueOperator(SocketAsyncEventArgs Operator)
        {
            Operator.SocketError = SocketError.Success;
            Operator.UserToken = null;

            if (Operator.Buffer != null)
            {
                ArrayPool<byte>.Shared.Return(Operator.Buffer);
                Operator.SetBuffer(null, 0, 0);
            }
            Operator.Completed -= OnOperatorCompleted;

            IOCPPool.Shared.Return(Operator);
        }
        private SocketAsyncEventArgs DequeueOperator()
        {
            SocketAsyncEventArgs Operator = IOCPPool.Shared.Rent();
            Operator.Completed += OnOperatorCompleted;
            return Operator;
        }

        public void SetSocketOption(SocketOptionLevel optionLevel, SocketOptionName optionName, object optionValue)
            => Socket.SetSocketOption(optionLevel, optionName, optionValue);
        public void SetSocketOption(SocketOptionLevel optionLevel, SocketOptionName optionName, bool optionValue)
            => Socket.SetSocketOption(optionLevel, optionName, optionValue);
        public void SetSocketOption(SocketOptionLevel optionLevel, SocketOptionName optionName, byte[] optionValue)
            => Socket.SetSocketOption(optionLevel, optionName, optionValue);
        public void SetSocketOption(SocketOptionLevel optionLevel, SocketOptionName optionName, int optionValue)
            => Socket.SetSocketOption(optionLevel, optionName, optionValue);

        public object GetSocketOption(SocketOptionLevel optionLevel, SocketOptionName optionName)
            => Socket.GetSocketOption(optionLevel, optionName);
        public void GetSocketOption(SocketOptionLevel optionLevel, SocketOptionName optionName, byte[] optionValue)
            => Socket.GetSocketOption(optionLevel, optionName, optionValue);
        public byte[] GetSocketOption(SocketOptionLevel optionLevel, SocketOptionName optionName, int optionLength)
            => Socket.GetSocketOption(optionLevel, optionName, optionLength);

        public int IOControl(IOControlCode ioControlCode, byte[] optionInValue, byte[] optionOutValue)
            => Socket.IOControl(ioControlCode, optionInValue, optionOutValue);
        public int IOControl(int ioControlCode, byte[] optionInValue, byte[] optionOutValue)
            => Socket.IOControl(ioControlCode, optionInValue, optionOutValue);

        private bool IsDisposed;
        public void Dispose()
        {
            if (IsDisposed)
                return;

            IsDisposed = true;
            Socket.Close();
            Socket.Dispose();
        }

        private sealed class IOCPStream : Stream
        {
            public override bool CanRead
                => true;

            public override bool CanWrite
                => true;

            public override bool CanSeek
                => false;

            public override long Length
                => throw new NotSupportedException();

            public override long Position
            {
                get => throw new NotSupportedException();
                set => throw new NotSupportedException();
            }

            private readonly IOCPToken Token;
            private readonly IOCPSocket Socket;
            public IOCPStream(IOCPSocket Socket, IOCPToken Token)
            {
                this.Socket = Socket;
                this.Token = Token;
            }

            public override int ReadByte()
                => this.Read<byte>();

            public override int Read(byte[] Buffer, int Offset, int Count)
                => Token is null ? Socket.Receive(Buffer, Offset, Count) :
                                   Socket.InternalReceiveFrom(Buffer, Offset, Count, Token);

            public override void Write(byte[] Buffer, int Offset, int Length)
            {
                if (Token is null)
                    Socket.Send(Buffer, Offset, Length);
                else
                    Socket.SendTo(Buffer, Offset, Length, Token.Remote);
            }

            public override long Seek(long offset, SeekOrigin origin)
                => throw new NotSupportedException();

            public override void SetLength(long value)
                => throw new NotSupportedException();

            public override void Flush()
            {

            }

        }

        private class IOCPPool
        {
            public static IOCPPool Shared { get; } = new IOCPPool();

            private readonly ConcurrentQueue<SocketAsyncEventArgs> Pool = new();

            public SocketAsyncEventArgs Rent()
                => Pool.TryDequeue(out SocketAsyncEventArgs Operator) ? Operator : new SocketAsyncEventArgs();

            public void Return(SocketAsyncEventArgs Operator)
                => Pool.Enqueue(Operator);

            public void Clear()
            {
#if NETSTANDARD2_1_OR_GREATER
                Pool.Clear();
#else
                while (!Pool.IsEmpty)
                    Pool.TryDequeue(out _);
#endif
            }

        }

        private class IOCPToken
        {
            public EndPoint Remote;

            public byte[] Buffer;

            public int Offset;

            public int Length;

            public IOCPToken()
            {
            }
            public IOCPToken(EndPoint Remote)
            {
                this.Remote = Remote;
            }

        }

    }
}