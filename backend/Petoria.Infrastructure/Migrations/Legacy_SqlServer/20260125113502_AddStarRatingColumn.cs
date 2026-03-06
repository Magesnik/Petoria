using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Petoria.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddStarRatingColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "StarRating",
                table: "Hotels",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StarRating",
                table: "Hotels");
        }
    }
}
