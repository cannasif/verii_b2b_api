using Microsoft.EntityFrameworkCore;
using Wms.Application.Common;
using Wms.Domain.Common;
using Wms.Domain.Entities.Identity;
using Wms.Infrastructure.Persistence.Context;

namespace Wms.WebApi.Startup;

public static class B2bIdentitySeed
{
    private const string AdminEmail = "admin@v3rii.com";
    private const string AdminPassword = "Veriipass123!";

    public static async Task EnsureIdentitySeedAsync(this WebApplication app)
    {
        await using var scope = app.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<WmsDbContext>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("B2bIdentitySeed");

        var adminRole = await dbContext.UserAuthorities
            .FirstOrDefaultAsync(x => x.Title == "admin" && !x.IsDeleted);

        if (adminRole == null)
        {
            adminRole = new UserAuthority
            {
                Title = "admin",
                BranchCode = "0",
                CreatedDate = DateTimeProvider.Now,
            };
            await dbContext.UserAuthorities.AddAsync(adminRole);
            await dbContext.SaveChangesAsync();
        }

        if (!await dbContext.UserAuthorities.AnyAsync(x => x.Title == "user" && !x.IsDeleted))
        {
            await dbContext.UserAuthorities.AddAsync(new UserAuthority
            {
                Title = "user",
                BranchCode = "0",
                CreatedDate = DateTimeProvider.Now,
            });
        }

        var adminUser = await dbContext.Users
            .FirstOrDefaultAsync(x => x.Email == AdminEmail || x.Username == "admin");

        if (adminUser == null)
        {
            await dbContext.Users.AddAsync(new User
            {
                Username = "admin",
                Email = AdminEmail,
                PasswordHash = passwordHasher.Hash(AdminPassword),
                FirstName = "B2B",
                LastName = "Admin",
                RoleId = adminRole.Id,
                IsActive = true,
                IsEmailConfirmed = true,
                BranchCode = "0",
                CreatedDate = DateTimeProvider.Now,
            });
        }
        else
        {
            var changed = false;

            if (adminUser.RoleId <= 0)
            {
                adminUser.RoleId = adminRole.Id;
                changed = true;
            }

            if (!passwordHasher.Verify(AdminPassword, adminUser.PasswordHash))
            {
                adminUser.PasswordHash = passwordHasher.Hash(AdminPassword);
                changed = true;
            }

            if (!adminUser.IsActive)
            {
                adminUser.IsActive = true;
                changed = true;
            }

            if (!adminUser.IsEmailConfirmed)
            {
                adminUser.IsEmailConfirmed = true;
                changed = true;
            }

            if (changed)
            {
                adminUser.SetUpdatedInfo();
            }
        }

        await dbContext.SaveChangesAsync();
        logger.LogInformation("B2B identity seed checked.");
    }
}
