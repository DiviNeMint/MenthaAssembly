using MenthaAssembly.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MenthaAssembly.Network.Primitives
{
    public abstract class UdpSocket : IOCPSocket
    {
        public event EventHandler<UdpMessage> Received;

        public IProtocolCoder ProtocolHandler { get; }

        public bool IsDisposed => _IsDisposed;

        protected Socket Socket;
        protected UdpSocket(IProtocolCoder Protocol)
        {
            ProtocolHandler = Protocol;
        }

        protected void SendTo(EndPoint Address, IMessage Message, int TimeoutMileseconds)
        {
            // Timeout
            CancellationTokenSource CancelToken = new CancellationTokenSource(TimeoutMileseconds);

            // Lock Send
            try
            {
                WaitSend(CancelToken.Token);
            }
            catch (OperationCanceledException)
            {
                // Release CancelToken
                CancelToken.Dispose();
                return;
            }
            catch (ObjectDisposedException)
            {
                // Release CancelToken
                CancelToken.Dispose();

                return;
            }

            try
            {
                // Encode Message
                Stream MessageStream;
                try
                {
                    MessageStream = ProtocolHandler.Encode(Message);
                    MessageStream.Seek(0, SeekOrigin.Begin);
                }
                catch
                {
                    Debug.WriteLine($"[Error][{GetType().Name}]Encoding {Message.GetType().Name} hanppen exception.");

                    // Release CancelToken
                    CancelToken.Dispose();

                    return;
                }

                // Check Message's Context
                byte[] Buffer = Dequeue();
                int Length = MessageStream?.Read(Buffer, sizeof(int), BufferSize - sizeof(int)) ?? 0;
                if (Length == 0)
                {
                    MessageStream?.Dispose();

                    // Enqueue Buffer
                    Enqueue(ref Buffer);

                    // Release CancelToken
                    CancelToken.Dispose();

                    return;
                }

                // Send Datas
                try
                {
                    Debug.WriteLine($"[Info][{GetType().Name}]Send {Message.GetType().Name} to [{Address}].");

                    do
                    {
                        SocketAsyncEventArgs e = Dequeue(false);
                        e.RemoteEndPoint = Address;
                        e.SetBuffer(Buffer, 0, Length);

                        if (!Socket.SendToAsync(e))
                            OnSendToProcess(e);

                        Buffer = Dequeue();
                        Length = MessageStream.Read(Buffer, 0, BufferSize);

                    } while (Length > 0);
                }
                catch
                {
                    //// Disconnect
                    //Token.Dispose();
                }
                finally
                {
                    MessageStream.Dispose();

                    // Enqueue Last Empty Buffer
                    Enqueue(ref Buffer);
                }
            }
            finally
            {
                ReleaseSend();
            }
        }
        protected async Task SendToAsync(EndPoint Address, IMessage Message, int TimeoutMileseconds)
        {
            // Timeout
            CancellationTokenSource CancelToken = new CancellationTokenSource(TimeoutMileseconds);

            // Lock Send
            try
            {
                await WaitSendAsync(CancelToken.Token);
            }
            catch (OperationCanceledException)
            {
                // Release CancelToken
                CancelToken.Dispose();

                // Timeout
                return;
            }
            catch (ObjectDisposedException)
            {
                // Release CancelToken
                CancelToken.Dispose();

                return;
            }

            try
            {
                // Encode Message
                Stream MessageStream;
                try
                {
                    MessageStream = ProtocolHandler.Encode(Message);
                    MessageStream.Seek(0, SeekOrigin.Begin);
                }
                catch
                {
                    Debug.WriteLine($"[Error][{GetType().Name}]Encoding {Message.GetType().Name} hanppen exception.");

                    // Release CancelToken
                    CancelToken.Dispose();

                    return;
                }

                // Check Message's Context
                byte[] Buffer = Dequeue();
                int Length = MessageStream?.Read(Buffer, sizeof(int), BufferSize - sizeof(int)) ?? 0;
                if (Length == 0)
                {
                    MessageStream?.Dispose();

                    // Enqueue Buffer
                    Enqueue(ref Buffer);

                    // Release CancelToken
                    CancelToken.Dispose();

                    return;
                }

                // Send Datas
                try
                {
                    Debug.WriteLine($"[Info][{GetType().Name}]Send {Message.GetType().Name} to [{Address}].");

                    do
                    {
                        SocketAsyncEventArgs e = Dequeue(false);
                        e.RemoteEndPoint = Address;
                        e.SetBuffer(Buffer, 0, Length);

                        if (!Socket.SendToAsync(e))
                            OnSendToProcess(e);

                        Buffer = Dequeue();
                        Length = MessageStream.Read(Buffer, 0, BufferSize);

                    } while (Length > 0);
                }
                catch
                {
                    //// Disconnect
                    //Token.Dispose();
                }
                finally
                {
                    MessageStream.Dispose();

                    // Enqueue Last Empty Buffer
                    Enqueue(ref Buffer);
                }
            }
            finally
            {
                ReleaseSend();
            }
        }

        protected void OnSendToProcess(SocketAsyncEventArgs e)
            => Enqueue(ref e);

        protected void OnReceiveFromProcess(SocketAsyncEventArgs e)
        {
            // Check Client's Connection Status
            if (e.SocketError == SocketError.Success &&
                e.BytesTransferred > 0)
            {
                IMessage ReceiveMessage;
                try
                {
                    // Decode Message
                    ConcatStream s = new ConcatStream(e.Buffer, 0, e.BytesTransferred, new NetworkStream(Socket));
                    ReceiveMessage = ProtocolHandler.Decode(s);
                    s.Dispose();

                    Debug.WriteLine($"[Info][{GetType().Name}]Receive {ReceiveMessage?.GetType().Name ?? "Null"}.");
                }
                catch (Exception Ex)
                {
                    if (!(Ex is IOException IOEx &&
                         IOEx.InnerException is ObjectDisposedException ODEx &&
                         ODEx.ObjectName == typeof(Socket).FullName) &&
                         !(Ex is SocketException))
                        Debug.WriteLine($"[Error][{GetType().Name}]Decode exception.");

                    // Push Resource to pool.
                    Enqueue(ref e);
                    return;
                }

                if (Received != null)
                    Received.BeginInvoke(this, new UdpMessage((IPEndPoint)e.RemoteEndPoint, ReceiveMessage), ar => Received.EndInvoke(ar), null);

                // Loop Receive
                e.RemoteEndPoint = CreateEndPoint();
                if (!Socket.ReceiveFromAsync(e))
                    OnReceiveFromProcess(e);

                return;
            }

            // Push Resource to pool.
            Enqueue(ref e);
        }

        protected sealed override void OnIOCompleted(object sender, SocketAsyncEventArgs e)
        {
            switch (e.LastOperation)
            {
                case SocketAsyncOperation.ReceiveFrom:
                    OnReceiveFromProcess(e);
                    break;
                case SocketAsyncOperation.SendTo:
                    OnSendToProcess(e);
                    break;
                default:
                    {
                        Debug.WriteLine($"[Error][{this.GetType().Name}Not support the operation {e.LastOperation}.");
                        throw new ArgumentException("The last operation completed on the socket was not a SendTo or ReceiveFrom");
                    }
            }
        }

        protected abstract IPEndPoint CreateEndPoint();

        private SemaphoreSlim SendLock;
        protected void WaitSend(CancellationToken CancelToken)
        {
            if (!IsDisposed)
                SendLock.Wait(CancelToken);
        }
        protected async Task WaitSendAsync(CancellationToken CancelToken)
        {
            if (!IsDisposed)
                await SendLock.WaitAsync(CancelToken);
        }

        protected void ReleaseSend()
        {
            if (!IsDisposed)
                SendLock.Release();
        }

        protected void Reset()
        {
            Socket?.Dispose();
            Socket = new Socket(SocketType.Dgram, ProtocolType.Udp);

            SendLock?.Dispose();
            SendLock = new SemaphoreSlim(1);
        }

        protected bool _IsDisposed = false;
        public override void Dispose()
        {
            if (_IsDisposed)
                return;

            try
            {
                Socket.Dispose();
                Socket = null;

                SendLock.Dispose();
                SendLock = null;
            }
            finally
            {
                _IsDisposed = true;
            }
        }

    }
}
