using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace abcomm_test.Migrations
{
    /// <inheritdoc />
    public partial class RestrictStatusesToAllowedNames : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                UPDATE "CrmTasks" AS task
                SET status_id = NULL
                WHERE task.status_id IN (
                    SELECT id
                    FROM "Statuses"
                    WHERE name NOT IN ('Новий', 'В роботі', 'Виконано', 'Скасовано')
                );

                UPDATE "CrmTasks" AS task
                SET status_id = canonical.id
                FROM "Statuses" AS existing
                JOIN (
                    SELECT name, MIN(id) AS id
                    FROM "Statuses"
                    WHERE name IN ('Новий', 'В роботі', 'Виконано', 'Скасовано')
                    GROUP BY name
                ) AS canonical ON canonical.name = existing.name
                WHERE task.status_id = existing.id
                  AND existing.id <> canonical.id;

                DELETE FROM "Statuses" AS status
                WHERE status.name NOT IN ('Новий', 'В роботі', 'Виконано', 'Скасовано')
                   OR status.id <> (
                       SELECT MIN(allowed.id)
                       FROM "Statuses" AS allowed
                       WHERE allowed.name = status.name
                   );

                INSERT INTO "Statuses" (name)
                SELECT allowed.name
                FROM (VALUES ('Новий'), ('В роботі'), ('Виконано'), ('Скасовано'))
                    AS allowed(name)
                WHERE NOT EXISTS (
                    SELECT 1
                    FROM "Statuses" AS existing
                    WHERE existing.name = allowed.name
                );
                """);

            migrationBuilder.CreateIndex(
                name: "IX_Statuses_name",
                table: "Statuses",
                column: "name",
                unique: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_Statuses_Name_Allowed",
                table: "Statuses",
                sql: "\"name\" IN ('Новий', 'В роботі', 'Виконано', 'Скасовано')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Statuses_name",
                table: "Statuses");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Statuses_Name_Allowed",
                table: "Statuses");
        }
    }
}
