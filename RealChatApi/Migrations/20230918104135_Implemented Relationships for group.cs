using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RealChatApi.Migrations
{
    /// <inheritdoc />
    public partial class ImplementedRelationshipsforgroup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ApplicationUserGroup_AspNetUsers_MembersId",
                table: "ApplicationUserGroup");

            migrationBuilder.DropForeignKey(
                name: "FK_ApplicationUserGroup_Groups_GroupsId",
                table: "ApplicationUserGroup");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ApplicationUserGroup",
                table: "ApplicationUserGroup");

            migrationBuilder.RenameTable(
                name: "ApplicationUserGroup",
                newName: "UserGroups");

            migrationBuilder.RenameIndex(
                name: "IX_ApplicationUserGroup_MembersId",
                table: "UserGroups",
                newName: "IX_UserGroups_MembersId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserGroups",
                table: "UserGroups",
                columns: new[] { "GroupsId", "MembersId" });

            migrationBuilder.AddForeignKey(
                name: "FK_UserGroups_AspNetUsers_MembersId",
                table: "UserGroups",
                column: "MembersId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserGroups_Groups_GroupsId",
                table: "UserGroups",
                column: "GroupsId",
                principalTable: "Groups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserGroups_AspNetUsers_MembersId",
                table: "UserGroups");

            migrationBuilder.DropForeignKey(
                name: "FK_UserGroups_Groups_GroupsId",
                table: "UserGroups");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserGroups",
                table: "UserGroups");

            migrationBuilder.RenameTable(
                name: "UserGroups",
                newName: "ApplicationUserGroup");

            migrationBuilder.RenameIndex(
                name: "IX_UserGroups_MembersId",
                table: "ApplicationUserGroup",
                newName: "IX_ApplicationUserGroup_MembersId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ApplicationUserGroup",
                table: "ApplicationUserGroup",
                columns: new[] { "GroupsId", "MembersId" });

            migrationBuilder.AddForeignKey(
                name: "FK_ApplicationUserGroup_AspNetUsers_MembersId",
                table: "ApplicationUserGroup",
                column: "MembersId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ApplicationUserGroup_Groups_GroupsId",
                table: "ApplicationUserGroup",
                column: "GroupsId",
                principalTable: "Groups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
