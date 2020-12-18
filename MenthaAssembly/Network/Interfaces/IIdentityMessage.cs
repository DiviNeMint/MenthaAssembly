namespace MenthaAssembly.Network
{
    public interface IIdentityMessage : IMessage
    {
        int UID { internal set; get; }

    }
}
