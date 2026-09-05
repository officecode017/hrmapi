using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRAttendance.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddEnforceGeofenceToLocation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "EnforceGeofence",
                table: "Locations",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EnforceGeofence",
                table: "Locations");
        }
    }
}
