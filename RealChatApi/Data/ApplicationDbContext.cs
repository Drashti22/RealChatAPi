using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(Microsoft.EntityFrameworkCore.DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }
}

public class ApplicationUser : IdentityUser
{
    public string Name { get; set; }

    public string Token { get; set; }
    
}

