using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace JobHunter.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitInfrastructure : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.CreateTable(
                name: "Materials",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Category = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AmsDesignation = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UnsDesignation = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsoDesignation = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Form = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TemperCondition = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Materials", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "JobMaterialLinks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    JobId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    MaterialId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobMaterialLinks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_JobMaterialLinks_JobLeads_JobId",
                        column: x => x.JobId,
                        principalTable: "JobLeads",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_JobMaterialLinks_Materials_MaterialId",
                        column: x => x.MaterialId,
                        principalTable: "Materials",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Materials",
                columns: new[] { "Id", "AmsDesignation", "Category", "Form", "IsoDesignation", "Name", "TemperCondition", "UnsDesignation" },
                values: new object[,]
                {
                    { 1, "AMS 4037", "Aluminum", "Sheet", "AlCu4Mg1", "Aluminum 2024-T3", "T3", "A92024" },
                    { 2, "AMS 4035", "Aluminum", "Plate", "AlCu4Mg1", "Aluminum 2024-T351", "T351", "A92024" },
                    { 3, "AMS 4027", "Aluminum", "Bar", "AlMg1SiCu", "Aluminum 6061-T6", "T6", "A96061" },
                    { 4, "AMS 4027", "Aluminum", "Plate", "AlMg1SiCu", "Aluminum 6061-T651", "T651", "A96061" },
                    { 5, "AMS 4045", "Aluminum", "Bar", "AlZn5.5MgCu", "Aluminum 7075-T6", "T6", "A97075" },
                    { 6, "AMS 4078", "Aluminum", "Plate", "AlZn5.5MgCu", "Aluminum 7075-T651", "T651", "A97075" },
                    { 7, "AMS 4050", "Aluminum", "Plate", "AlZn6CuMgZr", "Aluminum 7050-T7451", "T7451", "A97050" },
                    { 8, "AMS 4911", "Titanium", "Bar", "TiAl6V4", "Ti-6Al-4V (Grade 5)", "Annealed", "R56400" },
                    { 9, "AMS 4930", "Titanium", "Bar", "TiAl6V4 ELI", "Ti-6Al-4V ELI (Grade 23)", "Annealed", "R56401" },
                    { 10, "AMS 4902", "Titanium", "Sheet", "Ti-Gr2", "CP Titanium Grade 2", "Annealed", "R50400" },
                    { 11, "AMS 4928", "Titanium", "Forging", "TiAl6V4", "Ti-6Al-4V STA", "STA", "R56400" },
                    { 12, "AMS 5663", "Nickel", "Bar", "NiCr19Fe19Nb5Mo3", "Inconel 718", "Aged", "N07718" },
                    { 13, "AMS 5666", "Nickel", "Sheet", "NiCr22Mo9Nb", "Inconel 625", "Annealed", "N06625" },
                    { 14, "AMS 5544", "Nickel", "Forging", "NiCr20Co13Mo4Ti3Al", "Waspaloy", "Aged", "N07001" },
                    { 15, "AMS 5536", "Nickel", "Sheet", "NiCr22Fe18Mo", "Hastelloy X", "Solution Treated", "N06002" },
                    { 16, "AMS 4676", "Nickel", "Bar", "NiCu30Al", "Monel K-500", "Age Hardened", "N05500" },
                    { 17, "AMS 5513", "Steel", "Sheet", "X5CrNi18-10", "304 Stainless Steel", "Annealed", "S30400" },
                    { 18, "AMS 5507", "Steel", "Bar", "X2CrNiMo17-12-2", "316L Stainless Steel", "Annealed", "S31603" },
                    { 19, "AMS 5643", "Steel", "Bar", "X5CrNiCuNb16-4", "17-4 PH Stainless", "H900", "S17400" },
                    { 20, "AMS 5659", "Steel", "Bar", "X5CrNiCuNb15-5", "15-5 PH Stainless", "H1025", "S15500" },
                    { 21, "AMS 5731", "Steel", "Bar", "X5NiCrTi26-15", "A286 Iron-Based Super", "Aged", "S66286" },
                    { 22, "AMS 6414", "Steel", "Bar", "34CrNiMo6", "4340 Alloy Steel", "Quenched & Tempered", "G43400" },
                    { 23, "AMS 6370", "Steel", "Tube", "25CrMo4", "4130 Alloy Steel", "Normalized", "G41300" },
                    { 24, "AMS 6417", "Steel", "Bar", "300M", "300M Ultra-High Strength", "Quenched & Tempered", "K44220" },
                    { 25, null, "Copper", "Bar", "Cu-ETP", "Copper C110 (ETP)", "Half Hard", "C11000" },
                    { 26, "AMS 4533", "Copper", "Bar", "CuBe2", "Beryllium Copper C172", "AT", "C17200" },
                    { 27, "AMS 4375", "Magnesium", "Sheet", "MgAl3Zn1", "Magnesium AZ31B", "H24", "M11311" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_JobMaterialLinks_JobId_MaterialId",
                table: "JobMaterialLinks",
                columns: new[] { "JobId", "MaterialId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_JobMaterialLinks_MaterialId",
                table: "JobMaterialLinks",
                column: "MaterialId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "JobMaterialLinks");

            migrationBuilder.DropTable(
                name: "JobLeads");

            migrationBuilder.DropTable(
                name: "Materials");
        }
    }
}
