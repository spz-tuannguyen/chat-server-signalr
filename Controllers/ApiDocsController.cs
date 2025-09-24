using chat_server.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace chat_server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ApiDocsController : ControllerBase
{
    /// <summary>
    /// Lấy documentation đầy đủ cho SignalR API
    /// </summary>
    [HttpGet("signalr")]
    public IActionResult GetSignalRApiDocumentation()
    {
        var apiDoc = new
        {
            HubUrl = SignalRContract.HubPath,
            Description = "Real-time chat server với auto-reconnection support",

            ClientMethods = new
            {
                Description = "Methods mà client có thể gọi lên server",
                Methods = new
                {
                    JoinChat = new
                    {
                        Parameters = new[] { "username: string", "room?: string" },
                        Description = "Join chat với username và optional room (default: General)"
                    },
                    SendMessage = new
                    {
                        Parameters = new[] { "message: string" },
                        Description = "Gửi message tới room hiện tại"
                    },
                    JoinRoom = new
                    {
                        Parameters = new[] { "roomName: string" },
                        Description = "Chuyển sang room khác"
                    },
                    SendPrivateMessage = new
                    {
                        Parameters = new[] { "targetUsername: string", "message: string" },
                        Description = "Gửi tin nhắn riêng cho user cụ thể"
                    }
                }
            },

            ServerEvents = new
            {
                Description = "Events mà server gửi xuống client",
                Events = new
                {
                    ReceiveMessage = new
                    {
                        Parameters = "MessageData",
                        Description = "Nhận tin nhắn từ room",
                        DataStructure = new
                        {
                            Username = "string",
                            Message = "string",
                            Timestamp = "DateTime",
                            Room = "string"
                        }
                    },
                    UserJoined = new
                    {
                        Parameters = new[] { "username: string", "room: string" },
                        Description = "User mới join room"
                    },
                    UserLeft = new
                    {
                        Parameters = new[] { "username: string", "room: string" },
                        Description = "User rời room"
                    },
                    UsersInRoom = new
                    {
                        Parameters = new[] { "users: string[]" },
                        Description = "Danh sách users hiện tại trong room"
                    },
                    ReceivePrivateMessage = new
                    {
                        Parameters = "PrivateMessageData",
                        Description = "Nhận tin nhắn riêng tư",
                        DataStructure = new
                        {
                            From = "string",
                            Message = "string",
                            Timestamp = "DateTime",
                            IsPrivate = "true"
                        }
                    },
                    PrivateMessageSent = new
                    {
                        Parameters = new[] { "targetUsername: string", "message: string" },
                        Description = "Xác nhận tin nhắn riêng đã gửi thành công"
                    },
                    Error = new
                    {
                        Parameters = new[] { "errorMessage: string" },
                        Description = "Thông báo lỗi từ server"
                    }
                }
            },

            ConnectionConfig = new
            {
                AutoReconnect = "Intervals: 0ms, 2s, 10s, 30s",
                KeepAliveInterval = "15 seconds",
                ClientTimeout = "30 seconds",
                HandshakeTimeout = "15 seconds",
                RateLimit = "60 requests per minute"
            },

            ExampleUsage = new
            {
                JavaScript = new
                {
                    Connection = @"
const connection = new signalR.HubConnectionBuilder()
    .withUrl('/chatHub')
    .withAutomaticReconnect([0, 2000, 10000, 30000])
    .build();",

                    EventListeners = @"
// Listen to server events
connection.on('ReceiveMessage', (messageData) => {
    console.log('Message:', messageData);
});

connection.on('UserJoined', (username, room) => {
    console.log(`${username} joined ${room}`);
});

connection.on('Error', (errorMessage) => {
    console.error('Error:', errorMessage);
});",

                    MethodCalls = @"
// Call server methods
await connection.invoke('JoinChat', 'MyUsername', 'General');
await connection.invoke('SendMessage', 'Hello everyone!');
await connection.invoke('SendPrivateMessage', 'TargetUser', 'Private msg');"
                }
            }
        };

        return Ok(apiDoc);
    }

    /// <summary>
    /// Lấy constants để client dễ reference
    /// </summary>
    [HttpGet("signalr/constants")]
    public IActionResult GetSignalRConstants()
    {
        return Ok(new
        {
            ServerEvents = new
            {
                ReceiveMessage = SignalRContract.ServerEvents.ReceiveMessage,
                UserJoined = SignalRContract.ServerEvents.UserJoined,
                UserLeft = SignalRContract.ServerEvents.UserLeft,
                UsersInRoom = SignalRContract.ServerEvents.UsersInRoom,
                ReceivePrivateMessage = SignalRContract.ServerEvents.ReceivePrivateMessage,
                PrivateMessageSent = SignalRContract.ServerEvents.PrivateMessageSent,
                Error = SignalRContract.ServerEvents.Error
            },

            ClientMethods = new
            {
                JoinChat = SignalRContract.ClientMethods.JoinChat,
                SendMessage = SignalRContract.ClientMethods.SendMessage,
                JoinRoom = SignalRContract.ClientMethods.JoinRoom,
                SendPrivateMessage = SignalRContract.ClientMethods.SendPrivateMessage
            },

            HubPath = SignalRContract.HubPath
        });
    }
}
