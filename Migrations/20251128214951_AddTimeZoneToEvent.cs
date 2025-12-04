using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace COMP2138_ICE.Migrations
{
    /// <inheritdoc />
    public partial class AddTimeZoneToEvent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TimeZoneId",
                table: "Events",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "a18be9c0-aa65-4af8-bd17-00bd9344e575",
                columns: new[] { "ConcurrencyStamp", "PasswordHash" },
                values: new object[] { "28cf5eb8-1efc-45aa-a2cc-2acf9d1c90b6", "AQAAAAIAAYagAAAAEJCeamDx//UioaKjBa/G+tSxv6uxeeiAfHiBCW30q4ovGeYuy6LnWIPA69tW9doxsA==" });

            migrationBuilder.UpdateData(
                table: "Events",
                keyColumn: "Id",
                keyValue: 1,
                column: "TimeZoneId",
                value: "UTC");

            migrationBuilder.UpdateData(
                table: "Events",
                keyColumn: "Id",
                keyValue: 2,
                column: "TimeZoneId",
                value: "UTC");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TimeZoneId",
                table: "Events");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "a18be9c0-aa65-4af8-bd17-00bd9344e575",
                columns: new[] { "ConcurrencyStamp", "PasswordHash" },
                values: new object[] { "7aea3fd4-f148-482d-9ea2-709400b932fc", "AQAAAAIAAYagAAAAEG3rhHU+7FlHuN8GOf7pBao8O8MQGIV1jnUw3UIqqrg4jT+ONBtKrmWsM24AfQh3EA==" });
        }
    }
}
