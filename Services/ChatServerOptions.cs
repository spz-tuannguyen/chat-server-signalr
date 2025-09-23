namespace chat_server.Services;

public class ChatServerOptions
{
    public const string SectionName = "ChatServer";

    public int MaxConcurrentConnections { get; set; } = 1000;
    public int MessageRateLimit { get; set; } = 10;
    public int MaxMessageLength { get; set; } = 1000;
    public string DefaultRoom { get; set; } = "General";
    public bool EnablePrivateMessages { get; set; } = true;
    public bool LogMessages { get; set; } = true;
}

public class SignalROptions
{
    public const string SectionName = "SignalR";

    public int KeepAliveIntervalSeconds { get; set; } = 15;
    public int ClientTimeoutSeconds { get; set; } = 30;
    public int HandshakeTimeoutSeconds { get; set; } = 15;
    public bool EnableDetailedErrors { get; set; } = true;
}
