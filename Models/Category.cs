using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Blog.Models;

public class Category
{
  // Primary Key
  public int Id { get; set; }

  [Required]
  [Display(Name = "Category Name")]
  [StringLength(50, ErrorMessage = "The {0} must be at least {2} and at most {1} characters", MinimumLength = 2)]
  public string? Name { get; set; }

  [Required]
  [StringLength(200, ErrorMessage = "The {0} must be at least {2} and at most {1} characters", MinimumLength = 2)]
  public string? Description { get; set; }

  // Image Properties
              public byte[]?    ImageData { get; set; }
              public string?    ImageType { get; set; }
  [NotMapped] public IFormFile? ImageFile { get; set; }

  // Navigation Collections
  public virtual ICollection<BlogPost> BlogPosts { get; set; } = new HashSet<BlogPost>();
}