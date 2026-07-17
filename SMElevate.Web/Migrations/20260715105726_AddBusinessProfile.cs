using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SMElevate.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddBusinessProfile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BusinessProfileId",
                table: "LoanRequests",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OwnerCNIC",
                table: "LoanRequests",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "BusinessProfiles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    NameOfBusiness = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OwnerCNIC = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ContactPerson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CellOrLandlineNo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NTNNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BusinessAddress = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AnnualSales = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    YearOfEstablishment = table.Column<int>(type: "int", nullable: false),
                    NoOfEmployees = table.Column<int>(type: "int", nullable: false),
                    BusinessPremise = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsBusinessRegistered = table.Column<bool>(type: "bit", nullable: false),
                    RegistrationAuthority = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BusinessStatus = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BusinessNature = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BusinessDescription = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BusinessEmailAddress = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsBusinessEmailVerified = table.Column<bool>(type: "bit", nullable: false),
                    IsBusinessMobileVerified = table.Column<bool>(type: "bit", nullable: false),
                    BusinessVerificationStatus = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BusinessVerifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    BusinessEmailOtpCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BusinessEmailOtpExpiry = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EmailOtpAttempts = table.Column<int>(type: "int", nullable: false),
                    EmailResendCount = table.Column<int>(type: "int", nullable: false),
                    LastEmailOtpSentAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    BusinessMobileOtpCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BusinessMobileOtpExpiry = table.Column<DateTime>(type: "datetime2", nullable: true),
                    MobileOtpAttempts = table.Column<int>(type: "int", nullable: false),
                    MobileResendCount = table.Column<int>(type: "int", nullable: false),
                    LastMobileOtpSentAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BusinessProfiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BusinessProfiles_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "BusinessShareholders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BusinessProfileId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ContactNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CNIC = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ShareholdingPercentage = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BusinessShareholders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BusinessShareholders_BusinessProfiles_BusinessProfileId",
                        column: x => x.BusinessProfileId,
                        principalTable: "BusinessProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LoanRequests_BusinessProfileId",
                table: "LoanRequests",
                column: "BusinessProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_BusinessProfiles_UserId",
                table: "BusinessProfiles",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_BusinessProfiles_UserId_IsActive",
                table: "BusinessProfiles",
                columns: new[] { "UserId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_BusinessShareholders_BusinessProfileId",
                table: "BusinessShareholders",
                column: "BusinessProfileId");

            migrationBuilder.AddForeignKey(
                name: "FK_LoanRequests_BusinessProfiles_BusinessProfileId",
                table: "LoanRequests",
                column: "BusinessProfileId",
                principalTable: "BusinessProfiles",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LoanRequests_BusinessProfiles_BusinessProfileId",
                table: "LoanRequests");

            migrationBuilder.DropTable(
                name: "BusinessShareholders");

            migrationBuilder.DropTable(
                name: "BusinessProfiles");

            migrationBuilder.DropIndex(
                name: "IX_LoanRequests_BusinessProfileId",
                table: "LoanRequests");

            migrationBuilder.DropColumn(
                name: "BusinessProfileId",
                table: "LoanRequests");

            migrationBuilder.DropColumn(
                name: "OwnerCNIC",
                table: "LoanRequests");
        }
    }
}
