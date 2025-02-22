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
                ViewBag.Quyen = UserSession.Quyen;

                // Thực hiện các logic cần thiết khác
            
                List<sanPham> model = new List<sanPham>();
                try
                {
                    using (SqlConnection conn = new SqlConnection(_connectionString))
                    {
                        conn.Open();
                        string query = "SELECT * FROM sanPham ORDER BY maSP";
                        using (SqlCommand cmd = new SqlCommand(query, conn))
                        {
                            using (SqlDataReader reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    model.Add(new sanPham
                                    {
                                        maSP = reader["maSP"].ToString(),
                                        tenSP = reader["tenSP"].ToString(),
                                        soLuong = Convert.ToInt32(reader["soLuong"]),
                                        gia = Convert.ToDecimal(reader["gia"])
                                    });
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = "Lỗi khi tải dữ liệu: " + ex.Message;
                }
                return View(model);
            }

            [HttpGet]
            public IActionResult GetDanhSachSanPham()
            {
                try
                {
                    List<sanPham> danhSachSP = new List<sanPham>();
                    using (SqlConnection conn = new SqlConnection(_connectionString))
                    {
                        conn.Open();
                        string query = "SELECT * FROM sanPham ORDER BY maSP";
                        using (SqlCommand cmd = new SqlCommand(query, conn))
                        {
                            using (SqlDataReader reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    danhSachSP.Add(new sanPham
                                    {
                                        maSP = reader["maSP"].ToString(),
                                        tenSP = reader["tenSP"].ToString(),
                                        soLuong = Convert.ToInt32(reader["soLuong"]),
                                        gia = Convert.ToDecimal(reader["gia"])
                                    });
                                }
                            }
                        }
                    }
                    return Json(new { success = true, data = danhSachSP });
                }
                catch (Exception ex)
                {
                    return Json(new { success = false, message = ex.Message });
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

            [HttpPost]
            public IActionResult NhapKho([FromBody] NhapKhoModel model)
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
                                string updateQuery = "UPDATE sanPham SET soLuong = soLuong + @soLuongNhap WHERE maSP = @maSP";
                                using (SqlCommand cmd = new SqlCommand(updateQuery, conn, transaction))
                                {
                                    cmd.Parameters.AddWithValue("@maSP", model.MaSP);
                                    cmd.Parameters.AddWithValue("@soLuongNhap", model.SoLuongNhap);
                                    int rowsAffected = cmd.ExecuteNonQuery();

                                    if (rowsAffected == 0)
                                    {
                                        throw new Exception("Không tìm thấy sản phẩm!");
                                    }
                                }
                                transaction.Commit();
                                return Json(new { success = true, message = "Nhập kho thành công!" });
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
                    return Json(new { success = false, message = "Lỗi nhập kho: " + ex.Message });
                }
            }

            [HttpPost]
            public IActionResult XuatKho([FromBody] XuatKhoModel model)
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
                                // Kiểm tra số lượng tồn kho
                                string checkQuery = "SELECT soLuong FROM sanPham WHERE maSP = @maSP";
                                int tonKho;
                                using (SqlCommand checkCmd = new SqlCommand(checkQuery, conn, transaction))
                                {
                                    checkCmd.Parameters.AddWithValue("@maSP", model.MaSP);
                                    tonKho = Convert.ToInt32(checkCmd.ExecuteScalar());
                                }

                                if (tonKho < model.SoLuongXuat)
                                {
                                    throw new Exception("Số lượng xuất vượt quá số lượng tồn kho!");
                                }

                                // Cập nhật số lượng
                                string updateQuery = "UPDATE sanPham SET soLuong = soLuong - @soLuongXuat WHERE maSP = @maSP";
                                using (SqlCommand cmd = new SqlCommand(updateQuery, conn, transaction))
                                {
                                    cmd.Parameters.AddWithValue("@maSP", model.MaSP);
                                    cmd.Parameters.AddWithValue("@soLuongXuat", model.SoLuongXuat);
                                    int rowsAffected = cmd.ExecuteNonQuery();

                                    if (rowsAffected == 0)
                                    {
                                        throw new Exception("Không tìm thấy sản phẩm!");
                                    }
                                }
                                transaction.Commit();
                                return Json(new { success = true, message = "Xuất kho thành công!" });
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
                    return Json(new { success = false, message = "Lỗi xuất kho: " + ex.Message });
                }
            }
        }

        public class StockUpdate
        {
            public string maSP { get; set; }
            public string soLuong { get; set; }
        }

        public class NhapKhoModel
        {
            public string MaSP { get; set; }
            public int SoLuongNhap { get; set; }
        }

        public class XuatKhoModel
        {
            public string MaSP { get; set; }
            public int SoLuongXuat { get; set; }
        }
    } 