using DoAn2VADT.Database.Entities.Base;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using DocumentFormat.OpenXml.Drawing.Charts;

namespace DoAn2VADT.Database.Entities
{
    [Table("OrderDetail")]
    public class OrderDetail : BaseEntity
    {

        [DisplayName("Số lượng")]
        public int? Quantity { get; set; }


        [DisplayName("Tổng tiền")]
        public decimal? Total { get; set; } = 0;


        [DisplayName("Sản phẩm")]
        public string ProductId { get; set; }

        // Sản phẩm
        [ForeignKey("ProductId")]
        public Product Product { set; get; }

        [DisplayName("Đơn hàng")]
        public string OrderId { get; set; }

        // Đơn hàng
        [ForeignKey("OrderId")]
        public Order Order { set; get; }
    }
}
