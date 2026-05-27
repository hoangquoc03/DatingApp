using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DatingApp.Migrations
{
    /// <inheritdoc />
    public partial class FinalizeDatabaseRelationsAndSwaggerFix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Matches_Users_UserOneId",
                table: "Matches");

            migrationBuilder.DropForeignKey(
                name: "FK_Matches_Users_UserTwoId",
                table: "Matches");

            migrationBuilder.DropIndex(
                name: "IX_Matches_UserOneId",
                table: "Matches");

            migrationBuilder.DropIndex(
                name: "IX_Matches_UserTwoId",
                table: "Matches");

            migrationBuilder.DropColumn(
                name: "UserOneId",
                table: "Matches");

            migrationBuilder.DropColumn(
                name: "UserTwoId",
                table: "Matches");

            migrationBuilder.CreateIndex(
                name: "IX_Matches_User2Id",
                table: "Matches",
                column: "User2Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Matches_Users_User1Id",
                table: "Matches",
                column: "User1Id",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Matches_Users_User2Id",
                table: "Matches",
                column: "User2Id",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Matches_Users_User1Id",
                table: "Matches");

            migrationBuilder.DropForeignKey(
                name: "FK_Matches_Users_User2Id",
                table: "Matches");

            migrationBuilder.DropIndex(
                name: "IX_Matches_User2Id",
                table: "Matches");

            migrationBuilder.AddColumn<Guid>(
                name: "UserOneId",
                table: "Matches",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "UserTwoId",
                table: "Matches",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Matches_UserOneId",
                table: "Matches",
                column: "UserOneId");

            migrationBuilder.CreateIndex(
                name: "IX_Matches_UserTwoId",
                table: "Matches",
                column: "UserTwoId");

            migrationBuilder.AddForeignKey(
                name: "FK_Matches_Users_UserOneId",
                table: "Matches",
                column: "UserOneId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Matches_Users_UserTwoId",
                table: "Matches",
                column: "UserTwoId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
