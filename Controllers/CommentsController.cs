﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

using Blog.Data;
using Blog.Models;

namespace Blog.Controllers;

[Authorize]
public class CommentsController : Controller
{
  private readonly ApplicationDbContext  _context;
  private readonly UserManager<BlogUser> _userManager;

  public CommentsController(ApplicationDbContext context, UserManager<BlogUser> userManager)
  {
    _context     = context;
    _userManager = userManager;
  }

  // GET: Comments
  public async Task<IActionResult> Index()
  {
    var applicationDbContext = _context.Comments.Include(c => c.Author).Include(c => c.BlogPost);

    return View(await applicationDbContext.ToListAsync());
  }

  // GET: Comments/Details/5
  public async Task<IActionResult> Details(int? id)
  {
    if (id == null || !_context.Comments.Any()) 
      return NotFound();

    var comment = await _context.Comments.Include(c => c.Author).Include(c => c.BlogPost).FirstOrDefaultAsync(m => m.Id == id);
    if (comment == null) 
      return NotFound();

    return View(comment);
  }

  // GET: Comments/Create
  public IActionResult Create()
  {
    ViewData["AuthorId"]   = new SelectList(_context.Users,     "Id", "Id");
    ViewData["BlogPostId"] = new SelectList(_context.BlogPosts, "Id", "Content");
     
    return View();
  }

  // POST: Comments/Create
  // To protect from overposting attacks, enable the specific properties you want to bind to.
  // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
  [HttpPost]
  [ValidateAntiForgeryToken]
  public async Task<IActionResult> Create([Bind("Id,Body,BlogPostId")] Comment comment)
  {
    ModelState.Remove("AuthorId");

    if (ModelState.IsValid)
    {
      comment.Created  = DateTime.UtcNow;
      comment.AuthorId = _userManager.GetUserId(User);

      var blogPost = await _context.BlogPosts.FirstOrDefaultAsync(b => b.Id == comment.BlogPostId);

      _context.Add(comment);
      await _context.SaveChangesAsync();

      return RedirectToAction("Details", "BlogPosts", new { slug = blogPost!.Slug });
    }

    ViewData["AuthorId"]   = new SelectList(_context.Users,     "Id", "Id",      comment.AuthorId);
    ViewData["BlogPostId"] = new SelectList(_context.BlogPosts, "Id", "Content", comment.BlogPostId);

    return View(comment);
  }

  // GET: Comments/Edit/5
  public async Task<IActionResult> Edit(int? id)
  {
    if (id == null || !_context.Comments.Any())
      return NotFound();

    var comment = await _context.Comments.FindAsync(id);
    if (comment == null) 
      return NotFound();

    ViewData["AuthorId"]   = new SelectList(_context.Users,     "Id", "Id",      comment.AuthorId);
    ViewData["BlogPostId"] = new SelectList(_context.BlogPosts, "Id", "Content", comment.BlogPostId);

    return View(comment);
  }

  // POST: Comments/Edit/5
  // To protect from overposting attacks, enable the specific properties you want to bind to.
  // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
  [HttpPost]
  [ValidateAntiForgeryToken]
  public async Task<IActionResult> Edit(int id, [Bind("Id,Body,Created,Updated,UpdateReason,BlogPostId,AuthorId")] Comment comment)
  {
    if (id != comment.Id) 
      return NotFound();

    if (ModelState.IsValid)
    {
      try
      {
        _context.Update(comment);
        await _context.SaveChangesAsync();
      }
      catch (DbUpdateConcurrencyException)
      {
        if (!CommentExists(comment.Id))
          return NotFound();

        throw;
      }

      return RedirectToAction(nameof(Index));
    }

    ViewData["AuthorId"]   = new SelectList(_context.Users,     "Id", "Id",      comment.AuthorId);
    ViewData["BlogPostId"] = new SelectList(_context.BlogPosts, "Id", "Content", comment.BlogPostId);

    return View(comment);
  }

  // GET: Comments/Delete/5
  public async Task<IActionResult> Delete(int? id)
  {
    if (id == null || !_context.Comments.Any()) 
      return NotFound();

    var comment = await _context.Comments.Include(c => c.Author).Include(c => c.BlogPost).FirstOrDefaultAsync(m => m.Id == id);
    if (comment == null) 
      return NotFound();

    return View(comment);
  }

  // POST: Comments/Delete/5
  [HttpPost]
  [ActionName("Delete")]
  [ValidateAntiForgeryToken]
  public async Task<IActionResult> DeleteConfirmed(int id)
  {
    if (!_context.Comments.Any()) 
      return Problem("Entity set 'ApplicationDbContext.Comments' is empty.");

    var comment = await _context.Comments.FindAsync(id);
    if (comment != null) 
      _context.Comments.Remove(comment);

    await _context.SaveChangesAsync();

    return RedirectToAction(nameof(Index));
  }

  private bool CommentExists(int id) => (_context.Comments?.Any(e => e.Id == id)).GetValueOrDefault();
}