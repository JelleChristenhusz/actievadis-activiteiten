using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations;

/// <summary>Converts integer keys to stable GUIDs without replacing application data.</summary>
public partial class UseGuidEntityIdsAndEnglishRoles : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Copy parents and children within the migration transaction. Entity-specific prefixes
        // and unsigned 32-bit suffixes preserve every old key, including custom rows.
        // GUID casing matches EF Core's SQLite parameters. Foreign keys stay enabled;
        // renaming parents updates references in the new child table.
        migrationBuilder.Sql("""
            CREATE TABLE "activities_guid" (
                "Id" TEXT NOT NULL CONSTRAINT "PK_activities" PRIMARY KEY,
                "Name" TEXT NOT NULL, "Location" TEXT NOT NULL, "Description" TEXT NOT NULL,
                "StartTime" TEXT NOT NULL, "EndTime" TEXT NOT NULL,
                "IncludesFood" INTEGER NOT NULL DEFAULT 0,
                "Cost" decimal(10,2) NOT NULL DEFAULT '0.0',
                "MaxParticipants" INTEGER NULL, "MinParticipants" INTEGER NULL,
                "ImageUrl" TEXT NULL, "Requirements" TEXT NULL
            );
            CREATE TABLE "users_guid" (
                "Id" TEXT NOT NULL CONSTRAINT "PK_users" PRIMARY KEY,
                "Name" TEXT NOT NULL, "Department" TEXT NOT NULL, "Email" TEXT NOT NULL,
                "PasswordHash" TEXT NOT NULL, "RoleId" INTEGER NOT NULL,
                CONSTRAINT "FK_users_roles_RoleId" FOREIGN KEY ("RoleId")
                    REFERENCES "roles" ("Id") ON DELETE RESTRICT
            );
            CREATE TABLE "registrations_guid" (
                "Id" TEXT NOT NULL CONSTRAINT "PK_registrations" PRIMARY KEY,
                "ActivityId" TEXT NOT NULL, "UserId" TEXT NOT NULL,
                "Comment" TEXT NULL, "RegisteredAt" TEXT NOT NULL,
                CONSTRAINT "FK_registrations_activities_ActivityId" FOREIGN KEY ("ActivityId")
                    REFERENCES "activities_guid" ("Id") ON DELETE CASCADE,
                CONSTRAINT "FK_registrations_users_UserId" FOREIGN KEY ("UserId")
                    REFERENCES "users_guid" ("Id") ON DELETE CASCADE
            );

            INSERT INTO "activities_guid"
                ("Id", "Name", "Location", "Description", "StartTime", "EndTime", "IncludesFood",
                 "Cost", "MaxParticipants", "MinParticipants", "ImageUrl", "Requirements")
            SELECT '10000000-0000-4000-8000-' || printf('%012X', "Id" & 4294967295),
                "Name", "Location", "Description", "StartTime", "EndTime", "IncludesFood",
                "Cost", "MaxParticipants", "MinParticipants", "ImageUrl", "Requirements"
            FROM "activities";

            INSERT INTO "users_guid" ("Id", "Name", "Department", "Email", "PasswordHash", "RoleId")
            SELECT '20000000-0000-4000-8000-' || printf('%012X', "Id" & 4294967295),
                "Name", "Department", "Email", "PasswordHash", "RoleId"
            FROM "users";

            INSERT INTO "registrations_guid" ("Id", "ActivityId", "UserId", "Comment", "RegisteredAt")
            SELECT '30000000-0000-4000-8000-' || printf('%012X', "Id" & 4294967295),
                '10000000-0000-4000-8000-' || printf('%012X', "ActivityId" & 4294967295),
                '20000000-0000-4000-8000-' || printf('%012X', "UserId" & 4294967295),
                "Comment", "RegisteredAt"
            FROM "registrations";

            DROP TABLE "registrations";
            DROP TABLE "users";
            DROP TABLE "activities";
            ALTER TABLE "activities_guid" RENAME TO "activities";
            ALTER TABLE "users_guid" RENAME TO "users";
            ALTER TABLE "registrations_guid" RENAME TO "registrations";

            CREATE UNIQUE INDEX "IX_users_Email" ON "users" ("Email");
            CREATE INDEX "IX_users_RoleId" ON "users" ("RoleId");
            CREATE INDEX "IX_registrations_ActivityId" ON "registrations" ("ActivityId");
            CREATE INDEX "IX_registrations_UserId" ON "registrations" ("UserId");

            UPDATE "roles" SET "Name" = 'Administrator' WHERE "Name" = 'Beheerder';

            -- Upgrade only the unchanged demo hash; never reset a changed password.
            UPDATE "users"
            SET "PasswordHash" = 'AQAAAAIAAzRQAAAAEAzwOW9EbuKg9OnBsmsNm476hyrb9xt5nTbSE8qchnaHeB9G1uz0KAeaFLQOdlXsfQ=='
            WHERE "Id" IN (
                '20000000-0000-4000-8000-000000000001', '20000000-0000-4000-8000-000000000002',
                '20000000-0000-4000-8000-000000000003', '20000000-0000-4000-8000-000000000005')
              AND "PasswordHash" = '100000.BYqjg4JzwylfnD6HkFduGw==.EOoQ6gFPxZyCm518hpKG1U5wdIoXnlGyfmY1BtbZbeI=';
            """);
    }

    /// <summary>Requires a backup because newly generated GUIDs have no original integer key.</summary>
    protected override void Down(MigrationBuilder migrationBuilder) =>
        throw new NotSupportedException(
            "GUID keys cannot safely be converted back to their original integer keys. Restore the pre-migration database backup.");
}
