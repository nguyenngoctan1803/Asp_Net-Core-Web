using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DoAn2VADT.Migrations
{
    /// <inheritdoc />
    public partial class v11 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Effective",
                table: "Product",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "ShipFee",
                table: "Order",
                type: "decimal(18,2)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Effective",
                table: "Product");

            migrationBuilder.DropColumn(
                name: "ShipFee",
                table: "Order");
        }
    }
}
