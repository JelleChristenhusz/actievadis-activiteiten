using Microsoft.EntityFrameworkCore.Migrations;

namespace Infrastructure.Migrations;

public partial class AddActivityGuests : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<bool>(name: "AllowsGuest", table: "activities", type: "INTEGER", nullable: false, defaultValue: false);
        migrationBuilder.AddColumn<bool>(name: "BringsGuest", table: "registrations", type: "INTEGER", nullable: false, defaultValue: false);
    }
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(name: "BringsGuest", table: "registrations");
        migrationBuilder.DropColumn(name: "AllowsGuest", table: "activities");
    }
}
