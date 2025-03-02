namespace QLSieuThiWeb.Models
{
    public class HoaDon
    {
        public string MaHD { get; set; }  
        public DateTime ThoiGian { get; set; }
        public string SDT { get; set; }
        public string TrangThai { get; set; }
        public decimal TongTien { get; set; }
        public object ChiTietDonHang { get; internal set; }
    }
    public class ChiTietDonHang
    {
        public string MaHD { get; set; }
        public string MaSP { get; set; }
        public int SLMua { get; set; }
        public decimal TongTienSP { get; set; }

    }
} 