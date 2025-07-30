using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChandafyApp.Migrations.ChandafyDb
{
    /// <inheritdoc />
    public partial class FixUserRelationship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Budgets_Members_MemberId",
                table: "Budgets");


            migrationBuilder.DropForeignKey(
                name: "FK_ReceiptRequests_Members_ApprovedById",
                table: "ReceiptRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_ReceiptRequests_Members_RequestById",
                table: "ReceiptRequests");

            migrationBuilder.DropTable(
                name: "Members");


            migrationBuilder.DropIndex(
                name: "IX_ReceiptRequests_ApprovedById",
                table: "ReceiptRequests");

            migrationBuilder.DropIndex(
                name: "IX_ReceiptRequests_RequestById",
                table: "ReceiptRequests");

            migrationBuilder.DropIndex(
                name: "IX_Budgets_UserId",
                table: "Budgets");

            migrationBuilder.DropForeignKey(
               name: "FK_Budgets_ApplicationUser_UserId",
               table: "Budgets");


            migrationBuilder.DropTable(
               name: "ApplicationUser");

            
            migrationBuilder.DropIndex(
                name: "IX_Budgets_MemberId",
                table: "Budgets");

            

            migrationBuilder.DropColumn(
                name: "Year",
                table: "FiscalYears");

            migrationBuilder.DropColumn(
                name: "MemberId",
                table: "Budgets");

            migrationBuilder.AlterColumn<string>(
                name: "RequestById",
                table: "ReceiptRequests",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "ApprovedById",
                table: "ReceiptRequests",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

           

            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ContactNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AIMS = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateOfBirth = table.Column<DateTime>(type: "datetime2", nullable: true),
                    JamaatId = table.Column<int>(type: "int", nullable: true),
                    UserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SecurityStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "bit", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUsers_Jamaats_JamaatId",
                        column: x => x.JamaatId,
                        principalTable: "Jamaats",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderKey = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true,
                filter: "[NormalizedName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_JamaatId",
                table: "AspNetUsers",
                column: "JamaatId");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true,
                filter: "[NormalizedUserName] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "AspNetUsers");

          
            migrationBuilder.AlterColumn<int>(
                name: "RequestById",
                table: "ReceiptRequests",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "ApprovedById",
                table: "ReceiptRequests",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

           
            migrationBuilder.AddColumn<int>(
                name: "Year",
                table: "FiscalYears",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MemberId",
                table: "Budgets",
                type: "int",
                nullable: false,
                defaultValue: 0);
            migrationBuilder.CreateTable(
               name: "ApplicationUser",
               columns: table => new
               {
                   Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                   FirstName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                   LastName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                   ContactNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                   AIMS = table.Column<string>(type: "nvarchar(max)", nullable: true),
                   DateOfBirth = table.Column<DateTime>(type: "datetime2", nullable: true),
                   JamaatId = table.Column<int>(type: "int", nullable: true),
                   UserName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                   NormalizedUserName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                   Email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                   NormalizedEmail = table.Column<string>(type: "nvarchar(max)", nullable: true),
                   EmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
                   PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                   SecurityStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                   ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                   PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                   PhoneNumberConfirmed = table.Column<bool>(type: "bit", nullable: false),
                   TwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false),
                   LockoutEnd = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                   LockoutEnabled = table.Column<bool>(type: "bit", nullable: false),
                   AccessFailedCount = table.Column<int>(type: "int", nullable: false)
               },
               constraints: table =>
               {
                   table.PrimaryKey("PK_ApplicationUser", x => x.Id);
                   table.ForeignKey(
                       name: "FK_ApplicationUser_Jamaats_JamaatId",
                       column: x => x.JamaatId,
                       principalTable: "Jamaats",
                       principalColumn: "Id");
               });
            migrationBuilder.CreateTable(
                name: "Members",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdentityUserId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    JamaatId = table.Column<int>(type: "int", nullable: false),
                    AIMS = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ContactNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateOfBirth = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastName = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Members", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Members_IdentityUser_IdentityUserId",
                        column: x => x.IdentityUserId,
                        principalTable: "IdentityUser",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Members_Jamaats_JamaatId",
                        column: x => x.JamaatId,
                        principalTable: "Jamaats",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ReceiptRequests_ApprovedById",
                table: "ReceiptRequests",
                column: "ApprovedById");

            migrationBuilder.CreateIndex(
                name: "IX_ReceiptRequests_RequestById",
                table: "ReceiptRequests",
                column: "RequestById");

            migrationBuilder.CreateIndex(
                 name: "IX_Budgets_UserId",
                 table: "Budgets",
                 column: "UserId");
            migrationBuilder.CreateIndex(
                name: "IX_ApplicationUser_JamaatId",
                table: "ApplicationUser",
                column: "JamaatId");

            migrationBuilder.AddForeignKey(
                name: "FK_Budgets_ApplicationUser_UserId",
                table: "Budgets",
                column: "UserId",
                principalTable: "ApplicationUser",
                principalColumn: "Id");

          
            migrationBuilder.CreateIndex(
                name: "IX_Budgets_MemberId",
                table: "Budgets",
                column: "MemberId");

            migrationBuilder.CreateIndex(
                name: "IX_Members_IdentityUserId",
                table: "Members",
                column: "IdentityUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Members_JamaatId",
                table: "Members",
                column: "JamaatId");

            migrationBuilder.AddForeignKey(
                name: "FK_Budgets_Members_MemberId",
                table: "Budgets",
                column: "MemberId",
                principalTable: "Members",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);           

            migrationBuilder.AddForeignKey(
                name: "FK_ReceiptRequests_Members_ApprovedById",
                table: "ReceiptRequests",
                column: "ApprovedById",
                principalTable: "Members",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ReceiptRequests_Members_RequestById",
                table: "ReceiptRequests",
                column: "RequestById",
                principalTable: "Members",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
