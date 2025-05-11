using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPerformanceIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Rentals_CustomerId",
                table: "Rentals");

            migrationBuilder.DropIndex(
                name: "IX_Rentals_VehicleId",
                table: "Rentals");

            migrationBuilder.CreateIndex(
                name: "Telemetries_Timestamp",
                table: "Telemetries",
                column: "Timestamp");

            migrationBuilder.CreateIndex(
                name: "Rentals_CustomerId_StartDate_EndDate_Status",
                table: "Rentals",
                columns: new[] { "CustomerId", "StartDate", "EndDate", "RentalStatus" });

            migrationBuilder.CreateIndex(
                name: "Rentals_RentalStatus",
                table: "Rentals",
                column: "RentalStatus");

            migrationBuilder.CreateIndex(
                name: "Rentals_StartDate_EndDate",
                table: "Rentals",
                columns: new[] { "StartDate", "EndDate" });

            migrationBuilder.CreateIndex(
                name: "Rentals_VehicleId_StartDate_EndDate_Status",
                table: "Rentals",
                columns: new[] { "VehicleId", "StartDate", "EndDate", "RentalStatus" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "Telemetries_Timestamp",
                table: "Telemetries");

            migrationBuilder.DropIndex(
                name: "Rentals_CustomerId_StartDate_EndDate_Status",
                table: "Rentals");

            migrationBuilder.DropIndex(
                name: "Rentals_RentalStatus",
                table: "Rentals");

            migrationBuilder.DropIndex(
                name: "Rentals_StartDate_EndDate",
                table: "Rentals");

            migrationBuilder.DropIndex(
                name: "Rentals_VehicleId_StartDate_EndDate_Status",
                table: "Rentals");

            migrationBuilder.CreateIndex(
                name: "IX_Rentals_CustomerId",
                table: "Rentals",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_Rentals_VehicleId",
                table: "Rentals",
                column: "VehicleId");
        }
    }
}
