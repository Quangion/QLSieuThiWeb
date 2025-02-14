namespace QLSieuThiWeb.Models
{
    public class TaoHoaDonViewModel
    {
        public string SDT { get; set; }
        public List<ChiTietDonHangViewModel> ChiTietDonHang { get; set; } = new List<ChiTietDonHangViewModel>();
    }

    public class ChiTietDonHangViewModel
    {
        public string MaSP { get; set; }    
        public int SoLuong { get; set; }
        public decimal DonGia { get; set; }
    }
} 