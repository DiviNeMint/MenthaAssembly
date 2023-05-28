using MenthaAssembly.IO;
using MenthaAssembly.Network.Primitives;
using System;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace MenthaAssembly.Network
{
    public abstract class TcpSlimBase : TcpBase<TcpSlimToken>
    {
        public event EventHandler<TcpSlimRequest> RequestReceived;

        private IProtocolCoder _Protocol;
        public IProtocolCoder Protocol
        {
            get => _Protocol;
            set
            {
                if (value is null)
                    throw new ArgumentNullException();

                _Protocol = value;
            }
        }

        internal TcpSlimBase(IProtocolCoder Protocol)
        {
            this.Protocol = Protocol;
            ReplyHandler = OnReplyProcess;
        }

        protected virtual IMessage Send(TcpSlimToken Token, IMessage Request, int TimeoutMileseconds)
        {
            TaskCompletionSource<IMessage> TaskToken = new TaskCompletionSource<IMessage>();

            // Timeout
            CancellationTokenSource CancelToken = new CancellationTokenSource(TimeoutMileseconds);
            CancelToken.Token.Register(() => TaskToken.TrySetResult(ErrorMessage.Timeout), false);

            // Encode Message
            Stream MessageStream;
            try
            {
                MessageStream = _Protocol.Encode(Request);
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

            // UID
            int UID = Token.NextUID();

            // Build Full Message
            Stream RequestStream = new ConcatStream(BitConverter.GetBytes(UID), MessageStream);

            Token.ResponseTaskSources.AddOrUpdate(UID, TaskToken, (k, v) => TaskToken);
            Token.ResponseCancelTokens.AddOrUpdate(UID, CancelToken, (k, v) => CancelToken);

            // Send Datas
            try
            {
                Debug.WriteLine($"[Info][{GetType().Name}]Send {Request.GetType().Name} to [{Token.Address}].");
                Token.Stream.Write(RequestStream, CancelToken.Token);
            }
            catch (TaskCanceledException)
            {
                TaskToken.TrySetResult(ErrorMessage.OperationCanceled);

                // Release CancelToken
                CancelToken.Dispose();

                return TaskToken.Task.Result;
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
            catch
            {
                // Disconnect
                Token.Stream?.Dispose();
            }
            finally
            {
                RequestStream.Dispose();
            }

            TaskToken.Task.Wait();
            return TaskToken.Task.Result;
        }
        protected virtual async Task<IMessage> SendAsync(TcpSlimToken Token, IMessage Request, int TimeoutMileseconds)
        {
            TaskCompletionSource<IMessage> TaskToken = new TaskCompletionSource<IMessage>();

            // Timeout
            CancellationTokenSource CancelToken = new CancellationTokenSource(TimeoutMileseconds);
            CancelToken.Token.Register(() => TaskToken.TrySetResult(ErrorMessage.Timeout), false);

            // Encode Message
            Stream MessageStream;
            try
            {
                MessageStream = _Protocol.Encode(Request);
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

            // UID
            int UID = Token.NextUID();

            // Build Full Message
            Stream RequestStream = new ConcatStream(BitConverter.GetBytes(UID), MessageStream);

            Token.ResponseTaskSources.AddOrUpdate(UID, TaskToken, (k, v) => TaskToken);
            Token.ResponseCancelTokens.AddOrUpdate(UID, CancelToken, (k, v) => CancelToken);

            // Send Datas
            try
            {
                Debug.WriteLine($"[Info][{GetType().Name}]Send {Request.GetType().Name} to [{Token.Address}].");
                await Token.Stream.WriteAsync(RequestStream, CancelToken.Token);
            }
            catch (TaskCanceledException)
            {
                TaskToken.TrySetResult(ErrorMessage.OperationCanceled);

                // Release CancelToken
                CancelToken.Dispose();

                return TaskToken.Task.Result;
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
            catch
            {
                // Disconnect
                Token.Stream?.Dispose();
            }
            finally
            {
                RequestStream.Dispose();
            }

            return await TaskToken.Task;
        }

        private readonly Action<TcpSlimToken, int, IMessage> ReplyHandler;
        protected override void OnReceived(TcpSlimToken Token, Stream Stream)
        {
            int UID;
            IMessage Message;
            try
            {
                // Decode Message
                UID = Stream.Read<int>();
                Message = _Protocol.Decode(Stream);

                Debug.WriteLine($"[Info][{GetType().Name}]Receive {Message?.GetType().Name ?? "NullMessage"}.");
            }
            catch (Exception Ex)
            {
                if (!(Ex is IOException IOEx &&
                     IOEx.InnerException is ObjectDisposedException ODEx &&
                     ODEx.ObjectName == typeof(Socket).FullName) &&
                     Ex is not SocketException)
                    Debug.WriteLine($"[Error][{GetType().Name}]Decode exception.");

                Token.Stream?.Dispose();
                return;
            }

            if (Message != null)
                ReplyHandler.BeginInvoke(Token, UID, Message, ReplyHandler.EndInvoke, null);

            // Loop Receive
            Token.Stream.Receive();
        }

        protected virtual void OnReplyProcess(TcpSlimToken Token, int UID, IMessage Message)
        {
            if (Token.ValidateResponseMessage(UID))
            {
                try
                {
                    // Set Response
                    if (Token.ResponseTaskSources.TryRemove(UID, out TaskCompletionSource<IMessage> ResponseTask))
                        ResponseTask.TrySetResult(Message);
                }
                finally
                {
                    // Release CancelToken
                    if (Token.ResponseCancelTokens.TryRemove(UID, out CancellationTokenSource CancelToken))
                        CancelToken.Dispose();
                }
            }
            else
            {
                // Handle Received Message
                if (RequestReceived is null)
                    Reply(Token, UID, ErrorMessage.NotSupport, 3000);
                else
                    RequestReceived(this, new TcpSlimRequest(this, Token, UID, Message));
            }
        }
        protected internal virtual void Reply(TcpSlimToken Token, int UID, IMessage Response, int TimeoutMileseconds)
        {
            // Timeout
            CancellationTokenSource CancelToken = new CancellationTokenSource(TimeoutMileseconds);

            // Encode Message
            Stream MessageStream;
            try
            {
                MessageStream = _Protocol.Encode(Response);
                MessageStream.Seek(0, SeekOrigin.Begin);
            }
            catch
            {
                Debug.WriteLine($"[Error][{GetType().Name}]Encoding {Response.GetType().Name} hanppen exception.");


                // Release CancelToken
                CancelToken.Dispose();

                return;
            }

            // Build Full Message
            Stream RequestStream = new ConcatStream(BitConverter.GetBytes(UID), MessageStream);

            // Send Datas
            try
            {
                Debug.WriteLine($"[Info][{GetType().Name}]Send {Response.GetType().Name} to [{Token.Address}].");
                Token.Stream.Write(RequestStream, CancelToken.Token);
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
            catch
            {
                // Disconnect
                Token.Stream?.Dispose();
            }
            finally
            {
                RequestStream.Dispose();
            }
        }

        private bool IsDisposed = false;
        public override void Dispose()
        {
            if (IsDisposed)
                return;

            IsDisposed = true;

            _Protocol = null;
            base.Dispose();
        }

    }

}
