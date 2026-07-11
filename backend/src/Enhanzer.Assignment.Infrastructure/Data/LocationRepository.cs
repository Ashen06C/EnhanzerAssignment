using Enhanzer.Assignment.Application.Locations;
using Enhanzer.Assignment.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Enhanzer.Assignment.Infrastructure.Data;

public sealed class LocationRepository(AssignmentDbContext dbContext) : ILocationRepository
{
    private const string EnsureLocationDetailsTableSql = """
        IF OBJECT_ID(N'dbo.Location_Details', N'U') IS NULL
        BEGIN
            CREATE TABLE dbo.Location_Details
            (
                Id BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Location_Details PRIMARY KEY,
                Location_Code NVARCHAR(50) NOT NULL,
                Location_Name NVARCHAR(200) NOT NULL,
                Created_At_Utc DATETIME2 NOT NULL,
                Updated_At_Utc DATETIME2 NOT NULL
            );
        END;

        IF NOT EXISTS
        (
            SELECT 1
            FROM sys.indexes
            WHERE name = N'UX_Location_Details_Location_Code'
              AND object_id = OBJECT_ID(N'dbo.Location_Details')
        )
        BEGIN
            CREATE UNIQUE INDEX UX_Location_Details_Location_Code
                ON dbo.Location_Details(Location_Code);
        END;
        """;

    public async Task<IReadOnlyCollection<LocationDto>> GetAllAsync(CancellationToken cancellationToken) =>
        await dbContext.LocationDetails
            .AsNoTracking()
            .OrderBy(location => location.LocationName)
            .Select(location => new LocationDto(location.LocationCode, location.LocationName))
            .ToListAsync(cancellationToken);

    public Task<bool> ExistsAsync(string locationCode, CancellationToken cancellationToken) =>
        dbContext.LocationDetails.AnyAsync(location => location.LocationCode == locationCode, cancellationToken);

    public async Task UpsertAsync(IEnumerable<LocationDto> locations, CancellationToken cancellationToken)
    {
        var distinctLocations = locations
            .Where(location => !string.IsNullOrWhiteSpace(location.LocationCode))
            .GroupBy(location => location.LocationCode.Trim(), StringComparer.OrdinalIgnoreCase)
            .Select(group => group.Last())
            .ToList();

        if (distinctLocations.Count == 0)
        {
            return;
        }

        await dbContext.Database.ExecuteSqlRawAsync(EnsureLocationDetailsTableSql, cancellationToken);

        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
        var codes = distinctLocations.Select(location => location.LocationCode.Trim()).ToList();
        var existingLocations = await dbContext.LocationDetails
            .Where(location => codes.Contains(location.LocationCode))
            .ToDictionaryAsync(location => location.LocationCode, StringComparer.OrdinalIgnoreCase, cancellationToken);

        var now = DateTime.UtcNow;
        foreach (var location in distinctLocations)
        {
            var code = location.LocationCode.Trim();
            var name = location.LocationName.Trim();

            if (existingLocations.TryGetValue(code, out var existing))
            {
                existing.LocationName = name;
                existing.UpdatedAtUtc = now;
            }
            else
            {
                dbContext.LocationDetails.Add(new LocationDetail
                {
                    LocationCode = code,
                    LocationName = name,
                    CreatedAtUtc = now,
                    UpdatedAtUtc = now
                });
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }
}
