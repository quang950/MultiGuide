using Microsoft.AspNetCore.Mvc;
using MultiGuide.Api.Contracts;
using MultiGuide.Business.Services;

namespace MultiGuide.Api.Controllers;

[ApiController]
[Route("api/v1/areas")]
[Produces("application/json")]
public sealed class AreasController(IAreaService areaService) : ControllerBase
{
    /// <summary>GET /api/v1/areas/{code} — Lấy thông tin khu từ QR (US-01)</summary>
    [HttpGet("{code}")]
    [ProducesResponseType<AreaResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetArea(
        string code,
        CancellationToken ct)
    {
        var area = await areaService.GetByCodeAsync(code, ct);
        return Ok(new AreaResponse(area.Code, area.Name, area.PriceUsd, area.PassTtlHours));
    }

    /// <summary>GET /api/v1/areas/{code}/languages — Danh sách ngôn ngữ đang bật của khu (US-02)</summary>
    [HttpGet("{code}/languages")]
    [ProducesResponseType<IReadOnlyList<LanguageResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetLanguages(
        string code,
        CancellationToken ct)
    {
        var languages = await areaService.GetLanguagesByAreaCodeAsync(code, ct);
        var response = languages.Select(l => new LanguageResponse(l.Code, l.Name)).ToList();
        return Ok(response);
    }
}
