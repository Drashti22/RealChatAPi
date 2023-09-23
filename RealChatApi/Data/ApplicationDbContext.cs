using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using RealChatApi.Models;
using System.Text.Json.Serialization;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(Microsoft.EntityFrameworkCore.DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Message> Messages { get; set; }

    public DbSet<Log> Logs { get; set; }

    public DbSet<Group> Groups { get; set; }

    public DbSet<GroupMember> GroupMembers { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configure one-to-many relationship between User and Message
        modelBuilder.Entity<Message>()
            .HasOne(m => m.Sender)
            .WithMany(u => u.SentMessages)
            .HasForeignKey(m => m.SenderId)
            .OnDelete(DeleteBehavior.ClientSetNull);


        modelBuilder.Entity<Message>()
            .HasOne(m => m.Receiver)
            .WithMany(u => u.ReceivedMessages)
            .HasForeignKey(m => m.ReceiverId)
            .OnDelete(DeleteBehavior.ClientSetNull);

        modelBuilder.Entity<GroupMember>()
             .HasKey(gu => new {gu.UserId, gu.GroupId });

        modelBuilder.Entity<GroupMember>()
            .HasOne(u => u.User)
            .WithMany(gu => gu.GroupMembers)
            .HasForeignKey(u => u.UserId);

        modelBuilder.Entity<GroupMember>()
            .HasOne(g => g.Group)
            .WithMany(gu => gu.GroupMembers)
            .HasForeignKey(g => g.GroupId);

        base.OnModelCreating(modelBuilder);
    }
}
public class ApplicationUser : IdentityUser
{
    public string Name { get; set; }

    public string Token { get; set; }

    [JsonIgnore]
    public virtual ICollection<Message>? SentMessages { get; set; }

    [JsonIgnore]
    public virtual ICollection<Message>? ReceivedMessages { get; set; }

    public virtual ICollection<GroupMember>? GroupMembers { get; set; } 
       
}
