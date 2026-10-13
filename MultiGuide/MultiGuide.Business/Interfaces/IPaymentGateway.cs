namespace MultiGuide.Business.Interfaces;

/// <summary>Kết quả từ cổng thanh toán mock.</summary>
public sealed record PaymentResult(bool Succeeded, string? FailureReason);

/// <summary>
/// Trừu tượng hoá cổng thanh toán.
/// Sprint 1: dùng MockPaymentGateway. Sau này thay bằng cổng thật mà không cần đổi Business.
/// </summary>
public interface IPaymentGateway
{
    /// <summary>
    /// Thực hiện thanh toán <paramref name="amountUsd"/> USD cho session <paramref name="sessionId"/>.
    /// </summary>
    Task<PaymentResult> ChargeAsync(Guid sessionId, decimal amountUsd, CancellationToken ct = default);
}
