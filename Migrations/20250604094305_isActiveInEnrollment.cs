using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CourseManagement.Migrations
{
    /// <inheritdoc />
    public partial class isActiveInEnrollment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "isActive",
                table: "Enrollments",
                type: "bit",
                nullable: false,
                defaultValue: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "isActive",
                table: "Enrollments");
        }
    }
}
