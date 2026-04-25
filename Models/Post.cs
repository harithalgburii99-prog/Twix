using System.ComponentModel.DataAnnotations;

namespace Twix.Models;

public class Post
{
    public int    Id         { get; set; }

    [Required, MaxLength(280)]
    public string Content    { get; set; } = "";

    public bool      IsDeleted { get; set; } = false;
    public DateTime  CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public int    AuthorId  { get; set; }
    public User   Author    { get; set; } = null!;

    public ICollection<Like>     Likes     { get; set; } = new List<Like>();
    public ICollection<Bookmark> Bookmarks { get; set; } = new List<Bookmark>();
}
