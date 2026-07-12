using Dento.Enums;
using Dento.Services.Interfaces;

namespace Dento.Services.Implementation;

/// <summary>
/// Resolves the appropriate <see cref="IPaymentStrategy"/> based on the requested <see cref="PaymentType"/>.
/// </summary>
public class PaymentStrategyFactory
{
    private readonly IEnumerable<IPaymentStrategy> _strategies;

    public PaymentStrategyFactory(IEnumerable<IPaymentStrategy> strategies)
    {
        _strategies = strategies;
    }

    public IPaymentStrategy Resolve(PaymentType paymentType)
    {
        var strategy = _strategies.FirstOrDefault(s => s.SupportedPaymentType == paymentType);

        if (strategy == null)
            throw new NotSupportedException($"Payment type '{paymentType}' is not supported.");

        return strategy;
    }
}
