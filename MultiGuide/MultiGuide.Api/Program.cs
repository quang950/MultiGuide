using MultiGuide.Api.Extensions;
using MultiGuide.Data;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// ── Services ────────────────────────────────────────────────────────────────
builder.Services.AddErrorHandlingServices();  // GlobalExceptionHandler, ProblemDetails

builder.Services.AddControllers()
    .AddJsonOptions(o =>
        o.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower);

builder.Services.AddOpenApi();

builder.Services.AddDataLayer(builder.Configuration);  // DbContext, repositories, IClock

// Guest business services (US-01, US-02, US-03)
builder.Services.AddGuestServices();

// ── App pipeline ─────────────────────────────────────────────────────────────
var app = builder.Build();

// Seed nếu được bật qua cấu hình
if (app.Configuration.GetValue<bool>("Seed:Enabled"))
{
    await using var scope = app.Services.CreateAsyncScope();
    await scope.ServiceProvider.GetRequiredService<MultiGuide.Data.Seeding.DbSeeder>().SeedAsync();
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseErrorHandling();   // CorrelationIdMiddleware + ExceptionHandler (phải trước các middleware khác)
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
