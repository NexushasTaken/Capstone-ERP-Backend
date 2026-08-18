using ERP.Repository.Model.Products;

namespace ERP.Repository.Interface.Data.ProductData
{
    public interface ICategoryData : IBaseData
    {
        Task<IEnumerable<Category>> GetAllCategoriesWithoutTracking(CancellationToken cancellation = default);
        Task<IEnumerable<Category>> GettAllCategoriesByIdWithTracking(int id, CancellationToken cancellation = default);
    }
}
