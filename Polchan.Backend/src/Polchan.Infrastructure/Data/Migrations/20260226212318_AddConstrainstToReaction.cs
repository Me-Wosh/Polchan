using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Polchan.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddConstrainstToReaction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Reactions_OwnerId",
                table: "Reactions");

            migrationBuilder.CreateIndex(
                name: "IX_Reactions_OwnerId_CommentId",
                table: "Reactions",
                columns: new[] { "OwnerId", "CommentId" },
                unique: true,
                filter: "CommentId IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Reactions_OwnerId_PostId",
                table: "Reactions",
                columns: new[] { "OwnerId", "PostId" },
                unique: true,
                filter: "PostId IS NOT NULL");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Reaction_PostOrComment",
                table: "Reactions",
                sql: "(PostId IS NOT NULL AND CommentId IS NULL) OR (PostId IS NULL AND CommentId IS NOT NULL)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Reactions_OwnerId_CommentId",
                table: "Reactions");

            migrationBuilder.DropIndex(
                name: "IX_Reactions_OwnerId_PostId",
                table: "Reactions");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Reaction_PostOrComment",
                table: "Reactions");

            migrationBuilder.CreateIndex(
                name: "IX_Reactions_OwnerId",
                table: "Reactions",
                column: "OwnerId");
        }
    }
}
