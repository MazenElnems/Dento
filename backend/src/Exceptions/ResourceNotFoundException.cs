namespace Dento.Exceptions;

public class ResourceNotFoundException : BaseException
{
    public readonly string ResourceName;
    public ResourceNotFoundException(string resource, string errorMessage = "Resource not found")
        : base(404, errorMessage)
    {
        ResourceName = resource;
    }
}
