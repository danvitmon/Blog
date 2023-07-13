#nullable disable

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using Blog.Models;
using Blog.Services.Interfaces;

namespace Blog.Areas.Identity.Pages.Account.Manage;

public class IndexModel : PageModel
{
  private readonly IImageService           _imageService;
  private readonly SignInManager<BlogUser> _signInManager;
  private readonly UserManager<BlogUser>   _userManager;

  public IndexModel(UserManager<BlogUser> userManager, SignInManager<BlogUser> signInManager, IImageService imageService)
  {
    _userManager   = userManager;
    _signInManager = signInManager;
    _imageService  = imageService;
  }

  /// <summary>
  ///   This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
  ///   directly from your code. This API may change or be removed in future releases.
  /// </summary>
  public string Username { get; set; }

  /// <summary>
  ///   This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
  ///   directly from your code. This API may change or be removed in future releases.
  /// </summary>
  [TempData]
  public string StatusMessage { get; set; }

  /// <summary>
  ///   This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
  ///   directly from your code. This API may change or be removed in future releases.
  /// </summary>
  [BindProperty]
  public InputModel Input { get; set; }

  private async Task LoadAsync(BlogUser user)
  {
    var userName    = await _userManager.GetUserNameAsync(user);
    var phoneNumber = await _userManager.GetPhoneNumberAsync(user);

    Username = userName;

    Input = new InputModel
    {
      PhoneNumber = phoneNumber,
      FirstName   = user.FirstName,
      LastName    = user.LastName,
      ImageData   = user.ImageData
    };
  }

  public async Task<IActionResult> OnGetAsync()
  {
    var user = await _userManager.GetUserAsync(User);
    if (user == null) 
     return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");

    await LoadAsync(user);

    return Page();
  }

  public async Task<IActionResult> OnPostAsync()
  {
    var user = await _userManager.GetUserAsync(User);
    if (user == null) 
     return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");

    if (!ModelState.IsValid)
    {
      await LoadAsync(user);

      return Page();
    }

    user.FirstName = Input.FirstName;
    user.LastName  = Input.LastName;

    if (Input.ImageFile != null)
    {
      user.ImageData = await _imageService.ConvertFileToByteArrayAsync(Input.ImageFile);
      user.ImageType = Input.ImageFile.ContentType;
    }


    await _userManager.UpdateAsync(user);


    var phoneNumber = await _userManager.GetPhoneNumberAsync(user);
    if (Input.PhoneNumber != phoneNumber)
    {
      var setPhoneResult = await _userManager.SetPhoneNumberAsync(user, Input.PhoneNumber);
      if (!setPhoneResult.Succeeded)
      {
        StatusMessage = "Unexpected error when trying to set phone number.";

        return RedirectToPage();
      }
    }

    await _signInManager.RefreshSignInAsync(user);
    StatusMessage = "Your profile has been updated";

    return RedirectToPage();
  }

  /// <summary>
  ///   This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
  ///   directly from your code. This API may change or be removed in future releases.
  /// </summary>
  public class InputModel
  {
    /// <summary>
    ///   This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
    ///   directly from your code. This API may change or be removed in future releases.
    /// </summary>
    [StringLength(40, ErrorMessage = "The {0} must be at least {2} and at most {1} characters", MinimumLength = 2)]
    [Display(Name = "First Name")]
    public string FirstName { get; set; }


    [StringLength(40, ErrorMessage = "The {0} must be at least {2} and at most {1} characters", MinimumLength = 2)]
    [Display(Name = "Last Name")]
    public string LastName { get; set; }

    public byte[] ImageData { get; set; }
    public string ImageType { get; set; }

    [NotMapped] public IFormFile ImageFile { get; set; }

    [Phone]
    [Display(Name = "Phone number")]
    public string PhoneNumber { get; set; }
  }
}