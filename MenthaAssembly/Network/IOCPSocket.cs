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
        private const int ReceiveBufferSize = 1024;

        public event EventHandler<IOCPSocket> Accepted;

        public event EventHandler<Stream> Received;

        public event EventHandler Disconnect;

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
            => TryConnect(Address, 5000);
        public bool TryConnect(EndPoint Address, int Timeout)
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
            CancellationTokenSource CancelToken = new(Timeout);
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

        private ReceiveToken ReadToken;
        public void Receive()
        {
            // Checks token.
            if (ReadToken is not null)
                return;

            ReadToken = new ReceiveToken();
            Receive(ReadToken);
        }
        private void Receive(ReceiveToken Token)
        {
            // Checks Dispose
            if (IsDisposed)
                return;

            // Checks Connect Status
            if (!Socket.Connected)
                return;

            // Receives
            if (Token.Buffer is null)
            {
                // Operator
                SocketAsyncEventArgs e = DequeueOperator();
                e.UserToken = Token;

                // Buffer
                byte[] Buffer = ArrayPool<byte>.Shared.Rent(ReceiveBufferSize);
                e.SetBuffer(Buffer, 0, ReceiveBufferSize);

                // Receives
                if (!Socket.ReceiveAsync(e))
                    OnReceiveProcess(e);
            }

            // Last Receive
            else
            {
                OnReceived();
            }
        }

        public int Receive(byte[] Buffer, int Offset, int Length)
        {
            // Checks Dispose
            if (IsDisposed)
                return 0;

            // Checks Connect Status
            if (!Socket.Connected)
                return 0;

            if (ReadToken?.Buffer is byte[] ReceiveBuffer)
            {
                int ReceiveLength = ReadToken.Length,
                    ReceiveOffset = ReadToken.Offset,
                    ReadLength = Math.Min(ReceiveLength - ReceiveOffset, Length);
                if (ReadLength > 0)
                {
                    Array.Copy(ReceiveBuffer, ReceiveOffset, Buffer, Offset, ReadLength);

                    ReceiveOffset += ReadLength;
                    if (ReceiveLength <= ReceiveOffset)
                    {
                        ArrayPool<byte>.Shared.Return(ReceiveBuffer);
                        ReadToken.Buffer = null;
                    }
                    else
                    {
                        ReadToken.Offset = ReceiveOffset;
                    }
                }

                return ReadLength;
            }
            else
            {
                // Token
                TaskCompletionSource<int> Token = new();

                // Operator
                SocketAsyncEventArgs e = DequeueOperator();
                e.UserToken = Token;
                e.SetBuffer(Buffer, Offset, Length);

                try
                {
                    // Receives
                    if (Socket.ReceiveAsync(e))
                        Token.Task.Wait();
                    else
                        OnReceiveProcess(e);

                    return Token.Task.Result;
                }
                finally
                {
                    // Release Operator
                    e.SetBuffer(null, 0, 0);
                    EnqueueOperator(e);
                }
            }
        }

        private readonly ConcurrentDictionary<EndPoint, ReceiveToken> ReadTokens = new();
        public void ReceiveFrom(EndPoint Remote)
        {
            // Checks token.
            if (ReadTokens.ContainsKey(Remote))
                return;

            ReceiveToken Token = new();
            ReadTokens.AddOrUpdate(Remote, Token, (k, v) => Token);
            ReceiveFrom(Remote, Token);
        }
        private void ReceiveFrom(EndPoint Remote, ReceiveToken Token)
        {
            // Checks Dispose
            if (IsDisposed)
                return;

            // Checks Connect Status
            if (!Socket.Connected)
                return;

            // Receives
            if (Token.Buffer is null)
            {
                // Operator
                SocketAsyncEventArgs e = DequeueOperator();
                e.RemoteEndPoint = Remote;
                e.UserToken = Token;

                // Buffer
                byte[] Buffer = ArrayPool<byte>.Shared.Rent(ReceiveBufferSize);
                e.SetBuffer(Buffer, 0, ReceiveBufferSize);

                // Receives
                if (!Socket.ReceiveFromAsync(e))
                    OnReceiveFromProcess(e);
            }

            // Last Receive
            else
            {
                OnReceived();
            }
        }

        public int ReceiveFrom(byte[] Buffer, int Offset, int Length, EndPoint Remote)
        {
            // Checks Dispose
            if (IsDisposed)
                return 0;

            // Checks Connect Status
            if (!Socket.Connected)
                return 0;

            // Token
            TaskCompletionSource<int> Token = new();

            // Operator
            SocketAsyncEventArgs e = DequeueOperator();
            e.UserToken = Token;
            e.RemoteEndPoint = Remote;
            e.SetBuffer(Buffer, Offset, Length);

            try
            {
                // Receives
                if (Socket.ReceiveFromAsync(e))
                    Token.Task.Wait();
                else
                    OnReceiveFromProcess(e);

                return Token.Task.Result;
            }
            finally
            {
                // Release Operator
                e.SetBuffer(null, 0, 0);
                EnqueueOperator(e);
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

            // Checks Connect Status
            if (!Socket.Connected)
                return;

            // Token
            TaskCompletionSource<bool> Token = new();

            // Operator
            SocketAsyncEventArgs e = DequeueOperator();
            e.UserToken = Token;
            e.RemoteEndPoint = Remote;
            e.SetBuffer(Buffer, Offset, Length);

            try
            {
                // Send
                if (Socket.SendToAsync(e))
                    Token.Task.Wait();
                else
                    OnSendToProcess(e);
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

        private void OnReceived()
        {
            if (Received != null)
            {
                Stream s = GetStream();
                Received.Invoke(this, s);
                s.Dispose();
            }

            Receive(ReadToken);
        }
        private void OnReceived(EndPoint Remote, ReceiveToken Token)
        {
            if (Received != null)
            {
                Stream s = GetStream(Remote);
                Received.Invoke(this, s);
                s.Dispose();
            }

            ReceiveFrom(Remote, Token);
        }

        private void OnDisconnect()
        {
            if (IsDisposed)
                return;

            Disconnect?.Invoke(this, EventArgs.Empty);
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
                    {
                        OnReceiveProcess(e);
                        break;
                    }
                case SocketAsyncOperation.ReceiveFrom:
                    {
                        OnReceiveFromProcess(e);
                        break;
                    }
                case SocketAsyncOperation.Send:
                    {
                        OnSendProcess(e);
                        break;
                    }
                case SocketAsyncOperation.SendTo:
                    {
                        OnSendToProcess(e);
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

            // Loop Accept
            InternalAccept(e);
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
            else if (e.UserToken is ReceiveToken Token)
            {
                if (e.SocketError == SocketError.Success &&
                    e.BytesTransferred > 0)
                {
                    Token.Buffer = e.Buffer;
                    Token.Offset = 0;
                    Token.Length = e.BytesTransferred;
                    OnReceived();
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
        private void OnReceiveFromProcess(SocketAsyncEventArgs e)
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
            else if (e.UserToken is ReceiveToken Token)
            {
                if (e.SocketError == SocketError.Success &&
                    e.BytesTransferred > 0)
                {
                    EndPoint Remote = e.RemoteEndPoint;

                    Token.Buffer = e.Buffer;
                    Token.Offset = 0;
                    Token.Length = e.BytesTransferred;
                    OnReceived(Remote, Token);
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
        private void OnSendToProcess(SocketAsyncEventArgs e)
        {
            // Result
            if (e.UserToken is TaskCompletionSource<bool> Token)
                Token.TrySetResult(e.SocketError == SocketError.Success);
        }

        public Stream GetStream()
            => new IOCPStream(this);
        public Stream GetStream(EndPoint Remote)
            => new IOCPStream(this, Remote);

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

            private readonly EndPoint Remote;
            private readonly IOCPSocket Socket;
            public IOCPStream(IOCPSocket Socket) : this(Socket, null)
            {
            }
            public IOCPStream(IOCPSocket Socket, EndPoint Remote)
            {
                this.Socket = Socket;
                this.Remote = Remote;
            }

            public override int Read(byte[] Buffer, int Offset, int Count)
                => Remote is null ? Socket.Receive(Buffer, Offset, Count) :
                                    Socket.ReceiveFrom(Buffer, Offset, Count, Remote);

            public override void Write(byte[] Buffer, int Offset, int Length)
            {
                if (Remote is null)
                    Socket.Send(Buffer, Offset, Length);
                else
                    Socket.SendTo(Buffer, Offset, Length, Remote);
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

        private class ReceiveToken
        {
            public byte[] Buffer;

            public int Offset;

            public int Length;

        }

    }
}