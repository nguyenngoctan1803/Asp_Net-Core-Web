using DocumentFormat.OpenXml.Drawing.Charts;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Xml.Linq;

namespace DoAn2VADT.Database.Entities.Base
{
	public class BaseEntity
    {
        [Key]
        [Column(Order = 1)]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public DateTime? CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; } = DateTime.Now;
        public DateTime? DeletedAt { get; set; }
        public string UpdateUserId { get; set; }
        public string CreateUserId { get; set; }
    }
}
