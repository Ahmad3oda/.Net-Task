using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TaskManagement.Domain.Entities;

namespace TaskManagement.Infrastructure.Data;

public class DbSeeder
{
    private readonly AppDbContext _context;

    public DbSeeder(AppDbContext context)
    {
        _context = context;
    }

    public async Task SeedAsync()
    {
        await _context.Database.MigrateAsync();

        if (!await _context.Roles.AnyAsync())
        {
            await _context.Roles.AddRangeAsync(
                new Role { Id = Guid.NewGuid(), Name = "Admin" },
                new Role { Id = Guid.NewGuid(), Name = "User" }
            );
            await _context.SaveChangesAsync();
        }

        if (!await _context.Users.AnyAsync(u => u.Username == "test"))
        {
            var adminRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "Admin");
            if (adminRole != null)
            {
                var adminUser = new User
                {
                    Id = Guid.NewGuid(),
                    Username = "test",
                    Email = "test@test.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("1234"),
                    RoleId = adminRole.Id,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await _context.Users.AddAsync(adminUser);
                await _context.SaveChangesAsync();
            }
        }
    }
}
