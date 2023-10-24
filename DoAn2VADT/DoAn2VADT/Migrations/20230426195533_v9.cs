using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DoAn2VADT.Migrations
{
    /// <inheritdoc />
    public partial class v9 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Shipdate",
                table: "Order",
                newName: "ShipDate");

            migrationBuilder.AddColumn<DateTime>(
                name: "ReceiveDate",
                table: "Order",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReceiveDate",
                table: "Order");

            migrationBuilder.RenameColumn(
                name: "ShipDate",
                table: "Order",
                newName: "Shipdate");
        }
    }
}
