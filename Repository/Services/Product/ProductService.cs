using ERP.Repository.Configuration.Exception_Extender;
using ERP.Repository.Interface.Data.ProductData;
using ERP.Repository.Interface.Product;
using ERP.Repository.Model.Products;
using ERP.Repository.ViewModel.Product;

namespace ERP.Repository.Services.Product
{
    public class ProductService(ICategoryData _category, IProductData _product) : IProductService
    {

        #region Category
        public async Task<IEnumerable<CategoryViewModel>> GetCategories()
        {

            var categories = await _category.GetAllCategoriesWithoutTracking();

            return categories.Select(c => new CategoryViewModel
            {
                Id = c.Id,
                Type = c.Type
            });
        }

        public async Task InsertCategory(string categoryName)
        {
            var category = new Category
            {
                Type = categoryName,
                Created_At = DateTime.UtcNow,
                IsActive = true
            };

            await _category.Save(category);
        }

        public async Task DeleteCategory(int id)
        {
            var category = await _category.GetCategoryByIdWithTracking(id);

            if (category == null)
            {
                throw new NotFound($"Category with ID {id} not found.");
            }

            category.IsActive = false;

            await _category.SaveChanges();


            var categoryReferences = await _product.GetAllProductReferenceByCategoryWithTracking(id);

            foreach (var reference in categoryReferences)
            {
                reference.IsActive = false;
            }

            await _product.SaveChanges();
        }

        #endregion

        #region Product

        public async Task<IEnumerable<ProductViewModel>> GetProducts()
        {
            var products = await _product.GetAllProductWithoutTracking();

            var product = products.Select(p => new ProductViewModel
            {
                Id = p.Id,
                CategoryId = p.CategoryId,
                Name = p.Name,
                Price = p.Price,
                Created_At = p.Created_At
            });

            return product;
        }

        #endregion
    }
}
