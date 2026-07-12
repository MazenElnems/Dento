using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AngDepiApi_DentalClinic.Migrations
{
    /// <inheritdoc />
    public partial class AddMedicalHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MedicalHistories",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    MedicalRecordId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    MedicalConditions = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Allergies = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PregnancyStatus = table.Column<int>(type: "int", nullable: false),
                    SmokingStatus = table.Column<int>(type: "int", nullable: false),
                    BleedingDisorders = table.Column<bool>(type: "bit", nullable: false),
                    HeartConditions = table.Column<bool>(type: "bit", nullable: false),
                    Diabetes = table.Column<bool>(type: "bit", nullable: false),
                    HighBloodPressure = table.Column<bool>(type: "bit", nullable: false),
                    MedicalNotes = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MedicalHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MedicalHistories_MedicalRecords_MedicalRecordId",
                        column: x => x.MedicalRecordId,
                        principalTable: "MedicalRecords",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MedicalHistories_MedicalRecordId",
                table: "MedicalHistories",
                column: "MedicalRecordId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MedicalHistories");
        }
    }
}
