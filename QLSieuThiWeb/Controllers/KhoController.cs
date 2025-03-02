using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLSieuThiWeb.Data;
using QLSieuThiWeb.Models;
using Microsoft.Extensions.Logging;

namespace QLSieuThiWeb.Controllers
{
    public class KhoController : Controller
    {
        private readonly QLSieuThiWebContext _context;
        private readonly ILogger<KhoController> _logger;

        public KhoController(QLSieuThiWebContext context, ILogger<KhoController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public IActionResult Index()
        {
            ViewBag.Quyen = UserSession.Quyen;

            List<sanPham> model = new List<sanPham>();
            try
            {
                model = _context.sanPham
                    .OrderBy(sp => sp.maSP)
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError("Lỗi khi tải danh sách sản phẩm: " + ex.Message);
                TempData["ErrorMessage"] = "Lỗi khi tải dữ liệu: " + ex.Message;
            }
            return View(model);
        }

        [HttpGet]
        public IActionResult GetDanhSachSanPham()
        {
            try
            {
                var danhSachSP = _context.sanPham
                    .OrderBy(sp => sp.maSP)
                    .ToList();
                return Json(new { success = true, data = danhSachSP });
            }
            catch (Exception ex)
            {
                _logger.LogError("Lỗi khi tải danh sách sản phẩm: " + ex.Message);
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult UpdateStock([FromBody] StockUpdate data)
        {
            try
            {
                // Kiểm tra dữ liệu đầu vào
                if (data == null || string.IsNullOrEmpty(data.maSP) || string.IsNullOrEmpty(data.soLuong))
                {
                    return Json(new { success = false, message = "Dữ liệu gửi lên không hợp lệ!" });
                }

                // Chuyển đổi soLuong từ string sang int
                if (!int.TryParse(data.soLuong, out int soLuong) || soLuong < 0)
                {
                    return Json(new { success = false, message = "Số lượng không hợp lệ!" });
                }

                var sanPham = _context.sanPham.FirstOrDefault(sp => sp.maSP == data.maSP);
                if (sanPham == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy sản phẩm!" });
                }

                sanPham.soLuong = soLuong; // soLuong giờ là int?
                int result = _context.SaveChanges();

                return Json(new { success = result > 0, message = "Cập nhật kho thành công!" });
            }
            catch (Exception ex)
            {
                _logger.LogError("Lỗi khi cập nhật kho: " + ex.Message);
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        [HttpPost]
        public IActionResult ClearStock([FromBody] string maSP)
        {
            try
            {
                // Kiểm tra dữ liệu đầu vào
                if (string.IsNullOrEmpty(maSP))
                {
                    return Json(new { success = false, message = "Mã sản phẩm không hợp lệ!" });
                }

                var sanPham = _context.sanPham.FirstOrDefault(sp => sp.maSP == maSP);
                if (sanPham == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy sản phẩm!" });
                }

                sanPham.soLuong = 0; // soLuong giờ là int?
                int result = _context.SaveChanges();

                return Json(new { success = result > 0, message = "Đã xả kho thành công!" });
            }
            catch (Exception ex)
            {
                _logger.LogError("Lỗi khi xả kho: " + ex.Message);
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        [HttpPost]
        public IActionResult NhapKho([FromBody] NhapKhoModel model)
        {
            try
            {
                // Kiểm tra dữ liệu đầu vào
                if (model == null || string.IsNullOrEmpty(model.MaSP) || model.SoLuongNhap <= 0)
                {
                    return Json(new { success = false, message = "Dữ liệu gửi lên không hợp lệ!" });
                }

                var sanPham = _context.sanPham.FirstOrDefault(sp => sp.maSP == model.MaSP);
                if (sanPham == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy sản phẩm!" });
                }

                // soLuong là int?, kiểm tra null trước khi tính toán
                sanPham.soLuong = (sanPham.soLuong ?? 0) + model.SoLuongNhap;
                int result = _context.SaveChanges();

                return Json(new { success = result > 0, message = "Nhập kho thành công!" });
            }
            catch (Exception ex)
            {
                _logger.LogError("Lỗi khi nhập kho: " + ex.Message);
                return Json(new { success = false, message = "Lỗi nhập kho: " + ex.Message });
            }
        }

        [HttpPost]
        public IActionResult XuatKho([FromBody] XuatKhoModel model)
        {
            try
            {
                // Kiểm tra dữ liệu đầu vào
                if (model == null || string.IsNullOrEmpty(model.MaSP) || model.SoLuongXuat <= 0)
                {
                    return Json(new { success = false, message = "Dữ liệu gửi lên không hợp lệ!" });
                }

                var sanPham = _context.sanPham.FirstOrDefault(sp => sp.maSP == model.MaSP);
                if (sanPham == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy sản phẩm!" });
                }

                // soLuong là int?, kiểm tra null trước khi tính toán
                int tonKho = sanPham.soLuong ?? 0;
                if (tonKho < model.SoLuongXuat)
                {
                    return Json(new { success = false, message = "Số lượng xuất vượt quá số lượng tồn kho!" });
                }

                sanPham.soLuong = tonKho - model.SoLuongXuat;
                int result = _context.SaveChanges();

                return Json(new { success = result > 0, message = "Xuất kho thành công!" });
            }
            catch (Exception ex)
            {
                _logger.LogError("Lỗi khi xuất kho: " + ex.Message);
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