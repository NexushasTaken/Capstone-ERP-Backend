using ERP.Repository.ViewModel.Product;

namespace ERP.Repository.Interface.Product
{
    public interface IProductService
    {
        Task<IEnumerable<CategoryViewModel>> GetCategories();
        Task InsertCategory(string categoryName);
        Task<IEnumerable<ProductViewModel>> GetProducts();
    }
}
