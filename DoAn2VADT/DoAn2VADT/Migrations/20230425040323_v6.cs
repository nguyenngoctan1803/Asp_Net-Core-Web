using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DoAn2VADT.Migrations
{
    /// <inheritdoc />
    public partial class v6 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Product_Brand_BranchId",
                table: "Product");

            migrationBuilder.DropIndex(
                name: "IX_Product_BranchId",
                table: "Product");

            migrationBuilder.DropColumn(
                name: "BranchId",
                table: "Product");

            migrationBuilder.AlterColumn<string>(
                name: "BrandId",
                table: "Product",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Product_BrandId",
                table: "Product",
                column: "BrandId");

            migrationBuilder.AddForeignKey(
                name: "FK_Product_Brand_BrandId",
                table: "Product",
                column: "BrandId",
                principalTable: "Brand",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Product_Brand_BrandId",
                table: "Product");

            migrationBuilder.DropIndex(
                name: "IX_Product_BrandId",
                table: "Product");

            migrationBuilder.AlterColumn<string>(
                name: "BrandId",
                table: "Product",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BranchId",
                table: "Product",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Product_BranchId",
                table: "Product",
                column: "BranchId");

            migrationBuilder.AddForeignKey(
                name: "FK_Product_Brand_BranchId",
                table: "Product",
                column: "BranchId",
                principalTable: "Brand",
                principalColumn: "Id");
        }
    }
}
