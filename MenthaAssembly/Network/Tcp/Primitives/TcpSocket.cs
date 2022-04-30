using MenthaAssembly.Utils;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace MenthaAssembly.Network.Primitives
{
    public abstract class TcpSocket : IOCPSocket
    {
        public event EventHandler<TcpRequest> RequestReceived;

        public event EventHandler<IPEndPoint> Disconnected;

        public IProtocolCoder ProtocolHandler { get; }

        public abstract bool IsDisposed { get; }

        protected TcpSocket(IProtocolCoder Protocol)
        {
            ProtocolHandler = Protocol;
            ReplyHandler = OnReplyProcess;
        }

        protected virtual IMessage Send(TcpToken Token, IMessage Request, int TimeoutMileseconds)
        {
            TaskCompletionSource<IMessage> TaskToken = new TaskCompletionSource<IMessage>();

            // Timeout
            CancellationTokenSource CancelToken = new CancellationTokenSource(TimeoutMileseconds);
            CancelToken.Token.Register(() => TaskToken.TrySetResult(ErrorMessage.Timeout), false);

            // Lock Send
            try
            {
                Token.WaitSend(CancelToken.Token);
            }
            catch (OperationCanceledException)
            {
                // Release CancelToken
                CancelToken.Dispose();

                return TaskToken.Task.Result;
            }
            catch (ObjectDisposedException)
            {
                TaskToken.TrySetResult(ErrorMessage.Disconnected);

                // Release CancelToken
                CancelToken.Dispose();

                return TaskToken.Task.Result;
            }

            try
            {
                // Encode Message
                Stream MessageStream;
                try
                {
                    MessageStream = ProtocolHandler.Encode(Request);
                    MessageStream.Seek(0, SeekOrigin.Begin);
                }
                catch
                {
                    Debug.WriteLine($"[Error][{GetType().Name}]Encoding {Request.GetType().Name} hanppen exception.");

                    // Set Result
                    TaskToken.TrySetResult(ErrorMessage.EncodeException);

                    // Release CancelToken
                    CancelToken.Dispose();

                    return TaskToken.Task.Result;
                }

                // Check Message's Context
                byte[] Buffer = Dequeue();
                int Length = MessageStream?.Read(Buffer, sizeof(int), BufferSize - sizeof(int)) ?? 0;
                if (Length == 0)
                {
                    MessageStream?.Dispose();

                    // Enqueue Buffer
                    Enqueue(ref Buffer);

                    // Set Result
                    TaskToken.TrySetResult(ErrorMessage.NotSupport);

                    // Release CancelToken
                    CancelToken.Dispose();

                    return TaskToken.Task.Result;
                }

                // UID
                int UID = Token.NextUID();
                PointerHelper.Copy(UID, Buffer, 0);
                Length += sizeof(int);

                Token.ResponseTaskSources.AddOrUpdate(UID, TaskToken, (k, v) => TaskToken);
                Token.ResponseCancelTokens.AddOrUpdate(UID, CancelToken, (k, v) => CancelToken);

                // Send Datas
                try
                {
                    Debug.WriteLine($"[Info][{GetType().Name}]Send {Request.GetType().Name} to [{Token.Address}].");

                    do
                    {
                        SocketAsyncEventArgs e = Dequeue(false);
                        e.UserToken = Token;
                        e.SetBuffer(Buffer, 0, Length);

                        if (!Token.SendAsync(e))
                            OnSendProcess(e);

                        Buffer = Dequeue();
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
                    Enqueue(ref Buffer);
                }
            }
            finally
            {
                Token.ReleaseSend();
            }

            TaskToken.Task.Wait();
            return TaskToken.Task.Result;
        }
        protected virtual async Task<IMessage> SendAsync(TcpToken Token, IMessage Request, int TimeoutMileseconds)
        {
            TaskCompletionSource<IMessage> TaskToken = new TaskCompletionSource<IMessage>();

            // Timeout
            CancellationTokenSource CancelToken = new CancellationTokenSource(TimeoutMileseconds);
            CancelToken.Token.Register(() => TaskToken.TrySetResult(ErrorMessage.Timeout), false);

            // Lock Send
            try
            {
                await Token.WaitSendAsync(CancelToken.Token);
            }
            catch (OperationCanceledException)
            {
                // Release CancelToken
                CancelToken.Dispose();

                // Timeout
                return TaskToken.Task.Result;
            }
            catch (ObjectDisposedException)
            {
                TaskToken.TrySetResult(ErrorMessage.Disconnected);

                // Release CancelToken
                CancelToken.Dispose();

                return TaskToken.Task.Result;
            }

            try
            {
                // Encode Message
                Stream MessageStream;
                try
                {
                    MessageStream = ProtocolHandler.Encode(Request);
                    MessageStream.Seek(0, SeekOrigin.Begin);
                }
                catch
                {
                    Debug.WriteLine($"[Error][{GetType().Name}]Encoding {Request.GetType().Name} hanppen exception.");

                    // Set Result
                    TaskToken.TrySetResult(ErrorMessage.EncodeException);

                    // Release CancelToken
                    CancelToken.Dispose();

                    return TaskToken.Task.Result;
                }

                // Check Message's Context
                byte[] Buffer = Dequeue();
                int Length = MessageStream?.Read(Buffer, sizeof(int), BufferSize - sizeof(int)) ?? 0;
                if (Length == 0)
                {
                    MessageStream?.Dispose();

                    // Enqueue Buffer
                    Enqueue(ref Buffer);

                    // Set Result
                    TaskToken.TrySetResult(ErrorMessage.NotSupport);

                    // Release CancelToken
                    CancelToken.Dispose();

                    return TaskToken.Task.Result;
                }

                // UID
                int UID = Token.NextUID();
                PointerHelper.Copy(UID, Buffer, 0);
                Length += sizeof(int);

                Token.ResponseTaskSources.AddOrUpdate(UID, TaskToken, (k, v) => TaskToken);
                Token.ResponseCancelTokens.AddOrUpdate(UID, CancelToken, (k, v) => CancelToken);

                // Send Datas
                try
                {
                    Debug.WriteLine($"[Info][{GetType().Name}]Send {Request.GetType().Name} to [{Token.Address}].");

                    do
                    {
                        SocketAsyncEventArgs e = Dequeue(false);
                        e.UserToken = Token;
                        e.SetBuffer(Buffer, 0, Length);

                        if (!Token.SendAsync(e))
                            OnSendProcess(e);

                        Buffer = Dequeue();
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
                    Enqueue(ref Buffer);
                }
            }
            finally
            {
                Token.ReleaseSend();
            }

            return await TaskToken.Task;
        }

        protected internal virtual async Task PingAsync(TcpToken Token, IMessage Message, int TimeoutMileseconds)
        {
            // Timeout
            CancellationTokenSource CancelToken = new CancellationTokenSource(TimeoutMileseconds);

            // SendLock
            try
            {
                await Token.WaitSendAsync(CancelToken.Token);
            }
            catch (OperationCanceledException)
            {
                // Release CancelToken
                CancelToken.Dispose();

                Debug.WriteLine($"[Warn][{GetType().Name}]Ping {Message.GetType().Name} Timeout.");
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
                    CancelToken.Dispose();
                    return;
                }

                // Check Message's Context
                byte[] Buffer = Dequeue();

                int Length = MessageStream?.Read(Buffer, sizeof(int), BufferSize - sizeof(int)) ?? 0;
                if (Length == 0)
                {
                    Debug.WriteLine($"[Warn]{ProtocolHandler.GetType().Name} not support {Message.GetType().Name}.");

                    // Enqueue Buffer
                    Enqueue(ref Buffer);
                    return;
                }

                // UID
                PointerHelper.Copy(Token.NextUID(), Buffer, 0);
                Length += sizeof(int);

                // Send Datas
                try
                {
                    Debug.WriteLine($"[Info][{GetType().Name}]Ping {Message.GetType().Name} to [{Token.Address}].");

                    do
                    {
                        SocketAsyncEventArgs e = Dequeue(false);
                        e.UserToken = Token;
                        e.SetBuffer(Buffer, 0, Length);

                        if (!Token.SendAsync(e))
                            OnSendProcess(e);

                        Buffer = Dequeue();
                        Length = MessageStream.Read(Buffer, 0, BufferSize);

                    } while (Length > 0 & !Token.IsDisposed);
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
                    Enqueue(ref Buffer);
                }
            }
            finally
            {
                Token.ReleaseSend();
            }
        }

        protected void OnSendProcess(SocketAsyncEventArgs e)
        {
            if (e.UserToken is TcpToken Token)
            {
                if (e.SocketError == SocketError.Success)
                {
                    // Reset Auto Ping Counter.
                    Token.PingCounter = 0;
                }
                else
                {
                    // Dispose socket so that OnReceiveProcess will trigger OnDisconnect.
                    Token.Dispose();
                }
            }

            Enqueue(ref e);
        }

        private readonly Action<TcpToken, int, IMessage> ReplyHandler;
        protected void OnReceiveProcess(SocketAsyncEventArgs e)
        {
            if (e.UserToken is TcpToken Token)
            {
                // Check Client's Connection Status
                if (e.SocketError == SocketError.Success &&
                    e.BytesTransferred > 0)
                {
                    // Reset Auto Ping Counter.
                    Token.PingCounter = 0;

                    int ReceiveUID = -1;
                    IMessage ReceiveMessage;
                    try
                    {
                        // Decode Message
                        ConcatStream s = new ConcatStream(e.Buffer, 0, e.BytesTransferred, Token.GetStream());
                        ReceiveUID = s.Read<int>();
                        ReceiveMessage = ProtocolHandler.Decode(s);
                        s.Dispose();

                        Debug.WriteLine($"[Info][{GetType().Name}]Receive {ReceiveMessage?.GetType().Name ?? "NullMessage"}.");
                    }
                    catch (Exception Ex)
                    {
                        if (!(Ex is IOException IOEx &&
                             IOEx.InnerException is ObjectDisposedException ODEx &&
                             ODEx.ObjectName == typeof(Socket).FullName) &&
                             !(Ex is SocketException))
                            Debug.WriteLine($"[Error][{GetType().Name}]Decode exception.");

                        // Trigger Disconnected Event.
                        OnDisconnected(Token);

                        // Push Resource to pool.
                        Enqueue(ref e);
                        return;
                    }

                    if (ReceiveMessage != null)
                        ReplyHandler.BeginInvoke(Token, ReceiveUID, ReceiveMessage, (ar) => ReplyHandler.EndInvoke(ar), null);

                    // Loop Receive
                    if (!Token.ReceiveAsync(e))
                        OnReceiveProcess(e);

                    return;
                }

                // Trigger Disconnected Event.
                OnDisconnected(Token);
            }

            // Push Resource to pool.
            Enqueue(ref e);
        }

        protected virtual void OnReplyProcess(TcpToken Token, int ReceiveUID, IMessage ReceiveMessage)
        {
            if (Token.ValidateResponseMessage(ReceiveUID))
            {
                try
                {
                    // Set Response
                    if (Token.ResponseTaskSources.TryRemove(ReceiveUID, out TaskCompletionSource<IMessage> ResponseTask))
                        ResponseTask.TrySetResult(ReceiveMessage);
                }
                finally
                {
                    // Release CancelToken
                    if (Token.ResponseCancelTokens.TryRemove(ReceiveUID, out CancellationTokenSource CancelToken))
                        CancelToken.Dispose();
                }
            }
            else
            {
                // Handle Received Message
                if (RequestReceived is null)
                    Reply(Token, ReceiveUID, ErrorMessage.NotSupport, 3000);
                else
                    RequestReceived(this, new TcpRequest(this, Token, ReceiveUID, ReceiveMessage));
            }
        }
        protected internal virtual void Reply(TcpToken Token, int UID, IMessage Response, int TimeoutMileseconds)
        {
            // Check Response
            if (Response is null)
                Response = ErrorMessage.ReceivingNotSupport;

            // Timeout
            CancellationTokenSource CancelToken = new CancellationTokenSource(TimeoutMileseconds);

            // SendLock
            try
            {
                Token.WaitSend(CancelToken.Token);
            }
            catch (OperationCanceledException)
            {
                // Release CancelToken
                CancelToken.Dispose();

                Debug.WriteLine($"[Warn]Reply [{UID}]{Response.GetType().Name} Timeout.");
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
                    MessageStream = ProtocolHandler.Encode(Response);
                    MessageStream.Seek(0, SeekOrigin.Begin);
                }
                catch
                {
                    MessageStream = ErrorMessage.Encode(ErrorMessage.ReceivingEncodeException);
                }

                // Check Message's Context
                byte[] Buffer = Dequeue();

                int Length = MessageStream?.Read(Buffer, sizeof(int), BufferSize - sizeof(int)) ?? 0;
                if (Length == 0)
                {
                    Debug.WriteLine($"[Warn]{ProtocolHandler.GetType().Name} not support {Response.GetType().Name}.");

                    MessageStream = ErrorMessage.Encode(ErrorMessage.ReceivingNotSupport);
                    Length = MessageStream.Read(Buffer, sizeof(int), BufferSize - sizeof(int));
                }

                // UID
                PointerHelper.Copy(UID, Buffer, 0);
                Length += sizeof(int);

                // Send Datas
                try
                {
                    Debug.WriteLine($"[Info][{GetType().Name}]Reply {Response.GetType().Name} to [{Token.Address}].");

                    do
                    {
                        SocketAsyncEventArgs e = Dequeue(false);
                        e.UserToken = Token;
                        e.SetBuffer(Buffer, 0, Length);

                        if (!Token.SendAsync(e))
                            OnSendProcess(e);

                        Buffer = Dequeue();
                        Length = MessageStream.Read(Buffer, 0, BufferSize);

                    } while (Length > 0 & !Token.IsDisposed);
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
                    Enqueue(ref Buffer);
                }
            }
            finally
            {
                Token.ReleaseSend();
            }
        }

        protected sealed override void OnIOCompleted(object sender, SocketAsyncEventArgs e)
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
                    {
                        Debug.WriteLine($"[Error][{this.GetType().Name}Not support the operation {e.LastOperation}.");
                        throw new ArgumentException("The last operation completed on the socket was not a Send or Receive");
                    }
            }
        }

        protected virtual void OnDisconnected(TcpToken Token)
        {
            if (!IsDisposed &&
                !Token.IsDisposed)
                Disconnected?.Invoke(this, Token.Address);

            Token.Dispose();
        }

    }
}
