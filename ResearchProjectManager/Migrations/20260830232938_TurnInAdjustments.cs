using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ResearchProjectManager.Migrations
{
    /// <inheritdoc />
    public partial class TurnInAdjustments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CreatorId",
                table: "Subtasks",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsBaseTask",
                table: "Subtasks",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "LastStatusUpdaterId",
                table: "Subtasks",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TaskAssignmentId",
                table: "Attachments",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Subtasks_CreatorId",
                table: "Subtasks",
                column: "CreatorId");

            migrationBuilder.CreateIndex(
                name: "IX_Subtasks_LastStatusUpdaterId",
                table: "Subtasks",
                column: "LastStatusUpdaterId");

            migrationBuilder.CreateIndex(
                name: "IX_Attachments_TaskAssignmentId",
                table: "Attachments",
                column: "TaskAssignmentId");

            migrationBuilder.AddForeignKey(
                name: "FK_Attachments_TaskAssignments_TaskAssignmentId",
                table: "Attachments",
                column: "TaskAssignmentId",
                principalTable: "TaskAssignments",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Subtasks_AspNetUsers_CreatorId",
                table: "Subtasks",
                column: "CreatorId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Subtasks_AspNetUsers_LastStatusUpdaterId",
                table: "Subtasks",
                column: "LastStatusUpdaterId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Attachments_TaskAssignments_TaskAssignmentId",
                table: "Attachments");

            migrationBuilder.DropForeignKey(
                name: "FK_Subtasks_AspNetUsers_CreatorId",
                table: "Subtasks");

            migrationBuilder.DropForeignKey(
                name: "FK_Subtasks_AspNetUsers_LastStatusUpdaterId",
                table: "Subtasks");

            migrationBuilder.DropIndex(
                name: "IX_Subtasks_CreatorId",
                table: "Subtasks");

            migrationBuilder.DropIndex(
                name: "IX_Subtasks_LastStatusUpdaterId",
                table: "Subtasks");

            migrationBuilder.DropIndex(
                name: "IX_Attachments_TaskAssignmentId",
                table: "Attachments");

            migrationBuilder.DropColumn(
                name: "CreatorId",
                table: "Subtasks");

            migrationBuilder.DropColumn(
                name: "IsBaseTask",
                table: "Subtasks");

            migrationBuilder.DropColumn(
                name: "LastStatusUpdaterId",
                table: "Subtasks");

            migrationBuilder.DropColumn(
                name: "TaskAssignmentId",
                table: "Attachments");
        }
    }
}
