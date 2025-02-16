using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using QLSieuThiWeb.Models;

namespace QLSieuThiWeb.Controllers
{
    public class AccountController : Controller
    {
        private readonly string _connectionString;

        public AccountController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        // GET: Account/Login
        public IActionResult Login()
        {
            return View();
        }

        // POST: Account/Login
        [HttpPost]
        public IActionResult Login(string username, string password)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    string query = "SELECT * FROM TKMK WHERE TK = @username AND MK = @password";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@username", username);
                        cmd.Parameters.AddWithValue("@password", password);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                // Lưu thông tin vào Session
                                HttpContext.Session.SetString("Username", username);
                                HttpContext.Session.SetString("quyen", reader["quyen"].ToString());


                                return Json(new { success = true, redirectUrl = Url.Action("Index", "Home") });
                            }
                            else
                            {
                                return Json(new { success = false, message = "Tài khoản hoặc mật khẩu không đúng!" });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        // POST: Account/Logout
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}