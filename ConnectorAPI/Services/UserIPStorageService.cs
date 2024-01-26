using System.Collections.Concurrent;
using Microsoft.Extensions.Caching.Memory;

namespace ConnectorAPI.Services;

public class UserIPStorageService
{
    private readonly ConcurrentDictionary<string, string> userIPs = new();

    public void RegisterIP(string username, string ip)
    {
        userIPs.AddOrUpdate(username, ip, (_, __) => ip);
    }

    public string GetIP(string username)
    {
        var success = userIPs.TryGetValue(username, out string? ip);

        if (success && ip is not null) return ip;
        else throw new KeyNotFoundException();
    }
}
