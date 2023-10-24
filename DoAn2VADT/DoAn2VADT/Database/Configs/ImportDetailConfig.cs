using DoAn2VADT.Database.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DoAn2VADT.Database.Entities
{
    public class ImportDetailConfig : IEntityTypeConfiguration<ImportDetail>
    {
        public void Configure(EntityTypeBuilder<ImportDetail> builder)
        {

        }
    }
}
