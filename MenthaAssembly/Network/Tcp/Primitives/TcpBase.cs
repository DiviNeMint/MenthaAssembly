using MenthaAssembly.Network.Messages;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices.ComTypes;
using System.Threading;
using System.Threading.Tasks;
using static MenthaAssembly.Network.Primitives.TcpBase;

namespace MenthaAssembly.Network.Primitives
{
    public abstract class TcpBase<Token> : IOCPBase
        where Token : ITcpToken
    {
        public event EventHandler<IPEndPoint> Disconnected;

        private ConcurrentQueue<Token> TokenPool = new ConcurrentQueue<Token>();

        protected Token DequeueToken()
        {
            return IsDisposed
                ? throw new ObjectDisposedException(GetType().Name)
                : TokenPool.TryDequeue(out Token Token) ? Token : CreateToken();
        }
        protected void EnqueueToken(Token Token)
        {
            if (IsDisposed)
                return;

            ResetToken(Token);
            TokenPool.Enqueue(Token);
        }

        protected abstract Token CreateToken();
        protected abstract void PrepareToken(Token Token, TcpStream Stream);
        protected abstract void ResetToken(Token Token);

        protected abstract void OnReceived(Token Token, Stream Stream);

        protected virtual void OnDisconnected(IPEndPoint Address)
            => Disconnected?.Invoke(this, Address);

        private bool IsDisposed = false;
        public override void Dispose()
        {
            if (IsDisposed)
                return;

            try
            {
                TokenPool = null;

                base.Dispose();
            }
            finally
            {
                IsDisposed = true;
            }

        }

    }

    public abstract class TcpBase : IDisposable
    {
        protected const int DefaultSendTimeout = 3000;

        public event EventHandler<TcpReceivedEventArgs> Received;

        public event EventHandler<EndPoint> Disconnected;

        public ISessionHandler SessionHandler { get; }

        protected TcpBase(ISessionHandler SessionHandler)
        {
            this.SessionHandler = SessionHandler;
        }

        protected IMessage Send(Session Session, IMessage Message, int TimeoutMileseconds)
        {
            // TaskToken
            TaskCompletionSource<IMessage> TaskToken = new();

            // TimeoutToken
            CancellationTokenSource CancelToken = new(TimeoutMileseconds);
            CancelToken.Token.Register(() => TaskToken.TrySetResult(ErrorMessage.Timeout), false);

            // Register Operator
            int UID = Session.RegisterResponseOperator(TaskToken, CancelToken);
            using Stream Stream = Session.Socket.GetStream();

            // Header
            try
            {
                Debug.WriteLine($"[Info][{GetType().Name}]Send {Message.GetType().Name} to [{Session.Socket.RemoteEndPoint}].");
                SessionHandler.EncodeHeader(Stream, UID, true, Message);
            }
            catch
            {
                Debug.WriteLine($"[Error][{GetType().Name}]Encoding {Message.GetType().Name} hanppen exception.");

                // Set Result
                TaskToken.TrySetResult(ErrorMessage.EncodeException);

                // Release CancelToken
                CancelToken.Dispose();

                return TaskToken.Task.Result;
            }

            // Message
            try
            {
                SessionHandler.EncodeMessage(Stream, Message);
            }
            catch (TaskCanceledException)
            {
                Debug.WriteLine($"[Error][{GetType().Name}]Encoding {Message.GetType().Name} hanppen exception.");
                TaskToken.TrySetResult(ErrorMessage.OperationCanceled);

                // Release CancelToken
                CancelToken.Dispose();

                return TaskToken.Task.Result;
            }
            catch (OperationCanceledException)
            {
                Debug.WriteLine($"[Error][{GetType().Name}]Encoding {Message.GetType().Name} hanppen exception.");
                // Release CancelToken
                CancelToken.Dispose();

                // Timeout
                return TaskToken.Task.Result;
            }
            catch (ObjectDisposedException)
            {
                Debug.WriteLine($"[Error][{GetType().Name}]Encoding {Message.GetType().Name} hanppen exception.");
                TaskToken.TrySetResult(ErrorMessage.Disconnected);

                // Release CancelToken
                CancelToken.Dispose();

                return TaskToken.Task.Result;
            }
            catch
            {
                Debug.WriteLine($"[Error][{GetType().Name}]Encoding {Message.GetType().Name} hanppen exception.");

                // Disconnect
                Session.Socket.Dispose();
            }

            TaskToken.Task.Wait();
            return TaskToken.Task.Result;
        }
        protected Task<IMessage> SendAsync(Session Session, IMessage Message, int TimeoutMileseconds)
        {
            // TaskToken
            TaskCompletionSource<IMessage> TaskToken = new();

            // TimeoutToken
            CancellationTokenSource CancelToken = new(TimeoutMileseconds);
            CancelToken.Token.Register(() => TaskToken.TrySetResult(ErrorMessage.Timeout), false);

            // Register Operator
            int UID = Session.RegisterResponseOperator(TaskToken, CancelToken);
            using Stream Stream = Session.Socket.GetStream();

            // Header
            try
            {
                Debug.WriteLine($"[Info][{GetType().Name}]Send {Message.GetType().Name} to [{Session.Socket.RemoteEndPoint}].");
                SessionHandler.EncodeHeader(Stream, UID, true, Message);
            }
            catch
            {
                Debug.WriteLine($"[Error][{GetType().Name}]Encoding {Message.GetType().Name} hanppen exception.");

                // Set Result
                TaskToken.TrySetResult(ErrorMessage.EncodeException);

                // Release CancelToken
                CancelToken.Dispose();

                return TaskToken.Task;
            }

            // Message
            try
            {
                SessionHandler.EncodeMessage(Stream, Message);
            }
            catch (TaskCanceledException)
            {
                Debug.WriteLine($"[Error][{GetType().Name}]Encoding {Message.GetType().Name} hanppen exception.");
                TaskToken.TrySetResult(ErrorMessage.OperationCanceled);

                // Release CancelToken
                CancelToken.Dispose();

                return TaskToken.Task;
            }
            catch (OperationCanceledException)
            {
                Debug.WriteLine($"[Error][{GetType().Name}]Encoding {Message.GetType().Name} hanppen exception.");
                // Release CancelToken
                CancelToken.Dispose();

                // Timeout
                return TaskToken.Task;
            }
            catch (ObjectDisposedException)
            {
                Debug.WriteLine($"[Error][{GetType().Name}]Encoding {Message.GetType().Name} hanppen exception.");
                TaskToken.TrySetResult(ErrorMessage.Disconnected);

                // Release CancelToken
                CancelToken.Dispose();

                return TaskToken.Task;
            }
            catch
            {
                Debug.WriteLine($"[Error][{GetType().Name}]Encoding {Message.GetType().Name} hanppen exception.");

                // Disconnect
                Session.Socket.Dispose();
            }

            return TaskToken.Task;
        }

        protected internal void Reply(Session Session, int UID, IMessage Message)
        {
            using Stream Stream = Session.Socket.GetStream();

            // Header
            try
            {
                Debug.WriteLine($"[Info][{GetType().Name}]Send {Message.GetType().Name} to [{Session.Socket.RemoteEndPoint}].");
                SessionHandler.EncodeHeader(Stream, UID, false, Message);
            }
            catch
            {
                Debug.WriteLine($"[Error][{GetType().Name}]Encoding {Message.GetType().Name} hanppen exception.");
                return;
            }

            // Message
            try
            {
                SessionHandler.EncodeMessage(Stream, Message);
            }
            catch
            {
                Debug.WriteLine($"[Error][{GetType().Name}]Encoding {Message.GetType().Name} hanppen exception.");

                // Disconnect
                Session.Socket.Dispose();
            }
        }

        protected virtual void OnReceived(TcpReceivedEventArgs e)
            => Received?.Invoke(this, e);

        protected virtual void OnDisconnected(EndPoint Address)
            => Disconnected?.Invoke(this, Address);

        public abstract void Dispose();

        protected internal class Session
        {
            public EndPoint Address { get; }

            public IOCPSocket Socket { get; }

            public ConcurrentDictionary<int, Tuple<TaskCompletionSource<IMessage>, CancellationTokenSource>> ResponseOperators { get; } = new();

            internal Session(IOCPSocket Socket)
            {
                this.Socket = Socket;
                Address = Socket.RemoteEndPoint;
                Random = new(Socket.RemoteEndPoint.GetHashCode());
            }

            private readonly Random Random;
            public int RegisterResponseOperator(TaskCompletionSource<IMessage> TaskToken, CancellationTokenSource CancelToken)
            {
                Tuple<TaskCompletionSource<IMessage>, CancellationTokenSource> Operator = Tuple.Create(TaskToken, CancelToken);
                int ID = Random.Next();
                while (!ResponseOperators.TryAdd(ID, Operator))
                    ID = Random.Next();

                return ID;
            }

        }

    }

}