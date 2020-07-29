namespace MenthaAssembly.Network
{
    public class ErrorMessage : IMessage
    {
        public static ErrorMessage Timeout { get; } = new ErrorMessage("Timeout.");

        public static ErrorMessage ClientNotFound { get; } = new ErrorMessage("Client Not Found.");

        public static ErrorMessage NotSupport { get; } = new ErrorMessage("Not Support.");

        public static ErrorMessage Disconnected { get; } = new ErrorMessage("Disconnected.");

        public string Message { get; }

        public ErrorMessage(string Message)
        {
            this.Message = Message;
        }

    }
}
