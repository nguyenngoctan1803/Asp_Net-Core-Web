using DoAn2VADT.Database.Entities.Base;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace DoAn2VADT.Database.Entities
{
    [Table("Category")]
    public class Category : BaseEntity
    {
        [Column(TypeName = "ntext")]
        [MaxLength(100)]
        [DisplayName("Danh mục")]
        public string Name { get; set; }


        [Column(TypeName = "ntext")]
        [DisplayName("Mô tả")]
        public string Description { get; set; }
        public ICollection<Product> Products { get; set; }
    }
}
