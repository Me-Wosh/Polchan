using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Polchan.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSoftDeletableEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "SoftDeleted",
                table: "Threads",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "SoftDeleted",
                table: "Posts",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "SoftDeleted",
                table: "Comments",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SoftDeleted",
                table: "Threads");

            migrationBuilder.DropColumn(
                name: "SoftDeleted",
                table: "Posts");

            migrationBuilder.DropColumn(
                name: "SoftDeleted",
                table: "Comments");
        }
    }
}
