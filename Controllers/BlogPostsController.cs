using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

using Blog.Data;
using Blog.Helpers;
using Blog.Models;
using Blog.Services.Interfaces;

namespace Blog.Controllers;

[Authorize]
public class BlogPostsController : Controller
{
  private readonly IBlogService          _blogService;
  private readonly ApplicationDbContext  _context;
  private readonly IImageService         _imageService;

  public BlogPostsController(ApplicationDbContext context, IImageService imageService, IBlogService blogService)
  {
    _context       = context;
    _imageService  = imageService;
    _blogService   = blogService;
  }

  // GET: BlogPosts
  public async Task<IActionResult> Index()
  {
    var applicationDbContext = _context.BlogPosts.Include(b => b.Category);

    return View(await applicationDbContext.ToListAsync());
  }

  public async Task<IActionResult> Favorites()
  {
    var applicationDbContext = _context.BlogPosts.Include(b => b.Category);

    return View(await applicationDbContext.ToListAsync());
  }

  // GET: BlogPosts/Details/5
  [AllowAnonymous]
  public async Task<IActionResult> Details(string? slug)
  {
    if (string.IsNullOrEmpty(slug)) 
      return NotFound();

    var blogPost = await _context.BlogPosts.Include(b => b.Category)
                                           .Include(b => b.Comments)
                                           .ThenInclude(comments => comments.Author)
                                           .Include(b => b.Tags)
                                           .Include(b => b.Likes)
                                           .FirstOrDefaultAsync(m => m.Slug == slug);

    if (blogPost == null) 
      return NotFound();

    return View(blogPost);
  }

  // GET: BlogPosts/Create
  public IActionResult Create()
  {
    BlogPost blogPost = new();
    ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Description");

    return View(blogPost);
  }

  // POST: BlogPosts/Create
  [HttpPost]
  [ValidateAntiForgeryToken]
  public async Task<IActionResult> Create([Bind("Id,Title,Abstract,Content,CreatedDate,UpdatedDate,Slug,IsDeleted,IsPublished,CategoryId,ImageData,ImageType,ImageFile")] BlogPost blogPost, string? stringTags)
  {
    ModelState.Remove("Slug");

    if (ModelState.IsValid)
    {
      if (!await _blogService.ValidSlugAsync(blogPost.Title, blogPost.Id))
      {
        ModelState.AddModelError("Title", "A similar Title/Slug is already in use.");

        ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", blogPost.CategoryId);

        return View(blogPost);
      }

      blogPost.Slug = StringHelper.BlogPostSlug(blogPost.Title);

      if (blogPost.ImageFile != null)
      {
        blogPost.ImageData = await _imageService.ConvertFileToByteArrayAsync(blogPost.ImageFile);
        blogPost.ImageType = blogPost.ImageFile.ContentType;
      }

      blogPost.CreatedDate = DateTime.UtcNow;

      _context.Add(blogPost);

      await _context.SaveChangesAsync();

      if (!string.IsNullOrEmpty(stringTags)) 
        await _blogService.AddTagsToBlogPostAsync(stringTags, blogPost.Id);

      return RedirectToAction(nameof(Index));
    }

    ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Description", blogPost.CategoryId);

    return View(blogPost);
  }

  // GET: BlogPosts/Edit/5
  public async Task<IActionResult> Edit(int? id)
  {
    if (id == null || !_context.BlogPosts.Any()) 
      return NotFound();

    var blogPost = await _context.BlogPosts.FindAsync(id);
    if (blogPost == null) 
      return NotFound();

    var tagNames = blogPost.Tags.Select(t => t.Name!).ToList();

    ViewData["tags"]       = string.Join(", ", tagNames) + ", ";
    ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Description", blogPost.CategoryId);

    return View(blogPost);
  }

  [HttpPost]
  [ValidateAntiForgeryToken]
  public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Abstract,Content,CreatedDate,UpdatedDate,Slug,IsDeleted,IsPublished,CategoryId,ImageData,ImageType")] BlogPost blogPost, string? stringTags)
  {
    if (id != blogPost.Id) 
      return NotFound();

    ModelState.Remove("Slug");

    if (ModelState.IsValid)
      try
      {
        if (!await _blogService.ValidSlugAsync(blogPost.Title, blogPost.Id))
        {
          ModelState.AddModelError("Title", "A similar Title/Slug is already in use.");

          ViewData["tags"]       = stringTags;
          ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", blogPost.CategoryId);

          return View(blogPost);
        }

        blogPost.Slug        = StringHelper.BlogPostSlug(blogPost.Title);
        blogPost.CreatedDate = DateTime.SpecifyKind(blogPost.CreatedDate, DateTimeKind.Utc);
        blogPost.UpdatedDate = DateTime.UtcNow;

        if (blogPost.ImageFile != null)
        {
          blogPost.ImageData = await _imageService.ConvertFileToByteArrayAsync(blogPost.ImageFile);
          blogPost.ImageType = blogPost.ImageFile.ContentType;
        }

        _context.Update(blogPost);

        await _context.SaveChangesAsync();

        await _blogService.RemoveAllBlogPostTagsAsync(blogPost.Id);
        if (!string.IsNullOrEmpty(stringTags)) 
          await _blogService.AddTagsToBlogPostAsync(stringTags, blogPost.Id);

        return RedirectToAction(nameof(Details), new { slug = blogPost.Slug });
      }
      catch (DbUpdateConcurrencyException)
      {
        if (!BlogPostExists(blogPost.Id))
          return NotFound();

        throw;
      }

    ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Description", blogPost.CategoryId);

    return View(blogPost);
  }

  // GET: BlogPosts/Delete/5
  public async Task<IActionResult> Delete(int? id)
  {
    if (id == null || !_context.BlogPosts.Any()) 
      return NotFound();

    var blogPost = await _context.BlogPosts.Include(b => b.Category).FirstOrDefaultAsync(m => m.Id == id);

    if (blogPost == null) 
      return NotFound();

    return View(blogPost);
  }

  // POST: BlogPosts/Delete/5
  [HttpPost]
  [ActionName("Delete")]
  [ValidateAntiForgeryToken]
  public async Task<IActionResult> DeleteConfirmed(int id)
  {
    if (!_context.BlogPosts.Any()) 
     return Problem("Entity set 'ApplicationDbContext.BlogPosts' is empty.");

    var blogPost = await _context.BlogPosts.FindAsync(id);

    if (blogPost != null) 
      _context.BlogPosts.Remove(blogPost);

    await _context.SaveChangesAsync();

    return RedirectToAction(nameof(Index));
  }

  private bool BlogPostExists(int id) => (_context.BlogPosts?.Any(e => e.Id == id)).GetValueOrDefault();

  public async Task<IActionResult> LikeBlogPost(int blogPostId, string blogUserId)
  {
    var       blogUser = await _context.Users.Include(u => u.BlogLikes).FirstOrDefaultAsync(u => u.Id == blogUserId);
    var       result   = false;
    BlogLike? blogLike;

    if (blogUser != null)
    {
      if (!blogUser.BlogLikes.Any(bl => bl.BlogPostId == blogPostId))
      {
        blogLike = new BlogLike
        {
          BlogPostId = blogPostId,
          IsLiked    = true
        };

        blogUser.BlogLikes.Add(blogLike);
      }
      else
      {
        blogLike = await _context.BlogLikes.FirstOrDefaultAsync(bl => bl.BlogPostId == blogPostId && bl.BlogUserId == blogUserId);

        blogLike!.IsLiked = !blogLike.IsLiked;
      }

      result = blogLike.IsLiked;
      await _context.SaveChangesAsync();
    }

    return Json(new
    {
      isLiked = result,
      count   = _context.BlogLikes.Count(bl => bl.BlogPostId == blogPostId && bl.IsLiked)
    });
  }

  [HttpGet]
  public async Task<IActionResult> Popular()
  {
    var model = await _blogService.GetPopularBlogPostsAsync();

    return View(model);
  }
}