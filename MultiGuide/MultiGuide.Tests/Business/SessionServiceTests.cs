using MultiGuide.Business.Interfaces;
using MultiGuide.Business.Services;
using MultiGuide.Domain.Constants;
using MultiGuide.Domain.Entities;
using MultiGuide.Domain.Exceptions;
using NSubstitute;

namespace MultiGuide.Tests.Business;

public sealed class SessionServiceTests
{
    private readonly IAreaRepository _areaRepo = Substitute.For<IAreaRepository>();
    private readonly ILanguageRepository _languageRepo = Substitute.For<ILanguageRepository>();
    private readonly ISessionRepository _sessionRepo = Substitute.For<ISessionRepository>();
    private readonly IClock _clock = Substitute.For<IClock>();
    private readonly SessionService _sut;

    private static readonly DateTime FixedUtcNow = new(2026, 10, 13, 12, 0, 0, DateTimeKind.Utc);

    public SessionServiceTests()
    {
        _clock.UtcNow.Returns(FixedUtcNow);
        _sut = new SessionService(_areaRepo, _languageRepo, _sessionRepo, _clock);
    }

    [Fact]
    public async Task CreateSessionAsync_ValidInput_ReturnsSessionInfo()
    {
        // Arrange
        var area = new TourArea { Id = 1, Code = "hoi-an", IsActive = true };
        _areaRepo.GetByCodeAsync("hoi-an").Returns(area);
        _languageRepo.GetActiveLanguagesByAreaCodeAsync("hoi-an")
            .Returns(new List<Language> { new() { Code = "vi", IsActive = true } });

        _sessionRepo.CreateAsync(Arg.Any<VisitorSession>())
            .Returns(ci => ci.Arg<VisitorSession>());   // trả lại entity được truyền vào

        // Act
        var result = await _sut.CreateSessionAsync("hoi-an", "vi");

        // Assert
        Assert.Equal("hoi-an", result.AreaCode);
        Assert.Equal("vi", result.LanguageCode);
        Assert.Equal(FixedUtcNow, result.StartedAt);   // dùng IClock, không phải DateTime.UtcNow thật
        Assert.NotEqual(Guid.Empty, result.SessionId);
    }

    [Fact]
    public async Task CreateSessionAsync_NonExistentArea_ThrowsNotFoundException()
    {
        // Arrange
        _areaRepo.GetByCodeAsync("unknown").Returns((TourArea?)null);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<NotFoundException>(() =>
            _sut.CreateSessionAsync("unknown", "vi"));

        Assert.Equal(ErrorCodes.AreaNotFound, ex.ErrorCode);
    }

    [Fact]
    public async Task CreateSessionAsync_InactiveArea_ThrowsNotFoundException()
    {
        // Arrange
        var area = new TourArea { Code = "closed", IsActive = false };
        _areaRepo.GetByCodeAsync("closed").Returns(area);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<NotFoundException>(() =>
            _sut.CreateSessionAsync("closed", "vi"));

        Assert.Equal(ErrorCodes.AreaNotFound, ex.ErrorCode);
    }

    [Fact]
    public async Task CreateSessionAsync_UnsupportedLanguage_ThrowsValidationException()
    {
        // Arrange
        var area = new TourArea { Id = 1, Code = "hoi-an", IsActive = true };
        _areaRepo.GetByCodeAsync("hoi-an").Returns(area);
        _languageRepo.GetActiveLanguagesByAreaCodeAsync("hoi-an")
            .Returns(new List<Language> { new() { Code = "vi", IsActive = true } });

        // Act & Assert — "ja" chưa có bản dịch trong khu này
        var ex = await Assert.ThrowsAsync<BusinessValidationException>(() =>
            _sut.CreateSessionAsync("hoi-an", "ja"));

        Assert.Equal(ErrorCodes.ValidationError, ex.ErrorCode);
        Assert.Equal(422, ex.StatusCode);
    }

    [Fact]
    public async Task CreateSessionAsync_UsesClockUtcNow_NotSystemTime()
    {
        // Arrange – clock trả về ngày cố định
        var area = new TourArea { Id = 1, Code = "hoi-an", IsActive = true };
        _areaRepo.GetByCodeAsync("hoi-an").Returns(area);
        _languageRepo.GetActiveLanguagesByAreaCodeAsync("hoi-an")
            .Returns(new List<Language> { new() { Code = "en", IsActive = true } });
        _sessionRepo.CreateAsync(Arg.Any<VisitorSession>())
            .Returns(ci => ci.Arg<VisitorSession>());

        // Act
        var result = await _sut.CreateSessionAsync("hoi-an", "en");

        // Assert – StartedAt phải bằng giá trị từ mock clock, không phải DateTime.UtcNow thật
        Assert.Equal(FixedUtcNow, result.StartedAt);
    }
}
