using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using chat_server.Contracts;

namespace chat_server.Hubs;

public class ChatHub : Hub<IChatClient>
{
    // Lưu trữ thông tin người dùng đang kết nối
    private static readonly ConcurrentDictionary<string, UserInfo> ConnectedUsers = new();

    public async Task JoinChat(string username, string? room = null)
    {
        var userInfo = new UserInfo
        {
            ConnectionId = Context.ConnectionId,
            Username = username,
            Room = room ?? "General",
            ConnectedAt = DateTime.UtcNow
        };

        ConnectedUsers.TryAdd(Context.ConnectionId, userInfo);

        // Join room
        await Groups.AddToGroupAsync(Context.ConnectionId, userInfo.Room);

        // Thông báo người dùng mới join với type-safe method
        await Clients.Group(userInfo.Room).UserJoined(username, userInfo.Room);

        // Gửi danh sách users hiện tại trong room
        var usersInRoom = ConnectedUsers.Values
            .Where(u => u.Room == userInfo.Room)
            .Select(u => u.Username)
            .ToList();

        await Clients.Group(userInfo.Room).UsersInRoom(usersInRoom);

        Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] User '{username}' joined room '{userInfo.Room}'");
    }

    public async Task SendMessage(string message)
    {
        if (ConnectedUsers.TryGetValue(Context.ConnectionId, out var userInfo))
        {
            var messageData = new MessageData
            {
                Username = userInfo.Username,
                Message = message,
                Timestamp = DateTime.UtcNow,
                Room = userInfo.Room
            };

            await Clients.Group(userInfo.Room).ReceiveMessage(messageData);
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] {userInfo.Username} in {userInfo.Room}: {message}");
        }
    }

    public async Task JoinRoom(string roomName)
    {
        if (ConnectedUsers.TryGetValue(Context.ConnectionId, out var userInfo))
        {
            // Leave current room
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, userInfo.Room);
            await Clients.Group(userInfo.Room).UserLeft(userInfo.Username, userInfo.Room);

            // Gửi danh sách users updated cho room cũ
            var usersInOldRoom = ConnectedUsers.Values
                .Where(u => u.Room == userInfo.Room && u.ConnectionId != Context.ConnectionId)
                .Select(u => u.Username)
                .ToList();
            await Clients.Group(userInfo.Room).UsersInRoom(usersInOldRoom);

            // Join new room
            userInfo.Room = roomName;
            await Groups.AddToGroupAsync(Context.ConnectionId, roomName);
            await Clients.Group(roomName).UserJoined(userInfo.Username, roomName);

            // Send updated user list
            var usersInRoom = ConnectedUsers.Values
                .Where(u => u.Room == roomName)
                .Select(u => u.Username)
                .ToList();

            await Clients.Group(roomName).UsersInRoom(usersInRoom);
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] User '{userInfo.Username}' moved to room '{roomName}'");
        }
    }

    public async Task SendPrivateMessage(string targetUsername, string message)
    {
        if (ConnectedUsers.TryGetValue(Context.ConnectionId, out var senderInfo))
        {
            var targetUser = ConnectedUsers.Values.FirstOrDefault(u => u.Username == targetUsername);

            if (targetUser != null)
            {
                var messageData = new PrivateMessageData
                {
                    From = senderInfo.Username,
                    Message = message,
                    Timestamp = DateTime.UtcNow,
                    IsPrivate = true
                };

                await Clients.Client(targetUser.ConnectionId).ReceivePrivateMessage(messageData);
                await Clients.Caller.PrivateMessageSent(targetUsername, message);

                Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Private message from {senderInfo.Username} to {targetUsername}: {message}");
            }
            else
            {
                await Clients.Caller.Error($"User '{targetUsername}' not found");
            }
        }
    }

    public override async Task OnConnectedAsync()
    {
        Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Client connected: {Context.ConnectionId}");
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        if (ConnectedUsers.TryRemove(Context.ConnectionId, out var userInfo))
        {
            await Clients.Group(userInfo.Room).UserLeft(userInfo.Username, userInfo.Room);

            // Send updated user list
            var usersInRoom = ConnectedUsers.Values
                .Where(u => u.Room == userInfo.Room)
                .Select(u => u.Username)
                .ToList();

            await Clients.Group(userInfo.Room).UsersInRoom(usersInRoom);

            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] User '{userInfo.Username}' disconnected from room '{userInfo.Room}'");

            if (exception != null)
            {
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Disconnect reason: {exception.Message}");
            }
        }

        await base.OnDisconnectedAsync(exception);
    }
}

public class UserInfo
{
    public string ConnectionId { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Room { get; set; } = "General";
    public DateTime ConnectedAt { get; set; }
}
