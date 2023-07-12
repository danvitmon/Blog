using Blog.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Blog.Data;

public static class DataUtility
{
  private const string? _adminRole = "Admin";
  private const string? _moderatorRole = "Moderator";

  public static string GetConnectionString(IConfiguration configuration)
  {
    var connectionString = configuration.GetConnectionString("DefaultConnection");
    var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");


    return string.IsNullOrEmpty(databaseUrl) ? connectionString! : BuildConnectionString(databaseUrl);
  }

  private static string BuildConnectionString(string databaseUrl)
  {
    var databaseUri = new Uri(databaseUrl);
    var userInfo = databaseUri.UserInfo.Split(':');
    var builder = new NpgsqlConnectionStringBuilder
    {
      Host = databaseUri.Host,
      Port = databaseUri.Port,
      Username = userInfo[0],
      Password = userInfo[1],
      Database = databaseUri.LocalPath.TrimStart('/'),
      SslMode = SslMode.Require,
      TrustServerCertificate = true
    };
    return builder.ToString();
  }


  public static async Task ManageDataAsync(IServiceProvider svcProvider)
  {
    // Obtaining the necessary services based on the IServiceProivder parameter
    var dbContextSvc = svcProvider.GetRequiredService<ApplicationDbContext>();
    var userManagerSvc = svcProvider.GetRequiredService<UserManager<BlogUser>>();
    var roleManagerSvc = svcProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var configurationSvc = svcProvider.GetRequiredService<IConfiguration>();

    // Align the database by checking Migrationgs
    await dbContextSvc.Database.MigrateAsync();

    // Seed Application Roles
    await SeedRolesAsync(roleManagerSvc);

    // Seed Demo User(s)
    await SeedBlogUsersAsync(userManagerSvc, configurationSvc);
  }


  private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
  {
    if (!await roleManager.RoleExistsAsync(_adminRole!)) await roleManager.CreateAsync(new IdentityRole(_adminRole!));

    if (!await roleManager.RoleExistsAsync(_moderatorRole!))
      await roleManager.CreateAsync(new IdentityRole(_moderatorRole!));
  }


  // Demo Users Seed Method
  private static async Task SeedBlogUsersAsync(UserManager<BlogUser> userManager, IConfiguration configuration)
  {
    var adminEmail = configuration["AdminLoginEmail"] ?? Environment.GetEnvironmentVariable("AdminLoginEmail");
    var adminPassword = configuration["AdminLoginPassword"] ?? Environment.GetEnvironmentVariable("AdminLoginPassword");

    var moderatorEmail = configuration["ModeratorLoginEmail"] ??
                         Environment.GetEnvironmentVariable("ModeratorLoginEmail");
    var moderatorPassword = configuration["ModeratorLoginPassword"] ??
                            Environment.GetEnvironmentVariable("ModeratorLoginPassword");


    try
    {
      // Seed the Admin
      var adminUser = new BlogUser
      {
        UserName = adminEmail,
        Email = adminEmail,
        FirstName = "Daniel",
        LastName = "Monastirsky",
        EmailConfirmed = true
      };

      var user = await userManager.FindByEmailAsync(adminUser.Email!);

      if (user == null)
      {
        // TODO: Use Enviroment Variable - Environment.GetEnvironmentVariable("DemoLoginPassword")!
        await userManager.CreateAsync(adminUser, adminPassword!);
        await userManager.AddToRoleAsync(adminUser, _adminRole!);
      }

      // Seed the Moderator
      var moderatorUser = new BlogUser
      {
        UserName = moderatorEmail,
        Email = moderatorEmail,
        FirstName = "Dan",
        LastName = "Mon",
        EmailConfirmed = true
      };

      var moduser = await userManager.FindByEmailAsync(moderatorUser.Email!);

      if (moduser == null)
      {
        // TODO: Use Enviroment Variable - Environment.GetEnvironmentVariable("DemoLoginPassword")!
        await userManager.CreateAsync(moderatorUser, moderatorPassword!);
        await userManager.AddToRoleAsync(moderatorUser, _moderatorRole!);
      }
    }
    catch (Exception ex)
    {
      Console.WriteLine("*************  ERROR  *************");
      Console.WriteLine("Error Seeding Demo Login User.");
      Console.WriteLine(ex.Message);
      Console.WriteLine("***********************************");
      throw;
    }
  }
}