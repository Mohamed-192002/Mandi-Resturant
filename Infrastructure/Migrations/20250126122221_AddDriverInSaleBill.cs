using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDriverInSaleBill : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Delivered",
                table: "SaleBills",
                newName: "MoneyDelivered");

            migrationBuilder.AddColumn<DateTime>(
                name: "DateDelivered",
                table: "SaleBills",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DriverId",
                table: "SaleBills",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SaleBills_DriverId",
                table: "SaleBills",
                column: "DriverId");

            migrationBuilder.AddForeignKey(
                name: "FK_SaleBills_Drivers_DriverId",
                table: "SaleBills",
                column: "DriverId",
                principalTable: "Drivers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SaleBills_Drivers_DriverId",
                table: "SaleBills");

            migrationBuilder.DropIndex(
                name: "IX_SaleBills_DriverId",
                table: "SaleBills");

            migrationBuilder.DropColumn(
                name: "DateDelivered",
                table: "SaleBills");

            migrationBuilder.DropColumn(
                name: "DriverId",
                table: "SaleBills");

            migrationBuilder.RenameColumn(
                name: "MoneyDelivered",
                table: "SaleBills",
                newName: "Delivered");
        }
    }
}
