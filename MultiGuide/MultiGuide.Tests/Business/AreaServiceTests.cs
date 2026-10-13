using MultiGuide.Business.Interfaces;
using MultiGuide.Business.Services;
using MultiGuide.Domain.Constants;
using MultiGuide.Domain.Entities;
using MultiGuide.Domain.Exceptions;
using NSubstitute;

namespace MultiGuide.Tests.Business;

public sealed class AreaServiceTests
{
    private readonly IAreaRepository _areaRepo = Substitute.For<IAreaRepository>();
    private readonly ILanguageRepository _languageRepo = Substitute.For<ILanguageRepository>();
    private readonly AreaService _sut;

    public AreaServiceTests()
    {
        _sut = new AreaService(_areaRepo, _languageRepo);
    }

    // ── GetByCodeAsync ────────────────────────────────────────────────────────

    [Fact]
    public async Task GetByCodeAsync_ActiveArea_ReturnsAreaInfo()
    {
        // Arrange
        var area = new TourArea { Code = "hoi-an", Name = "Hội An", PriceUsd = 2m, PassTtlHours = 24, IsActive = true };
        _areaRepo.GetByCodeAsync("hoi-an").Returns(area);

        // Act
        var result = await _sut.GetByCodeAsync("hoi-an");

        // Assert
        Assert.Equal("hoi-an", result.Code);
        Assert.Equal("Hội An", result.Name);
        Assert.Equal(2m, result.PriceUsd);
        Assert.Equal(24, result.PassTtlHours);
    }

    [Fact]
    public async Task GetByCodeAsync_NonExistentArea_ThrowsNotFoundException()
    {
        // Arrange
        _areaRepo.GetByCodeAsync("unknown").Returns((TourArea?)null);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<NotFoundException>(() =>
            _sut.GetByCodeAsync("unknown"));

        Assert.Equal(ErrorCodes.AreaNotFound, ex.ErrorCode);
        Assert.Equal(404, ex.StatusCode);
    }

    [Fact]
    public async Task GetByCodeAsync_InactiveArea_ThrowsNotFoundException()
    {
        // Arrange
        var area = new TourArea { Code = "closed", IsActive = false };
        _areaRepo.GetByCodeAsync("closed").Returns(area);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<NotFoundException>(() =>
            _sut.GetByCodeAsync("closed"));

        Assert.Equal(ErrorCodes.AreaNotFound, ex.ErrorCode);
    }

    // ── GetLanguagesByAreaCodeAsync ───────────────────────────────────────────

    [Fact]
    public async Task GetLanguagesByAreaCodeAsync_ActiveArea_ReturnsLanguages()
    {
        // Arrange
        var area = new TourArea { Code = "hoi-an", IsActive = true };
        _areaRepo.GetByCodeAsync("hoi-an").Returns(area);
        _languageRepo.GetActiveLanguagesByAreaCodeAsync("hoi-an")
            .Returns(new List<Language>
            {
                new() { Code = "vi", Name = "Tiếng Việt", IsActive = true },
                new() { Code = "en", Name = "English", IsActive = true },
            });

        // Act
        var result = await _sut.GetLanguagesByAreaCodeAsync("hoi-an");

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Contains(result, l => l.Code == "vi");
        Assert.Contains(result, l => l.Code == "en");
    }

    [Fact]
    public async Task GetLanguagesByAreaCodeAsync_InactiveArea_ThrowsNotFoundException()
    {
        // Arrange
        var area = new TourArea { Code = "closed", IsActive = false };
        _areaRepo.GetByCodeAsync("closed").Returns(area);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<NotFoundException>(() =>
            _sut.GetLanguagesByAreaCodeAsync("closed"));

        Assert.Equal(ErrorCodes.AreaNotFound, ex.ErrorCode);
    }
}
