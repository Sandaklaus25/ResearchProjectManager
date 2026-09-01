using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ResearchProjectManager.Migrations
{
    /// <inheritdoc />
    public partial class UpdateAssignmentArchitecture : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Attachments_AspNetUsers_UserId",
                table: "Attachments");

            migrationBuilder.DropForeignKey(
                name: "FK_Attachments_TaskAssignments_TaskAssignmentId",
                table: "Attachments");

            migrationBuilder.DropIndex(
                name: "IX_Attachments_TaskAssignmentId",
                table: "Attachments");

            migrationBuilder.DropColumn(
                name: "TaskAssignmentId",
                table: "Attachments");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "Attachments",
                newName: "CommentId");

            migrationBuilder.RenameIndex(
                name: "IX_Attachments_UserId",
                table: "Attachments",
                newName: "IX_Attachments_CommentId");

            migrationBuilder.AddColumn<int>(
                name: "CourseId",
                table: "Teams",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Teams_CourseId",
                table: "Teams",
                column: "CourseId");

            migrationBuilder.AddForeignKey(
                name: "FK_Attachments_Comments_CommentId",
                table: "Attachments",
                column: "CommentId",
                principalTable: "Comments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Teams_Courses_CourseId",
                table: "Teams",
                column: "CourseId",
                principalTable: "Courses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Attachments_Comments_CommentId",
                table: "Attachments");

            migrationBuilder.DropForeignKey(
                name: "FK_Teams_Courses_CourseId",
                table: "Teams");

            migrationBuilder.DropIndex(
                name: "IX_Teams_CourseId",
                table: "Teams");

            migrationBuilder.DropColumn(
                name: "CourseId",
                table: "Teams");

            migrationBuilder.RenameColumn(
                name: "CommentId",
                table: "Attachments",
                newName: "UserId");

            migrationBuilder.RenameIndex(
                name: "IX_Attachments_CommentId",
                table: "Attachments",
                newName: "IX_Attachments_UserId");

            migrationBuilder.AddColumn<int>(
                name: "TaskAssignmentId",
                table: "Attachments",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Attachments_TaskAssignmentId",
                table: "Attachments",
                column: "TaskAssignmentId");

            migrationBuilder.AddForeignKey(
                name: "FK_Attachments_AspNetUsers_UserId",
                table: "Attachments",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Attachments_TaskAssignments_TaskAssignmentId",
                table: "Attachments",
                column: "TaskAssignmentId",
                principalTable: "TaskAssignments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
