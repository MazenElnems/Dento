namespace Dento.DTOs;

public class VerifyEmailRequestDto
{
    public required string Code { get; init; } 
    public required string UserId { get; init; }
}
