using ERP.Repository.Configuration.Helper;
using ERP.Repository.Interface.Data.ProductData;
using ERP.Repository.Model.Products;
using ERP.Repository.ViewModel.Products;
using Microsoft.EntityFrameworkCore;

namespace ERP.Repository.Data.ProductData
{
    public class CategoryData(DatabaseContext _context) : BaseData(_context), ICategoryData
    {
        private IQueryable<Category> FilteringQuery(string? name)
        {
            var query = BaseQuery<Category>(false).Where(c => c.IsActive == true);

            if (!string.IsNullOrWhiteSpace(name))
            {
                var pattern = SearchPattern.Contains(name);
                query = query.Where(c => EF.Functions.ILike(c.Type, pattern));
            }

            return query;
        }

        // filter: 0 = newest first, 1 = Id, 2 = Type A-Z, 3 = Type Z-A. Ties fall back to Id so paging stays stable.
        private static IQueryable<Category> SortingQuery(IQueryable<Category> query, int filter)
        {
            var sorted = filter switch
            {
                1 => query.OrderBy(c => c.Id),
                2 => query.OrderBy(c => c.Type),
                3 => query.OrderByDescending(c => c.Type),
                _ => query.OrderByDescending(c => c.Created_At),
            };

            return sorted.ThenBy(c => c.Id);
        }

        public async Task<IEnumerable<Category>> GetAllCategoriesWithoutTracking(int page, int pageSize, string? name, int filter, CancellationToken cancellation = default)
        {
            var categories = await SortingQuery(FilteringQuery(name), filter).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellation);

            return categories;
        }

        public async Task<int> CategoryTotalCount(string? name, CancellationToken cancellation)
        {
            return await FilteringQuery(name).CountAsync(cancellation);
        }

        public async Task<Category> GetCategoryByIdWithTracking(int id, CancellationToken cancellation = default)
        {
            var category = await BaseQuery<Category>(true).FirstOrDefaultAsync(c => c.Id == id && c.IsActive == true, cancellation);
            return category;
        }
    }
}   