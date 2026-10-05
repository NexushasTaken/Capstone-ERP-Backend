using ERP.Repository.Configuration.Helper;
using ERP.Repository.Interface.Data.ProductData;
using ERP.Repository.Model.Products;
using Microsoft.EntityFrameworkCore;

namespace ERP.Repository.Data.ProductData
{
    public class ProductData(DatabaseContext _context) : BaseData(_context), IProductData
    {

        // categoryPresent: 0 = only products with a category, 1 = only products without one.
        public IQueryable<Product> FilteringQuery(IQueryable<Product> query, string? name, int categoryPresent)
        {
            query = query.Where(p => p.IsActive == true);

            if (!string.IsNullOrWhiteSpace(name))
            {
                var pattern = SearchPattern.Contains(name);
                query = query.Where(p => EF.Functions.ILike(p.Name, pattern));
            }

            query = categoryPresent == 1
                ? query.Where(p => p.CategoryId == null)
                : query.Where(p => p.CategoryId != null);

            return query;
        }

        // filter: 0 = newest first, 1 = Id, 2 = Name A-Z, 3 = Name Z-A, 4 = Price low-high, 5 = Price high-low.
        // Ties fall back to Id so paging stays stable.
        private static IQueryable<Product> SortingQuery(IQueryable<Product> query, int filter)
        {
            var sorted = filter switch
            {
                1 => query.OrderBy(p => p.Id),
                2 => query.OrderBy(p => p.Name),
                3 => query.OrderByDescending(p => p.Name),
                4 => query.OrderBy(p => p.Price),
                5 => query.OrderByDescending(p => p.Price),
                _ => query.OrderByDescending(p => p.Created_At),
            };

            return sorted.ThenBy(p => p.Id);
        }

        public async Task<IEnumerable<Product>> GetAllProductWithoutTracking(int page, int pageSize, string? name, int categoryPresent, int filter)
        {
            var products = FilteringQuery(BaseQuery<Product>(false).Include(p => p.Category), name, categoryPresent);

            // Sort before paging, so the order holds across pages.
            var results = await SortingQuery(products, filter).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            return results;
        }

        public async Task<ICollection<Product>> GetAllProductReferenceByCategoryWithTracking(int categoryId)
        {
            var products = await BaseQuery<Product>(true).Where(p => p.CategoryId == categoryId && p.IsActive == true).OrderByDescending(p => p.Id).ToListAsync();

            return products;
        }

        public async Task<Product> GetProductByIdWithTracking(int id)
        {
            var product = await BaseQuery<Product>(true).FirstOrDefaultAsync(p => p.Id == id && p.IsActive == true);

            return product;
        }
        public async Task<Product> GetProductByIdWithoutTracking(int id)
        {
            var product = await BaseQuery<Product>(false).FirstOrDefaultAsync(p => p.Id == id && p.IsActive == true);

            return product;
        }

        public async Task<int> ProductTotalCountWithoutTracking(string? name, int categoryPresent)
        {
            return await FilteringQuery(BaseQuery<Product>(false), name, categoryPresent).CountAsync();
        }

        public async Task<IEnumerable<Product>> GetAllProductForInventoryInsert()
        {
            var product = await BaseQuery<Product>(false)
                .Include(p => p.Category)
                .Where(p => p.IsActive == true)
                .ToListAsync();

            return product;
        }
    }
}
