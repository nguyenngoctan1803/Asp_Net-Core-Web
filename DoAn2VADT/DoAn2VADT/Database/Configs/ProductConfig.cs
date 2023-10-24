using DoAn2VADT.Database.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DoAn2VADT.Database.Entities
{
    public class ProductConfig : IEntityTypeConfiguration<Product>
    {
        public void Configure(EntityTypeBuilder<Product> builder)
        {
            builder.Property(x => x.Name).HasMaxLength(250).HasColumnType("ntext");
            builder.Property(x => x.Description).HasColumnType("ntext");

            builder.HasMany<OrderDetail>(x => x.OrderDetails);
            builder.HasMany<ImportDetail>(x => x.ImportDetails);
            builder.HasMany<Cart>(x => x.Carts);
            builder.HasMany<Feedback>(x => x.Feedbacks);
        }
    }
}
