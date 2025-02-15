using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

public class BaoCaoController : Controller
{
    private readonly string _connectionString = "Server=QUANGION;Database=TT3;Trusted_Connection=True;MultipleActiveResultSets=true;Encrypt=False";

    public async Task<IActionResult> Index()
    {
        var sanPhamBanChay = await GetSanPhamBanChay();
        var doanhSoHomNay = await GetDoanhSo("ngay", DateTime.Now);
        var doanhSoThangNay = await GetDoanhSo("thang", DateTime.Now);
        var doanhSoNamNay = await GetDoanhSo("nam", DateTime.Now);

        ViewBag.SanPhamBanChay = sanPhamBanChay;
        ViewBag.DoanhSoHomNay = doanhSoHomNay;
        ViewBag.DoanhSoThangNay = doanhSoThangNay;
        ViewBag.DoanhSoNamNay = doanhSoNamNay;

        return View();
    }

    private async Task<List<dynamic>> GetSanPhamBanChay()
    {
        List<dynamic> sanPhamBanChay = new List<dynamic>();

        string query = @"SELECT cthd.MaSP, sp.tenSP, SUM(CAST(cthd.SLMua AS int)) AS SoLuongBan
                         FROM chiTietHoaDon cthd
                         JOIN sanPham sp ON cthd.MaSP = sp.MaSP
                         GROUP BY cthd.MaSP, sp.tenSP
                         ORDER BY SoLuongBan DESC;";

        using (SqlConnection conn = new SqlConnection(_connectionString))
        {
            await conn.OpenAsync();
            using (SqlCommand cmd = new SqlCommand(query, conn))
            using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    sanPhamBanChay.Add(new
                    {
                        MaSP = reader["MaSP"],
                        TenSP = reader["tenSP"].ToString(),
                        SoLuongBan = reader["SoLuongBan"]
                    });
                }
            }
        }
        return sanPhamBanChay;
    }

    private async Task<int> GetDoanhSo(string type, DateTime date)
    {
        string query = "";

        if (type == "ngay")
        {
            query = "SELECT SUM(CAST(TongTien AS int)) FROM hoaDon WHERE CAST(thoiGian AS DATE) = @date";
        }
        else if (type == "thang")
        {
            query = "SELECT SUM(CAST(TongTien AS int)) FROM hoaDon WHERE MONTH(thoiGian) = MONTH(@date) AND YEAR(thoiGian) = YEAR(@date)";
        }
        else if (type == "nam")
        {
            query = "SELECT SUM(CAST(TongTien AS int)) FROM hoaDon WHERE YEAR(thoiGian) = YEAR(@date)";
        }

        using (SqlConnection conn = new SqlConnection(_connectionString))
        {
            await conn.OpenAsync();
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@date", date);
                var result = await cmd.ExecuteScalarAsync();
                return result != DBNull.Value ? Convert.ToInt32(result) : 0;
            }
        }
    }
}
