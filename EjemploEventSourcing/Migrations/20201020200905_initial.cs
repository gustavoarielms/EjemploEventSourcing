using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace EjemploEventSourcing.Migrations
{
    public partial class initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Aggregates",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    Type = table.Column<string>(nullable: true),
                    LastVersion = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Aggregates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Events",
                columns: table => new
                {
                    AggregateVersion = table.Column<int>(nullable: false),
                    AggregateId = table.Column<string>(nullable: false),
                    DateTimeCreate = table.Column<DateTime>(nullable: false),
                    MetaData = table.Column<string>(type: "jsonb", nullable: true),
                    AggregateData = table.Column<string>(type: "jsonb", nullable: true),
                    EventType = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Events", x => new { x.AggregateId, x.AggregateVersion });
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Aggregates");

            migrationBuilder.DropTable(
                name: "Events");
        }
    }
}
