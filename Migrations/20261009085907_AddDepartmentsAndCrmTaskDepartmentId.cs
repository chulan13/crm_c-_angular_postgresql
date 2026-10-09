using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace abcomm_test.Migrations
{
    /// <inheritdoc />
    public partial class AddDepartmentsAndCrmTaskDepartmentId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Departments",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Departments", x => x.id);
                });

            migrationBuilder.Sql("""
                INSERT INTO "Departments" (name)
                SELECT DISTINCT department
                FROM "CrmTasks"
                WHERE department IS NOT NULL;
                """);

            migrationBuilder.AddColumn<int>(
                name: "department_id",
                table: "CrmTasks",
                type: "integer",
                nullable: true);

            migrationBuilder.Sql("""
                UPDATE "CrmTasks" AS task
                SET department_id = department.id
                FROM "Departments" AS department
                WHERE task.department = department.name;
                """);

            migrationBuilder.DropColumn(
                name: "department",
                table: "CrmTasks");

            migrationBuilder.CreateIndex(
                name: "IX_CrmTasks_department_id",
                table: "CrmTasks",
                column: "department_id");

            migrationBuilder.AddForeignKey(
                name: "FK_CrmTasks_Departments_department_id",
                table: "CrmTasks",
                column: "department_id",
                principalTable: "Departments",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "department",
                table: "CrmTasks",
                type: "text",
                nullable: true);

            migrationBuilder.Sql("""
                UPDATE "CrmTasks" AS task
                SET department = department.name
                FROM "Departments" AS department
                WHERE task.department_id = department.id;
                """);

            migrationBuilder.DropForeignKey(
                name: "FK_CrmTasks_Departments_department_id",
                table: "CrmTasks");

            migrationBuilder.DropIndex(
                name: "IX_CrmTasks_department_id",
                table: "CrmTasks");

            migrationBuilder.DropColumn(
                name: "department_id",
                table: "CrmTasks");

            migrationBuilder.DropTable(
                name: "Departments");
        }
    }
}
