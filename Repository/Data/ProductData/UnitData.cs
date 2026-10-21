using ERP.Repository.Configuration.Helper;
using ERP.Repository.Interface.Data.ProductData;
using ERP.Repository.Model.Products;
using ERP.Repository.ViewModel.Products;
using Microsoft.EntityFrameworkCore;

namespace ERP.Repository.Data.ProductData
{
    public class UnitData(DatabaseContext _context) : BaseData(_context), IUnitData
    {
        private IQueryable<Unit> FilteringQuery(string? name)
        {
            var query = BaseQuery<Unit>(false).Where(u => u.IsActive);

            if (!string.IsNullOrWhiteSpace(name))
            {
                var pattern = SearchPattern.Contains(name);
                var hasId = int.TryParse(name.Trim(), out var id);

                query = query.Where(u => EF.Functions.ILike(u.Name, pattern) || (hasId && u.Id == id));
            }

            return query;
        }

        // filter: 0 = newest first, 1 = Id, 2 = Name A-Z, 3 = Name Z-A. Ties fall back to Id so paging stays stable.
        private static IQueryable<Unit> SortingQuery(IQueryable<Unit> query, int filter)
        {
            var sorted = filter switch
            {
                1 => query.OrderBy(u => u.Id),
                2 => query.OrderBy(u => u.Name),
                3 => query.OrderByDescending(u => u.Name),
                _ => query.OrderByDescending(u => u.Created_At),
            };

            return sorted.ThenBy(u => u.Id);
        }

        public async Task<IEnumerable<UnitViewModel>> GetAllUnitsWithoutTracking(
            int page,
            int pageSize,
            string? name,
            int filter,
            CancellationToken cancellation = default
        )
        {
            return await SortingQuery(FilteringQuery(name), filter)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(u => new UnitViewModel
                {
                    Id = u.Id,
                    Name = u.Name,
                    ProductCount = u.Products.Count(p => p.IsActive),
                    Created_At = u.Created_At,
                })
                .ToListAsync(cancellation);
        }

        public async Task<int> UnitTotalCount(string? name, CancellationToken cancellation = default)
        {
            return await FilteringQuery(name).CountAsync(cancellation);
        }

        public async Task<Unit?> GetUnitByIdWithTracking(int id)
        {
            return await BaseQuery<Unit>(true).FirstOrDefaultAsync(u => u.Id == id && u.IsActive);
        }

        public async Task<bool> UnitExists(int id)
        {
            return await BaseQuery<Unit>(false).AnyAsync(u => u.Id == id && u.IsActive);
        }

        // Case-insensitive; excludeId skips the unit being renamed.
        public async Task<bool> CheckExistingUnit(string name, int excludeId = 0)
        {
            var lower = name.Trim().ToLower();

            return await BaseQuery<Unit>(false)
                .AnyAsync(u => u.IsActive && u.Id != excludeId && u.Name.ToLower() == lower);
        }

        public async Task<int> ProductCount(int unitId)
        {
            return await BaseQuery<Product>(false).CountAsync(p => p.UnitId == unitId && p.IsActive);
        }

        // Active units whose name matches one of names (case-insensitive), keyed by lowercase name.
        public async Task<Dictionary<string, int>> GetUnitIdsByNames(IEnumerable<string> names)
        {
            var lower = names.Select(n => n.ToLower()).Distinct().ToList();

            var matches = await BaseQuery<Unit>(false)
                .Where(u => u.IsActive && lower.Contains(u.Name.ToLower()))
                .Select(u => new { u.Id, u.Name })
                .ToListAsync();

            return matches.GroupBy(u => u.Name.ToLower()).ToDictionary(g => g.Key, g => g.First().Id);
        }
    }
}
