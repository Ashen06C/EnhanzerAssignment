using Enhanzer.Assignment.Domain.Entities;

namespace Enhanzer.Assignment.Application.PurchaseBills;

public interface IPurchaseBillRepository
{
    Task AddAsync(PurchaseBill bill, CancellationToken cancellationToken);
}
