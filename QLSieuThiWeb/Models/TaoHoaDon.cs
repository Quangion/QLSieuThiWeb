using System.Collections.Generic;

namespace QLSieuThiWeb.Models
{
    public class TaoHoaDon
    {
        public string SDT { get; set; }
        public string TrangThai { get; set; }
        public List<ChiTietHoaDon> ChiTietHoaDon { get; set; } = new List<ChiTietHoaDon>();
        public List<SanPhamViewModel> DanhSachSanPham { get; set; } = new List<SanPhamViewModel>();
    }

    public class ChiTietHoaDon
    {
        public string MaSP { get; set; }
        public int SLMua { get; set; }
        public decimal DonGia { get; set; }
        public decimal TongTienSP { get; set; }
        public SanPhamViewModel SanPham { get; set; }  // Thêm reference đến SanPhamViewModel
    }
} 