using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace COMP2138_ICE.Migrations
{
    /// <inheritdoc />
    public partial class CompleteRatingSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "RedeemedAt",
                table: "Tickets",
                type: "timestamp without time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Ratings",
                type: "timestamp without time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AddColumn<int>(
                name: "PurchaseId",
                table: "Ratings",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<DateTime>(
                name: "PurchaseDate",
                table: "Purchases",
                type: "timestamp without time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<DateTime>(
                name: "DateTime",
                table: "Events",
                type: "timestamp without time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "a18be9c0-aa65-4af8-bd17-00bd9344e575",
                columns: new[] { "ConcurrencyStamp", "PasswordHash" },
                values: new object[] { "7aea3fd4-f148-482d-9ea2-709400b932fc", "AQAAAAIAAYagAAAAEG3rhHU+7FlHuN8GOf7pBao8O8MQGIV1jnUw3UIqqrg4jT+ONBtKrmWsM24AfQh3EA==" });

            migrationBuilder.CreateIndex(
                name: "IX_Ratings_PurchaseId",
                table: "Ratings",
                column: "PurchaseId");

            migrationBuilder.AddForeignKey(
                name: "FK_Ratings_Purchases_PurchaseId",
                table: "Ratings",
                column: "PurchaseId",
                principalTable: "Purchases",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Ratings_Purchases_PurchaseId",
                table: "Ratings");

            migrationBuilder.DropIndex(
                name: "IX_Ratings_PurchaseId",
                table: "Ratings");

            migrationBuilder.DropColumn(
                name: "PurchaseId",
                table: "Ratings");

            migrationBuilder.AlterColumn<DateTime>(
                name: "RedeemedAt",
                table: "Tickets",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Ratings",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone");

            migrationBuilder.AlterColumn<DateTime>(
                name: "PurchaseDate",
                table: "Purchases",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone");

            migrationBuilder.AlterColumn<DateTime>(
                name: "DateTime",
                table: "Events",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "a18be9c0-aa65-4af8-bd17-00bd9344e575",
                columns: new[] { "ConcurrencyStamp", "PasswordHash" },
                values: new object[] { "c607df8b-7ef0-402f-8b2a-48e9f3b9ec3a", "AQAAAAIAAYagAAAAEFUEzZgKZetnmpaVLmjf1gIJ/S6eq1N32CCTaLOgCQQoiKWqOihmQ4pb2cnVmWOHIA==" });
        }
    }
}
