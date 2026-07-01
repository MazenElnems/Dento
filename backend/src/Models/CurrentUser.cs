namespace Dento.Models;

public class CurrentUser
{
    public string Id { get; }
    public string Email { get; } 
    public string Role { get; } 
    public bool IsAuthenticated { get; }

    private CurrentUser(string id, string email, string role, bool isAuthenticated)
    {
        Id = id;
        Email = email;
        Role = role;
        IsAuthenticated = isAuthenticated;
    }

    public static CurrentUser Unauthenticated =>
        new(string.Empty, string.Empty, string.Empty, false);

    public static CurrentUser Authenticated(string id, string email, string role) =>
        new(id, email, role, true);

    public bool IsInRole(string role) =>
        string.Equals(Role, role, StringComparison.OrdinalIgnoreCase);
}
