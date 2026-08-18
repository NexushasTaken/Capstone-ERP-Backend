using ERP.Repository.ViewModel.Products;

namespace ERP.Repository.Interface.Products
{
    public interface IProductService
    {
        Task<IEnumerable<CategoryViewModel>> GetCategories();
        Task InsertCategory(string categoryName);
        Task<ProductPageViewModel> GetProducts(int page, int pageSize, string? name);
        Task DeleteCategory(int id);
        Task InsertProduct(ProductPostViewModel product);
        Task DeleteProduct(int id);
    }
}
