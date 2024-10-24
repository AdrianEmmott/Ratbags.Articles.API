﻿using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ratbags.Articles.API.Migrations
{
    /// <inheritdoc />
    public partial class addedIntroField : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Introduction",
                table: "Articles",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Introduction",
                table: "Articles");
        }
    }
}
