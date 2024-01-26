using System.Text.Encodings.Web;
using ConnectorAPI.DbContexts.ConnectorDb;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace ConnectorAPI.Services;

public class IPAdressAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>, IAuthenticationHandler
{
    public static readonly string SchemName = "IPAdressVerificationScheme";
    private readonly UserManager<User> userManager;
    private readonly UserIPStorageService userIP;

    public IPAdressAuthenticationHandler(IOptionsMonitor<AuthenticationSchemeOptions> options,
                                         ILoggerFactory logger,
                                         UrlEncoder encoder,
                                         UserManager<User> userManager,
                                         UserIPStorageService userIP) : base(options, logger, encoder)
    {
        this.userManager = userManager;
        this.userIP = userIP;
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        await Task.CompletedTask;

        var username = userManager.GetUserName(Request.HttpContext.User);
        if (username is null) return AuthenticateResult.NoResult();

        try
        {
            var storedIP = userIP.GetIP(username);
            var reqIp = Request.HttpContext.Connection.RemoteIpAddress?.ToString();

            if (storedIP == reqIp)
                return AuthenticateResult.Success(
                    new AuthenticationTicket(Request.HttpContext.User, SchemName)
                );
            else return AuthenticateResult.Fail(new Exception($"non matching ip adresses: {storedIP} != {reqIp}"));
        }
        catch (Exception e)
        {
            return AuthenticateResult.Fail(e);
        }

    }
}
