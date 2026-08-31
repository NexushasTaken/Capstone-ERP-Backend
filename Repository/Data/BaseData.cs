using ERP.Repository.Interface.Data;
using Microsoft.EntityFrameworkCore;

namespace ERP.Repository.Data
{
    public class BaseData(DatabaseContext _context) : IBaseData 
    {
        public async Task Save<T>(T data, CancellationToken cancellation = default) where T : class
        {
            _context.Add(data);
            await _context.SaveChangesAsync(cancellation);
        }
        public async Task SaveMany<T>(IEnumerable<T> data, CancellationToken cancellation = default) where T : class
        {
            _context.AddRange(data);
            await _context.SaveChangesAsync(cancellation);
        }
        public async Task SaveChanges(CancellationToken cancellation = default)
        {
            await _context.SaveChangesAsync(cancellation);
        }

        public IQueryable<T> BaseQuery<T>(bool withTracking) where T : class
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
