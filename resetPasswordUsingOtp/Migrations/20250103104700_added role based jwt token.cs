using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace resetPasswordUsingOtp.Migrations
{
    /// <inheritdoc />
    public partial class addedrolebasedjwttoken : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "2ea26a0b-f22d-45e8-af3a-1f34dcab7486", "2ea26a0b-f22d-45e8-af3a-1f34dcab7486", "User", "USER" },
                    { "87d2ef13-de5e-4877-aedc-75df20cad5a2", "87d2ef13-de5e-4877-aedc-75df20cad5a2", "Admin", "ADMIN" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "2ea26a0b-f22d-45e8-af3a-1f34dcab7486");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "87d2ef13-de5e-4877-aedc-75df20cad5a2");
        }
    }
}
