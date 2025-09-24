using chat_server.Hubs;
using chat_server.Services;
using chat_server.Middleware;
using System.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

// Đọc cấu hình
var chatServerOptions = builder.Configuration.GetSection(ChatServerOptions.SectionName).Get<ChatServerOptions>() ?? new ChatServerOptions();
var signalROptions = builder.Configuration.GetSection(SignalROptions.SectionName).Get<SignalROptions>() ?? new SignalROptions();

// Đăng ký options
builder.Services.Configure<ChatServerOptions>(builder.Configuration.GetSection(ChatServerOptions.SectionName));
builder.Services.Configure<SignalROptions>(builder.Configuration.GetSection(SignalROptions.SectionName));

// Thêm controllers để serve API documentation
builder.Services.AddControllers();

// Thêm SignalR services với cấu hình từ appsettings
builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = signalROptions.EnableDetailedErrors;
    options.KeepAliveInterval = TimeSpan.FromSeconds(signalROptions.KeepAliveIntervalSeconds);
    options.ClientTimeoutInterval = TimeSpan.FromSeconds(signalROptions.ClientTimeoutSeconds);
    options.HandshakeTimeout = TimeSpan.FromSeconds(signalROptions.HandshakeTimeoutSeconds);

    // Giới hạn kích thước message buffer để tránh memory leak
    options.MaximumReceiveMessageSize = chatServerOptions.MaxMessageLength;
});

// Thêm CORS để cho phép client kết nối
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.SetIsOriginAllowed(_ => true) // Cho phép mọi origin (cho development)
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials(); // Quan trọng cho SignalR
    });
});

// Thêm logging
builder.Services.AddLogging(logging =>
{
    logging.AddConsole();
    logging.SetMinimumLevel(LogLevel.Information);
});

var app = builder.Build();

// Sử dụng CORS
app.UseCors("AllowAll");

// Serve static files từ wwwroot
app.UseStaticFiles();

// Sử dụng middleware rate limiting
app.UseMiddleware<RateLimitingMiddleware>(60); // 60 requests per minute

// Map SignalR hub
app.MapHub<ChatHub>("/chatHub");

// Map controllers để serve API documentation
app.MapControllers();

app.Urls.Add("http://0.0.0.0:5000");

// Endpoint để kiểm tra server status
app.MapGet("/", () => new
{
    Status = "Chat Server is running",
    Timestamp = DateTime.UtcNow,
    SignalRHub = "/chatHub",
    Documentation = new
    {
        JSON = "/api/apidocs/signalr",
        Constants = "/api/apidocs/signalr/constants"
    },
    Configuration = new
    {
        MaxConnections = chatServerOptions.MaxConcurrentConnections,
        DefaultRoom = chatServerOptions.DefaultRoom,
        KeepAliveInterval = $"{signalROptions.KeepAliveIntervalSeconds}s",
        ClientTimeout = $"{signalROptions.ClientTimeoutSeconds}s"
    }
});

// Health check endpoint
app.MapGet("/health", () => new
{
    Status = "Healthy",
    Timestamp = DateTime.UtcNow,
    Uptime = DateTime.UtcNow - Process.GetCurrentProcess().StartTime
});

// API để lấy rooms hiện có
app.MapGet("/api/rooms", () => new
{
    Rooms = new[] { "General", "Tech", "Random", "Gaming" },
    DefaultRoom = chatServerOptions.DefaultRoom
});

Console.WriteLine("=== Chat Server Starting ===");
Console.WriteLine($"SignalR Hub URL: /chatHub");
Console.WriteLine($"Keep Alive: {signalROptions.KeepAliveIntervalSeconds}s");
Console.WriteLine($"Client Timeout: {signalROptions.ClientTimeoutSeconds}s");
Console.WriteLine($"Max Connections: {chatServerOptions.MaxConcurrentConnections}");
Console.WriteLine($"Default Room: {chatServerOptions.DefaultRoom}");
Console.WriteLine("==============================");

app.Run();
