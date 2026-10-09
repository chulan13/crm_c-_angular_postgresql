using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace abcomm_test.Migrations
{
    /// <inheritdoc />
    public partial class AddStatusesAndCrmTaskStatusId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Statuses",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Statuses", x => x.id);
                });

            migrationBuilder.Sql("""
                INSERT INTO "Statuses" (name)
                SELECT DISTINCT status
                FROM "CrmTasks"
                WHERE status IS NOT NULL;
                """);

            migrationBuilder.AddColumn<int>(
                name: "status_id",
                table: "CrmTasks",
                type: "integer",
                nullable: true);

            migrationBuilder.Sql("""
                UPDATE "CrmTasks" AS task
                SET status_id = status.id
                FROM "Statuses" AS status
                WHERE task.status = status.name;
                """);

            migrationBuilder.DropColumn(
                name: "status",
                table: "CrmTasks");

            migrationBuilder.CreateIndex(
                name: "IX_CrmTasks_status_id",
                table: "CrmTasks",
                column: "status_id");

            migrationBuilder.AddForeignKey(
                name: "FK_CrmTasks_Statuses_status_id",
                table: "CrmTasks",
                column: "status_id",
                principalTable: "Statuses",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "status",
                table: "CrmTasks",
                type: "text",
                nullable: true);

            migrationBuilder.Sql("""
                UPDATE "CrmTasks" AS task
                SET status = status.name
                FROM "Statuses" AS status
                WHERE task.status_id = status.id;
                """);

            migrationBuilder.DropForeignKey(
                name: "FK_CrmTasks_Statuses_status_id",
                table: "CrmTasks");

            migrationBuilder.DropIndex(
                name: "IX_CrmTasks_status_id",
                table: "CrmTasks");

            migrationBuilder.DropColumn(
                name: "status_id",
                table: "CrmTasks");

            migrationBuilder.DropTable(
                name: "Statuses");
        }
    }
}
