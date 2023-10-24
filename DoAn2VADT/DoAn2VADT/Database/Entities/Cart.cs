using DoAn2VADT.Database.Entities.Base;
using DoAn2VADT.Database.Entities;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel;

namespace DoAn2VADT.Database.Entities
{
    [Table("Cart")]
    public class Cart : BaseEntity
    {
        [DisplayName("Số lượng")]
        public int? Quantity { get; set; }

        [DisplayName("Sản phẩm")]
        public string ProductId { get; set; }
        // Sản phẩm
        [ForeignKey("ProductId")]
        public Product Product { set; get; }
        // Thuộc người?
        public string SessionId { get; set; }
    }
}