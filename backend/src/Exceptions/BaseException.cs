namespace Dento.Exceptions
{
    public class BaseException : Exception
    {
        public readonly int StatusCode;
        public readonly string? ErrorMessage;

        public BaseException(int statusCode, string? errorMessage = null)
            : base(errorMessage)
        {
            StatusCode = statusCode;
            ErrorMessage = errorMessage;
        }
    }
}
