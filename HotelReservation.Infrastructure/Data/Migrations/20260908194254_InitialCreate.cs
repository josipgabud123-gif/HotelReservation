using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace HotelReservation.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Accounts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FirstName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    AccountType = table.Column<string>(type: "nvarchar(8)", maxLength: 8, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Accounts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Amenities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Type = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Amenities", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Accommodations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    Address = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    City = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Country = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    ApprovalStatus = table.Column<int>(type: "int", nullable: false),
                    RejectionReason = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    OwnerId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Accommodations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Accommodations_Accounts_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AccommodationAmenities",
                columns: table => new
                {
                    AccommodationId = table.Column<int>(type: "int", nullable: false),
                    AmenityId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccommodationAmenities", x => new { x.AccommodationId, x.AmenityId });
                    table.ForeignKey(
                        name: "FK_AccommodationAmenities_Accommodations_AccommodationId",
                        column: x => x.AccommodationId,
                        principalTable: "Accommodations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AccommodationAmenities_Amenities_AmenityId",
                        column: x => x.AmenityId,
                        principalTable: "Amenities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AccommodationPhotos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Path = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    IsPrimary = table.Column<bool>(type: "bit", nullable: false),
                    AccommodationId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccommodationPhotos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AccommodationPhotos_Accommodations_AccommodationId",
                        column: x => x.AccommodationId,
                        principalTable: "Accommodations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AccommodationUnits",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Capacity = table.Column<int>(type: "int", nullable: false),
                    PricePerNight = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false),
                    CancellationPolicy = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    AccommodationId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccommodationUnits", x => x.Id);
                    table.CheckConstraint("CK_AccommodationUnit_Capacity", "[Capacity] > 0");
                    table.CheckConstraint("CK_AccommodationUnit_PricePerNight", "[PricePerNight] > 0");
                    table.ForeignKey(
                        name: "FK_AccommodationUnits_Accommodations_AccommodationId",
                        column: x => x.AccommodationId,
                        principalTable: "Accommodations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AccommodationUnitAmenities",
                columns: table => new
                {
                    AccommodationUnitId = table.Column<int>(type: "int", nullable: false),
                    AmenityId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccommodationUnitAmenities", x => new { x.AccommodationUnitId, x.AmenityId });
                    table.ForeignKey(
                        name: "FK_AccommodationUnitAmenities_AccommodationUnits_AccommodationUnitId",
                        column: x => x.AccommodationUnitId,
                        principalTable: "AccommodationUnits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AccommodationUnitAmenities_Amenities_AmenityId",
                        column: x => x.AmenityId,
                        principalTable: "Amenities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AccommodationUnitPhotos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Path = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    IsPrimary = table.Column<bool>(type: "bit", nullable: false),
                    AccommodationUnitId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccommodationUnitPhotos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AccommodationUnitPhotos_AccommodationUnits_AccommodationUnitId",
                        column: x => x.AccommodationUnitId,
                        principalTable: "AccommodationUnits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AvailabilityBlocks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: false),
                    AccommodationId = table.Column<int>(type: "int", nullable: true),
                    AccommodationUnitId = table.Column<int>(type: "int", nullable: true),
                    CreatedByOwnerId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AvailabilityBlocks", x => x.Id);
                    table.CheckConstraint("CK_AvailabilityBlock_Dates", "[EndDate] > [StartDate]");
                    table.CheckConstraint("CK_AvailabilityBlock_Target", "(\r\n                    [AccommodationId] IS NOT NULL\r\n                    AND [AccommodationUnitId] IS NULL\r\n                  )\r\n                  OR\r\n                  (\r\n                    [AccommodationId] IS NULL\r\n                    AND [AccommodationUnitId] IS NOT NULL\r\n                  )");
                    table.ForeignKey(
                        name: "FK_AvailabilityBlocks_AccommodationUnits_AccommodationUnitId",
                        column: x => x.AccommodationUnitId,
                        principalTable: "AccommodationUnits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AvailabilityBlocks_Accommodations_AccommodationId",
                        column: x => x.AccommodationId,
                        principalTable: "Accommodations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AvailabilityBlocks_Accounts_CreatedByOwnerId",
                        column: x => x.CreatedByOwnerId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Reservations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CheckInDate = table.Column<DateOnly>(type: "date", nullable: false),
                    CheckOutDate = table.Column<DateOnly>(type: "date", nullable: false),
                    GuestCount = table.Column<int>(type: "int", nullable: false),
                    TotalPrice = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false),
                    CancellationPolicyAtBooking = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CancelledAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AccountId = table.Column<int>(type: "int", nullable: false),
                    AccommodationUnitId = table.Column<int>(type: "int", nullable: false),
                    CancelledByAccountId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reservations", x => x.Id);
                    table.CheckConstraint("CK_Reservation_Dates", "[CheckOutDate] > [CheckInDate]");
                    table.CheckConstraint("CK_Reservation_GuestCount", "[GuestCount] > 0");
                    table.CheckConstraint("CK_Reservation_TotalPrice", "[TotalPrice] > 0");
                    table.ForeignKey(
                        name: "FK_Reservations_AccommodationUnits_AccommodationUnitId",
                        column: x => x.AccommodationUnitId,
                        principalTable: "AccommodationUnits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Reservations_Accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Reservations_Accounts_CancelledByAccountId",
                        column: x => x.CancelledByAccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Reviews",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Rating = table.Column<int>(type: "int", nullable: false),
                    Comment = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AccountId = table.Column<int>(type: "int", nullable: false),
                    AccommodationId = table.Column<int>(type: "int", nullable: false),
                    ReservationId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reviews", x => x.Id);
                    table.CheckConstraint("CK_Review_Rating", "[Rating] >= 1 AND [Rating] <= 5");
                    table.ForeignKey(
                        name: "FK_Reviews_Accommodations_AccommodationId",
                        column: x => x.AccommodationId,
                        principalTable: "Accommodations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Reviews_Accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Reviews_Reservations_ReservationId",
                        column: x => x.ReservationId,
                        principalTable: "Reservations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Amenities",
                columns: new[] { "Id", "Type" },
                values: new object[,]
                {
                    { 1, 1 },
                    { 2, 2 },
                    { 3, 3 },
                    { 4, 4 },
                    { 5, 5 },
                    { 6, 6 },
                    { 7, 7 },
                    { 8, 8 },
                    { 9, 9 },
                    { 10, 10 },
                    { 11, 11 },
                    { 12, 12 },
                    { 13, 13 },
                    { 14, 14 },
                    { 15, 15 },
                    { 16, 16 },
                    { 17, 17 },
                    { 18, 18 },
                    { 19, 19 },
                    { 20, 20 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_AccommodationAmenities_AmenityId",
                table: "AccommodationAmenities",
                column: "AmenityId");

            migrationBuilder.CreateIndex(
                name: "IX_AccommodationPhotos_AccommodationId",
                table: "AccommodationPhotos",
                column: "AccommodationId",
                unique: true,
                filter: "[IsPrimary] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_Accommodations_ApprovalStatus_IsActive",
                table: "Accommodations",
                columns: new[] { "ApprovalStatus", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_Accommodations_City_Country",
                table: "Accommodations",
                columns: new[] { "City", "Country" });

            migrationBuilder.CreateIndex(
                name: "IX_Accommodations_OwnerId",
                table: "Accommodations",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_AccommodationUnitAmenities_AmenityId",
                table: "AccommodationUnitAmenities",
                column: "AmenityId");

            migrationBuilder.CreateIndex(
                name: "IX_AccommodationUnitPhotos_AccommodationUnitId",
                table: "AccommodationUnitPhotos",
                column: "AccommodationUnitId",
                unique: true,
                filter: "[IsPrimary] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_AccommodationUnits_AccommodationId_Name",
                table: "AccommodationUnits",
                columns: new[] { "AccommodationId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_Email",
                table: "Accounts",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Amenities_Type",
                table: "Amenities",
                column: "Type",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AvailabilityBlocks_AccommodationId",
                table: "AvailabilityBlocks",
                column: "AccommodationId");

            migrationBuilder.CreateIndex(
                name: "IX_AvailabilityBlocks_AccommodationUnitId",
                table: "AvailabilityBlocks",
                column: "AccommodationUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_AvailabilityBlocks_CreatedByOwnerId",
                table: "AvailabilityBlocks",
                column: "CreatedByOwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_AccommodationUnitId_Status_CheckInDate_CheckOutDate",
                table: "Reservations",
                columns: new[] { "AccommodationUnitId", "Status", "CheckInDate", "CheckOutDate" });

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_AccountId",
                table: "Reservations",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_CancelledByAccountId",
                table: "Reservations",
                column: "CancelledByAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_AccommodationId",
                table: "Reviews",
                column: "AccommodationId");

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_AccountId",
                table: "Reviews",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_ReservationId",
                table: "Reviews",
                column: "ReservationId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AccommodationAmenities");

            migrationBuilder.DropTable(
                name: "AccommodationPhotos");

            migrationBuilder.DropTable(
                name: "AccommodationUnitAmenities");

            migrationBuilder.DropTable(
                name: "AccommodationUnitPhotos");

            migrationBuilder.DropTable(
                name: "AvailabilityBlocks");

            migrationBuilder.DropTable(
                name: "Reviews");

            migrationBuilder.DropTable(
                name: "Amenities");

            migrationBuilder.DropTable(
                name: "Reservations");

            migrationBuilder.DropTable(
                name: "AccommodationUnits");

            migrationBuilder.DropTable(
                name: "Accommodations");

            migrationBuilder.DropTable(
                name: "Accounts");
        }
    }
}
