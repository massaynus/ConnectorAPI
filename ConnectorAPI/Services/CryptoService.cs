using System.Security.Cryptography;
using System.Text;

namespace ConnectorAPI.Service;

public class CryptoService
{
    // We can generated a public/private key for each User and store them
    private readonly RSACryptoServiceProvider csp;
    private readonly RSAParameters publicKey;
    private readonly RSAParameters privateKey;

    public CryptoService()
    {
        csp = new();
        publicKey = csp.ExportParameters(false);
        privateKey = csp.ExportParameters(true);
    }


    public string Encrypt(string input)
    {
        csp.ImportParameters(publicKey);

        var inputBytes = Encoding.UTF8.GetBytes(input);
        var cypherData = csp.Encrypt(inputBytes, false);

        return Convert.ToBase64String(cypherData);
    }

    public string Decrypt(string input)
    {
        csp.ImportParameters(privateKey);

        var cipherData = Convert.FromBase64String(input);
        var dataBytes = csp.Decrypt(cipherData, false);

        return Encoding.UTF8.GetString(dataBytes);
    }


}
