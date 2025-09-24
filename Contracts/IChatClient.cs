using chat_server.Hubs;

namespace chat_server.Contracts;

/// <summary>
/// Định nghĩa tất cả events mà server có thể gửi xuống client
/// </summary>
public interface IChatClient
{
    /// <summary>
    /// Nhận tin nhắn từ room
    /// </summary>
    Task ReceiveMessage(MessageData message);

    /// <summary>
    /// Thông báo user mới join room
    /// </summary>
    Task UserJoined(string username, string room);

    /// <summary>
    /// Thông báo user rời room
    /// </summary>
    Task UserLeft(string username, string room);

    /// <summary>
    /// Danh sách users hiện tại trong room
    /// </summary>
    Task UsersInRoom(List<string> users);

    /// <summary>
    /// Nhận tin nhắn riêng tư
    /// </summary>
    Task ReceivePrivateMessage(PrivateMessageData message);

    /// <summary>
    /// Xác nhận tin nhắn riêng đã gửi
    /// </summary>
    Task PrivateMessageSent(string targetUsername, string message);

    /// <summary>
    /// Thông báo lỗi
    /// </summary>
    Task Error(string errorMessage);
}

/// <summary>
/// Data structure cho tin nhắn thông thường
/// </summary>
public class MessageData
{
    public string Username { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string Room { get; set; } = string.Empty;
}

/// <summary>
/// Data structure cho tin nhắn riêng tư
/// </summary>
public class PrivateMessageData
{
    public string From { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public bool IsPrivate { get; set; } = true;
}
