using Twix.Models;
using Twix.Helpers;

namespace Twix.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(TwixDb db)
    {
        await db.Database.EnsureCreatedAsync();

        if (!db.Users.Any())
        {
            db.Users.AddRange(
                new User
                {
                    UserName     = "admin",
                    Email        = "admin@twix.app",
                    PasswordHash = PasswordHasher.Hash("Admin123!"),
                    DisplayName  = "Twix Admin",
                    Bio          = "Platform administrator.",
                    IsAdmin      = true,
                    CreatedAt    = DateTime.UtcNow
                },
                new User
                {
                    UserName     = "demo",
                    Email        = "demo@twix.app",
                    PasswordHash = PasswordHasher.Hash("Demo123!"),
                    DisplayName  = "Demo User",
                    Bio          = "Living life one Twix at a time 🔥",
                    Location     = "Earth",
                    CreatedAt    = DateTime.UtcNow
                }
            );
            await db.SaveChangesAsync();

            // Seed a few starter posts
            var admin = db.Users.First(u => u.UserName == "admin");
            var demo  = db.Users.First(u => u.UserName == "demo");
            db.Posts.AddRange(
                new Post { Content = "Welcome to Twix! 🎉 The platform is live.", AuthorId = admin.Id },
                new Post { Content = "This is a demo post. Say it louder! 🔥",    AuthorId = demo.Id  }
            );
            await db.SaveChangesAsync();
        }
    }
}
