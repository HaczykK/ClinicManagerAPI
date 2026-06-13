using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClinicManagerAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddVisitsCompositeIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Visits_AssignedDoctorId",
                table: "Visits");

            migrationBuilder.DropIndex(
                name: "IX_Visits_Date",
                table: "Visits");

            migrationBuilder.CreateIndex(
                name: "IX_Visits_AssignedDoctorId_Date",
                table: "Visits",
                columns: new[] { "AssignedDoctorId", "Date" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Visits_AssignedDoctorId_Date",
                table: "Visits");

            migrationBuilder.CreateIndex(
                name: "IX_Visits_AssignedDoctorId",
                table: "Visits",
                column: "AssignedDoctorId");

            migrationBuilder.CreateIndex(
                name: "IX_Visits_Date",
                table: "Visits",
                column: "Date");
        }
    }
}
