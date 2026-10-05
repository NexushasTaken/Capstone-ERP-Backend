using ERP.Repository.Model.Products;

namespace ERP.Repository.Interface.Data.ProductData
{
    public interface IProductData : IBaseData
    {
        Task<IEnumerable<Product>> GetAllProductWithoutTracking(int page, int pageSize, string? name, int categoryPresent, int filter);
        Task<ICollection<Product>> GetAllProductReferenceByCategoryWithTracking(int categoryId);
        Task<Product> GetProductByIdWithTracking(int id);
        Task<int> ProductTotalCountWithoutTracking(string? name, int categoryPresent);
        Task<Product> GetProductByIdWithoutTracking(int id);
        Task<IEnumerable<Product>> GetAllProductForInventoryInsert();
    }
}
 