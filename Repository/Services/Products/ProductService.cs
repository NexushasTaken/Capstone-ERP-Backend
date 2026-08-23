using ERP.Repository.Configuration.Exception_Extender;
using ERP.Repository.Configuration.Helper;
using ERP.Repository.Configuration.Validation;
using ERP.Repository.Interface.Data.ProductData;
using ERP.Repository.Interface.Products;
using ERP.Repository.Model.Products;
using ERP.Repository.ViewModel.Products;
using System.ComponentModel;

namespace ERP.Repository.Services.Products
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
                Type = c.Type,
                Created_At = c.Created_At
            });
        }

        public async Task InsertCategory(string categoryName)
        {
            ProductValidation.CategoryInsertValidation(categoryName);

            var category = new Category
            {
                Type = categoryName.ToLower(),
                Created_At = DateTime.UtcNow,
                IsActive = true
            };

            await _category.Save(category);
        }

        public async Task DeleteCategory(int id)
        {

            ProductValidation.CategoryDeleteValidation(id);

            var category = await _category.GetCategoryByIdWithTracking(id);

            if (category == null)
            {
                throw new NotFound($"Category with ID {id} not found.");
            }

            category.IsActive = false;
            category.Deleted_At = DateTime.UtcNow;

            await _category.SaveChanges();


            var categoryReferences = await _product.GetAllProductReferenceByCategoryWithTracking(id);

            foreach (var reference in categoryReferences)
            {
                reference.IsActive = false;
            }

            await _product.SaveChanges();
        }

        public async Task UpdateCategory(CategoryUpdateViewModel category)
        {
            ProductValidation.CategoryUpdateValidation(category);

            var ct = await _category.GetCategoryByIdWithTracking(category.Id);

            if(ct == null)
            {
                throw new NotFound("Category not found");
            }

            ct.Type = category.Type;
            ct.Updated_At = DateTime.UtcNow;
            await _category.SaveChanges();

            return;
        }

        #endregion

        #region Product

        public async Task<ProductPageViewModel> GetProducts(int page, int pageSize, string? name)
        {
            GlobalValidation.PageValidation(page, pageSize);

            var products = await _product.GetAllProductWithoutTracking(page, pageSize, name.ToLower());

            var count = await _product.ProductTotalCountWithoutTracking(name.ToLower());

            var product = products.Select(p => new ProductViewModel
                {
                    Id = p.Id,
                    CategoryId = p.CategoryId,
                    Name = p.Name,
                    Price = p.Price,
                    CategoryName = p.Category?.Type,
                    Created_At = DateConverter.ConvertToPH(p.Created_At)
                });

            var final = new ProductPageViewModel
            {
                Products = product,
                PageCount = (int)Math.Ceiling(count / (double)pageSize),
                Rows = count
            };

            return final;
        }

        public async Task InsertProduct(ProductPostViewModel product)
        {
            ProductValidation.ProductInsertValidation(product);

            var pr = new Product
            {
                CategoryId = product.CategoryId,
                Name = product.Name.ToLower(),
                Price = product.Price,
                IsActive = true,
                Created_At = DateTime.UtcNow,
            };

            await _product.Save(pr);
        }

        public async Task DeleteProduct(int id)
        {
            var product = await _product.GetProductByIdWithTracking(id);

            if(product == null)
            {
                throw new NotFound($"Product not found");
            }

            product.IsActive = false;
            product.Deleted_At = DateTime.UtcNow;

            await _product.SaveChanges();

            return;
        }

        public async Task UpdateProduct(ProductUpdateViewModel product)
        {
            ProductValidation.ProductUpdateValidation(product);

            var pr = await _product.GetProductByIdWithTracking(product.Id);

            if (pr == null)
            {
                throw new NotFound("Product not found");
            }

            pr.Name = product.Name.ToLower();
            pr.Price = product.Price;
            pr.CategoryId = product.CategoryId;
            pr.Updated_At = DateTime.UtcNow;

            await _product.SaveChanges();

            return;
        }

        #endregion
    }
}
