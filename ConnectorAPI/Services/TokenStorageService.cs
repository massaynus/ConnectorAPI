using System.Net;

namespace ConnectorAPI.Services;

public class TokenStorageService
{

    private readonly Dictionary<string, TokenInfo> userTokens = new();

    public TokenInfo GetTokenInfo(string userId)
    {
        return GetOrInitUserId(userId);
    }

    public void SetSharedKeys(string userId, int k1, int k2)
    {
        var token = GetOrInitUserId(userId);
        token.SharedKey1 = k1;
        token.SharedKey2 = k2;
    }

    public void SetPrivateKey(string userId, int privateKey)
    {
        var token = GetOrInitUserId(userId);
        token.PrivateKey = privateKey;
    }

    public void SetIPAdress(string userId, IPAddress addr)
    {
        var token = GetOrInitUserId(userId);
        token.IPAddress = addr;
    }

    public void SetComputedKey(string userId, uint key)
    {
        var token = GetOrInitUserId(userId);
        token.ComputedKey = key;
    }

    private TokenInfo GetOrInitUserId(string userId)
    {
        if (!userTokens.ContainsKey(userId)) userTokens.Add(userId, new());
        return userTokens[userId];
    }

}

public class TokenInfo
{
    public int SharedKey1 { get; set; }
    public int SharedKey2 { get; set; }
    public int PrivateKey { get; set; }
    public IPAddress? IPAddress { get; set; }
    public uint ComputedKey { get; set; }
}
