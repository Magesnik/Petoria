using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Petoria.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddGlobalPromoCodeSupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PromoCodes_Hotels_HotelId",
                table: "PromoCodes");

            migrationBuilder.DropIndex(
                name: "IX_PromoCodes_HotelId_Code",
                table: "PromoCodes");

            migrationBuilder.AlterColumn<int>(
                name: "HotelId",
                table: "PromoCodes",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.CreateIndex(
                name: "IX_PromoCodes_HotelId_Code",
                table: "PromoCodes",
                columns: new[] { "HotelId", "Code" },
                unique: true,
                filter: "[HotelId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_PromoCodes_Hotels_HotelId",
                table: "PromoCodes",
                column: "HotelId",
                principalTable: "Hotels",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PromoCodes_Hotels_HotelId",
                table: "PromoCodes");

            migrationBuilder.DropIndex(
                name: "IX_PromoCodes_HotelId_Code",
                table: "PromoCodes");

            migrationBuilder.AlterColumn<int>(
                name: "HotelId",
                table: "PromoCodes",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PromoCodes_HotelId_Code",
                table: "PromoCodes",
                columns: new[] { "HotelId", "Code" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_PromoCodes_Hotels_HotelId",
                table: "PromoCodes",
                column: "HotelId",
                principalTable: "Hotels",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
