﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using Microsoft.AspNetCore.Identity;

namespace Blog.Models;

public class BlogUser : IdentityUser
{
  [Required]
  [Display(Name = "First Name")]
  [StringLength(50, ErrorMessage = "The {0} must be at least {2} and at most {1} characters", MinimumLength = 2)]
  public string? FirstName { get; set; }

  [Required]
  [Display(Name = "Last Name")]
  [StringLength(50, ErrorMessage = "The {0} must be at least {2} and at most {1} characters", MinimumLength = 2)]
  public string? LastName { get; set; }

  [NotMapped]
  [Display(Name = "First Name")]
  public string FullName => $"{FirstName} {LastName}";

  // Image Properties
              public byte[]?    ImageData { get; set; }
              public string?    ImageType { get; set; }
  [NotMapped] public IFormFile? ImageFile { get; set; } // Future extension

  // Navigation Properties
  public virtual ICollection<Comment>  Comments  { get; set; } = new HashSet<Comment>(); // Future extension
  public virtual ICollection<BlogLike> BlogLikes { get; set; } = new HashSet<BlogLike>();
}