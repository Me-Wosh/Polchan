using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Polchan.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddThreadCategoryAndThreadSubscriptionsEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Category",
                table: "Threads",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "ThreadSubscriptions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SubscriberId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ThreadId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ThreadSubscriptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ThreadSubscriptions_Threads_ThreadId",
                        column: x => x.ThreadId,
                        principalTable: "Threads",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ThreadSubscriptions_Users_SubscriberId",
                        column: x => x.SubscriberId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ThreadSubscriptions_SubscriberId_ThreadId",
                table: "ThreadSubscriptions",
                columns: new[] { "SubscriberId", "ThreadId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ThreadSubscriptions_ThreadId",
                table: "ThreadSubscriptions",
                column: "ThreadId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ThreadSubscriptions");

            migrationBuilder.DropColumn(
                name: "Category",
                table: "Threads");
        }
    }
}
