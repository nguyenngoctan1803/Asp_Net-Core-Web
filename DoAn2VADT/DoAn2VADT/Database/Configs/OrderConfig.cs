using DoAn2VADT.Database.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DoAn2VADT.Database.Entities
{
    public class OrderConfig : IEntityTypeConfiguration<Order>
    {
        public void Configure(EntityTypeBuilder<Order> builder)
        {
            builder.Property(x => x.Name).HasColumnType("ntext").HasMaxLength(250);
            builder.Property(x => x.Phone).HasMaxLength(20);
            builder.Property(x => x.Address).HasColumnType("ntext");
            builder.Property(x => x.Reason).HasColumnType("ntext");
            builder.HasMany<OrderDetail>(x => x.OrderDetails);
        }
    }
}
