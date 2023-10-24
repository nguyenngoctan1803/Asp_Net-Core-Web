using DoAn2VADT.Database.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DoAn2VADT.Database.Entities
{
    public class AccountConfig : IEntityTypeConfiguration<Account>
    {
        public void Configure(EntityTypeBuilder<Account> builder)
        {
            builder.Property(x => x.UserName).HasMaxLength(250).IsRequired(false);
            builder.Property(x => x.Password).HasMaxLength(250).IsRequired(false);
            builder.HasMany<Order>(x => x.Orders);
        }
    }
}
