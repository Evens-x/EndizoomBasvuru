using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EndizoomBasvuru.Migrations
{
    /// <inheritdoc />
    public partial class mig_7 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "IsTemplate",
                table: "Companies",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "Admins",
                keyColumn: "Id",
                keyValue: 1,
                column: "Password",
                value: "$2a$11$lRfU4OwfuKWNpEY0BGOGwOdhqI6wQIpZlJXz.2criJauV1MJ9gEJ6");

            migrationBuilder.UpdateData(
                table: "Admins",
                keyColumn: "Id",
                keyValue: 2,
                column: "Password",
                value: "$2a$11$OaxRTrgpEF5IevYIxoA3O.k7iNqJ8TKeiMprBH8Od01NNAWOV4W3y");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsTemplate",
                table: "Companies");

            migrationBuilder.UpdateData(
                table: "Admins",
                keyColumn: "Id",
                keyValue: 1,
                column: "Password",
                value: "$2a$11$UKVZsxy3PZwCCalKvM98zOPp.otFLTwSIkEeUTk1ZRNMGuOGN3oW6");

            migrationBuilder.UpdateData(
                table: "Admins",
                keyColumn: "Id",
                keyValue: 2,
                column: "Password",
                value: "$2a$11$FSAATB3r3BEHRxAE4gP1LuMxZe1H99VAWefwFbWgPj9cIccpA7mq2");
        }
    }
}
