using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTestAdministrator : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "users",
                columns: new[] { "Id", "Department", "Email", "Name", "PasswordHash", "RoleId" },
                values: new object[] { 1, "IT", "test@actievadis.nl", "Test Beheerder", "100000.BYqjg4JzwylfnD6HkFduGw==.EOoQ6gFPxZyCm518hpKG1U5wdIoXnlGyfmY1BtbZbeI=", 1 });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "users",
                keyColumn: "Id",
                keyValue: 1);
        }
    }
}
