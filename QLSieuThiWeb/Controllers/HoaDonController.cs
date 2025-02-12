using Microsoft.AspNetCore.Mvc;
using System;
using System.Data.SqlClient;
using System.Linq;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using QLSieuThiWeb.Models;

namespace QLSieuThiWeb.Controllers
{
    public class HoaDonController : Controller
    {
        private readonly string _connectionString;

        public HoaDonController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public IActionResult Index()
        {
            var products = new List<sanPham>();
            try
            {
                using (var conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    using (var cmd = new SqlCommand("SELECT * FROM sanPham", conn))
                    {
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                products.Add(new sanPham
                                {
                                    maSP = reader["maSP"].ToString(),
                                    tenSP = reader["tenSP"].ToString(),
                                    soLuong = reader["soLuong"].ToString(),
                                    gia = reader["gia"].ToString()
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }

            ViewBag.Products = products;
            return View();
        }

        [HttpGet]
        public JsonResult GetSanPhamByMa(string maSP)
        {
            try
            {
                using (var conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    using (var cmd = new SqlCommand("SELECT * FROM sanPham WHERE maSP = @maSP", conn))
                    {
                        cmd.Parameters.AddWithValue("@maSP", maSP);
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                return Json(new sanPham
                                {
                                    maSP = reader["maSP"].ToString(),
                                    tenSP = reader["tenSP"].ToString(),
                                    soLuong = reader["soLuong"].ToString(),
                                    gia = reader["gia"].ToString()
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            return Json(null);
        }

        [HttpGet]
        public IActionResult GetProducts()
        {
            try
            {
                List<object> products = new List<object>();
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    string query = "SELECT * FROM sanPham";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                products.Add(new
                                {
                                    maSP = reader["maSP"].ToString().Trim(),
                                    tenSP = reader["tenSP"].ToString().Trim(),
                                    donGia = Convert.ToDecimal(reader["donGia"]),
                                    soLuong = Convert.ToInt32(reader["soLuong"])
                                });
                            }
                        }
                    }
                }
                return Json(products);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetProducts: {ex.Message}");
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult ThanhToan([FromBody] ThanhToanRequest request)
        {
            try
            {
                if (request.GioHang == null || !request.GioHang.Any())
                {
                    return Json(new { success = false, message = "Giỏ hàng trống!" });
                }

                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    using (SqlTransaction transaction = conn.BeginTransaction())
                    {
                        try
                        {
                            // 1. Thêm hóa đơn
                            string queryHD = @"INSERT INTO hoaDon (thoiGian, sdt, trangThai, tongTien) 
                                            VALUES (GETDATE(), @sdt, @trangThai, @tongTien);
                                            SELECT CAST(SCOPE_IDENTITY() as int)";

                            int maHD;
                            using (SqlCommand cmd = new SqlCommand(queryHD, conn, transaction))
                            {
                                cmd.Parameters.AddWithValue("@sdt", request.SDT);
                                cmd.Parameters.AddWithValue("@trangThai", request.TrangThai);
                                cmd.Parameters.AddWithValue("@tongTien", request.TongTien);
                                
                                maHD = (int)cmd.ExecuteScalar();
                            }

                            // 2. Thêm chi tiết hóa đơn và cập nhật kho
                            foreach (var item in request.GioHang)
                            {
                                string queryCTHD = @"INSERT INTO chiTietHoaDon (maHD, maSP, soLuong, donGia) 
                                                   VALUES (@maHD, @maSP, @soLuong, @donGia)";
                                using (SqlCommand cmd = new SqlCommand(queryCTHD, conn, transaction))
                                {
                                    cmd.Parameters.AddWithValue("@maHD", maHD);
                                    cmd.Parameters.AddWithValue("@maSP", item.maSP);
                                    cmd.Parameters.AddWithValue("@soLuong", item.soLuong);
                                    cmd.Parameters.AddWithValue("@donGia", item.donGia);
                                    cmd.ExecuteNonQuery();
                                }

                                string queryKho = @"UPDATE sanPham 
                                                  SET soLuong = soLuong - @soLuong 
                                                  WHERE maSP = @maSP";
                                using (SqlCommand cmd = new SqlCommand(queryKho, conn, transaction))
                                {
                                    cmd.Parameters.AddWithValue("@soLuong", item.soLuong);
                                    cmd.Parameters.AddWithValue("@maSP", item.maSP);
                                    cmd.ExecuteNonQuery();
                                }
                            }

                            transaction.Commit();
                            return Json(new { success = true, message = "Thanh toán thành công!", maHD = maHD });
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            throw new Exception("Lỗi khi thanh toán: " + ex.Message);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public IActionResult GetProductByTen(string tenSP)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    string query = "SELECT * FROM sanPham WHERE tenSP = @tenSP";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@tenSP", tenSP);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                var product = new
                                {
                                    maSP = reader["maSP"].ToString().Trim(),
                                    tenSP = reader["tenSP"].ToString().Trim(),
                                    donGia = Convert.ToDecimal(reader["donGia"]),
                                    soLuong = Convert.ToInt32(reader["soLuong"])
                                };
                                return Json(new { success = true, product });
                            }
                        }
                    }
                }
                return Json(new { success = false, message = "Sản phẩm không tồn tại!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public IActionResult SearchProducts(string term)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    string query = @"SELECT * FROM sanPham 
                                   WHERE maSP LIKE @term + '%' 
                                   OR tenSP LIKE N'%' + @term + '%'";
                    
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@term", term);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            var products = new List<object>();
                            while (reader.Read())
                            {
                                products.Add(new
                                {
                                    maSP = reader["maSP"].ToString().Trim(),
                                    tenSP = reader["tenSP"].ToString().Trim(),
                                    donGia = Convert.ToDecimal(reader["donGia"]),
                                    soLuong = Convert.ToInt32(reader["soLuong"])
                                });
                            }
                            return Json(products);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new List<object>());
            }
        }
    }

    public class ThanhToanRequest
    {
        public string? SDT { get; set; }
        public string? TrangThai { get; set; }
        public string? TongTien { get; set; }
        public string? MaKH { get; set; }
        public List<GioHang>? GioHang { get; set; }
    }
} 
