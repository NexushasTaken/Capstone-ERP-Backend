using pr = ERP.Repository.Model.Products;

namespace ERP.Repository.Interface.Data.ProductData
{
    public interface IProductData : IBaseData
    {
        Task<IEnumerable<pr.Product>> GetAllProductWithoutTracking();
        Task<ICollection<pr.Product>> GetAllProductReferenceByCategoryWithTracking(int categoryId);
    }
}
 