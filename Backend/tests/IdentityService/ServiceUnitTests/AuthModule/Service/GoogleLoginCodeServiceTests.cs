using FluentAssertions;
using IdentityService.Application.AuthenticationModule.Dto;
using IdentityService.Application.AuthenticationModule.Service;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using Shared.Cryptography.Application.Cryptography.Contract;
using Xunit;

namespace IdentityService.UnitTests.AuthModule.Service;

public class GoogleLoginCodeServiceTests
{
    [Fact]
    public void CreateCode_ShouldCreateConsumableOneTimeCode()
    {
        using var cache = new MemoryCache(new MemoryCacheOptions());
        var authenticity = new Mock<IAuthenticity>();
        authenticity.Setup(x => x.Hash(It.IsAny<string>(), It.IsAny<System.Security.Cryptography.HashAlgorithm>()))
            .Returns<string, System.Security.Cryptography.HashAlgorithm>((value, _) => value);
        var service = new GoogleLoginCodeService(cache, authenticity.Object);
        var context = new GoogleLoginCodeCacheDto { UserId = Guid.NewGuid(), GoogleSubjectId = "google-id" };

        string code = service.CreateCode(context);

        service.ConsumeCode($" {code} ").Should().BeSameAs(context);
        service.ConsumeCode(code).Should().BeNull();
    }

    [Fact]
    public void ConsumeCode_ShouldReturnNull_WhenCodeIsMissing()
    {
        using var cache = new MemoryCache(new MemoryCacheOptions());
        var authenticity = new Mock<IAuthenticity>();
        authenticity.Setup(x => x.Hash(It.IsAny<string>(), It.IsAny<System.Security.Cryptography.HashAlgorithm>()))
            .Returns<string, System.Security.Cryptography.HashAlgorithm>((value, _) => value);
        var service = new GoogleLoginCodeService(cache, authenticity.Object);

        service.ConsumeCode("missing").Should().BeNull();
    }
}
