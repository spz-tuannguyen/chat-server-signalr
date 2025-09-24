namespace chat_server.Contracts;

/// <summary>
/// Constants cho tất cả SignalR events và methods để client dễ reference
/// </summary>
public static class SignalRContract
{
    /// <summary>
    /// Events mà server gửi xuống client
    /// </summary>
    public static class ServerEvents
    {
        public const string ReceiveMessage = nameof(IChatClient.ReceiveMessage);
        public const string UserJoined = nameof(IChatClient.UserJoined);
        public const string UserLeft = nameof(IChatClient.UserLeft);
        public const string UsersInRoom = nameof(IChatClient.UsersInRoom);
        public const string ReceivePrivateMessage = nameof(IChatClient.ReceivePrivateMessage);
        public const string PrivateMessageSent = nameof(IChatClient.PrivateMessageSent);
        public const string Error = nameof(IChatClient.Error);
    }

    /// <summary>
    /// Methods mà client có thể gọi lên server
    /// </summary>
    public static class ClientMethods
    {
        public const string JoinChat = "JoinChat";
        public const string SendMessage = "SendMessage";
        public const string JoinRoom = "JoinRoom";
        public const string SendPrivateMessage = "SendPrivateMessage";
    }

    /// <summary>
    /// Hub endpoint path
    /// </summary>
    public const string HubPath = "/chatHub";
}
