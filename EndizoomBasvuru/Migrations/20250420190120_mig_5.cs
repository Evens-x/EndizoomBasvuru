using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EndizoomBasvuru.Migrations
{
    /// <inheritdoc />
    public partial class mig_5 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "UpdatedById",
                table: "Companies",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Assignments",
                table: "Admins",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Admins",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Assignments", "Password" },
                values: new object[] { null, "$2a$11$hkG4GbtgOaZeYsEYSfIeqO.xaUSOPnu7zcpZfrdAJeljhyOmAfpVi" });

            migrationBuilder.UpdateData(
                table: "Admins",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Assignments", "Password" },
                values: new object[] { null, "$2a$11$oUBM1y65hqoiVrJNhtHbM.m2Yby1TuB9jMpqCYu8381WeT80a3Utm" });

            migrationBuilder.CreateIndex(
                name: "IX_Companies_UpdatedById",
                table: "Companies",
                column: "UpdatedById");

            migrationBuilder.AddForeignKey(
                name: "FK_Companies_Admins_UpdatedById",
                table: "Companies",
                column: "UpdatedById",
                principalTable: "Admins",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Companies_Admins_UpdatedById",
                table: "Companies");

            migrationBuilder.DropIndex(
                name: "IX_Companies_UpdatedById",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "UpdatedById",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "Assignments",
                table: "Admins");

            migrationBuilder.UpdateData(
                table: "Admins",
                keyColumn: "Id",
                keyValue: 1,
                column: "Password",
                value: "$2a$11$5Op/syT/R.ovphHoytbS3eLr3DGzp1CtyxI95tp1llJuHRtbByI1W");

            migrationBuilder.UpdateData(
                table: "Admins",
                keyColumn: "Id",
                keyValue: 2,
                column: "Password",
                value: "$2a$11$bkPVEMy148dLaunknGO7fegLkgX4clgecDUenayf0JZL48KnBzZIC");
        }
    }
}
