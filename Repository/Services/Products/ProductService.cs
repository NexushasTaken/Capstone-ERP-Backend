using ERP.Repository.Configuration.Enum;
using ERP.Repository.Configuration.Exception_Extender;
using ERP.Repository.Configuration.Helper;
using ERP.Repository.Configuration.Validation;
using ERP.Repository.Interface.AuditLogs;
using ERP.Repository.Interface.Data.ProductData;
using ERP.Repository.Interface.Products;
using ERP.Repository.Model.Products;
using ERP.Repository.ViewModel.Products;
using System.ComponentModel;

namespace ERP.Repository.Services.Products
{
    public class ProductService(ICategoryData _category, IProductData _product, IAuditLogService _auditLog) : IProductService
    {

        #region Category
        public async Task<CategoryPageViewModel> GetCategories(int page, int pageSize, CancellationToken cancellation)
        {
            var categories = await _category.GetAllCategoriesWithoutTracking(page, pageSize, cancellation);
            var count = await _category.CategoryTotalCount(cancellation);

             var final =  categories.Select(c => new CategoryViewModel
            {
                Id = c.Id,
                Type = c.Type,
                Created_At = c.Created_At
            });

            return new CategoryPageViewModel
            {
                Categories = final,
                PageCount = (int)Math.Ceiling(count / (double)pageSize),
                Rows = count
            };
        }

        public async Task InsertCategory(string categoryName)
        {
            ProductValidation.CategoryInsertValidation(categoryName);

            var now = DateTime.UtcNow;

            var category = new Category
            {
                Type = categoryName.ToLower(),
                Created_By = _auditLog.CurrentUserId,
                Created_At = now,
                IsActive = true
            };

            await _category.Save(category);

            _auditLog.Log(AuditModuleEnum.Category, AuditActionEnum.Create, $"Added category '{category.Type}'", category.Id, now);
            await _category.SaveChanges();
        }

        public async Task DeleteCategory(int id)
        {

            ProductValidation.CategoryDeleteValidation(id);

            var category = await _category.GetCategoryByIdWithTracking(id);

            if (category == null)
            {
                throw new NotFound($"Category with ID {id} not found.");
            }

            var now = DateTime.UtcNow;

            category.IsActive = false;
            category.Deleted_By = _auditLog.CurrentUserId;
            category.Deleted_At = now;

            var categoryReferences = await _product.GetAllProductReferenceByCategoryWithTracking(id);

            foreach (var reference in categoryReferences)
            {
                reference.CategoryId = null;
                reference.Updated_By = _auditLog.CurrentUserId;
                reference.Updated_At = now;
            }

            var uncategorized = categoryReferences.Count();
            var message = uncategorized > 0
                ? $"Deleted category '{category.Type}' ({uncategorized} product(s) set to uncategorized)"
                : $"Deleted category '{category.Type}'";

            _auditLog.Log(AuditModuleEnum.Category, AuditActionEnum.Delete, message, category.Id, now);

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

            var now = DateTime.UtcNow;
            var oldType = ct.Type;

            ct.Type = category.Type;
            ct.Updated_By = _auditLog.CurrentUserId;
            ct.Updated_At = now;

            _auditLog.Log(AuditModuleEnum.Category, AuditActionEnum.Update, $"Renamed category '{oldType}' to '{ct.Type}'", ct.Id, now);

            await _category.SaveChanges();

            return;
        }

        #endregion

        #region Product

        public async Task<ProductPageViewModel> GetProducts(int page, int pageSize, string? name, int categoryPresent)
        {
            GlobalValidation.PageValidation(page, pageSize);

            var products = await _product.GetAllProductWithoutTracking(page, pageSize, name.ToLower(), categoryPresent);

            var count = await _product.ProductTotalCountWithoutTracking(name.ToLower(), categoryPresent);

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

            var now = DateTime.UtcNow;

            var pr = new Product
            {
                CategoryId = product.CategoryId == 0 ? null : product.CategoryId,
                Name = product.Name.ToLower(),
                Price = product.Price,
                IsActive = true,
                Created_By = _auditLog.CurrentUserId,
                Created_At = now,
            };

            await _product.Save(pr);

            _auditLog.Log(AuditModuleEnum.Product, AuditActionEnum.Create, $"Added product '{pr.Name}' (₱{pr.Price:N2})", pr.Id, now);
            await _product.SaveChanges();
        }

        public async Task DeleteProduct(int id)
        {
            var product = await _product.GetProductByIdWithTracking(id);

            if(product == null)
            {
                throw new NotFound($"Product not found");
            }

            var now = DateTime.UtcNow;

            product.IsActive = false;
            product.Deleted_By = _auditLog.CurrentUserId;
            product.Deleted_At = now;

            _auditLog.Log(AuditModuleEnum.Product, AuditActionEnum.Delete, $"Deleted product '{product.Name}'", product.Id, now);

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

            var now = DateTime.UtcNow;
            var oldName = pr.Name;
            var changes = new List<string>();

            var newName = product.Name.ToLower();
            var newCategoryId = product.CategoryId == 0 ? null : product.CategoryId;

            if (pr.Name != newName)
            {
                changes.Add($"name '{pr.Name}' → '{newName}'");
            }

            if (pr.Price != product.Price)
            {
                changes.Add($"price ₱{pr.Price:N2} → ₱{product.Price:N2}");
            }

            if (pr.CategoryId != newCategoryId)
            {
                changes.Add("category changed");
            }

            pr.Name = newName;
            pr.Price = product.Price;
            pr.CategoryId = newCategoryId;
            pr.Updated_By = _auditLog.CurrentUserId;
            pr.Updated_At = now;

            var message = changes.Count > 0
                ? $"Updated product '{oldName}': {string.Join(", ", changes)}"
                : $"Updated product '{oldName}' (no changes)";

            _auditLog.Log(AuditModuleEnum.Product, AuditActionEnum.Update, message, pr.Id, now);

            await _product.SaveChanges();

            return;
        }

        #endregion
    }
}
