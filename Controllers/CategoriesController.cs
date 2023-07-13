using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using X.PagedList;

using Blog.Data;
using Blog.Models;
using Blog.Services.Interfaces;

namespace Blog.Controllers;

public class CategoriesController : Controller
{
  private readonly IBlogService         _blogService;
  private readonly ApplicationDbContext _context;

  public CategoriesController(ApplicationDbContext context, IBlogService blogService)
  {
    _context     = context;
    _blogService = blogService;
  }

  // GET: Categories
  public async Task<IActionResult> Index()
  {
    return _context.Categories.Any() ? View(await _context.Categories.ToListAsync()) : Problem("Entity set 'ApplicationDbContext.Categories' is empty.");
  }

  // GET: Categories/Details/5
  public async Task<IActionResult> Details(int? id, int? pageNum)
  {
    if (id == null || !_context.Categories.Any()) 
      return NotFound();

    var category1 = await _context.Categories.Include(c => c.BlogPosts).FirstOrDefaultAsync(c => c.Id == id);

    if (category1 == null) 
      return NotFound();

    var pageSize  = 5;
    var page      = pageNum ?? 1;
    var blogPosts = await category1.BlogPosts.ToPagedListAsync(page, pageSize);

    ViewData["ActionName"] = "Details";
    ViewData["Categories"] = await _blogService.GetCategoriesAsync();

    return View(blogPosts);
  }

  // GET: Categories/Create
  public IActionResult Create() => View();

  // POST: Categories/Create
  // To protect from overposting attacks, enable the specific properties you want to bind to.
  // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
  [HttpPost]
  [ValidateAntiForgeryToken]
  public async Task<IActionResult> Create([Bind("Id,Name,Description,ImageData,ImageType")] Category category)
  {
    if (ModelState.IsValid)
    {
      _context.Add(category);
      await _context.SaveChangesAsync();

      return RedirectToAction(nameof(Index));
    }

    return View(category);
  }

  // GET: Categories/Edit/5
  public async Task<IActionResult> Edit(int? id)
  {
    if (id == null || !_context.Categories.Any()) 
      return NotFound();

    var category = await _context.Categories.FindAsync(id);
    if (category == null) 
      return NotFound();

    return View(category);
  }

  // POST: Categories/Edit/5
  // To protect from overposting attacks, enable the specific properties you want to bind to.
  // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
  [HttpPost]
  [ValidateAntiForgeryToken]
  public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,ImageData,ImageType")] Category category)
  {
    if (id != category.Id)  
      return NotFound();

    if (ModelState.IsValid)
    {
      try
      {
        _context.Update(category);
        await _context.SaveChangesAsync();
      }
      catch (DbUpdateConcurrencyException)
      {
        if (!CategoryExists(category.Id))
          return NotFound();

        throw;
      }

      return RedirectToAction(nameof(Index));
    }

    return View(category);
  }

  // GET: Categories/Delete/5
  public async Task<IActionResult> Delete(int? id)
  {
    if (id == null || !_context.Categories.Any()) 
      return NotFound();

    var category = await _context.Categories.FirstOrDefaultAsync(m => m.Id == id);
    if (category == null) 
      return NotFound();

    return View(category);
  }

  // POST: Categories/Delete/5
  [HttpPost]
  [ActionName("Delete")]
  [ValidateAntiForgeryToken]
  public async Task<IActionResult> DeleteConfirmed(int id)
  {
    if (!_context.Categories.Any()) 
      return Problem("Entity set 'ApplicationDbContext.Categories' is empty.");

    var category = await _context.Categories.FindAsync(id);
    if (category != null) 
      _context.Categories.Remove(category);

    await _context.SaveChangesAsync();

    return RedirectToAction(nameof(Index));
  }

  private bool CategoryExists(int id) => (_context.Categories?.Any(e => e.Id == id)).GetValueOrDefault();
}