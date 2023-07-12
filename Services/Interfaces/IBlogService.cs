using Blog.Models;

namespace Blog.Services.Interfaces;

public interface IBlogService
{
  // TODO: BlogPost CRUD
  // TODO: Comment CRUD
  // TODO: Category CRUD
  // TODO: Tag CRUD
  public Task<IEnumerable<Category>> GetCategoriesAsync();

  public Task<IEnumerable<BlogPost>> GetPopularBlogPostsAsync();
  public Task<IEnumerable<BlogPost>> GetPopularBlogPostsAsync(int? count);

  public Task<IEnumerable<BlogPost>> GetRecentBlogPostsAsync();
  public Task<IEnumerable<BlogPost>> GetRecentBlogPostsAsync(int? count);

  public Task AddTagsToBlogPostAsync(IEnumerable<int?> tagIds, int? blogPostId);
  public Task AddTagsToBlogPostAsync(string tagNames, int? blogPostId);

  public Task<bool> IsTagOnBlogPostAsync(int? tagId, int? blogPostId);
  public Task RemoveAllBlogPostTagsAsync(int blogPostId);

  public IEnumerable<BlogPost> SearchBlogPosts(string? searchString);
  public Task<bool> ValidSlugAsync(string? title, int? blogPostId);
  public Task<List<Tag>> GetTagsAsync();
  public Task<bool> UserLikedBlogAsync(int blogPostId, string blogUserId);
  public Task<IEnumerable<BlogPost>> GetFavoriteBlogPostsAsync(string blogPostId);
}