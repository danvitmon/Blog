using Blog.Data;
using Blog.Models;
using Blog.Services;
using Blog.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = DataUtility.GetConnectionString(builder.Configuration) ??
                       throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
// Database Service
builder.Services.AddDbContext<ApplicationDbContext>(options =>
  options.UseNpgsql(connectionString));

// Error Service
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// Identity Service
builder.Services
  .AddIdentity<BlogUser, IdentityRole>(options => options.SignIn.RequireConfirmedAccount = true)
  .AddDefaultUI()
  .AddDefaultTokenProviders()
  .AddEntityFrameworkStores<ApplicationDbContext>();

// MVC Routing Service
builder.Services.AddMvc();


// Custom Services
// Add Image Service
builder.Services.AddScoped<IEmailSender, EmailService>();
builder.Services.AddScoped<IImageService, ImageService>();
builder.Services.AddScoped<IBlogService, BlogService>();

// Add Email Service
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));

var app = builder.Build();

var scope = app.Services.CreateScope();
await DataUtility.ManageDataAsync(scope.ServiceProvider);

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
  app.UseMigrationsEndPoint();
else
{
  app.UseExceptionHandler("/Home/Error");
  // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
  app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
  "custom",
  "Content/{slug}",
  new { controller = "BlogPosts", action = "Details" }
);

app.MapControllerRoute(
  "default",
  "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();