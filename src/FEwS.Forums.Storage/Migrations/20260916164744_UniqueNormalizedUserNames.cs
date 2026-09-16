using Microsoft.EntityFrameworkCore.Migrations;

namespace FEwS.Forums.Storage.Migrations;

public partial class UniqueNormalizedUserNames : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            DO $$
            BEGIN
                IF EXISTS (
                    SELECT upper(btrim("UserName"))
                    FROM "Users"
                    WHERE "UserName" IS NOT NULL
                    GROUP BY upper(btrim("UserName"))
                    HAVING count(*) > 1
                ) THEN
                    RAISE EXCEPTION 'Duplicate normalized user names exist. Resolve conflicting accounts before applying UniqueNormalizedUserNames.';
                END IF;
            END $$;
            """);

        migrationBuilder.DropColumn(name: "NormalizedUserName", table: "Users");
        migrationBuilder.AddColumn<string>(
            name: "NormalizedUserName",
            table: "Users",
            type: "text",
            nullable: true,
            computedColumnSql: "upper(btrim(\"UserName\"))",
            stored: true);
        migrationBuilder.CreateIndex(
            name: "UX_Users_NormalizedUserName",
            table: "Users",
            column: "NormalizedUserName",
            unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(name: "UX_Users_NormalizedUserName", table: "Users");
        migrationBuilder.DropColumn(name: "NormalizedUserName", table: "Users");
        migrationBuilder.AddColumn<string>(
            name: "NormalizedUserName",
            table: "Users",
            type: "text",
            nullable: true);
    }
}
