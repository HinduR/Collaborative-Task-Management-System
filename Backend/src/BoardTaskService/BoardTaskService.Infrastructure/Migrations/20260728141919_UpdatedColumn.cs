using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BoardTaskService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdatedColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid[]>(
                name: "role_id",
                schema: "boardtask",
                table: "workflow_column",
                type: "uuid[]",
                nullable: false,
                defaultValue: new Guid[0]);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "role_id",
                schema: "boardtask",
                table: "workflow_column");
        }
    }
}
