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
    public class TcpStream : IOCPStream
    {
        public event EventHandler<IPEndPoint, TcpStream> Disconnected;

        public event EventHandler<IPEndPoint, TcpStream> ReceiveCompleted;

        public TcpStream(Socket Socket, IOCPPool Pool) : base(Socket, Pool)
        {
        }

        private byte[] ReceivedBuffer;
        private int ReceivedOffset = 0,
                    ReceivedLength = 0;
        public override int Read(byte[] Buffer, int Offset, int Length)
        {
            TaskCompletionSource<int> Token = new TaskCompletionSource<int>();

            // Timeout
            CancellationTokenSource CancelToken = new CancellationTokenSource(ReadTimeout);
            CancelToken.Token.Register(() => Token.TrySetResult(0), false);

            // Lock Receive
            try
            {
                LockRead(CancelToken.Token);
            }
            catch (OperationCanceledException)
            {
                // Release CancelToken
                CancelToken.Dispose();

                return Token.Task.Result;
            }
            catch (ObjectDisposedException)
            {
                Token.TrySetResult(0);

                // Release CancelToken
                CancelToken.Dispose();

                return Token.Task.Result;
            }

            try
            {
                if (ReceivedBuffer is null)
                {
                    SocketAsyncEventArgs e = Dequeue();
                    e.UserToken = Token;
                    e.SetBuffer(Buffer, Offset, Length);

                    if (!Socket.ReceiveAsync(e))
                        OnReadProcess(e);

                    Token.Task.Wait();
                    return Token.Task.Result;
                }
                else
                {
                    Token.TrySetCanceled();
                    if (ReceivedLength > Length)
                    {
                        Array.Copy(ReceivedBuffer, ReceivedOffset, Buffer, Offset, Length);
                        ReceivedOffset += Length;
                        return Length;
                    }
                    else
                    {
                        Array.Copy(ReceivedBuffer, ReceivedOffset, Buffer, Offset, ReceivedLength);
                        ReceivedBuffer = null;
                        return ReceivedLength;
                    }
                }
            }
            catch
            {
                OnDisconnected();
            }
            finally
            {
                CancelToken.Dispose();
                ReleaseRead();
            }

            return 0;
        }
        public override async Task<int> ReadAsync(byte[] Buffer, int Offset, int Length, CancellationToken CancellationToken)
        {
            TaskCompletionSource<int> Token = new TaskCompletionSource<int>();

            // Timeout
            CancellationTokenSource TimeoutToken = new CancellationTokenSource(WriteTimeout),
                                    CancelToken = CancellationTokenSource.CreateLinkedTokenSource(TimeoutToken.Token, CancellationToken);

            CancelToken.Token.Register(() => Token.TrySetResult(0), false);

            // Lock Receive
            try
            {
                await LockReadAsync(CancelToken.Token);
            }
            catch (OperationCanceledException)
            {
                TimeoutToken.Dispose();
                CancelToken.Dispose();
                return Token.Task.Result;
            }
            catch (ObjectDisposedException)
            {
                Token.TrySetResult(0);
                TimeoutToken.Dispose();
                CancelToken.Dispose();
                return Token.Task.Result;
            }

            try
            {
                if (ReceivedBuffer is null)
                {
                    SocketAsyncEventArgs e = Dequeue();
                    e.UserToken = Token;
                    e.SetBuffer(Buffer, Offset, Length);

                    if (!Socket.ReceiveAsync(e))
                        OnReadProcess(e);

                    return await Token.Task;
                }
                else
                {
                    Token.TrySetCanceled();
                    if (ReceivedLength > Length)
                    {
                        Array.Copy(ReceivedBuffer, ReceivedOffset, Buffer, Offset, Length);
                        ReceivedOffset += Length;
                        return Length;
                    }
                    else
                    {
                        Array.Copy(ReceivedBuffer, ReceivedOffset, Buffer, Offset, ReceivedLength);
                        ReceivedBuffer = null;
                        return ReceivedLength;
                    }
                }
            }
            catch
            {
                OnDisconnected();
            }
            finally
            {
                TimeoutToken.Dispose();
                CancelToken.Dispose();
                ReleaseRead();
            }

            return 0;
        }

        public override void Write(byte[] Buffer, int Offset, int Length)
        {
            TaskCompletionSource<bool> Token = new TaskCompletionSource<bool>();

            // Timeout
            CancellationTokenSource CancelToken = new CancellationTokenSource(WriteTimeout);
            CancelToken.Token.Register(() => Token.TrySetResult(false), false);

            // Lock Send
            try
            {
                LockWrite(CancelToken.Token);
            }
            catch (OperationCanceledException)
            {
                // Release CancelToken
                CancelToken.Dispose();
                return;
            }
            catch (ObjectDisposedException)
            {
                Token.TrySetResult(false);

                // Release CancelToken
                CancelToken.Dispose();
                return;
            }

            try
            {
                SocketAsyncEventArgs e = Dequeue();
                e.UserToken = Token;
                e.SetBuffer(Buffer, Offset, Length);

                if (!Socket.SendAsync(e))
                    OnWriteProcess(e);

                Token.Task.Wait();
            }
            catch
            {
                OnDisconnected();
            }
            finally
            {
                CancelToken.Dispose();
                ReleaseWrite();
            }
        }
        public override void Write(Stream Datas, CancellationToken CancellationToken)
        {
            // Lock Send
            LockWrite(CancellationToken);

            byte[] Buffer = DequeueBuffer();
            try
            {
                int Length = Datas.Read(Buffer, 0, Buffer.Length);
                while (Length > 0)
                {
                    SocketAsyncEventArgs e = Dequeue();
                    e.SetBuffer(Buffer, 0, Length);

                    if (!Socket.SendAsync(e))
                        OnSendProcess(e);

                    Buffer = DequeueBuffer();
                    Length = Datas.Read(Buffer, 0, Buffer.Length);
                }
            }
            catch (TaskCanceledException ex)
            {
                throw ex;
            }
            catch
            {
                OnDisconnected();
            }
            finally
            {
                EnqueueBuffer(Buffer);
                ReleaseWrite();
            }
        }
        public override async Task WriteAsync(byte[] Buffer, int Offset, int Length, CancellationToken CancellationToken)
        {
            TaskCompletionSource<bool> Token = new TaskCompletionSource<bool>();
            CancellationToken.Register(() => Token.TrySetResult(false), false);

            // Lock Send
            try
            {
                await LockWriteAsync(CancellationToken);
            }
            catch (TaskCanceledException ex)
            {
                Token.TrySetResult(false);
                throw ex;
            }
            catch (ObjectDisposedException ex)
            {
                Token.TrySetResult(false);
                throw ex;
            }

            try
            {
                SocketAsyncEventArgs e = Dequeue();
                e.UserToken = Token;
                e.SetBuffer(Buffer, Offset, Length);

                if (!Socket.SendAsync(e))
                    OnWriteProcess(e);

                await Token.Task;
            }
            catch (TaskCanceledException ex)
            {
                Token.TrySetResult(false);
                throw ex;
            }
            catch
            {
                OnDisconnected();
            }
            finally
            {
                ReleaseWrite();
            }
        }
        public override async Task WriteAsync(Stream Datas, CancellationToken CancellationToken)
        {
            // Lock Send
            await LockWriteAsync(CancellationToken);

            byte[] Buffer = DequeueBuffer();
            try
            {
                int Length = await Datas.ReadAsync(Buffer, 0, Buffer.Length, CancellationToken);
                while (Length > 0)
                {
                    SocketAsyncEventArgs e = Dequeue();
                    e.SetBuffer(Buffer, 0, Length);

                    if (!Socket.SendAsync(e))
                        OnSendProcess(e);

                    Buffer = DequeueBuffer();
                    Length = await Datas.ReadAsync(Buffer, 0, Buffer.Length, CancellationToken);
                }
            }
            catch (TaskCanceledException ex)
            {
                throw ex;
            }
            catch
            {
                OnDisconnected();
            }
            finally
            {
                EnqueueBuffer(Buffer);
                ReleaseWrite();
            }
        }

        public void Receive()
        {
            // Lock Receive
            try
            {
                LockRead();
            }
            catch (ObjectDisposedException)
            {
                return;
            }

            byte[] Buffer = DequeueBuffer();

            SocketAsyncEventArgs e = Dequeue();
            e.SetBuffer(Buffer, 0, Buffer.Length);

            if (!Socket.ReceiveAsync(e))
                OnReceiveProcess(e);
        }

        public unsafe void SetKeepAlive(bool Enable, uint Interval)
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

        protected override void OnIOCompleted(object sender, SocketAsyncEventArgs e)
        {
            switch (e.LastOperation)
            {
                case SocketAsyncOperation.Receive:
                    {
                        if (e.UserToken is null)
                            OnReceiveProcess(e);
                        else
                            OnReadProcess(e);
                        break;
                    }
                case SocketAsyncOperation.Send:
                    {
                        if (e.UserToken is null)
                            OnSendProcess(e);
                        else
                            OnWriteProcess(e);
                        break;
                    }
                default:
                    {
                        Debug.WriteLine($"[Error][{GetType().Name}Not support the operation {e.LastOperation}.");
                        throw new ArgumentException("The last operation completed on the socket was not a Send or Receive");
                    }
            }
        }

        protected virtual void OnReadProcess(SocketAsyncEventArgs e)
        {
            if (e.SocketError == SocketError.Success &&
                e.BytesTransferred > 0 &&
                e.UserToken is TaskCompletionSource<int> Token)
            {
                Token.TrySetResult(e.BytesTransferred);
                return;
            }

            Enqueue(e);
            OnDisconnected();
        }

        protected virtual void OnWriteProcess(SocketAsyncEventArgs e)
        {
            if (e.SocketError == SocketError.Success &&
                e.UserToken is TaskCompletionSource<bool> Token)
            {
                Token.TrySetResult(true);
                return;
            }

            Enqueue(e);
            OnDisconnected();
        }

        protected virtual void OnReceiveProcess(SocketAsyncEventArgs e)
        {
            if (e.SocketError == SocketError.Success &&
                e.BytesTransferred > 0)
            {
                ReceivedBuffer = e.Buffer;
                ReceivedOffset = 0;
                ReceivedLength = e.BytesTransferred;

                Enqueue(e);
                ReleaseRead();

                ReceiveCompleted?.Invoke(Address, this);
                return;
            }

            Enqueue(e);
            OnDisconnected();
        }

        protected virtual void OnSendProcess(SocketAsyncEventArgs e)
        {
            EnqueueBuffer(e.Buffer);
            Enqueue(e);

            if (e.SocketError != SocketError.Success)
                OnDisconnected();
        }

        protected virtual void OnDisconnected()
        {
            if (IsDisposed)
                return;

            IPEndPoint Address = base.Address;

            Dispose();
            Disconnected?.Invoke(Address, this);
        }

        private bool IsDisposed = false;
        protected override void Dispose(bool Disposing)
        {
            if (IsDisposed)
                return;

            IsDisposed = true;
            base.Dispose(Disposing);
        }

    }
}
