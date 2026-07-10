namespace Dento.Services.Interfaces;

public interface IPaymobHmacVarifier
{
    bool Verify(
        string combinedQueryParameters,
        string receivedHmac
    );
}
