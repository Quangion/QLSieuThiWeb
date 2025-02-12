using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using QLSieuThiWeb.Models;
using System.Data;

namespace QLSieuThiWeb.Controllers
{
    public class KhachHangController : Controller
    {
        private readonly string _connectionString = "Server=QUANGION;Database=TT3;Trusted_Connection=True;MultipleActiveResultSets=true;Encrypt=False";

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
                                var kh = new KhachHang();
                                kh.maKH = reader["MaKH"]?.ToString();
                                kh.tenKH = reader["TenKH"]?.ToString();
                                kh.sdt = reader["SDT"]?.ToString();
                                kh.diaChi = reader["DiaChi"]?.ToString();
                                customers.Add(kh);
                            }
                        }
                    }
                }
                return Json(customers);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult CheckMaKH([FromBody] string maKH)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    string query = "SELECT COUNT(*) FROM KhachHang WHERE MaKH = @MaKH";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@MaKH", maKH);
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
        public IActionResult AddCustomer([FromBody] KhachHang customer)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    string query = "INSERT INTO KhachHang (MaKH, TenKH, SDT, DiaChi) VALUES (@MaKH, @TenKH, @SDT, @DiaChi)";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@MaKH", customer.maKH);
                        cmd.Parameters.AddWithValue("@TenKH", customer.tenKH);
                        cmd.Parameters.AddWithValue("@SDT", customer.sdt);
                        cmd.Parameters.AddWithValue("@DiaChi", (object)customer.diaChi ?? DBNull.Value);

                        int result = cmd.ExecuteNonQuery();
                        if (result > 0)
                        {
                            return Json(new { success = true, message = "Thêm khách hàng thành công!" });
                        }
                        else
                        {
                            return Json(new { success = false, message = "Không thể thêm khách hàng!" });
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
        public IActionResult UpdateCustomer([FromBody] KhachHang customer)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    string query = "UPDATE KhachHang SET TenKH = @TenKH, SDT = @SDT, DiaChi = @DiaChi WHERE MaKH = @MaKH";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@MaKH", customer.maKH);
                        cmd.Parameters.AddWithValue("@TenKH", customer.tenKH);
                        cmd.Parameters.AddWithValue("@SDT", customer.sdt);
                        cmd.Parameters.AddWithValue("@DiaChi", (object)customer.diaChi ?? DBNull.Value);

                        int result = cmd.ExecuteNonQuery();
                        if (result > 0)
                        {
                            return Json(new { success = true, message = "Cập nhật thông tin thành công!" });
                        }
                        else
                        {
                            return Json(new { success = false, message = "Không tìm thấy khách hàng!" });
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
        public IActionResult DeleteCustomer([FromBody] string maKH)
        {
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
                        if (result > 0)
                        {
                            return Json(new { success = true, message = "Xóa khách hàng thành công!" });
                        }
                        else
                        {
                            return Json(new { success = false, message = "Không tìm thấy khách hàng!" });
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
        public IActionResult SearchByPhone([FromBody] string phoneNumber)
        {
            List<KhachHang> khachHangs = new List<KhachHang>();
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    string query = "SELECT * FROM KhachHang WHERE SDT LIKE @SDT";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@SDT", "%" + phoneNumber + "%");
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var kh = new KhachHang();
                                if (reader["MaKH"] != DBNull.Value) kh.maKH = reader["MaKH"].ToString();
                                if (reader["TenKH"] != DBNull.Value) kh.tenKH = reader["TenKH"].ToString();
                                if (reader["SDT"] != DBNull.Value) kh.sdt = reader["SDT"].ToString();
                                if (reader["DiaChi"] != DBNull.Value) kh.diaChi = reader["DiaChi"].ToString();
                                khachHangs.Add(kh);
                            }
                        }
                    }
                }
                return Json(khachHangs);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }
    }
} 