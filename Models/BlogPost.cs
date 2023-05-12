using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Blog.Models
{
    public class BlogPost
    {
        // Primary Key
        public int Id { get; set; }

        [Required]
        [StringLength(200, ErrorMessage = "The {0} must be at least {2} and at most {1} characters", MinimumLength = 2)]
        public string? Title { get; set; }

        [StringLength(600, ErrorMessage = "The {0} must be at least {2} and at most {1} characters", MinimumLength = 2)]
        public string? Abstract { get; set; }

        [Required]
        public string? Content { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime CreatedDate { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime UpdatedDate { get; set; }

        public string? Slug { get; set; }

        [Display(Name = "Deleted?")]
        public bool IsDeleted { get; set; }

        [Display(Name = "Published?")]
        public bool IsPublished { get; set; }

        // Foreign Key 1-to-1
        public int CategoryId { get; set; }

        // Image Properties
        public byte[]? ImageData { get; set; }
        
        public string? ImageType { get; set; }

        [NotMapped]
        public IFormFile? ImageFile { get; set; }

        // Navigation Properties
        public virtual Category? Category { get; set; }

        // Navigation Collections
        // Many-to-Many
        public virtual ICollection<Comment> Comments { get; set; } = new HashSet<Comment>();

        // 1-to-Many
        public virtual ICollection<Tag> Tags { get; set; } = new HashSet<Tag>();
    }
}
