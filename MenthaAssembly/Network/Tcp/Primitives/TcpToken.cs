﻿using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace MenthaAssembly.Network.Primitives
{
    public class TcpToken : IOCPToken
    {
        public ConcurrentDictionary<int, TaskCompletionSource<IMessage>> ResponseTaskSources { get; }

        public ConcurrentDictionary<int, CancellationTokenSource> ResponseCancelTokens { get; }

        public int PingCounter { set; get; }

        internal TcpToken(Socket Socket, bool ClientSide) : base(Socket)
        {
            ResponseTaskSources = new ConcurrentDictionary<int, TaskCompletionSource<IMessage>>();
            ResponseCancelTokens = new ConcurrentDictionary<int, CancellationTokenSource>();

            this.ClientSide = ClientSide;
            LastRequsetUID = ClientSide ? -1 : -2;
        }

        private int LastRequsetUID;
        public int NextUID()
        {
            Interlocked.Add(ref LastRequsetUID, 2);
            return LastRequsetUID;
        }

        private readonly bool ClientSide;
        public bool ValidateResponseMessage(int MessageUID)
            => ((MessageUID & 1) > 0) == ClientSide && MessageUID <= LastRequsetUID;

        public int GetLastUID()
            => LastRequsetUID;

        internal bool IsDisposed = false;
        public override void Dispose()
        {
            base.Dispose();

            if (IsDisposed)
                return;

            try
            {
                // Dispose Response Task
                foreach (TaskCompletionSource<IMessage> Task in ResponseTaskSources.Values)
                    Task.TrySetResult(ErrorMessage.Disconnected);
                ResponseTaskSources.Clear();

                foreach (CancellationTokenSource Token in ResponseCancelTokens.Values)
                    Token.Dispose();
                ResponseCancelTokens.Clear();
            }
            finally
            {
                IsDisposed = true;
            }
        }

    }
}
