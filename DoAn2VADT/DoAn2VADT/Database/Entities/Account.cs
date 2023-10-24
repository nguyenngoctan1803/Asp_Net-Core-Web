using DoAn2VADT.Database.Entities.Base;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Xml.Linq;

namespace DoAn2VADT.Database.Entities
{
    [Table("Account")]
    public class Account : BaseEntity
    {
        [Column(TypeName = "ntext")]
        [MaxLength(100)]
        [DisplayName("Tên")]
        public string Name { get; set; }
        [MaxLength(100)]
        [DisplayName("Email đăng nhập")]
        [EmailAddress]
        public string UserName { get; set; }
        [DisplayName("Mật khẩu")]
        [MaxLength(50)]
        public string Password { get; set; }
        [DisplayName("Vai trò")]
        [TempData]
        public string Role { get; set; }

        public ICollection<Order> Orders { get; set; }
        public ICollection<Import> Imports { get; set; }
    }
}
