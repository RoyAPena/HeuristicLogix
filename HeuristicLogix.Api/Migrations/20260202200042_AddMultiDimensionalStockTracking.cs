using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HeuristicLogix.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddMultiDimensionalStockTracking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                schema: "Inventory",
                table: "Items",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LocationCode",
                schema: "Inventory",
                table: "Items",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ReservedStockQuantity",
                schema: "Inventory",
                table: "Items",
                type: "decimal(18,4)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "StagingStockQuantity",
                schema: "Inventory",
                table: "Items",
                type: "decimal(18,4)",
                nullable: false,
                defaultValue: 0m);

            // Add index for LocationCode
            migrationBuilder.CreateIndex(
                name: "IX_Items_LocationCode",
                schema: "Inventory",
                table: "Items",
                column: "LocationCode",
                filter: "[LocationCode] IS NOT NULL");

            // Add check constraint to prevent negative available stock
            migrationBuilder.AddCheckConstraint(
                name: "CK_Items_AvailableStock",
                schema: "Inventory",
                table: "Items",
                sql: "[CurrentStockQuantity] >= [ReservedStockQuantity]");

            migrationBuilder.CreateIndex(
                name: "IX_UnitsOfMeasure_UnitOfMeasureName",
                schema: "Core",
                table: "UnitsOfMeasure",
                column: "UnitOfMeasureName");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UnitsOfMeasure_UnitOfMeasureName",
                schema: "Core",
                table: "UnitsOfMeasure");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Items_AvailableStock",
                schema: "Inventory",
                table: "Items");

            migrationBuilder.DropIndex(
                name: "IX_Items_LocationCode",
                schema: "Inventory",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "ImageUrl",
                schema: "Inventory",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "LocationCode",
                schema: "Inventory",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "ReservedStockQuantity",
                schema: "Inventory",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "StagingStockQuantity",
                schema: "Inventory",
                table: "Items");
        }
    }
}
