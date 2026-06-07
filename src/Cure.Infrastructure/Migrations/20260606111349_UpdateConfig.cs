using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cure.Infrastructure.Migrations
{
    public partial class UpdateConfig : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PatientFiles_Patients_PatientId1",
                table: "PatientFiles");

            migrationBuilder.DropIndex(
                name: "IX_PatientFiles_PatientId1",
                table: "PatientFiles");

            migrationBuilder.DropColumn(
                name: "PatientId1",
                table: "PatientFiles");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "PatientId1",
                table: "PatientFiles",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_PatientFiles_PatientId1",
                table: "PatientFiles",
                column: "PatientId1");

            migrationBuilder.AddForeignKey(
                name: "FK_PatientFiles_Patients_PatientId1",
                table: "PatientFiles",
                column: "PatientId1",
                principalTable: "Patients",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
