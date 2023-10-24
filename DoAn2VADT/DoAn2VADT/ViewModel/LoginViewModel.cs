using System;
using System.ComponentModel.DataAnnotations;

namespace DoAn2VADT.ViewModel
{
    public class LoginViewModel
    {
        [Key]
        [MaxLength(100)]
        public string UserName { get; set; }

        [Display(Name = "Mật khẩu")]
        [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
        [MinLength(5, ErrorMessage = "Bạn cần đặt mật khẩu tối thiểu 5 ký tự")]
        public string Password { get; set; }
    }
}
