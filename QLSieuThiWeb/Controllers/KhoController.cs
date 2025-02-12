using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using QLSieuThiWeb.Models;
using Microsoft.Extensions.Configuration;

namespace QLSieuThiWeb.Controllers
{
    public class KhoController : Controller
    {
        private readonly string _connectionString;

        public KhoController(IConfiguration configuration)
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
        public IActionResult UpdateStock([FromBody] StockUpdate data)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    string query = "UPDATE sanPham SET soLuong = @soLuong WHERE maSP = @maSP";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@maSP", data.maSP);
                        cmd.Parameters.AddWithValue("@soLuong", data.soLuong);

                        int result = cmd.ExecuteNonQuery();
                        if (result > 0)
                        {
                            return Json(new { success = true, message = "Cập nhật kho thành công!" });
                        }
                        else
                        {
                            return Json(new { success = false, message = "Không tìm thấy sản phẩm!" });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        [HttpPost]
        public IActionResult ClearStock([FromBody] string maSP)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    string query = "UPDATE sanPham SET soLuong = 0 WHERE maSP = @maSP";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@maSP", maSP);

                        int result = cmd.ExecuteNonQuery();
                        if (result > 0)
                        {
                            return Json(new { success = true, message = "Đã xả kho thành công!" });
                        }
                        else
                        {
                            return Json(new { success = false, message = "Không tìm thấy sản phẩm!" });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }
    }

    public class StockUpdate
    {
        public string maSP { get; set; }
        public string soLuong { get; set; }
    }
} 