using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KioskAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddPlaceOfBirthToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PlaceOfBirth",
                table: "Users",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PlaceOfBirth",
                table: "Users");
        }
    }
}
