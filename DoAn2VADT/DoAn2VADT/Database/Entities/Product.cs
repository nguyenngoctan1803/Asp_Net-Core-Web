using DoAn2VADT.Database.Entities.Base;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
namespace DoAn2VADT.Database.Entities
{
    [Table("Product")]
    public class Product : BaseEntity
    {
        [Required]
        [DisplayName("Tên sản phẩm")]
        public string Name { get; set; }

        [DisplayName("Mã")]
        [Required]
        public string Code { get; set; } = DateTime.Now.Ticks.ToString();
        [Column(TypeName = "ntext")]
        [DisplayName("Mô tả")]
        public string Description { get; set; }
        [DisplayName("Ảnh")]
        public string Image { get; set; }
        [DisplayName("Màu sắc")]
        public string Color { get; set; }
        [DisplayName("Bộ xử lý")]
        public string Main { get; set; }
        [DisplayName("Bộ nhớ")]
        public string Memory { get; set; }
        [DisplayName("Màn hình")]
        public string Screen { get; set; }
        [DisplayName("Ram")]
        public int Ram { get; set; } = 0;
        [Required]
        [DisplayName("Giá bán")]
        public decimal? Price { get; set; }
        [DisplayName("Giảm giá")]
        public decimal? Discount { get; set; } = 0;
        [Required]
        [DisplayName("Số lượng")]
        public int? Quantity { get; set; }
        [DisplayName("Hiệu lực")]
        public bool Effective { get; set; } = true;
        [DisplayName("Danh mục")]
        public string CategoryId { get; set; }
        [DisplayName("Thương hiệu")]
        public string BrandId { get; set; }
        [ForeignKey("BrandId")]
        public Brand Brand { get; set; }
        [ForeignKey("CategoryId")]
        public Category Category { get; set; }

        public ICollection<OrderDetail> OrderDetails { get; set; }
        public ICollection<ImportDetail> ImportDetails { get; set; }
        public ICollection<Cart> Carts { get; set; }
        public ICollection<Feedback> Feedbacks { get; set; }
    }
}
