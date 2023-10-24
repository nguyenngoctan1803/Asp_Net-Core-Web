using DoAn2VADT.Database.Entities.Base;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace DoAn2VADT.Database.Entities
{
    [Table("Customer")]
    public class Customer : BaseEntity
    {
        [Column(TypeName = "ntext")]
        [MaxLength(100)]
        [DisplayName("Tên khách hàng")]
        public string Name { get; set; }
        [DisplayName("Email")]
        [MaxLength(100)]
        public string Email { get; set; }
        [DisplayName("Số điện thoại")]
        [MaxLength(20)]
        public string Phone { get; set; }
        [Column(TypeName = "ntext")]
        [DisplayName("Địa chỉ")]
        public string Address { get; set; }
        [MaxLength(50)]
        [DisplayName("Mật khẩu")]
        public string Password { get; set; }
        public ICollection<Order> Orders { get; set; }
    }
}
