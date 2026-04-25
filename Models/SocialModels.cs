namespace Twix.Models;

public class Like
{
    public int  Id     { get; set; }
    public int  PostId { get; set; }
    public Post Post   { get; set; } = null!;
    public int  UserId { get; set; }
    public User User   { get; set; } = null!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class Follow
{
    public int  Id          { get; set; }
    public int  FollowerId  { get; set; }
    public User Follower    { get; set; } = null!;
    public int  FollowingId { get; set; }
    public User Following   { get; set; } = null!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class Bookmark
{
    public int  Id     { get; set; }
    public int  PostId { get; set; }
    public Post Post   { get; set; } = null!;
    public int  UserId { get; set; }
    public User User   { get; set; } = null!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class Notification
{
    public int    Id          { get; set; }
    public int    RecipientId { get; set; }
    public User   Recipient   { get; set; } = null!;
    public int    ActorId     { get; set; }
    public User   Actor       { get; set; } = null!;
    public string Type        { get; set; } = ""; // like | follow | reply
    public int?   PostId      { get; set; }
    public bool   IsRead      { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
