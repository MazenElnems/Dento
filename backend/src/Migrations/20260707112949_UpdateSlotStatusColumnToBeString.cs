using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AngDepiApi_DentalClinic.Migrations
{
    /// <inheritdoc />
    public partial class UpdateSlotStatusColumnToBeString : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_DentistAvailability_DentistId",
                table: "DentistAvailability");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Slots",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.CreateIndex(
                name: "IX_DentistAvailability_DentistId",
                table: "DentistAvailability",
                column: "DentistId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_DentistAvailability_DentistId",
                table: "DentistAvailability");

            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "Slots",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_DentistAvailability_DentistId",
                table: "DentistAvailability",
                column: "DentistId");
        }
    }
}
