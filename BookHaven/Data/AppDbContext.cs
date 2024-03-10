using BookHaven.Models;
using Microsoft.EntityFrameworkCore;

namespace BookHaven.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options):base(options) //Using options to get connection string and pass it to base class of DbContext
        { 
       
        
        
        }

        public DbSet<Category> Categories { get; set; }
    }
}
