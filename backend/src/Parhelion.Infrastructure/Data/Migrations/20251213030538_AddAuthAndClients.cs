using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Parhelion.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAuthAndClients : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Color",
                table: "Trucks",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "CurrentOdometerKm",
                table: "Trucks",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EngineNumber",
                table: "Trucks",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "InsuranceExpiration",
                table: "Trucks",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InsurancePolicy",
                table: "Trucks",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastMaintenanceDate",
                table: "Trucks",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "NextMaintenanceDate",
                table: "Trucks",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "VerificationExpiration",
                table: "Trucks",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VerificationNumber",
                table: "Trucks",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Vin",
                table: "Trucks",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Year",
                table: "Trucks",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "RecipientClientId",
                table: "Shipments",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SenderId",
                table: "Shipments",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ActionType",
                table: "ShipmentCheckpoints",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "HandledByDriverId",
                table: "ShipmentCheckpoints",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "LoadedOntoTruckId",
                table: "ShipmentCheckpoints",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NewCustodian",
                table: "ShipmentCheckpoints",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PreviousCustodian",
                table: "ShipmentCheckpoints",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Curp",
                table: "Drivers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EmergencyContact",
                table: "Drivers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EmergencyPhone",
                table: "Drivers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "HireDate",
                table: "Drivers",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LicenseExpiration",
                table: "Drivers",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LicenseType",
                table: "Drivers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Nss",
                table: "Drivers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Rfc",
                table: "Drivers",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Clients",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CompanyName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    TradeName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ContactName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Email = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Phone = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    TaxId = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    LegalName = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    BillingAddress = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ShippingAddress = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    PreferredProductTypes = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    Priority = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    Notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Clients", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Clients_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RefreshTokens",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    TokenHash = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsRevoked = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    RevokedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RevokedReason = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    CreatedFromIp = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true),
                    UserAgent = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RefreshTokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RefreshTokens_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Shipments_RecipientClientId",
                table: "Shipments",
                column: "RecipientClientId");

            migrationBuilder.CreateIndex(
                name: "IX_Shipments_SenderId",
                table: "Shipments",
                column: "SenderId");

            migrationBuilder.CreateIndex(
                name: "IX_ShipmentCheckpoints_HandledByDriverId",
                table: "ShipmentCheckpoints",
                column: "HandledByDriverId");

            migrationBuilder.CreateIndex(
                name: "IX_ShipmentCheckpoints_LoadedOntoTruckId",
                table: "ShipmentCheckpoints",
                column: "LoadedOntoTruckId");

            migrationBuilder.CreateIndex(
                name: "IX_Clients_Email",
                table: "Clients",
                column: "Email");

            migrationBuilder.CreateIndex(
                name: "IX_Clients_TenantId",
                table: "Clients",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Clients_TenantId_CompanyName",
                table: "Clients",
                columns: new[] { "TenantId", "CompanyName" });

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_ExpiresAt",
                table: "RefreshTokens",
                column: "ExpiresAt");

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_TokenHash",
                table: "RefreshTokens",
                column: "TokenHash");

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_UserId",
                table: "RefreshTokens",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_ShipmentCheckpoints_Drivers_HandledByDriverId",
                table: "ShipmentCheckpoints",
                column: "HandledByDriverId",
                principalTable: "Drivers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_ShipmentCheckpoints_Trucks_LoadedOntoTruckId",
                table: "ShipmentCheckpoints",
                column: "LoadedOntoTruckId",
                principalTable: "Trucks",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Shipments_Clients_RecipientClientId",
                table: "Shipments",
                column: "RecipientClientId",
                principalTable: "Clients",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Shipments_Clients_SenderId",
                table: "Shipments",
                column: "SenderId",
                principalTable: "Clients",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ShipmentCheckpoints_Drivers_HandledByDriverId",
                table: "ShipmentCheckpoints");

            migrationBuilder.DropForeignKey(
                name: "FK_ShipmentCheckpoints_Trucks_LoadedOntoTruckId",
                table: "ShipmentCheckpoints");

            migrationBuilder.DropForeignKey(
                name: "FK_Shipments_Clients_RecipientClientId",
                table: "Shipments");

            migrationBuilder.DropForeignKey(
                name: "FK_Shipments_Clients_SenderId",
                table: "Shipments");

            migrationBuilder.DropTable(
                name: "Clients");

            migrationBuilder.DropTable(
                name: "RefreshTokens");

            migrationBuilder.DropIndex(
                name: "IX_Shipments_RecipientClientId",
                table: "Shipments");

            migrationBuilder.DropIndex(
                name: "IX_Shipments_SenderId",
                table: "Shipments");

            migrationBuilder.DropIndex(
                name: "IX_ShipmentCheckpoints_HandledByDriverId",
                table: "ShipmentCheckpoints");

            migrationBuilder.DropIndex(
                name: "IX_ShipmentCheckpoints_LoadedOntoTruckId",
                table: "ShipmentCheckpoints");

            migrationBuilder.DropColumn(
                name: "Color",
                table: "Trucks");

            migrationBuilder.DropColumn(
                name: "CurrentOdometerKm",
                table: "Trucks");

            migrationBuilder.DropColumn(
                name: "EngineNumber",
                table: "Trucks");

            migrationBuilder.DropColumn(
                name: "InsuranceExpiration",
                table: "Trucks");

            migrationBuilder.DropColumn(
                name: "InsurancePolicy",
                table: "Trucks");

            migrationBuilder.DropColumn(
                name: "LastMaintenanceDate",
                table: "Trucks");

            migrationBuilder.DropColumn(
                name: "NextMaintenanceDate",
                table: "Trucks");

            migrationBuilder.DropColumn(
                name: "VerificationExpiration",
                table: "Trucks");

            migrationBuilder.DropColumn(
                name: "VerificationNumber",
                table: "Trucks");

            migrationBuilder.DropColumn(
                name: "Vin",
                table: "Trucks");

            migrationBuilder.DropColumn(
                name: "Year",
                table: "Trucks");

            migrationBuilder.DropColumn(
                name: "RecipientClientId",
                table: "Shipments");

            migrationBuilder.DropColumn(
                name: "SenderId",
                table: "Shipments");

            migrationBuilder.DropColumn(
                name: "ActionType",
                table: "ShipmentCheckpoints");

            migrationBuilder.DropColumn(
                name: "HandledByDriverId",
                table: "ShipmentCheckpoints");

            migrationBuilder.DropColumn(
                name: "LoadedOntoTruckId",
                table: "ShipmentCheckpoints");

            migrationBuilder.DropColumn(
                name: "NewCustodian",
                table: "ShipmentCheckpoints");

            migrationBuilder.DropColumn(
                name: "PreviousCustodian",
                table: "ShipmentCheckpoints");

            migrationBuilder.DropColumn(
                name: "Curp",
                table: "Drivers");

            migrationBuilder.DropColumn(
                name: "EmergencyContact",
                table: "Drivers");

            migrationBuilder.DropColumn(
                name: "EmergencyPhone",
                table: "Drivers");

            migrationBuilder.DropColumn(
                name: "HireDate",
                table: "Drivers");

            migrationBuilder.DropColumn(
                name: "LicenseExpiration",
                table: "Drivers");

            migrationBuilder.DropColumn(
                name: "LicenseType",
                table: "Drivers");

            migrationBuilder.DropColumn(
                name: "Nss",
                table: "Drivers");

            migrationBuilder.DropColumn(
                name: "Rfc",
                table: "Drivers");
        }
    }
}
