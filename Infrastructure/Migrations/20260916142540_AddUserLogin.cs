using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUserLogin : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PasswordHash",
                table: "users",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "Id",
                keyValue: 2,
                column: "PasswordHash",
                value: "100000.BYqjg4JzwylfnD6HkFduGw==.EOoQ6gFPxZyCm518hpKG1U5wdIoXnlGyfmY1BtbZbeI=");

            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "Id",
                keyValue: 3,
                column: "PasswordHash",
                value: "100000.BYqjg4JzwylfnD6HkFduGw==.EOoQ6gFPxZyCm518hpKG1U5wdIoXnlGyfmY1BtbZbeI=");

            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "Id",
                keyValue: 5,
                column: "PasswordHash",
                value: "100000.BYqjg4JzwylfnD6HkFduGw==.EOoQ6gFPxZyCm518hpKG1U5wdIoXnlGyfmY1BtbZbeI=");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PasswordHash",
                table: "users");
        }
    }
}
