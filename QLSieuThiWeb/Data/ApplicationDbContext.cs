using Microsoft.EntityFrameworkCore;
using QLSieuThiWeb.Models;

namespace QLSieuThiWeb.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<TKMK> TKMK { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TKMK>()
                .ToTable("TKMK");

            base.OnModelCreating(modelBuilder);
        }
    }
} 