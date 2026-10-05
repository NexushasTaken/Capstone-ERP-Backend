using ERP.Repository.Model.Products;

namespace ERP.Repository.Interface.Data.ProductData
{
    public interface ICategoryData : IBaseData
    {
        Task<IEnumerable<Category>> GetAllCategoriesWithoutTracking(int page, int pageSize, string? name, int filter, CancellationToken cancellation = default);
        Task<int> CategoryTotalCount(string? name, CancellationToken cancellation = default);
        Task<Category> GetCategoryByIdWithTracking(int id, CancellationToken cancellation = default);
    }
}
