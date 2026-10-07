using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AfterStorm.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "matches",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ReplayHash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Map = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    GameMode = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    PlayedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    DurationSeconds = table.Column<int>(type: "integer", nullable: false),
                    GameBuild = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_matches", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "match_players",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MatchId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReplayPlayerId = table.Column<int>(type: "integer", nullable: false),
                    BattleTag = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    ToonHandle = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Hero = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Team = table.Column<int>(type: "integer", nullable: false),
                    Won = table.Column<bool>(type: "boolean", nullable: false),
                    Kills = table.Column<int>(type: "integer", nullable: true),
                    Deaths = table.Column<int>(type: "integer", nullable: true),
                    Assists = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_match_players", x => x.Id);
                    table.ForeignKey(
                        name: "FK_match_players_matches_MatchId",
                        column: x => x.MatchId,
                        principalTable: "matches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "talent_selections",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MatchPlayerId = table.Column<Guid>(type: "uuid", nullable: false),
                    Tier = table.Column<int>(type: "integer", nullable: false),
                    InternalName = table.Column<string>(type: "character varying(180)", maxLength: 180, nullable: false),
                    DisplayName = table.Column<string>(type: "character varying(180)", maxLength: 180, nullable: false),
                    GameLoop = table.Column<int>(type: "integer", nullable: false),
                    SelectedAtSeconds = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_talent_selections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_talent_selections_match_players_MatchPlayerId",
                        column: x => x.MatchPlayerId,
                        principalTable: "match_players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_match_players_MatchId",
                table: "match_players",
                column: "MatchId");

            migrationBuilder.CreateIndex(
                name: "IX_matches_ReplayHash",
                table: "matches",
                column: "ReplayHash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_talent_selections_MatchPlayerId",
                table: "talent_selections",
                column: "MatchPlayerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "talent_selections");

            migrationBuilder.DropTable(
                name: "match_players");

            migrationBuilder.DropTable(
                name: "matches");
        }
    }
}
