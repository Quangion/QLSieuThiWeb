using Microsoft.EntityFrameworkCore;
using QLSieuThiWeb.Models;

namespace QLSieuThiWeb.Data
{
    public class QLSieuThiWebContext : DbContext
    {
        public QLSieuThiWebContext(DbContextOptions<QLSieuThiWebContext> options)
            : base(options)
        {
        }

        // DbSet đại diện cho bảng KhachHang trong cơ sở dữ liệu
        public DbSet<KhachHang> KhachHang { get; set; }

        // Nếu bạn có entity khác như TKMK, hãy đảm bảo đã định nghĩa lớp TKMK trong Models và khai báo DbSet tại đây:
        // public DbSet<TKMK> TKMKs { get; set; }
        public DbSet<sanPham> sanPham { get; set; }

        public DbSet<TKMK> TKMK { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer("Server=QUANGION;Database=TT3;Trusted_Connection=True;MultipleActiveResultSets=true;Encrypt=False");

        }

    }
}
