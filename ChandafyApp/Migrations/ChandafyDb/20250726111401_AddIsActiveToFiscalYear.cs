using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChandafyApp.Migrations.ChandafyDb
{
    /// <inheritdoc />
    public partial class AddIsActiveToFiscalYear : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "FiscalYearId",
                table: "Payments",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "FiscalYears",
                type: "bit",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Payments_FiscalYearId",
                table: "Payments",
                column: "FiscalYearId");

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_FiscalYears_FiscalYearId",
                table: "Payments",
                column: "FiscalYearId",
                principalTable: "FiscalYears",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Payments_FiscalYears_FiscalYearId",
                table: "Payments");

            migrationBuilder.DropIndex(
                name: "IX_Payments_FiscalYearId",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "FiscalYearId",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "FiscalYears");
        }
    }
}
