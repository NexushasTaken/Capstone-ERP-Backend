using ERP.Repository.ViewModel.Products;

namespace ERP.Repository.Interface.Products
{
    public interface IProductService
    {
        Task<CategoryPageViewModel> GetCategories(int page, int pageSize, string? name, int filter, CancellationToken cancellation);
        Task InsertCategory(string categoryName);
        Task<ProductPageViewModel> GetProducts(int page, int pageSize, string? name, int categoryPresent, int filter);
        Task DeleteCategory(int id);
        Task InsertProduct(ProductPostViewModel product);
        Task DeleteProduct(int id);
        Task UpdateProduct(ProductUpdateViewModel product);
        Task UpdateCategory(CategoryUpdateViewModel category);
    }
}
