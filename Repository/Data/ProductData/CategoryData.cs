using ERP.Repository.Interface.Data.ProductData;
using ERP.Repository.Model.Products;
using ERP.Repository.ViewModel.Product;
using Microsoft.EntityFrameworkCore;

namespace ERP.Repository.Data.ProductData
{
    public class CategoryData(DatabaseContext _context) : BaseData(_context), ICategoryData
    {
        public async Task<IEnumerable<Category>> GetAllCategoriesWithoutTracking(CancellationToken cancellation = default)
        {
            var categories = await BaseQuery<Category>(false).Where(c => c.IsActive == true).ToListAsync(cancellation);

            return categories;
        }

        public async Task<IEnumerable<Category>> GettAllCategoriesByIdWithTracking(int id, CancellationToken cancellation = default)
        {
            var categories = await BaseQuery<Category>(true).Where(c => c.Id == id && c.IsActive == true).ToListAsync(cancellation);

            return categories;
        }
    }
}   