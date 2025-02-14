using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using QLSieuThiWeb.Models;
using Microsoft.Data.SqlClient;
namespace QLSieuThiWeb.Controllers
{
    public class QuanLyHoaDonController : Controller
    {
        private readonly string _connectionString;

        public QuanLyHoaDonController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public IActionResult Index()
        {
            List<HoaDonViewModel> model = new List<HoaDonViewModel>();
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    string query = "SELECT maHD, thoiGian, sdt, trangThai, tongTien FROM HoaDon ORDER BY thoiGian DESC";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                model.Add(new HoaDonViewModel
                                {
                                    MaHD = reader["maHD"].ToString(),
                                    ThoiGian = Convert.ToDateTime(reader["thoiGian"]),
                                    SDT = reader["sdt"].ToString(),
                                    TrangThai = reader["trangThai"].ToString(),
                                    TongTien = Convert.ToDecimal(reader["tongTien"])
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Lỗi khi tải dữ liệu: " + ex.Message);
            }
            return View(model);
        }

        [HttpGet]
        public IActionResult ChiTietHoaDon(string maHD)
        {
            List<ChiTietHoaDonViewModel> chiTiet = new List<ChiTietHoaDonViewModel>();
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    string query = "SELECT maHD, maSP, SLMua, tongTienSP FROM ChiTietHoaDon WHERE maHD = @maHD";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@maHD", maHD);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                chiTiet.Add(new ChiTietHoaDonViewModel
                                {
                                    MaHD = reader["maHD"].ToString(),
                                    MaSP = reader["maSP"].ToString(),
                                    SLMua = Convert.ToInt32(reader["SLMua"]),
                                    TongTienSP = Convert.ToDecimal(reader["tongTienSP"])
                                });
                            }
                        }
                    }
                }
                return Json(new { success = true, data = chiTiet });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult CapNhatTrangThai(string maHD)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    string query = "UPDATE HoaDon SET trangThai = N'Đã giao' WHERE maHD = @maHD";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@maHD", maHD);
                        int result = cmd.ExecuteNonQuery();
                        if (result > 0)
                        {
                            return Json(new { success = true });
                        }
                    }
                }
                return Json(new { success = false, message = "Không tìm thấy hóa đơn" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
} 