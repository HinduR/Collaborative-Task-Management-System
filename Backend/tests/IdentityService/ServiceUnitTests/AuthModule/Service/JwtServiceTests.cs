using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using FluentAssertions;
using IdentityService.Application.AuthenticationModule.Dto;
using IdentityService.Application.AuthenticationModule.Service;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace IdentityService.UnitTests.AuthModule.Service;

public class JwtServiceTests
{
    [Fact]
    public void GenerateAndValidateAccessToken_ShouldRoundTripClaims()
    {
        using RSA rsa = RSA.Create(2048);
        var service = new JwtService(Configuration(rsa));
        var user = new TokenUserDto
        {
            UserId = Guid.NewGuid(),
            UserName = "Ada",
            RoleId = Guid.NewGuid(),
            RoleName = "Admin",
            PermissionList = ["BOARD.READ", " board.read ", "", "BOARD.WRITE"]
        };

        string token = service.GenerateAccessToken(user, DateTime.UtcNow.AddMinutes(5));
        var principal = service.ValidateAccessToken(token);

        principal.Claims.Should().Contain(claim => claim.Value == user.UserId.ToString());
        principal.FindFirst("role_id")!.Value.Should().Be(user.RoleId.ToString());
        principal.FindAll("permission").Select(claim => claim.Value)
            .Should().BeEquivalentTo(["BOARD.READ", "BOARD.WRITE"]);
    }

    [Fact]
    public void GenerateAccessToken_ShouldThrow_WhenPrivateKeyIsMissing()
    {
        var service = new JwtService(new ConfigurationBuilder().Build());

        service.Invoking(x => x.GenerateAccessToken(new TokenUserDto(), DateTime.UtcNow.AddMinutes(1)))
            .Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void ValidateAccessToken_ShouldThrow_WhenPublicKeyIsMissing()
    {
        var service = new JwtService(new ConfigurationBuilder().Build());

        service.Invoking(x => x.ValidateAccessToken("token"))
            .Should().Throw<InvalidOperationException>();
    }

    private static IConfiguration Configuration(RSA rsa)
    {
        string privateKey = rsa.ExportPkcs8PrivateKeyPem();
        string publicKey = rsa.ExportSubjectPublicKeyInfoPem();

        return new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["JwtConfig:PrivateKey"] = privateKey.Replace("\n", "\\n"),
                ["JwtConfig:PublicKey"] = publicKey.Replace("\n", "\\n"),
                ["JwtConfig:Issuer"] = "round-table",
                ["JwtConfig:Audience"] = "round-table-client"
            })
            .Build();
    }
}
