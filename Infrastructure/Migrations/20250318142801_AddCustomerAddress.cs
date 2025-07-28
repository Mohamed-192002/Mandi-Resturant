using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCustomerAddress : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CustomerReceive",
                table: "SaleBills");

            migrationBuilder.AddColumn<string>(
                name: "CustomerAddress",
                table: "SaleBills",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CustomerAddress",
                table: "SaleBills");

            migrationBuilder.AddColumn<bool>(
                name: "CustomerReceive",
                table: "SaleBills",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
