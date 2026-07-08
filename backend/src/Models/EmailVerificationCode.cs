using System.ComponentModel.DataAnnotations.Schema;

namespace Dento.Models;

public class EmailVerificationCode
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    [ForeignKey("User")]
    public string UserId { get; set; } = string.Empty;
    public Patient User { get; set; } = null!;
    public bool IsActive { get; set; }
}
