using BookHaven.Models;
using Microsoft.EntityFrameworkCore;

namespace BookHaven.DataAccess.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) //Using options to get connection string and pass it to base class of DbContext
        {



        }

        public DbSet<Category> Categories { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder) //to Add data in Category Table
        {
            modelBuilder.Entity<Category>().HasData(
                new Category {Id = 1,Name = "Action",DisplayOrder = 1},
                new Category {Id = 2,Name = "SciFi",DisplayOrder = 2},
                new Category {Id = 3,Name = "History",DisplayOrder = 3});
        }
}
}
