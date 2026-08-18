using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StreamFlix.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddUserRole : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // defaultValue "User" (no ""): así, cualquier fila existente antes de esta
            // migración queda con un rol válido del enum UserRole en vez de un string
            // vacío que fallaría al deserializarse la primera vez que EF la lea.
            migrationBuilder.AddColumn<string>(
                name: "Role",
                table: "Users",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "User");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Role",
                table: "Users");
        }
    }
}
