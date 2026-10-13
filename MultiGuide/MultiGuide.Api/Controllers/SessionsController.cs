using Microsoft.AspNetCore.Mvc;
using MultiGuide.Api.Contracts;
using MultiGuide.Business.Services;

namespace MultiGuide.Api.Controllers;

[ApiController]
[Route("api/v1/sessions")]
[Produces("application/json")]
public sealed class SessionsController(ISessionService sessionService) : ControllerBase
{
    /// <summary>POST /api/v1/sessions — Tạo session với ngôn ngữ đã chọn (US-03)</summary>
    [HttpPost]
    [ProducesResponseType<SessionResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> CreateSession(
        [FromBody] CreateSessionRequest request,
        CancellationToken ct)
    {
        var session = await sessionService.CreateSessionAsync(
            request.AreaCode,
            request.LanguageCode,
            ct);

        var response = new SessionResponse(
            session.SessionId,
            session.AreaCode,
            session.LanguageCode,
            session.StartedAt);

        return CreatedAtAction(nameof(CreateSession), new { id = session.SessionId }, response);
    }
}
