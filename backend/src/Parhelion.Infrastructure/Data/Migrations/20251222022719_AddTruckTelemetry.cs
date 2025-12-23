using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Parhelion.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddTruckTelemetry : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "LastLatitude",
                table: "Trucks",
                type: "numeric(10,6)",
                precision: 10,
                scale: 6,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastLocationUpdate",
                table: "Trucks",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "LastLongitude",
                table: "Trucks",
                type: "numeric(10,6)",
                precision: 10,
                scale: 6,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastLatitude",
                table: "Trucks");

            migrationBuilder.DropColumn(
                name: "LastLocationUpdate",
                table: "Trucks");

            migrationBuilder.DropColumn(
                name: "LastLongitude",
                table: "Trucks");
        }
    }
}
