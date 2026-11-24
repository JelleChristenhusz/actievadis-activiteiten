using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "activities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Location = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: false),
                    StartTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    EndTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IncludesFood = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false),
                    Cost = table.Column<decimal>(type: "decimal(10,2)", nullable: false, defaultValue: 0m),
                    MaxParticipants = table.Column<int>(type: "INTEGER", nullable: true),
                    MinParticipants = table.Column<int>(type: "INTEGER", nullable: true),
                    ImageUrl = table.Column<string>(type: "TEXT", nullable: true),
                    Requirements = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_activities", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Department = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "TEXT", maxLength: 320, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "registrations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ActivityId = table.Column<int>(type: "INTEGER", nullable: false),
                    UserId = table.Column<int>(type: "INTEGER", nullable: false),
                    Comment = table.Column<string>(type: "TEXT", nullable: true),
                    RegisteredAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_registrations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_registrations_activities_ActivityId",
                        column: x => x.ActivityId,
                        principalTable: "activities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_registrations_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "activities",
                columns: new[] { "Id", "Cost", "Description", "EndTime", "ImageUrl", "IncludesFood", "Location", "MaxParticipants", "MinParticipants", "Name", "Requirements", "StartTime" },
                values: new object[,]
                {
                    { 1, 5.00m, "Overnight LAN party met snacks en toernooien.", new DateTime(2024, 2, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, "Headquarters - Room A", 50, 5, "LAN Party", "Bring your own PC and ethernet cable", new DateTime(2024, 1, 31, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 2, 3.50m, "Gezellige pizza avond met bordspellen.", new DateTime(2024, 1, 11, 3, 0, 0, 0, DateTimeKind.Utc), null, true, "Community Kitchen", 20, 4, "Pizza Avond", "Let us know dietary restrictions", new DateTime(2024, 1, 11, 0, 0, 0, 0, DateTimeKind.Utc) }
                });

            migrationBuilder.InsertData(
                table: "activities",
                columns: new[] { "Id", "Description", "EndTime", "ImageUrl", "Location", "MaxParticipants", "MinParticipants", "Name", "Requirements", "StartTime" },
                values: new object[] { 3, "Vrijwillige hardloop training voor alle niveaus.", new DateTime(2024, 1, 6, 19, 0, 0, 0, DateTimeKind.Utc), null, "Stadspark", 30, 2, "Hardloop Sessie", "Sportschoenen aanbevolen", new DateTime(2024, 1, 6, 18, 0, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.InsertData(
                table: "users",
                columns: new[] { "Id", "Department", "Email", "Name" },
                values: new object[,]
                {
                    { 2, "IT", "sanne.jansen@actievadis.nl", "Sanne Jansen" },
                    { 3, "Marketing", "daan.devries@actievadis.nl", "Daan de Vries" },
                    { 5, "HR", "mila.bakker@actievadis.nl", "Mila Bakker" }
                });

            migrationBuilder.InsertData(
                table: "registrations",
                columns: new[] { "Id", "ActivityId", "Comment", "RegisteredAt", "UserId" },
                values: new object[,]
                {
                    { 1, 1, "Breng m'n headset mee", new DateTime(2024, 1, 16, 0, 0, 0, 0, DateTimeKind.Utc), 2 },
                    { 2, 1, "Doe mee aan FPS toernooi", new DateTime(2024, 1, 17, 0, 0, 0, 0, DateTimeKind.Utc), 3 },
                    { 3, 2, "Vegetarisch graag", new DateTime(2024, 1, 8, 0, 0, 0, 0, DateTimeKind.Utc), 5 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_registrations_ActivityId",
                table: "registrations",
                column: "ActivityId");

            migrationBuilder.CreateIndex(
                name: "IX_registrations_UserId",
                table: "registrations",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_users_Email",
                table: "users",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "registrations");

            migrationBuilder.DropTable(
                name: "activities");

            migrationBuilder.DropTable(
                name: "users");
        }
    }
}
