using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace samplenpgsql.Migrations
{
    /// <inheritdoc />
    public partial class initDb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CONTRACTORS",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    NAME = table.Column<string>(type: "text", nullable: false),
                    INN = table.Column<int>(type: "integer", nullable: false),
                    L = table.Column<long>(type: "bigint", nullable: false),
                    CREATED = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DELETED = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    LONG_DATA = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CONTRACTORS", x => x.ID);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CONTRACTORS_CREATED",
                table: "CONTRACTORS",
                column: "CREATED");

            migrationBuilder.CreateIndex(
                name: "IX_CONTRACTORS_DELETED",
                table: "CONTRACTORS",
                column: "DELETED");

            migrationBuilder.CreateIndex(
                name: "IX_CONTRACTORS_INN",
                table: "CONTRACTORS",
                column: "INN");

            migrationBuilder.CreateIndex(
                name: "IX_CONTRACTORS_L",
                table: "CONTRACTORS",
                column: "L");

            migrationBuilder.CreateIndex(
                name: "IX_CONTRACTORS_LONGDATA",
                table: "CONTRACTORS",
                column: "LONG_DATA",
                filter: "CHAR_LENGTH(\"LONG_DATA\") > 0");

            migrationBuilder.CreateIndex(
                name: "IX_CONTRACTORS_NAME",
                table: "CONTRACTORS",
                column: "NAME");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CONTRACTORS");
        }
    }
}
