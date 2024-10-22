using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ratbags.Articles.API.Migrations
{
    /// <inheritdoc />
    public partial class changeFieldNameToBannerUrl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ImageUrl",
                table: "Articles",
                newName: "BannerImageUrl");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "BannerImageUrl",
                table: "Articles",
                newName: "ImageUrl");
        }
    }
}
