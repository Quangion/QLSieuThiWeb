public class HoaDon
{
    public int maHD { get; set; }
    public DateTime thoiGian { get; set; }
    public string? sdt { get; set; }
    public string? trangThai { get; set; }
    public string? tongTien { get; set; }
    public string? maKH { get; set; }
}

public class ChiTietHoaDon
{
    public int maHD { get; set; }
    public string? maSP { get; set; }
    public int soLuong { get; set; }
    public decimal donGia { get; set; }
}

public class GioHang
{
    public string? maSP { get; set; }
    public string? tenSP { get; set; }
    public decimal donGia { get; set; }
    public int soLuong { get; set; }
    public decimal thanhTien { get; set; }
} 