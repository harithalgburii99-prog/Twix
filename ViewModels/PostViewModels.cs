using System.ComponentModel.DataAnnotations;

namespace Twix.ViewModels;

public class CreatePostViewModel
{
    [Required, MinLength(1), MaxLength(280)]
    public string Content { get; set; } = "";
}

public class PostViewModel
{
    public int      Id           { get; set; }
    public string   Content      { get; set; } = "";
    public DateTime CreatedAt    { get; set; }
    public int      LikeCount    { get; set; }
    public int      ReplyCount   { get; set; }
    public bool     IsLiked      { get; set; }
    public bool     IsBookmarked { get; set; }
    public bool     IsOwn        { get; set; }
    public int      AuthorId     { get; set; }
    public string   AuthorName   { get; set; } = "";
    public string   AuthorHandle { get; set; } = "";
    public bool     AuthorIsAdmin{ get; set; }
}
