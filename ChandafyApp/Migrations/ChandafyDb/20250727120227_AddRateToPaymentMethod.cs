using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChandafyApp.Migrations.ChandafyDb
{
    /// <inheritdoc />
    public partial class AddRateToPaymentMethod : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Rule",
                table: "PaymentMethods");

            migrationBuilder.AddColumn<decimal>(
                name: "Rate",
                table: "PaymentMethods",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Rate",
                table: "PaymentMethods");

            migrationBuilder.AddColumn<string>(
                name: "Rule",
                table: "PaymentMethods",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
