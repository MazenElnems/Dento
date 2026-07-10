using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AngDepiApi_DentalClinic.Migrations
{
    /// <inheritdoc />
    public partial class UpdateCascadeConstraintInSlotTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Payments_Appointments_AppointmentId",
                table: "Payments");

            migrationBuilder.DropForeignKey(
                name: "FK_Slots_DentistAvailability_DentistAvailabilityId",
                table: "Slots");

            migrationBuilder.DropIndex(
                name: "IX_Payments_AppointmentId",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "AppointmentId",
                table: "Payments");

            migrationBuilder.AlterColumn<string>(
                name: "SlotId",
                table: "Appointments",
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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Slots_DentistAvailability_DentistAvailabilityId",
                table: "Slots");

            migrationBuilder.AddColumn<string>(
                name: "AppointmentId",
                table: "Payments",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "SlotId",
                table: "Appointments",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_AppointmentId",
                table: "Payments",
                column: "AppointmentId");

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_Appointments_AppointmentId",
                table: "Payments",
                column: "AppointmentId",
                principalTable: "Appointments",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Slots_DentistAvailability_DentistAvailabilityId",
                table: "Slots",
                column: "DentistAvailabilityId",
                principalTable: "DentistAvailability",
                principalColumn: "Id");
        }
    }
}
