using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Twix.Data;
using Twix.Models;
using Twix.ViewModels;

namespace Twix.Controllers;

[Authorize]
public class HomeController : Controller
{
    private readonly TwixDb _db;
    public HomeController(TwixDb db) => _db = db;

    private int CurrentUserId =>
        int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);

    // GET /
    public IActionResult Index()
    {
        var posts = GetPosts(_db.Posts.Where(p => !p.IsDeleted));
        return View(posts);
    }

    // GET /Home/Following
    public IActionResult Following()
    {
        var uid        = CurrentUserId;
        var followedIds = _db.Follows.Where(f => f.FollowerId == uid).Select(f => f.FollowingId);
        var posts = GetPosts(_db.Posts.Where(p => !p.IsDeleted && followedIds.Contains(p.AuthorId)));
        return View("Index", posts);
    }

    // GET /Home/Search?q=hello
    public IActionResult Search(string q)
    {
        if (string.IsNullOrWhiteSpace(q)) return RedirectToAction("Index");
        q = q.Trim().ToLower();
        var posts = GetPosts(_db.Posts.Where(p =>
            !p.IsDeleted && (p.Content.ToLower().Contains(q) ||
                             p.Author.UserName.ToLower().Contains(q) ||
                             p.Author.DisplayName.ToLower().Contains(q))));
        ViewBag.Query = q;
        return View("Index", posts);
    }

    // GET /Home/Bookmarks
    public IActionResult Bookmarks()
    {
        var uid     = CurrentUserId;
        var postIds = _db.Bookmarks.Where(b => b.UserId == uid).Select(b => b.PostId);
        var posts   = GetPosts(_db.Posts.Where(p => !p.IsDeleted && postIds.Contains(p.Id)));
        return View("Index", posts);
    }

    // POST /Home/Create
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreatePostViewModel model)
    {
        if (!ModelState.IsValid) return RedirectToAction("Index");
        _db.Posts.Add(new Post { Content = model.Content.Trim(), AuthorId = CurrentUserId });
        await _db.SaveChangesAsync();
        return RedirectToAction("Index");
    }

    // POST /Home/Edit
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, string content)
    {
        var post = await _db.Posts.FindAsync(id);
        if (post == null || post.AuthorId != CurrentUserId) return Forbid();
        post.Content   = content.Trim();
        post.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return RedirectToAction("Index");
    }

    // POST /Home/Delete
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var post = await _db.Posts.FindAsync(id);
        if (post == null || post.AuthorId != CurrentUserId) return Forbid();
        post.IsDeleted = true;
        await _db.SaveChangesAsync();
        return RedirectToAction("Index");
    }

    // POST /Home/Like
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Like(int postId, string? returnUrl)
    {
        var uid      = CurrentUserId;
        var existing = _db.Likes.FirstOrDefault(l => l.PostId == postId && l.UserId == uid);
        if (existing != null) _db.Likes.Remove(existing);
        else                  _db.Likes.Add(new Like { PostId = postId, UserId = uid });
        await _db.SaveChangesAsync();
        return Redirect(returnUrl ?? "/");
    }

    // POST /Home/Bookmark
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Bookmark(int postId, string? returnUrl)
    {
        var uid      = CurrentUserId;
        var existing = _db.Bookmarks.FirstOrDefault(b => b.PostId == postId && b.UserId == uid);
        if (existing != null) _db.Bookmarks.Remove(existing);
        else                  _db.Bookmarks.Add(new Bookmark { PostId = postId, UserId = uid });
        await _db.SaveChangesAsync();
        return Redirect(returnUrl ?? "/");
    }

    // ── Helper ──────────────────────────────────────────────────────────────
    private List<PostViewModel> GetPosts(IQueryable<Post> query)
    {
        var uid = CurrentUserId;
        return query
            .Include(p => p.Author)
            .Include(p => p.Likes)
            .Include(p => p.Bookmarks)
            .OrderByDescending(p => p.CreatedAt)
            .Take(100)
            .ToList()
            .Select(p => new PostViewModel
            {
                Id           = p.Id,
                Content      = p.Content,
                CreatedAt    = p.CreatedAt,
                LikeCount    = p.Likes.Count,
                IsLiked      = p.Likes.Any(l => l.UserId == uid),
                IsBookmarked = p.Bookmarks.Any(b => b.UserId == uid),
                IsOwn        = p.AuthorId == uid,
                AuthorId     = p.AuthorId,
                AuthorName   = p.Author.DisplayName,
                AuthorHandle = p.Author.UserName,
                AuthorIsAdmin= p.Author.IsAdmin
            }).ToList();
    }
}
