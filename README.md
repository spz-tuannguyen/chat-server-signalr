# Chat Server với SignalR

## Mô tả

Đây là một chat server được xây dựng bằng ASP.NET Core và SignalR, được thiết kế với trọng tâm là tính ổn định và khả năng tự động kết nối lại.

## Tính năng

### Tính năng cơ bản

- ✅ Gửi/nhận tin nhắn real-time
- ✅ Hỗ trợ nhiều phòng chat (rooms)
- ✅ Tin nhắn riêng tư (private messages)
- ✅ Danh sách người dùng online
- ✅ Thông báo khi có người join/leave

### Tính năng ổn định

- ✅ Auto-reconnection khi bị disconnect
- ✅ Keep-alive mechanism (15s interval)
- ✅ Client timeout handling (30s)
- ✅ Rate limiting (60 requests/minute)
- ✅ Error handling và logging chi tiết
- ✅ Health check endpoint

## Cách chạy server

### 1. Restore packages

```bash
dotnet restore
```

### 2. Chạy server

```bash
dotnet run
```

Server sẽ chạy trên: `http://localhost:5289`

### 3. Kiểm tra server

- Status: `GET /`
- Health check: `GET /health`
- Available rooms: `GET /api/rooms`

## SignalR Hub API

### Connection URL

```
/chatHub
```

### Client Methods (Gửi từ client lên server)

#### 1. Join Chat

```javascript
connection.invoke("JoinChat", username, room);
```

- `username`: Tên người dùng
- `room`: Tên phòng (optional, mặc định là "General")

#### 2. Send Message

```javascript
connection.invoke("SendMessage", message);
```

- `message`: Nội dung tin nhắn

#### 3. Join Room

```javascript
connection.invoke("JoinRoom", roomName);
```

- `roomName`: Tên phòng muốn chuyển đến

#### 4. Send Private Message

```javascript
connection.invoke("SendPrivateMessage", targetUsername, message);
```

- `targetUsername`: Tên người nhận
- `message`: Nội dung tin nhắn riêng

### Server Methods (Server gửi xuống client)

#### 1. Receive Message

```javascript
connection.on("ReceiveMessage", (messageData) => {
  // messageData: { Username, Message, Timestamp, Room }
});
```

#### 2. User Joined

```javascript
connection.on("UserJoined", (username, room) => {
  // Thông báo có người mới join
});
```

#### 3. User Left

```javascript
connection.on("UserLeft", (username, room) => {
  // Thông báo có người leave
});
```

#### 4. Users In Room

```javascript
connection.on("UsersInRoom", (usersList) => {
  // Danh sách users trong phòng hiện tại
});
```

#### 5. Receive Private Message

```javascript
connection.on("ReceivePrivateMessage", (messageData) => {
  // messageData: { From, Message, Timestamp, IsPrivate }
});
```

#### 6. Error

```javascript
connection.on("Error", (errorMessage) => {
  // Xử lý lỗi
});
```

## Cấu hình

Cấu hình server trong `appsettings.json`:

```json
{
  "ChatServer": {
    "MaxConcurrentConnections": 1000,
    "MessageRateLimit": 10,
    "MaxMessageLength": 1000,
    "DefaultRoom": "General",
    "EnablePrivateMessages": true,
    "LogMessages": true
  },
  "SignalR": {
    "KeepAliveIntervalSeconds": 15,
    "ClientTimeoutSeconds": 30,
    "HandshakeTimeoutSeconds": 15,
    "EnableDetailedErrors": true
  }
}
```

## Example Client Code (JavaScript)

```javascript
// Kết nối
const connection = new signalR.HubConnectionBuilder()
  .withUrl("https://localhost:5001/chatHub")
  .withAutomaticReconnect([0, 2000, 10000, 30000]) // Auto-reconnect
  .build();

// Xử lý events
connection.on("ReceiveMessage", (messageData) => {
  console.log(`${messageData.Username}: ${messageData.Message}`);
});

connection.on("UserJoined", (username, room) => {
  console.log(`${username} joined ${room}`);
});

// Bắt đầu kết nối
connection
  .start()
  .then(() => {
    console.log("Connected to chat server");

    // Join chat
    connection.invoke("JoinChat", "YourUsername", "General");

    // Send message
    connection.invoke("SendMessage", "Hello everyone!");
  })
  .catch((err) => console.error(err));

// Auto-reconnection handling
connection.onreconnecting(() => {
  console.log("Reconnecting...");
});

connection.onreconnected(() => {
  console.log("Reconnected!");
  // Re-join room if needed
  connection.invoke("JoinChat", "YourUsername", "General");
});

connection.onclose(() => {
  console.log("Connection closed");
});
```

## Logging

Server sẽ log các events sau:

- User join/leave
- Messages sent
- Connection/disconnection events
- Errors và exceptions

## Production Notes

1. **CORS**: Trong production, nên cấu hình CORS cụ thể thay vì `AllowAnyOrigin()`
2. **HTTPS**: Luôn sử dụng HTTPS trong production
3. **Rate Limiting**: Có thể tùy chỉnh rate limiting theo nhu cầu
4. **Database**: Có thể thêm database để lưu lịch sử chat
5. **Authentication**: Có thể thêm authentication/authorization

## Troubleshooting

1. **Connection failed**: Kiểm tra CORS settings
2. **Auto-reconnect không hoạt động**: Đảm bảo client có cấu hình `withAutomaticReconnect()`
3. **Messages bị mất**: Kiểm tra network và server logs
4. **High memory usage**: Kiểm tra số lượng concurrent connections
