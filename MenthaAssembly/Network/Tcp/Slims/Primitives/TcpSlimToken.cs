using System.Collections.Concurrent;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace MenthaAssembly.Network.Primitives
{
    public class TcpSlimToken : ITcpToken
    {
        public ConcurrentDictionary<int, TaskCompletionSource<IMessage>> ResponseTaskSources { get; }

        public ConcurrentDictionary<int, CancellationTokenSource> ResponseCancelTokens { get; }

        private TcpStream _Stream;
        public TcpStream Stream => _Stream;

        private IPEndPoint _Address;
        public IPEndPoint Address => _Address;

        internal TcpSlimToken(bool ClientSide)
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

        public int GetLastUID()
            => LastRequsetUID;

        private readonly bool ClientSide;
        public bool ValidateResponseMessage(int MessageUID)
            => ((MessageUID & 1) > 0) == ClientSide && MessageUID <= LastRequsetUID;

        public void Prepare(TcpStream Stream)
        {
            _Stream = Stream;
            _Address = (IPEndPoint)Stream.Socket.RemoteEndPoint;
        }
        public void Clear()
        {
            if (_Stream != null)
            {
                _Stream.Dispose();
                _Stream = null;
            }

            _Address = null;

            // Clear Response Task
            foreach (TaskCompletionSource<IMessage> Task in ResponseTaskSources.Values)
                Task.TrySetResult(ErrorMessage.Disconnected);
            ResponseTaskSources.Clear();

            foreach (CancellationTokenSource Token in ResponseCancelTokens.Values)
                Token.Dispose();
            ResponseCancelTokens.Clear();

            LastRequsetUID = ClientSide ? -1 : -2;
        }

    }
}
