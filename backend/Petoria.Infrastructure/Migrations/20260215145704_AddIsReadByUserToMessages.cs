using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Petoria.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddIsReadByUserToMessages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsReadByUser",
                table: "HotelMessages",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsReadByUser",
                table: "HotelMessages");
        }
    }
}
