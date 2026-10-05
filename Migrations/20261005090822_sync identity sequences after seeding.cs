using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERP.Migrations
{
    /// <inheritdoc />
    public partial class syncidentitysequencesafterseeding : Migration
    {
        /// <inheritdoc />
        // HasData inserts rows with explicit Ids, which doesn't advance Postgres identity
        // sequences. The next insert then reused Id 1 and failed on the primary key
        // (e.g. creating an account). Move each seeded table's sequence past its highest Id.
        private static readonly string[] SeededTables =
        [
            "UserRoles", "OrderTypes", "OrderStatuses", "InventoryStatuses", "InventoryLabels",
            "UserAccounts", "UserInformations"
        ];

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            foreach (var table in SeededTables)
            {
                migrationBuilder.Sql($"""
                    SELECT setval(
                        pg_get_serial_sequence('"{table}"', 'Id'),
                        COALESCE((SELECT MAX("Id") FROM "{table}"), 1),
                        EXISTS (SELECT 1 FROM "{table}"));
                    """);
            }
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
