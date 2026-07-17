using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SMElevate.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddBusinessBankDetail : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BusinessBankId",
                table: "BusinessProfiles",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BusinessIBAN",
                table: "BusinessProfiles",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_BusinessProfiles_BusinessBankId",
                table: "BusinessProfiles",
                column: "BusinessBankId");

            migrationBuilder.AddForeignKey(
                name: "FK_BusinessProfiles_Banks_BusinessBankId",
                table: "BusinessProfiles",
                column: "BusinessBankId",
                principalTable: "Banks",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BusinessProfiles_Banks_BusinessBankId",
                table: "BusinessProfiles");

            migrationBuilder.DropIndex(
                name: "IX_BusinessProfiles_BusinessBankId",
                table: "BusinessProfiles");

            migrationBuilder.DropColumn(
                name: "BusinessBankId",
                table: "BusinessProfiles");

            migrationBuilder.DropColumn(
                name: "BusinessIBAN",
                table: "BusinessProfiles");
        }
    }
}
