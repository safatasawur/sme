using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SMElevate.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddFieldOptionsToSchemeFormField : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FieldOptions",
                table: "SchemeFormFields",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FieldOptions",
                table: "SchemeFormFields");
        }
    }
}
