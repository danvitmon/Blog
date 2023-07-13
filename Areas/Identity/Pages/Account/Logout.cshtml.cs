#nullable disable

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using Blog.Models;

namespace Blog.Areas.Identity.Pages.Account;

public class LogoutModel : PageModel
{
  private readonly ILogger<LogoutModel>    _logger;
  private readonly SignInManager<BlogUser> _signInManager;

  public LogoutModel(SignInManager<BlogUser> signInManager, ILogger<LogoutModel> logger)
  {
    _signInManager = signInManager;
    _logger        = logger;
  }

  public async Task<IActionResult> OnPost(string returnUrl = null)
  {
    await _signInManager.SignOutAsync();
    _logger.LogInformation("User logged out.");

    if (returnUrl != null)
      return LocalRedirect(returnUrl);

    return RedirectToPage();
  }
}