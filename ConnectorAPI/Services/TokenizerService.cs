using System.Security.Cryptography;
using ConnectorAPI.DbContexts.ConnectorDb;
using ConnectorAPI.Services;
using Microsoft.AspNetCore.Identity;

namespace ConnectorAPI;

// Implemented a simple Deffie Hellman Exchange
public class TokenizerService
{

    private readonly HttpContext _context;
    private readonly ILogger<TokenizerService> _logger;
    private readonly UserManager<User> _userManager;
    private readonly TokenStorageService _tokenStorage;

    public TokenizerService(IHttpContextAccessor httpContextAccessor, ILogger<TokenizerService> logger, UserManager<User> userManager, TokenStorageService tokenStorage)
    {
        if (httpContextAccessor.HttpContext is null) throw new ArgumentNullException(nameof(httpContextAccessor.HttpContext));

        _context = httpContextAccessor.HttpContext;
        _logger = logger;
        _userManager = userManager;
        _tokenStorage = tokenStorage;
    }

    public (int, int) GeneratePublicKeys()
    {
        var userId = _userManager.GetUserId(_context.User);
        if (userId is null) throw new ArgumentNullException("userId");

        var p1 = GeneratePrime();
        var p2 = GeneratePrime();

        _tokenStorage.SetSharedKeys(userId, p1, p2);

        return (p1, p2);
    }

    public int GeneratePrivateKey()
    {
        var userId = _userManager.GetUserId(_context.User);
        var ipAddr = _context.Connection.RemoteIpAddress;

        if (userId is null) throw new ArgumentNullException("userId");
        if (ipAddr is null) throw new ArgumentNullException("ipAddr");

        var pk = GeneratePrime();

        _tokenStorage.SetPrivateKey(userId, pk);
        _tokenStorage.SetIPAdress(userId, ipAddr);

        return pk;
    }

    public int GenerateCombinedKey()
    {

        var userId = _userManager.GetUserId(_context.User);
        if (userId is null) throw new ArgumentNullException("userId");

        var token = _tokenStorage.GetTokenInfo(userId);
        int secret = (int)(Math.Pow(token.SharedKey2, token.PrivateKey) % token.SharedKey1);

        return secret;
    }

    public double GenerateSecretKey(double combinedKey)
    {
        var userId = _userManager.GetUserId(_context.User);
        if (userId is null) throw new ArgumentNullException("userId");

        var token = _tokenStorage.GetTokenInfo(userId);
        uint secret = (uint)(Math.Pow(combinedKey, token.PrivateKey) % token.SharedKey1);

        _tokenStorage.SetComputedKey(userId, secret);

        return secret;
    }

    private int GeneratePrime()
    {
        // TODO: Implement a Sieve Of Eratosthenes
        return RandomNumberGenerator.GetInt32(int.MaxValue);
    }

    public class NullIPAdressException : Exception
    {
        public NullIPAdressException() : base("IP Adress cannot be null")
        {

        }
    }

}
