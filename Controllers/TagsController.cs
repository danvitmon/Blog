using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using X.PagedList;

using Blog.Data;
using Blog.Models;
using Blog.Services.Interfaces;

namespace Blog.Controllers;

public class TagsController : Controller
{
  private readonly IBlogService         _blogService;
  private readonly ApplicationDbContext _context;

  public TagsController(ApplicationDbContext context, IBlogService blogService)
  {
    _context     = context;
    _blogService = blogService;
  }

  // GET: Tags
  public async Task<IActionResult> Index()
  {
    return _context.Tags != null ? View(await _context.Tags.ToListAsync()) : Problem("Entity set 'ApplicationDbContext.Tags'  is null.");
  }

  // GET: Tags/Details/5
  public async Task<IActionResult> Details(int? tagId, int? pageNum)
  {
    if (tagId == null || _context.Tags == null) 
     return NotFound();

    var tag = await _context.Tags.Include(t => t.BlogPosts).FirstOrDefaultAsync(t => t.Id == tagId);

    if (tag == null) 
     return NotFound();

    var pageSize  = 5;
    var page      = pageNum ?? 1;
    var blogPosts = await tag.BlogPosts.ToPagedListAsync(page, pageSize);

    ViewData["ActionName"] = "Details";
    ViewData["Tags"]       = await _blogService.GetTagsAsync();

    return View(blogPosts);
  }

  // GET: Tags/Create
  public IActionResult Create() => View();

  // POST: Tags/Create
  // To protect from overposting attacks, enable the specific properties you want to bind to.
  // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
  [HttpPost]
  [ValidateAntiForgeryToken]
  public async Task<IActionResult> Create([Bind("Id,Name")] Tag tag)
  {
    if (ModelState.IsValid)
    {
      _context.Add(tag);
      await _context.SaveChangesAsync();

      return RedirectToAction(nameof(Index));
    }

    return View(tag);
  }

  // GET: Tags/Edit/5
  public async Task<IActionResult> Edit(int? id)
  {
    if (id == null || _context.Tags == null) 
     return NotFound();

    var tag = await _context.Tags.FindAsync(id);

    if (tag == null) 
     return NotFound();

    return View(tag);
  }

  // POST: Tags/Edit/5
  // To protect from overposting attacks, enable the specific properties you want to bind to.
  // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
  [HttpPost]
  [ValidateAntiForgeryToken]
  public async Task<IActionResult> Edit(int id, [Bind("Id,Name")] Tag tag)
  {
    if (id != tag.Id) 
     return NotFound();

    if (ModelState.IsValid)
    {
      try
      {
        _context.Update(tag);
        await _context.SaveChangesAsync();
      }
      catch (DbUpdateConcurrencyException)
      {
        if (!TagExists(tag.Id))
         return NotFound();

        throw;
      }

      return RedirectToAction(nameof(Index));
    }

    return View(tag);
  }

  // GET: Tags/Delete/5
  public async Task<IActionResult> Delete(int? id)
  {
    if (id == null || _context.Tags == null) 
     return NotFound();

    var tag = await _context.Tags.FirstOrDefaultAsync(m => m.Id == id);

    if (tag == null) 
     return NotFound();

    return View(tag);
  }

  // POST: Tags/Delete/5
  [HttpPost]
  [ActionName("Delete")]
  [ValidateAntiForgeryToken]
  public async Task<IActionResult> DeleteConfirmed(int id)
  {
    if (_context.Tags == null) 
     return Problem("Entity set 'ApplicationDbContext.Tags'  is null.");

    var tag = await _context.Tags.FindAsync(id);
    if (tag != null) _context.Tags.Remove(tag);

    await _context.SaveChangesAsync();

    return RedirectToAction(nameof(Index));
  }

  private bool TagExists(int id)
  {
    return (_context.Tags?.Any(e => e.Id == id)).GetValueOrDefault();
  }
}