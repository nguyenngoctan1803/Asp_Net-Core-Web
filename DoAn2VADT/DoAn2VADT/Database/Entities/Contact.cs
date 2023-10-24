using DoAn2VADT.Database.Entities.Base;
using DoAn2VADT.Database.Entities;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel;

namespace DoAn2VADT.Database.Entities
{
    [Table("Contact")]
    public class Contact:BaseEntity
    {
        public string Name { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string Topic { get; set; }
        public string Message { get; set; }
        public string SessionId { get; set; }
    }
}
