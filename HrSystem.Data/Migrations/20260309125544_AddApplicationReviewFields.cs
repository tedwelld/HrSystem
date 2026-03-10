using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HrSystem.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddApplicationReviewFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ReviewReply",
                table: "ApplicationScorecards",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "ReviewedAtUtc",
                table: "ApplicationScorecards",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ReviewedByAdminId",
                table: "ApplicationScorecards",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TestScore",
                table: "ApplicationScorecards",
                type: "decimal(5,2)",
                precision: 5,
                scale: 2,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationScorecards_ReviewedByAdminId",
                table: "ApplicationScorecards",
                column: "ReviewedByAdminId");

            migrationBuilder.AddForeignKey(
                name: "FK_ApplicationScorecards_Users_ReviewedByAdminId",
                table: "ApplicationScorecards",
                column: "ReviewedByAdminId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ApplicationScorecards_Users_ReviewedByAdminId",
                table: "ApplicationScorecards");

            migrationBuilder.DropIndex(
                name: "IX_ApplicationScorecards_ReviewedByAdminId",
                table: "ApplicationScorecards");

            migrationBuilder.DropColumn(
                name: "ReviewReply",
                table: "ApplicationScorecards");

            migrationBuilder.DropColumn(
                name: "ReviewedAtUtc",
                table: "ApplicationScorecards");

            migrationBuilder.DropColumn(
                name: "ReviewedByAdminId",
                table: "ApplicationScorecards");

            migrationBuilder.DropColumn(
                name: "TestScore",
                table: "ApplicationScorecards");
        }
    }
}
