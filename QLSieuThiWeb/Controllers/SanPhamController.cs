using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using QLSieuThiWeb.Models;
using System.Data;
using Microsoft.Extensions.Configuration;

namespace QLSieuThiWeb.Controllers
{
    public class SanPhamController : Controller
    {
        private readonly string _connectionString;

        public SanPhamController(IConfiguration configuration)
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

            return View(products);
        }

        [HttpPost]
        public JsonResult GetAllProducts()
        {
            List<sanPham> sanPhams = new List<sanPham>();
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("SELECT * FROM sanPham", conn))
                    {
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var sp = new sanPham();
                                sp.maSP = reader["maSP"]?.ToString();
                                sp.tenSP = reader["tenSP"]?.ToString();
                                sp.soLuong = reader["soLuong"]?.ToString() ?? "0";
                                sp.gia = reader["gia"]?.ToString() ?? "0";
                                sanPhams.Add(sp);
                            }
                        }
                    }
                }
                return Json(sanPhams);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return Json(new { error = "Lỗi khi tải dữ liệu" });
            }
        }

        [HttpPost]
        public IActionResult CheckMaSP([FromBody] string maSP)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    string query = "SELECT COUNT(*) FROM sanPham WHERE maSP = @maSP";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@maSP", maSP);
                        int count = (int)cmd.ExecuteScalar();
                        return Json(new { exists = count > 0 });
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult AddProduct([FromBody] sanPham sp)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    
                    // Kiểm tra mã sản phẩm đã tồn tại
                    string checkQuery = "SELECT COUNT(*) FROM sanPham WHERE maSP = @maSP";
                    using (SqlCommand checkCmd = new SqlCommand(checkQuery, conn))
                    {
                        checkCmd.Parameters.AddWithValue("@maSP", sp.maSP);
                        int count = (int)checkCmd.ExecuteScalar();
                        if (count > 0)
                        {
                            return Json(new { success = false, message = "Mã sản phẩm đã tồn tại!" });
                        }
                    }

                    // Nếu chưa tồn tại thì thêm mới
                    string insertQuery = @"INSERT INTO sanPham (maSP, tenSP, soLuong, gia) 
                                         VALUES (@maSP, @tenSP, @soLuong, @gia)";

                    using (SqlCommand cmd = new SqlCommand(insertQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@maSP", sp.maSP);
                        cmd.Parameters.AddWithValue("@tenSP", sp.tenSP ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@soLuong", sp.soLuong ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@gia", sp.gia ?? (object)DBNull.Value);

                        int result = cmd.ExecuteNonQuery();
                        if (result > 0)
                        {
                            return Json(new { success = true, message = "Thêm sản phẩm thành công!" });
                        }
                        else
                        {
                            return Json(new { success = false, message = "Không thể thêm sản phẩm!" });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi khi thêm sản phẩm: " + ex.Message });
            }
        }

        [HttpPost]
        public IActionResult UpdateProduct([FromBody] sanPham sp)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    string query = @"UPDATE sanPham 
                                   SET tenSP = @tenSP, 
                                       soLuong = @soLuong, 
                                       gia = @gia 
                                   WHERE maSP = @maSP";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@maSP", sp.maSP);
                        cmd.Parameters.AddWithValue("@tenSP", sp.tenSP ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@soLuong", sp.soLuong ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@gia", sp.gia ?? (object)DBNull.Value);

                        int result = cmd.ExecuteNonQuery();
                        if (result > 0)
                        {
                            return Json(new { success = true, message = "Cập nhật sản phẩm thành công!" });
                        }
                        else
                        {
                            return Json(new { success = false, message = "Không tìm thấy sản phẩm để cập nhật!" });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi khi cập nhật: " + ex.Message });
            }
        }

        [HttpPost]
        public IActionResult DeleteProduct([FromBody] string maSP)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    string query = "DELETE FROM sanPham WHERE maSP = @maSP";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@maSP", maSP);

                        int result = cmd.ExecuteNonQuery();
                        if (result > 0)
                        {
                            return Json(new { success = true, message = "Xóa sản phẩm thành công!" });
                        }
                        else
                        {
                            return Json(new { success = false, message = "Không tìm thấy sản phẩm để xóa!" });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi khi xóa sản phẩm: " + ex.Message });
            }
        }

        [HttpPost]
        public JsonResult ThemSanPham(string maSP, string tenSP, string soLuong, string gia)
        {
            try
            {
                using (var conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    
                    // Kiểm tra mã sản phẩm đã tồn tại chưa
                    using (var cmdCheck = new SqlCommand("SELECT COUNT(*) FROM sanPham WHERE maSP = @maSP", conn))
                    {
                        cmdCheck.Parameters.AddWithValue("@maSP", maSP);
                        int exists = (int)cmdCheck.ExecuteScalar();
                        if (exists > 0)
                        {
                            return Json(new { success = false, message = "Mã sản phẩm đã tồn tại!" });
                        }
                    }

                    // Thêm sản phẩm mới
                    string query = "INSERT INTO sanPham (maSP, tenSP, soLuong, gia) VALUES (@maSP, @tenSP, @soLuong, @gia)";
                    using (var cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@maSP", maSP);
                        cmd.Parameters.AddWithValue("@tenSP", tenSP);
                        cmd.Parameters.AddWithValue("@soLuong", soLuong);
                        cmd.Parameters.AddWithValue("@gia", gia);
                        cmd.ExecuteNonQuery();
                    }
                }
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public JsonResult XoaSanPham(string maSP)
        {
            try
            {
                using (var conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    string query = "DELETE FROM sanPham WHERE maSP = @maSP";
                    using (var cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@maSP", maSP);
                        cmd.ExecuteNonQuery();
                    }
                }
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public JsonResult GetSanPham(string maSP)
        {
            try
            {
                using (var conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    string query = "SELECT * FROM sanPham WHERE maSP = @maSP";
                    using (var cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@maSP", maSP);
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                return Json(new
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
                return Json(null);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public JsonResult CapNhatSanPham(sanPham sp)
        {
            try
            {
                using (var conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    string query = "UPDATE sanPham SET tenSP = @tenSP, soLuong = @soLuong, gia = @gia WHERE maSP = @maSP";
                    using (var cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@maSP", sp.maSP);
                        cmd.Parameters.AddWithValue("@tenSP", sp.tenSP);
                        cmd.Parameters.AddWithValue("@soLuong", sp.soLuong);
                        cmd.Parameters.AddWithValue("@gia", sp.gia);
                        cmd.ExecuteNonQuery();
                    }
                }
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
} 