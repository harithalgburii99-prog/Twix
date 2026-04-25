using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Twix.Data;
using Twix.Models;
using Twix.ViewModels;

namespace Twix.Controllers;

[Authorize]
public class ProfileController : Controller
{
    private readonly TwixDb _db;
    public ProfileController(TwixDb db) => _db = db;

    private int CurrentUserId =>
        int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);

    // GET /Profile/{username}
    [HttpGet("/Profile/{username}")]
    public IActionResult Index(string username)
    {
        var uid      = CurrentUserId;
        var profile  = _db.Users.FirstOrDefault(u => u.UserName == username.ToLower());
        if (profile == null) return NotFound();

        var posts = _db.Posts
            .Include(p => p.Likes)
            .Include(p => p.Bookmarks)
            .Where(p => p.AuthorId == profile.Id && !p.IsDeleted)
            .OrderByDescending(p => p.CreatedAt)
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
                AuthorId     = profile.Id,
                AuthorName   = profile.DisplayName,
                AuthorHandle = profile.UserName,
                AuthorIsAdmin= profile.IsAdmin
            }).ToList();

        ViewBag.Profile       = profile;
        ViewBag.FollowerCount = _db.Follows.Count(f => f.FollowingId == profile.Id);
        ViewBag.FollowingCount= _db.Follows.Count(f => f.FollowerId  == profile.Id);
        ViewBag.IsFollowing   = _db.Follows.Any(f => f.FollowerId == uid && f.FollowingId == profile.Id);
        ViewBag.IsOwn         = profile.Id == uid;
        return View(posts);
    }

    // POST /Profile/Follow
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Follow(int userId)
    {
        var uid = CurrentUserId;
        if (userId == uid) return BadRequest();
        var existing = _db.Follows.FirstOrDefault(f => f.FollowerId == uid && f.FollowingId == userId);
        if (existing != null) _db.Follows.Remove(existing);
        else                  _db.Follows.Add(new Follow { FollowerId = uid, FollowingId = userId });
        await _db.SaveChangesAsync();
        var targetUser = await _db.Users.FindAsync(userId);
        return RedirectToAction("Index", new { username = targetUser!.UserName });
    }

    // GET /Profile/Edit
    [HttpGet]
    public IActionResult Edit()
    {
        var user = _db.Users.Find(CurrentUserId)!;
        return View(new EditProfileViewModel
        {
            DisplayName = user.DisplayName,
            Bio         = user.Bio,
            Location    = user.Location
        });
    }

    // POST /Profile/Edit
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(EditProfileViewModel model)
    {
        if (!ModelState.IsValid) return View(model);
        var user = await _db.Users.FindAsync(CurrentUserId);
        if (user == null) return NotFound();
        user.DisplayName = model.DisplayName;
        user.Bio         = model.Bio;
        user.Location    = model.Location;
        await _db.SaveChangesAsync();
        return RedirectToAction("Index", new { username = user.UserName });
    }
}
