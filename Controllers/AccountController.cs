using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Twix.Data;
using Twix.Helpers;
using Twix.Models;
using Twix.ViewModels;

namespace Twix.Controllers;

public class AccountController : Controller
{
    private readonly TwixDb _db;
    public AccountController(TwixDb db) => _db = db;

    // GET /Account/Login
    [HttpGet]
    public IActionResult Login(string? returnUrl)
    {
        if (User.Identity?.IsAuthenticated == true) return RedirectToAction("Index", "Home");
        ViewBag.ReturnUrl = returnUrl;
        return View();
    }

    // POST /Account/Login
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl)
    {
        if (!ModelState.IsValid) return View(model);

        var email = model.Email.Trim().ToLower();
        var user  = _db.Users.FirstOrDefault(u => u.Email == email);

        if (user == null || !PasswordHasher.Verify(model.Password, user.PasswordHash))
        {
            ModelState.AddModelError("", "Invalid email or password.");
            return View(model);
        }

        if (user.IsBanned)
        {
            ModelState.AddModelError("", "Your account has been suspended.");
            return View(model);
        }

        await SignInUser(user, model.RememberMe);

        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);

        return user.IsAdmin
            ? RedirectToAction("Index", "Admin")
            : RedirectToAction("Index", "Home");
    }

    // GET /Account/Register
    [HttpGet]
    public IActionResult Register() => View();

    // POST /Account/Register
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var email    = model.Email.Trim().ToLower();
        var username = model.UserName.Trim().ToLower();

        if (_db.Users.Any(u => u.Email == email))
            ModelState.AddModelError("Email", "Email already registered.");
        if (_db.Users.Any(u => u.UserName == username))
            ModelState.AddModelError("UserName", "Username already taken.");
        if (!ModelState.IsValid) return View(model);

        var user = new User
        {
            UserName     = username,
            Email        = email,
            DisplayName  = string.IsNullOrWhiteSpace(model.DisplayName) ? username : model.DisplayName,
            Bio          = model.Bio,
            PasswordHash = PasswordHasher.Hash(model.Password),
            CreatedAt    = DateTime.UtcNow
        };
        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        await SignInUser(user, false);
        return RedirectToAction("Index", "Home");
    }

    // POST /Account/Logout
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Login");
    }

    // ── Helper ──────────────────────────────────────────────────────────────
    private async Task SignInUser(User user, bool remember)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name,           user.UserName),
            new(ClaimTypes.Email,          user.Email),
            new("DisplayName",             user.DisplayName),
            new(ClaimTypes.Role,           user.IsAdmin ? "Admin" : "User")
        };
        var identity  = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);
        var props     = new AuthenticationProperties { IsPersistent = remember };
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, props);
    }
}
