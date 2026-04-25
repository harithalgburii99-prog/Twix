using Microsoft.EntityFrameworkCore;
using Twix.Models;

namespace Twix.Data;

public class TwixDb : DbContext
{
    public TwixDb(DbContextOptions<TwixDb> options) : base(options) { }

    public DbSet<User>         Users         => Set<User>();
    public DbSet<Post>         Posts         => Set<Post>();
    public DbSet<Like>         Likes         => Set<Like>();
    public DbSet<Follow>       Follows       => Set<Follow>();
    public DbSet<Bookmark>     Bookmarks     => Set<Bookmark>();
    public DbSet<Notification> Notifications => Set<Notification>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        // Unique indexes
        b.Entity<User>().HasIndex(u => u.UserName).IsUnique();
        b.Entity<User>().HasIndex(u => u.Email).IsUnique();
        b.Entity<Like>().HasIndex(l => new { l.PostId, l.UserId }).IsUnique();
        b.Entity<Follow>().HasIndex(f => new { f.FollowerId, f.FollowingId }).IsUnique();
        b.Entity<Bookmark>().HasIndex(bm => new { bm.PostId, bm.UserId }).IsUnique();

        // No cascade delete chains
        b.Entity<Like>()
            .HasOne(l => l.Post).WithMany(p => p.Likes).HasForeignKey(l => l.PostId)
            .OnDelete(DeleteBehavior.Cascade);
        b.Entity<Like>()
            .HasOne(l => l.User).WithMany(u => u.Likes).HasForeignKey(l => l.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        b.Entity<Bookmark>()
            .HasOne(bm => bm.Post).WithMany(p => p.Bookmarks).HasForeignKey(bm => bm.PostId)
            .OnDelete(DeleteBehavior.Cascade);
        b.Entity<Bookmark>()
            .HasOne(bm => bm.User).WithMany(u => u.Bookmarks).HasForeignKey(bm => bm.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        b.Entity<Follow>()
            .HasOne(f => f.Follower).WithMany(u => u.Following).HasForeignKey(f => f.FollowerId)
            .OnDelete(DeleteBehavior.Restrict);
        b.Entity<Follow>()
            .HasOne(f => f.Following).WithMany(u => u.Followers).HasForeignKey(f => f.FollowingId)
            .OnDelete(DeleteBehavior.Restrict);

        b.Entity<Notification>()
            .HasOne(n => n.Recipient).WithMany().HasForeignKey(n => n.RecipientId)
            .OnDelete(DeleteBehavior.Cascade);
        b.Entity<Notification>()
            .HasOne(n => n.Actor).WithMany().HasForeignKey(n => n.ActorId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
