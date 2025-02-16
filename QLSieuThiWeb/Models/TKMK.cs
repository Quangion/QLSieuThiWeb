using System.ComponentModel.DataAnnotations;

namespace QLSieuThiWeb.Models
{
    public class TKMK
    {
        [Key]
        [Required(ErrorMessage = "Tài khoản không được để trống")]
        [Display(Name = "Tài khoản")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Tài khoản phải từ 3-50 ký tự")]
        public string TK { get; set; }

        [Required(ErrorMessage = "Mật khẩu không được để trống")]
        [Display(Name = "Mật khẩu")]
        [DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Mật khẩu phải từ 3-100 ký tự")]
        public string MK { get; set; }
        public string quyen { get; set; }
    }
}
