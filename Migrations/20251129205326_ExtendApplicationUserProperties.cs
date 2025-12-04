using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace COMP2138_ICE.Migrations
{
    /// <inheritdoc />
    public partial class ExtendApplicationUserProperties : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DateOfBirth",
                table: "AspNetUsers",
                type: "timestamp without time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "FullName",
                table: "AspNetUsers",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ProfilePictureUrl",
                table: "AspNetUsers",
                type: "text",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "a18be9c0-aa65-4af8-bd17-00bd9344e575",
                columns: new[] { "ConcurrencyStamp", "DateOfBirth", "FullName", "PasswordHash", "ProfilePictureUrl" },
                values: new object[] { "0a7974a7-02f8-4a81-bb9f-8fd76ee0b310", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "", "AQAAAAIAAYagAAAAED0PTQVS4MyF38hpU56zKRETWn1Rz3Hmep+YKMhOuQkLzoDt5PK1bq/DgyMPrs4bww==", null });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DateOfBirth",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "FullName",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "ProfilePictureUrl",
                table: "AspNetUsers");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "a18be9c0-aa65-4af8-bd17-00bd9344e575",
                columns: new[] { "ConcurrencyStamp", "PasswordHash" },
                values: new object[] { "28cf5eb8-1efc-45aa-a2cc-2acf9d1c90b6", "AQAAAAIAAYagAAAAEJCeamDx//UioaKjBa/G+tSxv6uxeeiAfHiBCW30q4ovGeYuy6LnWIPA69tW9doxsA==" });
        }
    }
}
