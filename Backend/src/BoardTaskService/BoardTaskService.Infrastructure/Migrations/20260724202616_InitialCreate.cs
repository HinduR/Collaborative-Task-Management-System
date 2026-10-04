using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BoardTaskService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "boardtask");

            migrationBuilder.CreateTable(
                name: "project",
                schema: "boardtask",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: false),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_project", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "board",
                schema: "boardtask",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    project_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: false),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_board", x => x.id);
                    table.ForeignKey(
                        name: "fk_board_project_project_id",
                        column: x => x.project_id,
                        principalSchema: "boardtask",
                        principalTable: "project",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_project_mapping",
                schema: "boardtask",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    project_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: false),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_project_mapping", x => x.id);
                    table.ForeignKey(
                        name: "fk_user_project_mapping_project_project_id",
                        column: x => x.project_id,
                        principalSchema: "boardtask",
                        principalTable: "project",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "workflow_column",
                schema: "boardtask",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    board_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    sort_order = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: false),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_workflow_column", x => x.id);
                    table.ForeignKey(
                        name: "fk_workflow_column_board_board_id",
                        column: x => x.board_id,
                        principalSchema: "boardtask",
                        principalTable: "board",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "board_access",
                schema: "boardtask",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    board_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_project_mapping_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: false),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_board_access", x => x.id);
                    table.ForeignKey(
                        name: "fk_board_access_board_board_id",
                        column: x => x.board_id,
                        principalSchema: "boardtask",
                        principalTable: "board",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_board_access_user_project_mapping_user_project_mapping_id",
                        column: x => x.user_project_mapping_id,
                        principalSchema: "boardtask",
                        principalTable: "user_project_mapping",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "board_task",
                schema: "boardtask",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    workflow_column_id = table.Column<Guid>(type: "uuid", nullable: false),
                    assignee_board_access_id = table.Column<Guid>(type: "uuid", nullable: true),
                    priority_id = table.Column<Guid>(type: "uuid", nullable: false),
                    task_type_id = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: false),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_board_task", x => x.id);
                    table.ForeignKey(
                        name: "fk_board_task_board_access_assignee_board_access_id",
                        column: x => x.assignee_board_access_id,
                        principalSchema: "boardtask",
                        principalTable: "board_access",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_board_task_workflow_column_workflow_column_id",
                        column: x => x.workflow_column_id,
                        principalSchema: "boardtask",
                        principalTable: "workflow_column",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "task_comment",
                schema: "boardtask",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    task_id = table.Column<Guid>(type: "uuid", nullable: false),
                    comment = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: false),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_task_comment", x => x.id);
                    table.ForeignKey(
                        name: "fk_task_comment_board_task_task_id",
                        column: x => x.task_id,
                        principalSchema: "boardtask",
                        principalTable: "board_task",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_board_project_id_name",
                schema: "boardtask",
                table: "board",
                columns: new[] { "project_id", "name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_board_access_board_id",
                schema: "boardtask",
                table: "board_access",
                column: "board_id");

            migrationBuilder.CreateIndex(
                name: "ix_board_access_user_project_mapping_id",
                schema: "boardtask",
                table: "board_access",
                column: "user_project_mapping_id");

            migrationBuilder.CreateIndex(
                name: "ix_board_task_assignee_board_access_id",
                schema: "boardtask",
                table: "board_task",
                column: "assignee_board_access_id");

            migrationBuilder.CreateIndex(
                name: "ix_board_task_workflow_column_id",
                schema: "boardtask",
                table: "board_task",
                column: "workflow_column_id");

            migrationBuilder.CreateIndex(
                name: "ix_task_comment_task_id",
                schema: "boardtask",
                table: "task_comment",
                column: "task_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_project_mapping_project_id",
                schema: "boardtask",
                table: "user_project_mapping",
                column: "project_id");

            migrationBuilder.CreateIndex(
                name: "ix_workflow_column_board_id_name",
                schema: "boardtask",
                table: "workflow_column",
                columns: new[] { "board_id", "name" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "task_comment",
                schema: "boardtask");

            migrationBuilder.DropTable(
                name: "board_task",
                schema: "boardtask");

            migrationBuilder.DropTable(
                name: "board_access",
                schema: "boardtask");

            migrationBuilder.DropTable(
                name: "workflow_column",
                schema: "boardtask");

            migrationBuilder.DropTable(
                name: "user_project_mapping",
                schema: "boardtask");

            migrationBuilder.DropTable(
                name: "board",
                schema: "boardtask");

            migrationBuilder.DropTable(
                name: "project",
                schema: "boardtask");
        }
    }
}
