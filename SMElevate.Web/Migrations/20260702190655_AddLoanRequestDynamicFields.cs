using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SMElevate.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddLoanRequestDynamicFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SchemeFormId",
                table: "LoanRequests",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SchemeId",
                table: "LoanRequests",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "LoanRequestFieldValues",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LoanRequestId = table.Column<int>(type: "int", nullable: false),
                    SchemeFormFieldId = table.Column<int>(type: "int", nullable: true),
                    FieldName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FieldValue = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoanRequestFieldValues", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LoanRequestFieldValues_LoanRequests_LoanRequestId",
                        column: x => x.LoanRequestId,
                        principalTable: "LoanRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LoanRequests_SchemeFormId",
                table: "LoanRequests",
                column: "SchemeFormId");

            migrationBuilder.CreateIndex(
                name: "IX_LoanRequests_SchemeId",
                table: "LoanRequests",
                column: "SchemeId");

            migrationBuilder.CreateIndex(
                name: "IX_LoanRequestFieldValues_LoanRequestId",
                table: "LoanRequestFieldValues",
                column: "LoanRequestId");

            migrationBuilder.AddForeignKey(
                name: "FK_LoanRequests_SchemeForms_SchemeFormId",
                table: "LoanRequests",
                column: "SchemeFormId",
                principalTable: "SchemeForms",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_LoanRequests_Schemes_SchemeId",
                table: "LoanRequests",
                column: "SchemeId",
                principalTable: "Schemes",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LoanRequests_SchemeForms_SchemeFormId",
                table: "LoanRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_LoanRequests_Schemes_SchemeId",
                table: "LoanRequests");

            migrationBuilder.DropTable(
                name: "LoanRequestFieldValues");

            migrationBuilder.DropIndex(
                name: "IX_LoanRequests_SchemeFormId",
                table: "LoanRequests");

            migrationBuilder.DropIndex(
                name: "IX_LoanRequests_SchemeId",
                table: "LoanRequests");

            migrationBuilder.DropColumn(
                name: "SchemeFormId",
                table: "LoanRequests");

            migrationBuilder.DropColumn(
                name: "SchemeId",
                table: "LoanRequests");
        }
    }
}
