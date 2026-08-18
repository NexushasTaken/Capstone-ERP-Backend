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
            var categories = await BaseQuery<Category>(false).Where(c => c.IsActive == true).OrderByDescending(c => c.Id).ToListAsync(cancellation);

            return categories;
        }

        public async Task<Category> GetCategoryByIdWithTracking(int id, CancellationToken cancellation = default)
        {
            var category = await BaseQuery<Category>(true).FirstOrDefaultAsync(c => c.Id == id && c.IsActive == true, cancellation);
            return category;
        }
    }
}   