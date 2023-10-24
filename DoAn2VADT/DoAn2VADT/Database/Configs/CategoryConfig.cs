using DoAn2VADT.Database.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DoAn2VADT.Database.Entities
{
    public class CategoryConfig : IEntityTypeConfiguration<Category>
    {
        public void Configure(EntityTypeBuilder<Category> builder)
        {
            builder.Property(x => x.Name).HasColumnType("ntext").HasMaxLength(250);
            builder.Property(x => x.Description).HasColumnType("ntext");
            builder.HasMany<Product>(x => x.Products);
        }
    }
}
