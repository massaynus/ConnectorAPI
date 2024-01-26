using ConnectorAPI.DTOs;
using ConnectorAPI.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ConnectorAPI.Controllers;

[ApiController]
[Route("[controller]")]
[AllowAnonymous]
public class TokenizerController : ControllerBase
{
    private readonly TokenizerService _tokenizer;
    private readonly CryptoService _cryptoService;

    public TokenizerController(TokenizerService tokenizer, CryptoService cryptoService)
    {
        _tokenizer = tokenizer;
        _cryptoService = cryptoService;
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

}
