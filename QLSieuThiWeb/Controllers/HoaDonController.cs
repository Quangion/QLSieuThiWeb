using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
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
            TaoHoaDon model = new TaoHoaDon();
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    string query = "SELECT maSP, tenSP, gia, soLuong FROM SanPham WHERE soLuong > 0";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                model.DanhSachSanPham.Add(new SanPhamViewModel
                                {
                                    MaSP = reader["maSP"].ToString(),
                                    TenSP = reader["tenSP"].ToString(),
                                    Gia = Convert.ToDecimal(reader["gia"]),
                                    SoLuong = Convert.ToInt32(reader["soLuong"])
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

        [HttpPost]
        public IActionResult LuuHoaDon([FromBody] TaoHoaDon model)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    using (SqlTransaction transaction = conn.BeginTransaction())
                    {
                        try
                        {
                            decimal tongTien = model.ChiTietHoaDon.Sum(x => x.TongTienSP);

                            string queryHD = @"INSERT INTO HoaDon (thoiGian, sdt, trangThai, tongTien) 
                                             VALUES (GETDATE(), @sdt, @trangThai, @tongTien);
                                             SELECT SCOPE_IDENTITY();";

                            int maHD;
                            using (SqlCommand cmd = new SqlCommand(queryHD, conn, transaction))
                            {
                                cmd.Parameters.AddWithValue("@sdt", (object)model.SDT ?? DBNull.Value);
                                cmd.Parameters.AddWithValue("@trangThai", model.TrangThai);
                                cmd.Parameters.AddWithValue("@tongTien", tongTien);
                                maHD = Convert.ToInt32(cmd.ExecuteScalar());
                            }

                            // Thêm chi tiết hóa đơn
                            foreach (var item in model.ChiTietHoaDon)
                            {
                                string queryCT = @"INSERT INTO ChiTietHoaDon (maHD, maSP, SLMua, tongTienSP) 
                                             VALUES (@maHD, @maSP, @slMua, @tongTienSP)";

                                using (SqlCommand cmd = new SqlCommand(queryCT, conn, transaction))
                                {
                                    cmd.Parameters.AddWithValue("@maHD", maHD);
                                    cmd.Parameters.AddWithValue("@maSP", item.MaSP);
                                    cmd.Parameters.AddWithValue("@slMua", item.SLMua);
                                    cmd.Parameters.AddWithValue("@tongTienSP", item.TongTienSP);
                                    cmd.ExecuteNonQuery();
                                }

                                // Cập nhật số lượng sản phẩm
                                string queryUpdateSP = "UPDATE SanPham SET soLuong = soLuong - @slMua WHERE maSP = @maSP";
                                using (SqlCommand cmd = new SqlCommand(queryUpdateSP, conn, transaction))
                                {
                                    cmd.Parameters.AddWithValue("@slMua", item.SLMua);
                                    cmd.Parameters.AddWithValue("@maSP", item.MaSP);
                                    cmd.ExecuteNonQuery();
                                }
                            }

                            transaction.Commit();
                            return Json(new { success = true, message = "Tạo hóa đơn thành công!" });
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            throw ex;
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
} 