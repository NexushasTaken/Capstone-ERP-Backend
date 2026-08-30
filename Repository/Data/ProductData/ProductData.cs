using ERP.Repository.Interface.Data.ProductData;
using ERP.Repository.Model.Products;
using Microsoft.EntityFrameworkCore;

namespace ERP.Repository.Data.ProductData
{
    public class ProductData(DatabaseContext _context) : BaseData(_context), IProductData
    {

        public IQueryable<Product> FilteringQuery(IQueryable<Product> query, int categoryPresent)
        {
            if(categoryPresent == 1)
            {
                query = query.Where(p => p.CategoryId == null);
            }

            return query;
        }

        public async Task<IEnumerable<Product>> GetAllProductWithoutTracking(int page, int pageSize, string? name, int categoryPresent)
        {
            var products = BaseQuery<Product>(false).Include(p => p.Category).Where(p => p.IsActive == true && p.Name.Contains(name));

            products = FilteringQuery(products,categoryPresent);

            var results = await products.Skip((page - 1) * pageSize).Take(pageSize).OrderByDescending(p => p.Created_At).ToListAsync();

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

        public async Task<int> ProductTotalCountWithoutTracking(string? name, int categoryPresent)
        {
            var count =  BaseQuery<Product>(false).Where(p => p.IsActive == true && p.Name.Contains(name));

             count = FilteringQuery(count, categoryPresent);

            var result = await count.CountAsync();

            return result;
        }
    }
}
