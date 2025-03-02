using System.ComponentModel.DataAnnotations;

namespace QLSieuThiWeb.Models
{
    public class sanPham
    {
        [Key]
        public string maSP { get; set; }
        public string? tenSP { get; set; }
        public decimal? gia { get; set; }
        public int? soLuong { get; set; }
    }
}
