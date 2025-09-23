using System.Collections.Concurrent;

namespace chat_server.Middleware;

public class RateLimitingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly int _maxRequestsPerMinute;
    private static readonly ConcurrentDictionary<string, UserRequestInfo> UserRequests = new();

    public RateLimitingMiddleware(RequestDelegate next, int maxRequestsPerMinute = 60)
    {
        _next = next;
        _maxRequestsPerMinute = maxRequestsPerMinute;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var clientIp = GetClientIpAddress(context);

        if (!IsRequestAllowed(clientIp))
        {
            context.Response.StatusCode = 429; // Too Many Requests
            await context.Response.WriteAsync("Rate limit exceeded. Please try again later.");
            return;
        }

        await _next(context);
    }

    private bool IsRequestAllowed(string clientIp)
    {
        var now = DateTime.UtcNow;
        var userInfo = UserRequests.GetOrAdd(clientIp, _ => new UserRequestInfo());

        lock (userInfo)
        {
            // Clean old requests (older than 1 minute)
            userInfo.Requests.RemoveAll(r => now - r > TimeSpan.FromMinutes(1));

            if (userInfo.Requests.Count >= _maxRequestsPerMinute)
            {
                return false;
            }

            userInfo.Requests.Add(now);
            return true;
        }
    }

    private string GetClientIpAddress(HttpContext context)
    {
        return context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }

    private class UserRequestInfo
    {
        public List<DateTime> Requests { get; } = new();
    }
}
