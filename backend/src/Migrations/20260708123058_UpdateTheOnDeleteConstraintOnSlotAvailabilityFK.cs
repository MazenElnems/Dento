using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AngDepiApi_DentalClinic.Migrations
{
    /// <inheritdoc />
    public partial class UpdateTheOnDeleteConstraintOnSlotAvailabilityFK : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Slots_DentistAvailability_DentistAvailabilityId",
                table: "Slots");

            migrationBuilder.AlterColumn<string>(
                name: "DentistAvailabilityId",
                table: "Slots",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddForeignKey(
                name: "FK_Slots_DentistAvailability_DentistAvailabilityId",
                table: "Slots",
                column: "DentistAvailabilityId",
                principalTable: "DentistAvailability",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Slots_DentistAvailability_DentistAvailabilityId",
                table: "Slots");

            migrationBuilder.AlterColumn<string>(
                name: "DentistAvailabilityId",
                table: "Slots",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Slots_DentistAvailability_DentistAvailabilityId",
                table: "Slots",
                column: "DentistAvailabilityId",
                principalTable: "DentistAvailability",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
