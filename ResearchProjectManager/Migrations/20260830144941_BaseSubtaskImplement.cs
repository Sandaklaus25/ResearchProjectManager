using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ResearchProjectManager.Migrations
{
    /// <inheritdoc />
    public partial class BaseSubtaskImplement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Subtasks_TaskAssignments_TaskAssignmentId",
                table: "Subtasks");

            migrationBuilder.AlterColumn<int>(
                name: "UserId",
                table: "Subtasks",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "TaskAssignmentId",
                table: "Subtasks",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Subtasks",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<int>(
                name: "ProjectTaskId",
                table: "Subtasks",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Subtasks_ProjectTaskId",
                table: "Subtasks",
                column: "ProjectTaskId");

            migrationBuilder.AddForeignKey(
                name: "FK_Subtasks_ProjectTasks_ProjectTaskId",
                table: "Subtasks",
                column: "ProjectTaskId",
                principalTable: "ProjectTasks",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Subtasks_TaskAssignments_TaskAssignmentId",
                table: "Subtasks",
                column: "TaskAssignmentId",
                principalTable: "TaskAssignments",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Subtasks_ProjectTasks_ProjectTaskId",
                table: "Subtasks");

            migrationBuilder.DropForeignKey(
                name: "FK_Subtasks_TaskAssignments_TaskAssignmentId",
                table: "Subtasks");

            migrationBuilder.DropIndex(
                name: "IX_Subtasks_ProjectTaskId",
                table: "Subtasks");

            migrationBuilder.DropColumn(
                name: "ProjectTaskId",
                table: "Subtasks");

            migrationBuilder.AlterColumn<int>(
                name: "UserId",
                table: "Subtasks",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "TaskAssignmentId",
                table: "Subtasks",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Subtasks",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Subtasks_TaskAssignments_TaskAssignmentId",
                table: "Subtasks",
                column: "TaskAssignmentId",
                principalTable: "TaskAssignments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
