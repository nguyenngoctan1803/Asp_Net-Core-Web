using DoAn2VADT.Database.Entities.Base;
using DoAn2VADT.Database.Entities;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel;

namespace DoAn2VADT.Database.Entities
{
    [Table("Feedback")]
    public class Feedback : BaseEntity
    {
        public string Name { get; set; }
        public string Phone { get; set; }
        public int? Rate { get; set; }
        public string Message { get; set; }
        public string ProductId { get; set; }
        // Sản phẩm
        [ForeignKey("ProductId")]
        public Product Product { set; get; }
        public string SessionId { get; set; }
    }
}