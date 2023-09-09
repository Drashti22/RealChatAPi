using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using RealChatApi.Models;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(Microsoft.EntityFrameworkCore.DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }
    public DbSet<Message> Messages { get; set; }


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


        base.OnModelCreating(modelBuilder);
    }
}
    public class ApplicationUser : IdentityUser
    {
        public string Name { get; set; }

        public string Token { get; set; }


        public virtual ICollection<Message>? SentMessages { get; set; }

        public virtual ICollection<Message>? ReceivedMessages { get; set; }
    }

