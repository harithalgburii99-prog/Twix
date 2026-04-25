using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Twix.Data;
using Twix.Models;

namespace Twix.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly TwixDb _db;
    public AdminController(TwixDb db) => _db = db;

    // ── DASHBOARD ─────────────────────────────────────────────────
    // GET /Admin
    [HttpGet("/Admin")]
    public IActionResult Index()
    {
        ViewBag.UserCount   = _db.Users.Count();
        ViewBag.PostCount   = _db.Posts.Count(p => !p.IsDeleted);
        ViewBag.LikeCount   = _db.Likes.Count();
        ViewBag.BannedCount = _db.Users.Count(u => u.IsBanned);
        ViewBag.RecentUsers = _db.Users
            .OrderByDescending(u => u.CreatedAt).Take(5).ToList();
        ViewBag.RecentPosts = _db.Posts
            .Include(p => p.Author)
            .Where(p => !p.IsDeleted)
            .OrderByDescending(p => p.CreatedAt).Take(5).ToList();
        return View();
    }

    // ══════════════════════════════════════════════════════════════
    // USERS
    // ══════════════════════════════════════════════════════════════

    // GET /Admin/Users
    [HttpGet("/Admin/Users")]
    public IActionResult Users(string? q)
    {
        var users = _db.Users.AsQueryable();
        if (!string.IsNullOrWhiteSpace(q))
            users = users.Where(u =>
                u.UserName.Contains(q) ||
                u.Email.Contains(q) ||
                u.DisplayName.Contains(q));
        ViewBag.Query = q;
        return View(users.OrderByDescending(u => u.CreatedAt).ToList());
    }

    // GET /Admin/EditUser/5
    [HttpGet("/Admin/EditUser/{id}")]
    public async Task<IActionResult> EditUser(int id)
    {
        var user = await _db.Users.FindAsync(id);
        if (user == null) return NotFound();
        return View(user);
    }

    // POST /Admin/EditUser
    [HttpPost("/Admin/EditUser")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditUser(int id, string displayName, string bio,
        string location, bool isAdmin, bool isBanned)
    {
        var user = await _db.Users.FindAsync(id);
        if (user == null) return NotFound();
        user.DisplayName = displayName;
        user.Bio         = bio;
        user.Location    = location;
        user.IsAdmin     = isAdmin;
        user.IsBanned    = isBanned;
        await _db.SaveChangesAsync();
        TempData["Success"] = $"User @{user.UserName} updated.";
        return RedirectToAction("Users");
    }

    // POST /Admin/BanUser
    [HttpPost("/Admin/BanUser")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> BanUser(int id)
    {
        var user = await _db.Users.FindAsync(id);
        if (user == null) return NotFound();
        user.IsBanned = !user.IsBanned;
        await _db.SaveChangesAsync();
        TempData["Success"] = user.IsBanned
            ? $"@{user.UserName} banned."
            : $"@{user.UserName} unbanned.";
        return RedirectToAction("Users");
    }

    // POST /Admin/MakeAdmin
    [HttpPost("/Admin/MakeAdmin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MakeAdmin(int id)
    {
        var user = await _db.Users.FindAsync(id);
        if (user == null) return NotFound();
        user.IsAdmin = !user.IsAdmin;
        await _db.SaveChangesAsync();
        TempData["Success"] = user.IsAdmin
            ? $"@{user.UserName} is now admin."
            : $"@{user.UserName} is no longer admin.";
        return RedirectToAction("Users");
    }

    // POST /Admin/DeleteUser
    [HttpPost("/Admin/DeleteUser")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteUser(int id)
    {
        var user = await _db.Users.FindAsync(id);
        if (user == null) return NotFound();
        var posts = _db.Posts.Where(p => p.AuthorId == id).ToList();
        posts.ForEach(p => p.IsDeleted = true);
        _db.Users.Remove(user);
        await _db.SaveChangesAsync();
        TempData["Success"] = "User deleted.";
        return RedirectToAction("Users");
    }

    // ══════════════════════════════════════════════════════════════
    // POSTS
    // ══════════════════════════════════════════════════════════════

    // GET /Admin/Posts
    [HttpGet("/Admin/Posts")]
    public IActionResult Posts(string? q)
    {
        var posts = _db.Posts.Include(p => p.Author)
                             .Include(p => p.Likes)
                             .AsQueryable();
        if (!string.IsNullOrWhiteSpace(q))
            posts = posts.Where(p =>
                p.Content.Contains(q) ||
                p.Author.UserName.Contains(q));
        ViewBag.Query = q;
        return View(posts.OrderByDescending(p => p.CreatedAt).ToList());
    }

    // GET /Admin/EditPost/5
    [HttpGet("/Admin/EditPost/{id}")]
    public async Task<IActionResult> EditPost(int id)
    {
        var post = await _db.Posts
            .Include(p => p.Author)
            .FirstOrDefaultAsync(p => p.Id == id);
        if (post == null) return NotFound();
        return View(post);
    }

    // POST /Admin/EditPost
    [HttpPost("/Admin/EditPost")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditPost(int id, string content)
    {
        var post = await _db.Posts.FindAsync(id);
        if (post == null) return NotFound();
        post.Content   = content.Trim();
        post.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        TempData["Success"] = "Post updated.";
        return RedirectToAction("Posts");
    }

    // POST /Admin/DeletePost
    [HttpPost("/Admin/DeletePost")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeletePost(int id)
    {
        var post = await _db.Posts.FindAsync(id);
        if (post == null) return NotFound();
        post.IsDeleted = true;
        await _db.SaveChangesAsync();
        TempData["Success"] = "Post removed.";
        return RedirectToAction("Posts");
    }

    // POST /Admin/RestorePost
    [HttpPost("/Admin/RestorePost")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RestorePost(int id)
    {
        var post = await _db.Posts.FindAsync(id);
        if (post == null) return NotFound();
        post.IsDeleted = false;
        await _db.SaveChangesAsync();
        TempData["Success"] = "Post restored.";
        return RedirectToAction("Posts");
    }
}
