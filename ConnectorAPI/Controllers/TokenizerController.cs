using ConnectorAPI.DbContexts;
using ConnectorAPI.DbContexts.ConnectorDb;
using ConnectorAPI.DTOs;
using ConnectorAPI.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.Operations;

namespace ConnectorAPI.Controllers;

[ApiController]
[Route("[controller]")]
[AllowAnonymous]
public class TokenizerController : ControllerBase
{
    private readonly TokenizerService _tokenizer;
    private readonly CryptoService _cryptoService;
    private readonly UserManager<User> _userManager;
    private readonly ConnectorDbContext _db;

    public TokenizerController(TokenizerService tokenizer, CryptoService cryptoService, UserManager<User> userManager, ConnectorDbContext db)
    {
        _tokenizer = tokenizer;
        _cryptoService = cryptoService;
        _userManager = userManager;
        _db = db;
    }

    [HttpGet(nameof(GetPublicKeys))]
    public async Task<ActionResult<TokenExchangeInfo>> GetPublicKeys()
    {
        await Task.CompletedTask;
        var pks = _tokenizer.GeneratePublicKeys();

        _tokenizer.GeneratePrivateKey();

        return Ok(new TokenExchangeInfo(
            PublicKey1: pks.Item1,
            PublicKey2: pks.Item2,
            CombinedKey: _tokenizer.GenerateCombinedKey()
        ));
    }

    [HttpPost(nameof(Encrypt))]
    public string Encrypt([FromBody] string input)
    {
        var encrypted = _cryptoService.Encrypt(input);
        var decrypted = _cryptoService.Decrypt(encrypted);

        return $"Encrypted: {encrypted}\nDecrypted:{decrypted}";

    }

    [Authorize]
    [HttpPatch(nameof(SetRSAPublicKey))]
    public async Task<ActionResult> SetRSAPublicKey([FromBody] string pubKey)
    {
        var user = await _userManager.GetUserAsync(HttpContext.User);
        if (user is null) return Forbid();

        _db.Attach(user);
        user.RSAPublicKey = pubKey;

        await _db.SaveChangesAsync();

        return Ok();
    }

}
