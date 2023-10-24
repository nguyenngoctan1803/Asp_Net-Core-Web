using DoAn2VADT.Database.Entities.Base;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using DoAn2VADT.Shared;

namespace DoAn2VADT.Database.Entities
{
    [Table("Order")]
    public class Order:BaseEntity
    {
        [DisplayName("Tên người nhận")]
        public string Name { get; set; }
        [DisplayName("Code")]
        public string Code { get; set; } = DateTime.Now.Ticks.ToString();

        [DisplayName("Giảm giá")]
        public decimal? Discount { get; set; } = 0;

        [DisplayName("Ngày giao")]
        public DateTime? ShipDate { get; set; }
        [DisplayName("Tiền ship")]
        public decimal? ShipFee { get; set; } = 30000;
        [DisplayName("Ngày nhận")]
        public DateTime? ReceiveDate { get; set; }

        [DisplayName("Số điện thoại")]
        public string Phone { get; set; }
        [Column(TypeName = "ntext")]
        [MaxLength(50)]
        [DisplayName("Địa chỉ")]
        public string Address { get; set; }

        [DisplayName("Tổng tiền")]
        public decimal? Total { get; set; } = 0;
        [DisplayName("Trạng thái")]
        public string Status { get; set; } = StatusConst.WAITCONFIRM;
        [DisplayName("Hình thức thanh toán")]
        public string PayWay { get; set; }
        [DisplayName("Trạng thái thanh toán")]
        public string PayStatus { get; set; }
        [DisplayName("Lý do hủy")]
        public string Reason { get; set; }
        // Người đặt hàng
        public string CustomerId { get; set; }
        [ForeignKey("CustomerId")]
        public Customer Customer { set; get; }
        // người xử lí đơn hàng
        [ForeignKey("UpdateUserId")]
        public Account Account { set; get; }

        public ICollection<OrderDetail> OrderDetails { get; set; }
    }
}
