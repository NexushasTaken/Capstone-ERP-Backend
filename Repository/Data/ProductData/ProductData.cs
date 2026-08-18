using ERP.Repository.Interface.Data.ProductData;
using ERP.Repository.Model.Products;
using Microsoft.EntityFrameworkCore;

namespace ERP.Repository.Data.ProductData
{
    public class ProductData(DatabaseContext _context) : BaseData(_context), IProductData
    {
        public async Task<IEnumerable<Product>> GetAllProductWithoutTracking()
        {
            var products = await BaseQuery<Product>(false).Include(p => p.Category).ToListAsync();

            return products;
        }
    }
}
