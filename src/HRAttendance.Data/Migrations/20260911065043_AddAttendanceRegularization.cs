using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRAttendance.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAttendanceRegularization : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            

            migrationBuilder.CreateTable(
                name: "AttendanceRegularizationRequests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AttendanceId = table.Column<int>(type: "int", nullable: false),
                    EmployeeId = table.Column<int>(type: "int", nullable: false),
                    RequestedInTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    RequestedOutTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    EmployeeRemark = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ReviewedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ReviewedBy = table.Column<int>(type: "int", nullable: true),
                    AdminRemark = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedBy = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<int>(type: "int", nullable: true),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AttendanceRegularizationRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AttendanceRegularizationRequests_EmployeeAttendances_AttendanceId",
                        column: x => x.AttendanceId,
                        principalTable: "EmployeeAttendances",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AttendanceRegularizationRequests_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

           

            migrationBuilder.CreateIndex(
                name: "IX_AttendanceRegularizationRequests_AttendanceId",
                table: "AttendanceRegularizationRequests",
                column: "AttendanceId");

            migrationBuilder.CreateIndex(
                name: "IX_AttendanceRegularizationRequests_EmployeeId",
                table: "AttendanceRegularizationRequests",
                column: "EmployeeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AttendanceRegularizationRequests");

            
        }
    }
}
