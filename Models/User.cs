using System.ComponentModel.DataAnnotations;

namespace Twix.Models;

public class User
{
    public int    Id           { get; set; }

    [Required, MaxLength(50)]
    public string UserName     { get; set; } = "";

    [Required, MaxLength(256)]
    public string Email        { get; set; } = "";

    [Required]
    public string PasswordHash { get; set; } = "";

    [MaxLength(100)]
    public string DisplayName  { get; set; } = "";

    [MaxLength(280)]
    public string Bio          { get; set; } = "";

    [MaxLength(100)]
    public string Location     { get; set; } = "";

    public bool   IsAdmin      { get; set; } = false;
    public bool   IsBanned     { get; set; } = false;
    public DateTime CreatedAt  { get; set; } = DateTime.UtcNow;

    // Navigation
    public ICollection<Post>     Posts     { get; set; } = new List<Post>();
    public ICollection<Like>     Likes     { get; set; } = new List<Like>();
    public ICollection<Follow>   Following { get; set; } = new List<Follow>();
    public ICollection<Follow>   Followers { get; set; } = new List<Follow>();
    public ICollection<Bookmark> Bookmarks { get; set; } = new List<Bookmark>();
}
