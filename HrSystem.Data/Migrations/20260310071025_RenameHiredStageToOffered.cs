using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HrSystem.Data.Migrations
{
    /// <inheritdoc />
    public partial class RenameHiredStageToOffered : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                UPDATE JobApplications
                SET Stage = 'Offered'
                WHERE Stage = 'Hired'
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                UPDATE JobApplications
                SET Stage = 'Hired'
                WHERE Stage = 'Offered'
                """);
        }
    }
}
