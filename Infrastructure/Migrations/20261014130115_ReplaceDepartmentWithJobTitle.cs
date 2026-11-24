using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ReplaceDepartmentWithJobTitle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Department",
                table: "users",
                newName: "JobTitle");

            // Departments were free text; job titles are a fixed list, and the job title now decides the role.
            // Current administrators become Facilitair so they keep their rights. Everyone else keeps a department
            // that already is a job title, Marketing becomes Sales, and the rest (IT, Test, ...) becomes Ontwikkelaar 1.
            // A normal user never becomes Facilitair here, so nobody gains administrator rights by this migration.
            migrationBuilder.Sql("""
                UPDATE users SET JobTitle = CASE
                    WHEN RoleId = 1 THEN 'Facilitair'
                    WHEN JobTitle IN ('Meewerkstagiair', 'Afstudeerder', 'Ontwikkelaar 1', 'Ontwikkelaar 2', 'Teamlead',
                                      'Tech Expert', 'HR', 'Sales', 'Directeur') THEN JobTitle
                    WHEN JobTitle = 'Marketing' THEN 'Sales'
                    ELSE 'Ontwikkelaar 1'
                END;
                """);

            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-4000-8000-000000000001"),
                column: "JobTitle",
                value: "Facilitair");

            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-4000-8000-000000000002"),
                column: "JobTitle",
                value: "Ontwikkelaar 1");

            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-4000-8000-000000000003"),
                column: "JobTitle",
                value: "Sales");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "JobTitle",
                table: "users",
                newName: "Department");

            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-4000-8000-000000000001"),
                column: "Department",
                value: "IT");

            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-4000-8000-000000000002"),
                column: "Department",
                value: "IT");

            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-4000-8000-000000000003"),
                column: "Department",
                value: "Marketing");
        }
    }
}
