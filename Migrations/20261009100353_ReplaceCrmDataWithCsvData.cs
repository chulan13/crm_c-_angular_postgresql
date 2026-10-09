using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace abcomm_test.Migrations
{
    public partial class ReplaceCrmDataWithCsvData : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "CrmTasks");
            migrationBuilder.DropTable(name: "Departments");
            migrationBuilder.DropTable(name: "Statuses");

            migrationBuilder.CreateTable(
                name: "Departments",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table => table.PrimaryKey("PK_Departments", x => x.id));

            migrationBuilder.CreateTable(
                name: "Statuses",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Statuses", x => x.id);
                    table.CheckConstraint(
                        "CK_Statuses_Name_Allowed",
                        "\"name\" IN ('Новий', 'В роботі', 'Виконано', 'Скасовано')");
                });

            migrationBuilder.CreateTable(
                name: "CrmTasks",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "text", nullable: true),
                    status_id = table.Column<Guid>(type: "uuid", nullable: true),
                    department_id = table.Column<Guid>(type: "uuid", nullable: true),
                    assignee = table.Column<string>(type: "text", nullable: true),
                    deadline = table.Column<DateOnly>(type: "date", nullable: true),
                    description = table.Column<string>(type: "text", nullable: true),
                    cancel_reason = table.Column<string>(
                        type: "character varying(250)",
                        maxLength: 250,
                        nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CrmTasks", x => x.id);
                    table.ForeignKey(
                        name: "FK_CrmTasks_Departments_department_id",
                        column: x => x.department_id,
                        principalTable: "Departments",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CrmTasks_Statuses_status_id",
                        column: x => x.status_id,
                        principalTable: "Statuses",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CrmTasks_department_id",
                table: "CrmTasks",
                column: "department_id");
            migrationBuilder.CreateIndex(
                name: "IX_CrmTasks_status_id",
                table: "CrmTasks",
                column: "status_id");
            migrationBuilder.CreateIndex(
                name: "IX_Statuses_name",
                table: "Statuses",
                column: "name",
                unique: true);

            migrationBuilder.InsertData(
                table: "Departments",
                columns: new[] { "id", "name" },
                values: new object[,]
                {
                    { new Guid("d2c9f095-8e4c-498a-802a-68c220dbe1bd"), "Відділ закупівель" },
                    { new Guid("1a6ad5b0-d24f-4ea5-9a10-43d3c0733a7d"), "Юридичний відділ" },
                    { new Guid("01778524-fe84-4630-a256-8eb579801ffd"), "Бухгалтерія" },
                    { new Guid("b8fa87dc-ea29-4ce1-b0c1-b713a1cd2087"), "Технічна підтримка" },
                    { new Guid("8ac0b78c-ff12-4df7-b6da-545d1ea95924"), "Відділ логістики" },
                    { new Guid("50ca67f6-1c24-48b4-ad12-4e6a2a87b7a9"), "Служба безпеки" }
                });

            migrationBuilder.InsertData(
                table: "Statuses",
                columns: new[] { "id", "name" },
                values: new object[,]
                {
                    { new Guid("5acbe650-d38a-4821-90da-ec8e3a7b0649"), "Новий" },
                    { new Guid("9f58ec81-9be2-4cd6-8a90-895b1386766b"), "В роботі" },
                    { new Guid("bef204f0-906f-46d8-903d-083cb42919b5"), "Виконано" },
                    { new Guid("d0b962a0-dd67-4c30-87b2-104419f3bb19"), "Скасовано" }
                });

            migrationBuilder.InsertData(
                table: "CrmTasks",
                columns: new[]
                {
                    "id", "name", "status_id", "department_id", "assignee",
                    "deadline", "description", "cancel_reason"
                },
                values: new object[,]
                {
                    { new Guid("ccd1e516-da30-4d18-93c8-b97581df29e5"), "Закрити заміну картриджів на 3 поверсі", new Guid("5acbe650-d38a-4821-90da-ec8e3a7b0649"), new Guid("01778524-fe84-4630-a256-8eb579801ffd"), "Ткаченко Ю.", null, "Запит від керівника відділу, деталі у листуванні.", null },
                    { new Guid("e9fb58e6-9bb5-435b-a821-4c022becd15b"), "Підготувати перелік постачальників на IV квартал", new Guid("9f58ec81-9be2-4cd6-8a90-895b1386766b"), new Guid("50ca67f6-1c24-48b4-ad12-4e6a2a87b7a9"), "Савченко Т.", new DateOnly(2026, 8, 29), "Залежить від відповіді контрагента.", null },
                    { new Guid("eba7b3d2-1e1a-4186-9023-c953d6a0aa06"), "Закрити звірку залишків на складі №2", new Guid("9f58ec81-9be2-4cd6-8a90-895b1386766b"), new Guid("01778524-fe84-4630-a256-8eb579801ffd"), "Шевчук А.", new DateOnly(2026, 9, 4), "Матеріали передані на папері.", null },
                    { new Guid("2cbab9d4-de3c-48cb-90a4-012b9e2baa99"), "Оновити перелік ключів від серверної", new Guid("5acbe650-d38a-4821-90da-ec8e3a7b0649"), new Guid("d2c9f095-8e4c-498a-802a-68c220dbe1bd"), null, null, "Запит від керівника відділу, деталі у листуванні.", null },
                    { new Guid("66ad6f36-97c1-4424-8313-a9d318b97590"), "Описати заявку на канцелярію", new Guid("9f58ec81-9be2-4cd6-8a90-895b1386766b"), new Guid("50ca67f6-1c24-48b4-ad12-4e6a2a87b7a9"), null, new DateOnly(2026, 8, 21), "Терміново, під контролем директора.", null },
                    { new Guid("48d3ae83-d3d5-4b9c-b236-bd6dadfd80e5"), "Закрити доступ до спільної папки відділу", new Guid("bef204f0-906f-46d8-903d-083cb42919b5"), new Guid("50ca67f6-1c24-48b4-ad12-4e6a2a87b7a9"), "Романюк С.", null, "Залежить від відповіді контрагента.", null },
                    { new Guid("b7189cec-127f-49b5-b5ec-557d09c17ff0"), "Підготувати графік відпусток", new Guid("d0b962a0-dd67-4c30-87b2-104419f3bb19"), new Guid("1a6ad5b0-d24f-4ea5-9a10-43d3c0733a7d"), "Лозова Н.", null, "Матеріали передані на папері.", "Бюджет на квартал закрито." },
                    { new Guid("621dc6ab-7bd2-4a52-aa3b-aaba596b05c3"), "Передати звірку залишків на складі №2", new Guid("d0b962a0-dd67-4c30-87b2-104419f3bb19"), new Guid("01778524-fe84-4630-a256-8eb579801ffd"), "Романюк С.", new DateOnly(2026, 8, 15), null, "Дубль задачі 'Узгодити рахунок', закрили ту." },
                    { new Guid("b64ff404-24e3-4e6d-af32-2b32c81cdd8d"), "Передати графік відпусток", new Guid("9f58ec81-9be2-4cd6-8a90-895b1386766b"), new Guid("1a6ad5b0-d24f-4ea5-9a10-43d3c0733a7d"), "Ткаченко Ю.", null, null, null },
                    { new Guid("214e3fee-8f52-4eb4-95ba-f5577ab84c07"), "Підготувати видаткові накладні за вересень", new Guid("d0b962a0-dd67-4c30-87b2-104419f3bb19"), new Guid("01778524-fe84-4630-a256-8eb579801ffd"), "Лозова Н.", new DateOnly(2026, 6, 23), "Терміново, під контролем директора.", "Втратило актуальність після зміни умов договору." },
                    { new Guid("da05c86c-4be1-4414-8775-18242a233606"), "Підготувати комплект документів для тендера", new Guid("bef204f0-906f-46d8-903d-083cb42919b5"), new Guid("1a6ad5b0-d24f-4ea5-9a10-43d3c0733a7d"), "Ковальчук О.", new DateOnly(2026, 10, 10), "Матеріали передані на папері.", null },
                    { new Guid("b39e8e09-c108-487c-a55d-b630c0618f5b"), "Оновити інструкцію з прийому заявок", new Guid("5acbe650-d38a-4821-90da-ec8e3a7b0649"), new Guid("1a6ad5b0-d24f-4ea5-9a10-43d3c0733a7d"), "Шевчук А.", new DateOnly(2026, 9, 1), "Матеріали передані на папері.", null },
                    { new Guid("34f2f312-8c14-48f6-bcd1-f6814c5a4bf7"), "Перевірити заявку на канцелярію", new Guid("bef204f0-906f-46d8-903d-083cb42919b5"), new Guid("d2c9f095-8e4c-498a-802a-68c220dbe1bd"), null, null, "Планова робота, без зовнішніх залежностей.", null },
                    { new Guid("549f80c2-900d-4194-ae8c-c46ecf4ab755"), "Порахувати доступ до спільної папки відділу", new Guid("5acbe650-d38a-4821-90da-ec8e3a7b0649"), new Guid("01778524-fe84-4630-a256-8eb579801ffd"), "Ткаченко Ю.", new DateOnly(2026, 8, 3), "Повторна задача, минулого разу не закрили.", null },
                    { new Guid("4ebad770-36dd-4ced-90e1-fcfac8fc6ef4"), "Закрити страхування вантажу до Рівного", new Guid("9f58ec81-9be2-4cd6-8a90-895b1386766b"), new Guid("1a6ad5b0-d24f-4ea5-9a10-43d3c0733a7d"), null, new DateOnly(2026, 11, 1), "Запит від керівника відділу, деталі у листуванні.", null }
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "CrmTasks");
            migrationBuilder.DropTable(name: "Departments");
            migrationBuilder.DropTable(name: "Statuses");

            migrationBuilder.CreateTable(
                name: "Departments",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table => table.PrimaryKey("PK_Departments", x => x.id));

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
                    table.CheckConstraint(
                        "CK_Statuses_Name_Allowed",
                        "\"name\" IN ('Новий', 'В роботі', 'Виконано', 'Скасовано')");
                });

            migrationBuilder.CreateTable(
                name: "CrmTasks",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "text", nullable: true),
                    status_id = table.Column<int>(type: "integer", nullable: true),
                    department_id = table.Column<int>(type: "integer", nullable: true),
                    assignee = table.Column<string>(type: "text", nullable: true),
                    deadline = table.Column<DateOnly>(type: "date", nullable: true),
                    description = table.Column<string>(type: "text", nullable: true),
                    cancel_reason = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CrmTasks", x => x.id);
                    table.ForeignKey(
                        name: "FK_CrmTasks_Departments_department_id",
                        column: x => x.department_id,
                        principalTable: "Departments",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CrmTasks_Statuses_status_id",
                        column: x => x.status_id,
                        principalTable: "Statuses",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex("IX_CrmTasks_department_id", "CrmTasks", "department_id");
            migrationBuilder.CreateIndex("IX_CrmTasks_status_id", "CrmTasks", "status_id");
            migrationBuilder.CreateIndex("IX_Statuses_name", "Statuses", "name", unique: true);
        }
    }
}
