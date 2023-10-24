using DoAn2VADT.Database.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DoAn2VADT.Database.Entities
{
    public class CustomerConfig : IEntityTypeConfiguration<Customer>
    {
        public void Configure(EntityTypeBuilder<Customer> builder)
        {
            builder.Property(x => x.Name).HasColumnType("ntext").HasMaxLength(250);
            builder.Property(x => x.Phone).HasMaxLength(20);
            builder.Property(x => x.Address).HasColumnType("ntext");
            builder.Property(x => x.Email).HasMaxLength(100);
            builder.HasMany<Order>(x => x.Orders);
            builder.HasMany<Order>(x => x.Orders);
        }
    }
}
