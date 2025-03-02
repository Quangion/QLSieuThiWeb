using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLSieuThiWeb.Data;
using QLSieuThiWeb.Models;
using Microsoft.Extensions.Logging;

namespace QLSieuThiWeb.Controllers
{
    public class SanPhamController : Controller
    {
        private readonly QLSieuThiWebContext _context;
        private readonly ILogger<SanPhamController> _logger;

        public SanPhamController(QLSieuThiWebContext context, ILogger<SanPhamController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // Hiển thị danh sách sản phẩm trên View
        public IActionResult Index()
        {
            try
            {
                var danhSachSP = _context.sanPham.ToList();
                return View(danhSachSP);
            }
            catch (Exception ex)
            {
                _logger.LogError("Lỗi khi tải danh sách sản phẩm: " + ex.Message);
                return View(new List<sanPham>());
            }
        }

        // Lấy thông tin sản phẩm theo maSP
        [HttpGet]
        public IActionResult GetSanPham(string maSP)
        {
            _logger.LogInformation($"Lấy thông tin sản phẩm: {maSP}");
            try
            {
                var sp = _context.sanPham.FirstOrDefault(s => s.maSP == maSP);
                if (sp == null)
                {
                    _logger.LogWarning($"Không tìm thấy sản phẩm: {maSP}");
                    return Json(null);
                }
                return Json(sp);
            }
            catch (Exception ex)
            {
                _logger.LogError("Lỗi khi lấy thông tin sản phẩm: " + ex.Message);
                return Json(new { error = "Lỗi khi lấy thông tin sản phẩm" });
            }
        }

        // Thêm sản phẩm mới
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

                // Kiểm tra xem mã sản phẩm đã tồn tại chưa
                bool exists = _context.sanPham.Any(s => s.maSP == sp.maSP);
                if (exists)
                {
                    return Json(new { success = false, message = "Mã sản phẩm đã tồn tại!" });
                }

                _context.sanPham.Add(sp);
                int result = _context.SaveChanges();
                return Json(new { success = result > 0, message = result > 0 ? "Thêm sản phẩm thành công!" : "Không thể thêm sản phẩm!" });
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError("Lỗi khi thêm sản phẩm: " + dbEx.Message);
                return Json(new { success = false, message = "Lỗi khi thêm sản phẩm: " + dbEx.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError("Lỗi khi thêm sản phẩm: " + ex.Message);
                return Json(new { success = false, message = "Lỗi khi thêm sản phẩm: " + ex.Message });
            }
        }

        // Cập nhật sản phẩm
        public IActionResult CapNhatSanPham([FromBody] sanPham sp)
        {
            if (sp == null || string.IsNullOrEmpty(sp.maSP))
            {
                return Json(new { success = false, message = "Dữ liệu gửi lên không hợp lệ!" });
            }

            _logger.LogInformation($"Bắt đầu cập nhật sản phẩm: {sp.maSP}");
            try
            {
                if (_context == null || _context.sanPham == null)
                {
                    _logger.LogError("Database context hoặc sanPham DbSet là null.");
                    return Json(new { success = false, message = "Lỗi hệ thống: Không thể kết nối đến cơ sở dữ liệu." });
                }

                var existingSP = _context.sanPham.FirstOrDefault(s => s.maSP == sp.maSP);
                if (existingSP == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy sản phẩm!" });
                }

                existingSP.tenSP = sp.tenSP;
                existingSP.soLuong = sp.soLuong;
                existingSP.gia = sp.gia;

                int result = _context.SaveChanges();
                return Json(new { success = result > 0, message = "Cập nhật sản phẩm thành công!" });
            }
            catch (Exception ex)
            {
                _logger.LogError("Lỗi khi cập nhật sản phẩm: " + ex.Message);
                return Json(new { success = false, message = "Lỗi khi cập nhật sản phẩm: " + ex.Message });
            }
        }
        // Xóa sản phẩm
        [HttpPost]
        public IActionResult XoaSanPham(string maSP)
        {
            try
            {
                var sp = _context.sanPham.FirstOrDefault(s => s.maSP == maSP);
                if (sp == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy sản phẩm để xóa!" });
                }

                _context.sanPham.Remove(sp);
                int result = _context.SaveChanges();
                return Json(new { success = result > 0, message = result > 0 ? "Xóa sản phẩm thành công!" : "Không thể xóa sản phẩm!" });
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError("Lỗi khi xóa sản phẩm: " + dbEx.Message);
                return Json(new { success = false, message = "Lỗi khi xóa sản phẩm: " + dbEx.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError("Lỗi khi xóa sản phẩm: " + ex.Message);
                return Json(new { success = false, message = "Lỗi khi xóa sản phẩm: " + ex.Message });
            }
        }
    }
}
