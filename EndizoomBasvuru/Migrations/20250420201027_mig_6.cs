using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EndizoomBasvuru.Migrations
{
    /// <inheritdoc />
    public partial class mig_6 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CompanyNumber",
                table: "Admins",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PhoneNumber",
                table: "Admins",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Admins",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CompanyNumber", "Password", "PhoneNumber" },
                values: new object[] { null, "$2a$11$UKVZsxy3PZwCCalKvM98zOPp.otFLTwSIkEeUTk1ZRNMGuOGN3oW6", null });

            migrationBuilder.UpdateData(
                table: "Admins",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CompanyNumber", "Password", "PhoneNumber" },
                values: new object[] { null, "$2a$11$FSAATB3r3BEHRxAE4gP1LuMxZe1H99VAWefwFbWgPj9cIccpA7mq2", null });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CompanyNumber",
                table: "Admins");

            migrationBuilder.DropColumn(
                name: "PhoneNumber",
                table: "Admins");

            migrationBuilder.UpdateData(
                table: "Admins",
                keyColumn: "Id",
                keyValue: 1,
                column: "Password",
                value: "$2a$11$hkG4GbtgOaZeYsEYSfIeqO.xaUSOPnu7zcpZfrdAJeljhyOmAfpVi");

            migrationBuilder.UpdateData(
                table: "Admins",
                keyColumn: "Id",
                keyValue: 2,
                column: "Password",
                value: "$2a$11$oUBM1y65hqoiVrJNhtHbM.m2Yby1TuB9jMpqCYu8381WeT80a3Utm");
        }
    }
}
