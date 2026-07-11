using Enhanzer.Assignment.Application.PurchaseBills;
using Enhanzer.Assignment.Domain.Entities;

namespace Enhanzer.Assignment.Infrastructure.Data;

public sealed class PurchaseBillRepository(AssignmentDbContext dbContext) : IPurchaseBillRepository
{
    public async Task AddAsync(PurchaseBill bill, CancellationToken cancellationToken)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
        dbContext.PurchaseBills.Add(bill);
        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }
}
