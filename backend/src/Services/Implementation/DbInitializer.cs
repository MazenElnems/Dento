using Dento.Constants;
using Dento.Data;
using Dento.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Dento.Services.Implementation;

public class DbInitializer(AppDbContext context) : IDbInitializer
{
    private readonly AppDbContext _context = context;

    public async Task InitializeAsync()
    {
        _context.Database.EnsureCreated();

        var pendingMigrations = await _context.Database.GetPendingMigrationsAsync();

        foreach (var migration in pendingMigrations)
        {
            await _context.Database.MigrateAsync();
        }

        var roles = await _context.Roles.ToListAsync();

        if (!roles.Any())
        {
            await _context.Roles.AddRangeAsync(GetDefaultRoles());
            await _context.SaveChangesAsync();
        }
    }

    public List<IdentityRole> GetDefaultRoles()
    {
        var roles = new List<IdentityRole>
        {
            new() { Name = RoleNames.Admin, NormalizedName = RoleNames.Admin.ToUpper() },
            new() { Name = RoleNames.Dentist, NormalizedName = RoleNames.Dentist.ToUpper() },
            new() { Name = RoleNames.Receptionist, NormalizedName = RoleNames.Receptionist.ToUpper() },
            new() { Name = RoleNames.Patient, NormalizedName = RoleNames.Patient.ToUpper() }
        };

        return roles;
    }
}
