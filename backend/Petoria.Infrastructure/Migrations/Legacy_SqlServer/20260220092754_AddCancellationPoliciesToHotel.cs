using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Petoria.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCancellationPoliciesToHotel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CancellationPolicies",
                table: "Hotels",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CancellationPolicies",
                table: "Hotels");
        }
    }
}
