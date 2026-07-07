using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AngDepiApi_DentalClinic.Migrations
{
    /// <inheritdoc />
    public partial class AddDentistAvailabilityAndSlotTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DentistAvailability",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    DentistId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    SAT = table.Column<bool>(type: "bit", nullable: false),
                    SUN = table.Column<bool>(type: "bit", nullable: false),
                    MON = table.Column<bool>(type: "bit", nullable: false),
                    TUE = table.Column<bool>(type: "bit", nullable: false),
                    WED = table.Column<bool>(type: "bit", nullable: false),
                    THU = table.Column<bool>(type: "bit", nullable: false),
                    FRI = table.Column<bool>(type: "bit", nullable: false),
                    FromHour = table.Column<TimeOnly>(type: "time", nullable: false),
                    ToHour = table.Column<TimeOnly>(type: "time", nullable: false),
                    SecondFromHour = table.Column<TimeOnly>(type: "time", nullable: true),
                    SecondToHour = table.Column<TimeOnly>(type: "time", nullable: true),
                    SlotLengthInMinutes = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DentistAvailability", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DentistAvailability_Dentists_DentistId",
                        column: x => x.DentistId,
                        principalTable: "Dentists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Slots",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    DentistAvailabilityId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    From = table.Column<TimeOnly>(type: "time", nullable: false),
                    To = table.Column<TimeOnly>(type: "time", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Slots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Slots_DentistAvailability_DentistAvailabilityId",
                        column: x => x.DentistAvailabilityId,
                        principalTable: "DentistAvailability",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DentistAvailability_DentistId",
                table: "DentistAvailability",
                column: "DentistId");

            migrationBuilder.CreateIndex(
                name: "IX_Slots_DentistAvailabilityId",
                table: "Slots",
                column: "DentistAvailabilityId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Slots");

            migrationBuilder.DropTable(
                name: "DentistAvailability");
        }
    }
}
