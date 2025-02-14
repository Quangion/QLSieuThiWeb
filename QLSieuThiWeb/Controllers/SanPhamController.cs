using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using QLSieuThiWeb.Models;
using System.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace QLSieuThiWeb.Controllers
{
    public class SanPhamController : Controller
    {
        private readonly string _connectionString;
        private readonly ILogger<SanPhamController> _logger;

        public SanPhamController(IConfiguration configuration, ILogger<SanPhamController> logger)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
            _logger = logger;
        }

        public IActionResult Index()
        {
            List<sanPham> danhSachSP = new List<sanPham>();
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
                            sanPham sp = new sanPham
                            {
                                maSP = reader["maSP"].ToString(),
                                tenSP = reader["tenSP"].ToString(),
                                soLuong = Convert.ToInt32(reader["soLuong"]),
                                gia = Convert.ToDecimal(reader["gia"])
                            };
                            danhSachSP.Add(sp);
                        }
                    }
                }
            }
            return View(danhSachSP);
        }

        [HttpGet]
        public IActionResult GetSanPham(string maSP)
        {
            _logger.LogInformation($"Lấy thông tin sản phẩm: {maSP}");

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                string query = "SELECT * FROM sanPham WHERE maSP = @maSP";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@maSP", maSP);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            var sp = new sanPham
                            {
                                maSP = reader["maSP"].ToString(),
                                tenSP = reader["tenSP"].ToString(),
                                soLuong = Convert.ToInt32(reader["soLuong"]),
                                gia = Convert.ToDecimal(reader["gia"])
                            };
                            return Json(sp);
                        }
                        else
                        {
                            _logger.LogWarning($"Không tìm thấy sản phẩm: {maSP}");
                            return Json(null);
                        }
                    }
                }
            }
        }

        [HttpPost]
        public IActionResult ThemSanPham([FromBody] sanPham sp)
        {
            try
            {
                // Kiểm tra dữ liệu đầu vào
                if (string.IsNullOrEmpty(sp.maSP) || string.IsNullOrEmpty(sp.tenSP))
                {
                    return Json(new { success = false, message = "Mã sản phẩm và tên sản phẩm không được để trống!" });
                }

                // Kiểm tra mã sản phẩm đã tồn tại chưa
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    conn.Open();

                    // Kiểm tra trùng mã
                    string checkQuery = "SELECT COUNT(*) FROM sanPham WHERE maSP = @maSP";
                    using (SqlCommand checkCmd = new SqlCommand(checkQuery, conn))
                    {
                        checkCmd.Parameters.AddWithValue("@maSP", sp.maSP);
                        int exists = (int)checkCmd.ExecuteScalar();
                        if (exists > 0)
                        {
                            return Json(new { success = false, message = "Mã sản phẩm đã tồn tại!" });
                        }
                    }

                    // Thêm sản phẩm mới
                    string insertQuery = "INSERT INTO sanPham (maSP, tenSP, soLuong, gia) VALUES (@maSP, @tenSP, @soLuong, @gia)";
                    using (SqlCommand cmd = new SqlCommand(insertQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@maSP", sp.maSP);
                        cmd.Parameters.AddWithValue("@tenSP", sp.tenSP);
                        cmd.Parameters.AddWithValue("@soLuong", sp.soLuong);
                        cmd.Parameters.AddWithValue("@gia", sp.gia);

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
            catch (SqlException ex)
            {
                // Xử lý lỗi SQL cụ thể
                string errorMessage = "Lỗi khi thêm sản phẩm: ";
                switch (ex.Number)
                {
                    case 2627:  // Unique constraint error
                        errorMessage += "Mã sản phẩm đã tồn tại!";
                        break;
                    case 547:   // Constraint violation
                        errorMessage += "Dữ liệu không hợp lệ!";
                        break;
                    default:
                        errorMessage += ex.Message;
                        break;
                }
                return Json(new { success = false, message = errorMessage });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi khi thêm sản phẩm: " + ex.Message });
            }
        }

        [HttpPost]
        public IActionResult CapNhatSanPham([FromBody] sanPham sp)
        {
            _logger.LogInformation($"Bắt đầu cập nhật sản phẩm: {sp.maSP}");

            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    string query = "UPDATE sanPham SET tenSP = @tenSP, soLuong = @soLuong, gia = @gia WHERE maSP = @maSP";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
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
                _logger.LogError($"Lỗi khi cập nhật sản phẩm: {ex.Message}");
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult XoaSanPham(string maSP)
        {
            try
            {
                // Kiểm tra mã sản phẩm có tồn tại
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    conn.Open();

                    // Kiểm tra sản phẩm tồn tại
                    string checkQuery = "SELECT COUNT(*) FROM sanPham WHERE maSP = @maSP";
                    using (SqlCommand checkCmd = new SqlCommand(checkQuery, conn))
                    {
                        checkCmd.Parameters.AddWithValue("@maSP", maSP);
                        int exists = (int)checkCmd.ExecuteScalar();
                        if (exists == 0)
                        {
                            return Json(new { success = false, message = "Không tìm thấy sản phẩm để xóa!" });
                        }
                    }

                    // Thực hiện xóa sản phẩm
                    string deleteQuery = "DELETE FROM sanPham WHERE maSP = @maSP";
                    using (SqlCommand cmd = new SqlCommand(deleteQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@maSP", maSP);
                        int result = cmd.ExecuteNonQuery();
                        if (result > 0)
                        {
                            return Json(new { success = true, message = "Xóa sản phẩm thành công!" });
                        }
                        else
                        {
                            return Json(new { success = false, message = "Không thể xóa sản phẩm!" });
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                // Xử lý lỗi SQL cụ thể
                string errorMessage = "Lỗi khi xóa sản phẩm: ";
                switch (ex.Number)
                {
                    case 547:   // Foreign key violation
                        errorMessage += "Không thể xóa vì sản phẩm đang được sử dụng!";
                        break;
                    default:
                        errorMessage += ex.Message;
                        break;
                }
                return Json(new { success = false, message = errorMessage });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi khi xóa sản phẩm: " + ex.Message });
            }
        }
    }
} 