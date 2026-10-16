using ERP.Repository.Interface.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace ERP.Repository.Data
{
    public class BaseData(DatabaseContext _context) : IBaseData
    {
        public async Task Save<T>(T data, CancellationToken cancellation = default)
            where T : class
        {
            _context.Add(data);
            await _context.SaveChangesAsync(cancellation);
        }

        public async Task SaveMany<T>(IEnumerable<T> data, CancellationToken cancellation = default)
            where T : class
        {
            _context.AddRange(data);
            await _context.SaveChangesAsync(cancellation);
        }

        public async Task SaveChanges(CancellationToken cancellation = default)
        {
            await _context.SaveChangesAsync(cancellation);
        }

        // Marks a row for hard delete; the next SaveChanges removes it.
        public void Remove<T>(T data)
            where T : class
        {
            _context.Remove(data);
        }

        // Every data class shares the scoped context, so one transaction covers writes made through any of them.
        public async Task<IDbContextTransaction> BeginTransaction(CancellationToken cancellation = default)
        {
            return await _context.Database.BeginTransactionAsync(cancellation);
        }

        public IQueryable<T> BaseQuery<T>(bool withTracking)
            where T : class
        {
            var query = _context.Set<T>().AsQueryable();

            if (!withTracking)
            {
                query = query.AsNoTracking();
            }
            return query;
        }
    }
}
