using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddVisitorRegistrations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_registrations_ActivityId",
                table: "registrations");

            migrationBuilder.AlterColumn<Guid>(
                name: "UserId",
                table: "registrations",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "TEXT");

            migrationBuilder.AddColumn<string>(
                name: "CancellationCodeHash",
                table: "registrations",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VisitorEmail",
                table: "registrations",
                type: "TEXT",
                maxLength: 320,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VisitorName",
                table: "registrations",
                type: "TEXT",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VisitorPhone",
                table: "registrations",
                type: "TEXT",
                maxLength: 30,
                nullable: true);

            migrationBuilder.UpdateData(
                table: "registrations",
                keyColumn: "Id",
                keyValue: new Guid("30000000-0000-4000-8000-000000000001"),
                columns: new[] { "CancellationCodeHash", "VisitorEmail", "VisitorName", "VisitorPhone" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "registrations",
                keyColumn: "Id",
                keyValue: new Guid("30000000-0000-4000-8000-000000000002"),
                columns: new[] { "CancellationCodeHash", "VisitorEmail", "VisitorName", "VisitorPhone" },
                values: new object[] { null, null, null, null });

            migrationBuilder.UpdateData(
                table: "registrations",
                keyColumn: "Id",
                keyValue: new Guid("30000000-0000-4000-8000-000000000003"),
                columns: new[] { "CancellationCodeHash", "VisitorEmail", "VisitorName", "VisitorPhone" },
                values: new object[] { null, null, null, null });

            migrationBuilder.CreateIndex(
                name: "IX_registrations_ActivityId_VisitorEmail",
                table: "registrations",
                columns: new[] { "ActivityId", "VisitorEmail" },
                unique: true,
                filter: "\"VisitorEmail\" IS NOT NULL");

            migrationBuilder.AddCheckConstraint(
                name: "CK_registrations_participant",
                table: "registrations",
                sql: "\"UserId\" IS NOT NULL OR (\"VisitorName\" IS NOT NULL AND \"VisitorEmail\" IS NOT NULL AND \"VisitorPhone\" IS NOT NULL AND \"CancellationCodeHash\" IS NOT NULL)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Without the visitor columns a registration needs a user again, so visitor registrations cannot stay.
            migrationBuilder.Sql("DELETE FROM registrations WHERE \"UserId\" IS NULL;");

            migrationBuilder.DropIndex(
                name: "IX_registrations_ActivityId_VisitorEmail",
                table: "registrations");

            migrationBuilder.DropCheckConstraint(
                name: "CK_registrations_participant",
                table: "registrations");

            migrationBuilder.DropColumn(
                name: "CancellationCodeHash",
                table: "registrations");

            migrationBuilder.DropColumn(
                name: "VisitorEmail",
                table: "registrations");

            migrationBuilder.DropColumn(
                name: "VisitorName",
                table: "registrations");

            migrationBuilder.DropColumn(
                name: "VisitorPhone",
                table: "registrations");

            migrationBuilder.AlterColumn<Guid>(
                name: "UserId",
                table: "registrations",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_registrations_ActivityId",
                table: "registrations",
                column: "ActivityId");
        }
    }
}
