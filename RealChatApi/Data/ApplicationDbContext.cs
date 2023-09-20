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

        modelBuilder.Entity<Group>()
            .HasMany(g => g.Messages )
            .WithOne(m => m.Group)
            .HasForeignKey(m => m.GroupId)
            .OnDelete(DeleteBehavior.ClientSetNull);

        modelBuilder.Entity<ApplicationUser>()
            .HasMany(u => u.Groups)
            .WithMany(g => g.Members)
            .UsingEntity(j => j.ToTable("UserGroups"));

        modelBuilder.Entity<GroupMember>()
        .HasKey(gm => new { gm.GroupId, gm.UserId });

        modelBuilder.Entity<GroupMember>()
            .HasOne(gm => gm.Group)
            .WithMany(g => g.GroupMembers)
            .HasForeignKey(gm => gm.GroupId);

        modelBuilder.Entity<GroupMember>()
            .HasOne(gm => gm.User)
            .WithMany(u => u.GroupMembers)
            .HasForeignKey(gm => gm.UserId);

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

    [JsonIgnore]
    public virtual ICollection<Group>? Groups { get; set; }

    [JsonIgnore]
    public virtual ICollection<GroupMember>? GroupMembers { get; set; } 
       
}
