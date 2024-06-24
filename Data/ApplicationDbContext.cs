using QAForSber.Models;
using Microsoft.EntityFrameworkCore;

namespace QAForSber.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
            
        }
        //public DbSet<Flowers> flowers_available { get; set; }
        public DbSet<Admin> Admin { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Указываем, что свойство IDPark является первичным ключом
            modelBuilder.Entity<Admin>().HasKey(p => p.IDAdmin);
        }
    }
}
