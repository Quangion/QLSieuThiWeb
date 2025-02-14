using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;

namespace QLSieuThiWeb.Controllers
{
    public class BaoCaoController : Controller
    {
        private readonly string _connectionString;

        public BaoCaoController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult GetHangBanChay(DateTime? tuNgay, DateTime? denNgay)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    string query = @"
                        SELECT 
                            sp.maSP,
                                    sp.tenSP, 
                            SUM(ct.SLMua) as tongSoLuong,
                            SUM(ct.tongTienSP) as tongTien
                        FROM sanPham sp
                        JOIN chiTietHoaDon ct ON sp.maSP = ct.maSP
                        JOIN hoaDon hd ON ct.maHD = hd.maHD
                        WHERE (@tuNgay IS NULL OR hd.thoiGian >= @tuNgay)
                        AND (@denNgay IS NULL OR hd.thoiGian <= @denNgay)
                        GROUP BY sp.maSP, sp.tenSP
                        ORDER BY tongSoLuong DESC";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        var result = new List<dynamic>();
                        int stt = 1;
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                result.Add(new
                                {
                                    stt = stt++,
                                    maSP = reader["MaSP"].ToString(),
                                    tenSP = reader["tenSP"].ToString(),
                                    soLuongBan = Convert.ToInt32(reader["SoLuongBan"])
                                });
                            }
                        }
                        return Json(new { success = true, data = result });
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public IActionResult GetDoanhThuThang(int thang, int nam)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    string query = @"SELECT SUM(CAST(tongTien as int)) as DoanhThu 
                                   FROM hoaDon 
                                   WHERE MONTH(thoiGian) = @thang 
                                   AND YEAR(thoiGian) = @nam";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@thang", thang);
                        cmd.Parameters.AddWithValue("@nam", nam);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                var doanhThu = reader["DoanhThu"] == DBNull.Value ? 0 : Convert.ToInt32(reader["DoanhThu"]);
                                return Json(new { 
                                    success = true, 
                                    data = new { 
                                        doanhThu = doanhThu,
                                        thang = thang,
                                        nam = nam
                                    }
                                });
                            }
                        }
                    }
                    return Json(new { success = false, message = "Không có dữ liệu" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public IActionResult GetDoanhThuNam(int nam)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    string query = @"SELECT SUM(CAST(tongTien as int)) as DoanhThu 
                                   FROM hoaDon 
                                   WHERE YEAR(thoiGian) = @nam";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@nam", nam);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                var doanhThu = reader["DoanhThu"] == DBNull.Value ? 0 : Convert.ToInt32(reader["DoanhThu"]);
                                return Json(new { 
                                    success = true, 
                                    data = new { 
                                        doanhThu = doanhThu,
                                        nam = nam
                                    }
                                });
                            }
                        }
                    }
                    return Json(new { success = false, message = "Không có dữ liệu" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
} 