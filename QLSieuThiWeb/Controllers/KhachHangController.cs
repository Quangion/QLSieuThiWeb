using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using QLSieuThiWeb.Models;
using System.Data;

namespace QLSieuThiWeb.Controllers
{
    public class KhachHangController : Controller
    {
        private readonly string _connectionString = "Server=QUANGION;Database=TT3;Trusted_Connection=True;Encrypt=False";

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult GetAllCustomers()
        {
            List<KhachHang> customers = new List<KhachHang>();
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("SELECT * FROM KhachHang", conn))
                    {
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var kh = new KhachHang
                                {
                                    maKH = reader["MaKH"]?.ToString(),
                                    tenKH = reader["TenKH"]?.ToString(),
                                    sdt = reader["SDT"]?.ToString(),
                                    diaChi = reader["DiaChi"]?.ToString()
                                };
                                customers.Add(kh);
                            }
                        }
                    }
                }
                return Json(customers);
            }
            catch (Exception ex)
            {
                return Json(new { error = "Lỗi khi tải dữ liệu: " + ex.Message });
            }
        }

        [HttpPost]
        public IActionResult AddCustomer([FromBody] KhachHang customer)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    // Kiểm tra số điện thoại đã tồn tại chưa
                    string checkQuery = "SELECT COUNT(*) FROM KhachHang WHERE SDT = @SDT";
                    using (SqlCommand checkCmd = new SqlCommand(checkQuery, conn))
                    {
                        checkCmd.Parameters.AddWithValue("@SDT", customer.sdt);
                        int count = (int)checkCmd.ExecuteScalar();
                        if (count > 0)
                        {
                            return Json(new { success = false, message = "Số điện thoại đã tồn tại!" });
                        }
                    }

                    string query = "INSERT INTO KhachHang (MaKH, TenKH, SDT, DiaChi) VALUES (@MaKH, @TenKH, @SDT, @DiaChi)";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@MaKH", customer.maKH);
                        cmd.Parameters.AddWithValue("@TenKH", customer.tenKH);
                        cmd.Parameters.AddWithValue("@SDT", customer.sdt);
                        cmd.Parameters.AddWithValue("@DiaChi", customer.diaChi ?? (object)DBNull.Value);
                        int result = cmd.ExecuteNonQuery();
                        return Json(new { success = result > 0, message = result > 0 ? "Thêm khách hàng thành công!" : "Không thể thêm khách hàng!" });
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        [HttpPost]
        public IActionResult UpdateCustomer([FromBody] KhachHang customer)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    // Kiểm tra nếu số điện thoại thay đổi, đảm bảo không trùng với khách hàng khác
                    string currentSdtQuery = "SELECT SDT FROM KhachHang WHERE MaKH = @MaKH";
                    using (SqlCommand currentCmd = new SqlCommand(currentSdtQuery, conn))
                    {
                        currentCmd.Parameters.AddWithValue("@MaKH", customer.maKH);
                        string currentSdt = currentCmd.ExecuteScalar()?.ToString();
                        if (currentSdt != customer.sdt)
                        {
                            string checkQuery = "SELECT COUNT(*) FROM KhachHang WHERE SDT = @SDT AND MaKH != @MaKH";
                            using (SqlCommand checkCmd = new SqlCommand(checkQuery, conn))
                            {
                                checkCmd.Parameters.AddWithValue("@SDT", customer.sdt);
                                checkCmd.Parameters.AddWithValue("@MaKH", customer.maKH);
                                int count = (int)checkCmd.ExecuteScalar();
                                if (count > 0)
                                {
                                    return Json(new { success = false, message = "Số điện thoại đã được sử dụng bởi khách hàng khác!" });
                                }
                            }
                        }
                    }

                    string query = "UPDATE KhachHang SET TenKH = @TenKH, SDT = @SDT, DiaChi = @DiaChi WHERE MaKH = @MaKH";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@MaKH", customer.maKH);
                        cmd.Parameters.AddWithValue("@TenKH", customer.tenKH);
                        cmd.Parameters.AddWithValue("@SDT", customer.sdt);
                        cmd.Parameters.AddWithValue("@DiaChi", customer.diaChi ?? (object)DBNull.Value);
                        int result = cmd.ExecuteNonQuery();
                        return Json(new { success = result > 0, message = result > 0 ? "Cập nhật thông tin thành công!" : "Không tìm thấy khách hàng!" });
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        [HttpPost]
        public IActionResult DeleteCustomer(string maKH)
        {
            Console.WriteLine(maKH);    
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    string query = "DELETE FROM KhachHang WHERE MaKH = @MaKH";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@MaKH", maKH);
                        int result = cmd.ExecuteNonQuery();
                        return Json(new { success = result > 0, message = result > 0 ? "Xóa khách hàng thành công!" : "Không tìm thấy khách hàng!" });
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        [HttpPost]
        public IActionResult GetHoaDonBySDT([FromBody] string sdt)
        {
            List<dynamic> hoaDons = new List<dynamic>();
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    string query = "SELECT MaHD, thoiGian, TongTien, TrangThai FROM HoaDon WHERE SDT = @SDT ORDER BY MaHD DESC";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@SDT", sdt);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var hoaDon = new
                                {
                                    MaHD = reader["MaHD"]?.ToString(),
                                    ThoiGian = reader["thoiGian"]?.ToString(),
                                    TongTien = reader["TongTien"]?.ToString(),
                                    TrangThai = reader["TrangThai"]?.ToString()
                                };
                                hoaDons.Add(hoaDon);
                            }
                        }
                    }
                }
                return Json(hoaDons);
            }
            catch (Exception ex)
            {
                return Json(new { error = "Lỗi khi tải lịch sử: " + ex.Message });
            }
        }
        

    }
}