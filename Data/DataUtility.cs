using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

using Npgsql;

using Blog.Models;

namespace Blog.Data;

public static class DataUtility
{
  private const string? AdminRole     = "Admin";
  private const string? ModeratorRole = "Moderator";

  public static string GetConnectionString(IConfiguration configuration)
  {
    var connectionString = configuration.GetConnectionString("DefaultConnection");
    var databaseUrl      = Environment.GetEnvironmentVariable("DATABASE_URL");

    return string.IsNullOrEmpty(databaseUrl) ? connectionString! : BuildConnectionString(databaseUrl);
  }

  private static string BuildConnectionString(string databaseUrl)
  {
    var databaseUri = new Uri(databaseUrl);
    var userInfo    = databaseUri.UserInfo.Split(':');
    var builder     = new NpgsqlConnectionStringBuilder
    {
      Host                   = databaseUri.Host,
      Port                   = databaseUri.Port,
      Username               = userInfo[0],
      Password               = userInfo[1],
      Database               = databaseUri.LocalPath.TrimStart('/'),
      SslMode                = SslMode.Require,
      TrustServerCertificate = true
    };

    return builder.ToString();
  }

  public static async Task ManageDataAsync(IServiceProvider svcProvider)
  {
    // Obtaining the necessary services based on the IServiceProvider parameter
    var dbContextSvc     = svcProvider.GetRequiredService<ApplicationDbContext>();
    var userManagerSvc   = svcProvider.GetRequiredService<UserManager<BlogUser>>();
    var roleManagerSvc   = svcProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var configurationSvc = svcProvider.GetRequiredService<IConfiguration>();

    // Align the database by checking Migrations
    await dbContextSvc.Database.MigrateAsync();

    // Seed Application Roles
    await SeedRolesAsync(roleManagerSvc);

    // Seed Demo User(s)
    await SeedBlogUsersAsync(userManagerSvc, configurationSvc);
  }

  private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
  {
    if (!await roleManager.RoleExistsAsync(AdminRole!)) 
      await roleManager.CreateAsync(new IdentityRole(AdminRole!));

    if (!await roleManager.RoleExistsAsync(ModeratorRole!))
      await roleManager.CreateAsync(new IdentityRole(ModeratorRole!));
  }

  // Demo Users Seed Method
  private static async Task SeedBlogUsersAsync(UserManager<BlogUser> userManager, IConfiguration configuration)
  {
    var adminEmail        = configuration["AdminLoginEmail"]        ?? Environment.GetEnvironmentVariable("AdminLoginEmail");
    var adminPassword     = configuration["AdminLoginPassword"]     ?? Environment.GetEnvironmentVariable("AdminLoginPassword");
    var moderatorEmail    = configuration["ModeratorLoginEmail"]    ?? Environment.GetEnvironmentVariable("ModeratorLoginEmail");
    var moderatorPassword = configuration["ModeratorLoginPassword"] ?? Environment.GetEnvironmentVariable("ModeratorLoginPassword");

    // Seed the Admin
    var adminUser = new BlogUser
    {
      UserName       = adminEmail,
      Email          = adminEmail,
      FirstName      = "Daniel",
      LastName       = "Monastirsky",
      EmailConfirmed = true
    };

    var user = await userManager.FindByEmailAsync(adminUser.Email!);

    if (user == null)
    {
      // TODO: Use Environment Variable - Environment.GetEnvironmentVariable("DemoLoginPassword")!
      await userManager.CreateAsync   (adminUser, adminPassword!);
      await userManager.AddToRoleAsync(adminUser, AdminRole!);
    }

    // Seed the Moderator
    var moderatorUser = new BlogUser
    {
      UserName       = moderatorEmail,
      Email          = moderatorEmail,
      FirstName      = "Dan",
      LastName       = "Mon",
      EmailConfirmed = true
    };

    var moduser = await userManager.FindByEmailAsync(moderatorUser.Email!);

    if (moduser == null)
    {
      // TODO: Use Environment Variable - Environment.GetEnvironmentVariable("DemoLoginPassword")!
      await userManager.CreateAsync   (moderatorUser, moderatorPassword!);
      await userManager.AddToRoleAsync(moderatorUser, ModeratorRole!);
    }
  }
}