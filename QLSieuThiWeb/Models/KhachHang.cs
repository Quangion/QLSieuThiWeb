using System.ComponentModel.DataAnnotations;

namespace QLSieuThiWeb.Models
{
    public class KhachHang
    {
        [Key]
        public string maKH { get; set; }
        public string? tenKH { get; set; }
        public string? sdt { get; set; }
        public string? diaChi { get; set; }
    }
} 