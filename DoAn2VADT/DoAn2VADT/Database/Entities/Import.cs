using DoAn2VADT.Database.Entities.Base;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Xml.Linq;

namespace DoAn2VADT.Database.Entities
{
    [Table("Import")]
    public class Import : BaseEntity
    {
        [DisplayName("Tổng tiền")]
        public decimal? Total { get; set; }

        // người xử lí nhập hàng
        [ForeignKey("CreateUserId")]
        public Account Account { set; get; }

        public ICollection<ImportDetail> ImportDetails { get; set; }
    }
}
