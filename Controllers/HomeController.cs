using Blog.Data;
using Blog.Models;
using Blog.Services;
using Blog.Services.Interfaces;
using MailKit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using X.PagedList;

namespace Blog.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;
        private readonly IImageService _imageService;
        private readonly IBlogService _blogService;
        private readonly UserManager<BlogUser> _userManager;
        private readonly IEmailSender _emailService;
        private readonly IConfiguration _configuration;

        public HomeController(ILogger<HomeController> logger, IImageService imageService, ApplicationDbContext context, IBlogService blogService, UserManager<BlogUser> userManager, IConfiguration configuration, IEmailSender emailSender)
        {
            _logger = logger;
            _context = context;
            _imageService = imageService;
            _blogService = blogService;
            _userManager = userManager;
            _userManager = userManager;
            _emailService = emailSender;
            _configuration = configuration;
        }

        public async Task<IActionResult> Index(int? pageNum)
        {
            int pageSize = 3;
            int page = pageNum ?? 1;

            IPagedList<BlogPost> blogPosts = await _context.BlogPosts.Include(b => b.Category).ToPagedListAsync(pageNum, pageSize);

            return View(blogPosts);
        }

        public async Task<IActionResult> SearchIndex(string? searchString, int? pageNum)
        {
            int pageSize = 3;
            int page = pageNum ?? 1;

            IPagedList<BlogPost> blogPosts = await _blogService.SearchBlogPosts(searchString).ToPagedListAsync(page, pageSize);

            return View(nameof(Index), blogPosts);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [Authorize]
        public async Task<IActionResult> ContactMe()
        {
            string? blogUserId = _userManager.GetUserId(User);

            if (blogUserId == null)
            {
                return NotFound();
            }
            BlogUser? blogUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == blogUserId);

            return View(blogUser);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ContactMe([Bind("FirstName,LastName,Email")] BlogUser blogUser, string? message)
        {
            string? swalMessage = string.Empty;

            if (ModelState.IsValid)
            {
                try
                {
                    string? adminEmail = _configuration["AdminLoginEmail"] ?? Environment.GetEnvironmentVariable("AdminLoginEmail");
                    await _emailService.SendEmailAsync(adminEmail!, $"Contact Me Message From - {blogUser.FullName}", message!);
                    swalMessage = "Email sent successfully!";
                }
                catch (Exception)
                {

                    throw;
                }
                swalMessage = "Error: Unable to send email.";
            }

            return RedirectToAction("Index", new { swalMessage });
        }

    }
}
