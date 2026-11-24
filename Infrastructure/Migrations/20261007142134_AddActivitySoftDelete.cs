using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddActivitySoftDelete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "activities",
                type: "TEXT",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "activities",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-4000-8000-000000000001"),
                column: "DeletedAt",
                value: null);

            migrationBuilder.UpdateData(
                table: "activities",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-4000-8000-000000000002"),
                column: "DeletedAt",
                value: null);

            migrationBuilder.UpdateData(
                table: "activities",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-4000-8000-000000000003"),
                column: "DeletedAt",
                value: null);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "activities");
        }
    }
}
