using System;
using GiftOfTheGivers.Web.Data.Relief;
using Microsoft.EntityFrameworkCore;

namespace GiftOfTheGivers.Web.Data.Relief;

internal sealed class DemoReliefManagementContext
    : ReliefManagementContext
{
    public DemoReliefManagementContext(
        DbContextOptions<ReliefManagementContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Replace SQL Server date/time default expressions with
        // SQLite-compatible expressions. Other SQL Server-only
        // defaults are removed for this demonstration context only.
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            foreach (var property in entityType.GetProperties())
            {
                var sqlDefault = property.GetDefaultValueSql();

                if (string.IsNullOrWhiteSpace(sqlDefault))
                {
                    continue;
                }

                if (sqlDefault.Contains(
                        "sysutcdatetime",
                        StringComparison.OrdinalIgnoreCase) ||
                    sqlDefault.Contains(
                        "getutcdate",
                        StringComparison.OrdinalIgnoreCase) ||
                    sqlDefault.Contains(
                        "sysdatetime",
                        StringComparison.OrdinalIgnoreCase) ||
                    sqlDefault.Contains(
                        "getdate",
                        StringComparison.OrdinalIgnoreCase))
                {
                    property.SetDefaultValueSql("CURRENT_TIMESTAMP");
                }
                else
                {
                    property.SetDefaultValueSql(null);
                }
            }
        }
    }
}
