using Dento.Constants;
using Dento.Data;
using Dento.Models;
using Dento.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Dento.Services.Implementation;

public class DbInitializer(AppDbContext context, IConfiguration configuration, UserManager<ApplicationUser> userManager) : IDbInitializer
{
    private readonly AppDbContext _context = context;
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly IConfiguration _configuration = configuration;

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

        var adminUserEmail = _configuration["AdminUser:Email"];
        var adminUserPassword = _configuration["AdminUser:Password"];

        if (adminUserEmail != null && adminUserPassword != null)
        {
            if (!_context.Admins.Any())
            {
                var admin = new Admin
                {
                    Email = adminUserEmail,
                    UserName = adminUserEmail,
                    FirstName = "Admin",
                    MiddleName = "Admin",
                    LastName = "Admin",
                    EmailConfirmed = true
                };
                await _userManager.CreateAsync(admin, adminUserPassword);
                await _userManager.AddToRoleAsync(admin, RoleNames.Admin);
            }
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
