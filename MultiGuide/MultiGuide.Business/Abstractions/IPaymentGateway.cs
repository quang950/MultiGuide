namespace MultiGuide.Business.Abstractions;

public sealed record PaymentRequest(
    Guid SessionId, decimal Amount, string Currency, string IdempotencyKey);

public sealed record PaymentResult(
    bool Succeeded, string? ProviderRef, string? FailureReason);

public interface IPaymentGateway
{
    Task<PaymentResult> ChargeAsync(PaymentRequest request, CancellationToken ct);
}
