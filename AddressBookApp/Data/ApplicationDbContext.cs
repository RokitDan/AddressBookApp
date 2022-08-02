using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using AddressBookApp.Models;

namespace AddressBookApp.Data
{   //login for the database. uses entity framework
    public class ApplicationDbContext : IdentityDbContext<AppUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<AddressBookApp.Models.Category>? Category { get; set; }
        public DbSet<AddressBookApp.Models.Contact>? Contact { get; set; }
    }
}