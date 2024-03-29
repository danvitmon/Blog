﻿#nullable disable

using System.Text;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;

using Blog.Models;

namespace Blog.Areas.Identity.Pages.Account;

[AllowAnonymous]
public class RegisterConfirmationModel : PageModel
{
  private readonly UserManager<BlogUser> _userManager;

  public RegisterConfirmationModel(UserManager<BlogUser> userManager) => _userManager = userManager;

  /// <summary>
  ///   This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
  ///   directly from your code. This API may change or be removed in future releases.
  /// </summary>
  public string Email { get; set; }

  /// <summary>
  ///   This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
  ///   directly from your code. This API may change or be removed in future releases.
  /// </summary>
  public bool DisplayConfirmAccountLink { get; set; }

  /// <summary>
  ///   This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
  ///   directly from your code. This API may change or be removed in future releases.
  /// </summary>
  public string EmailConfirmationUrl { get; set; }

  public async Task<IActionResult> OnGetAsync(string email, string returnUrl = null)
  {
    if (email == null) 
      return RedirectToPage("/Index");

    returnUrl ??= Url.Content("~/");

    var user = await _userManager.FindByEmailAsync(email);
    if (user == null) 
      return NotFound($"Unable to load user with email '{email}'.");

    Email                     = email;
    DisplayConfirmAccountLink = false;

    if (DisplayConfirmAccountLink)
    {
      var userId           = await _userManager.GetUserIdAsync(user);
      var code             = await _userManager.GenerateEmailConfirmationTokenAsync(user);
      code                 = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
      EmailConfirmationUrl = Url.Page("/Account/ConfirmEmail", null, new { area = "Identity", userId, code, returnUrl }, Request.Scheme);
    }

    return Page();
  }
}