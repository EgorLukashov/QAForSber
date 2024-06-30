using QAForSber.Models;
using Microsoft.EntityFrameworkCore;

namespace QAForSber.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
            
        }
        public DbSet<Admin> Admin { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //Specify that IdAdmin is id (Primary Key)
            modelBuilder.Entity<Admin>().HasKey(p => p.IDAdmin);
        }
    }
}
