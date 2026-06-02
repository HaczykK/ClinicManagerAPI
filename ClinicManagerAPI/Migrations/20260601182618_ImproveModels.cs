using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClinicManagerAPI.Migrations
{
    /// <inheritdoc />
    public partial class ImproveModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AssignedDoctor",
                table: "Visits");

            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "Visits",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "AssignedDoctorId",
                table: "Visits",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Patients",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Patients",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "MedicalRecords",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FileName",
                table: "MedicalRecords",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UploadedAt",
                table: "MedicalRecords",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateIndex(
                name: "IX_Visits_AssignedDoctorId",
                table: "Visits",
                column: "AssignedDoctorId");

            migrationBuilder.AddForeignKey(
                name: "FK_Visits_AspNetUsers_AssignedDoctorId",
                table: "Visits",
                column: "AssignedDoctorId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Visits_AspNetUsers_AssignedDoctorId",
                table: "Visits");

            migrationBuilder.DropIndex(
                name: "IX_Visits_AssignedDoctorId",
                table: "Visits");

            migrationBuilder.DropColumn(
                name: "AssignedDoctorId",
                table: "Visits");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Patients");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Patients");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "MedicalRecords");

            migrationBuilder.DropColumn(
                name: "FileName",
                table: "MedicalRecords");

            migrationBuilder.DropColumn(
                name: "UploadedAt",
                table: "MedicalRecords");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Visits",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<string>(
                name: "AssignedDoctor",
                table: "Visits",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
