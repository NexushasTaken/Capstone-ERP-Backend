using System.ComponentModel;
using ERP.Repository.Configuration.Enum;
using ERP.Repository.Configuration.Exception_Extender;
using ERP.Repository.Configuration.Helper;
using ERP.Repository.Configuration.Validation;
using ERP.Repository.Interface.AuditLogs;
using ERP.Repository.Interface.Data.ProductData;
using ERP.Repository.Interface.Data.UserAccountData;
using ERP.Repository.Interface.Inventories;
using ERP.Repository.Interface.Products;
using ERP.Repository.Model.Products;
using ERP.Repository.ViewModel.Products;
using FluentValidation;

namespace ERP.Repository.Services.Products
{
    public class ProductService(
        ICategoryData _category,
        IUnitData _unit,
        IProductData _product,
        IUserAccountData _userAccount,
        IAuditLogService _auditLog,
        IValidator<CategoryUpdateViewModel> _categoryUpdateValidator,
        IValidator<ProductPostViewModel> _productPostValidator,
        IValidator<ProductUpdateViewModel> _productUpdateValidator,
        IValidator<CategoryImportViewModel> _categoryImportValidator,
        IValidator<ProductImportViewModel> _productImportValidator,
        IInventoryService _inventoryService
    ) : IProductService
    {
        #region Category
        public async Task<CategoryPageViewModel> GetCategories(
            int page,
            int pageSize,
            string? name,
            int filter,
            CancellationToken cancellation
        )
        {
            PageQueryValidator.Ensure(page, pageSize);

            var categories = await _category.GetAllCategoriesWithoutTracking(
                page,
                pageSize,
                name,
                filter,
                cancellation
            );
            var count = await _category.CategoryTotalCount(name, cancellation);

            var final = categories.Select(c => new CategoryViewModel
            {
                Id = c.Id,
                Type = c.Type,
                Created_At = c.Created_At,
            });

            return new CategoryPageViewModel
            {
                Categories = final,
                PageCount = (int)Math.Ceiling(count / (double)pageSize),
                Rows = count,
            };
        }

        public async Task InsertCategory(string categoryName)
        {
            // categoryName arrives as a query parameter, so it's checked here rather than by a validator.
            if (string.IsNullOrWhiteSpace(categoryName))
            {
                throw new ValidationFailed("categoryName", "Category Name is required");
            }

            if (await _category.CheckExistingCategory(categoryName))
            {
                throw new ValidationFailed("categoryName", "A category with this name already exists.");
            }

            var now = DateTime.UtcNow;

            var category = new Category
            {
                Type = categoryName.ToLower(),
                Created_By = _auditLog.CurrentUserId,
                Created_At = now,
                IsActive = true,
            };

            await _category.Save(category);

            _auditLog.Log(
                AuditModuleEnum.Category,
                AuditActionEnum.Create,
                $"Added category '{category.Type}'",
                category.Id,
                now
            );
            await _category.SaveChanges();
        }

        public async Task DeleteCategory(int id)
        {
            if (id <= 0)
            {
                throw new BadRequest("Category ID is required");
            }

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
            var message =
                uncategorized > 0
                    ? $"Deleted category '{category.Type}' ({uncategorized} product(s) set to uncategorized)"
                    : $"Deleted category '{category.Type}'";

            _auditLog.Log(AuditModuleEnum.Category, AuditActionEnum.Delete, message, category.Id, now);

            await _product.SaveChanges();
        }

        public async Task UpdateCategory(CategoryUpdateViewModel category)
        {
            await _categoryUpdateValidator.EnsureValidAsync(category);

            var ct = await _category.GetCategoryByIdWithTracking(category.Id);

            if (ct == null)
            {
                throw new NotFound("Category not found");
            }

            if (await _category.CheckExistingCategory(category.Type, category.Id))
            {
                throw new ValidationFailed("type", "A category with this name already exists.");
            }

            var now = DateTime.UtcNow;
            var oldType = ct.Type;

            ct.Type = category.Type;
            ct.Updated_By = _auditLog.CurrentUserId;
            ct.Updated_At = now;

            _auditLog.Log(
                AuditModuleEnum.Category,
                AuditActionEnum.Update,
                $"Renamed category '{oldType}' to '{ct.Type}'",
                ct.Id,
                now
            );

            await _category.SaveChanges();

            return;
        }

        // All-or-nothing: every row is checked first, then all rows are saved in one transaction.
        public async Task<int> ImportCategories(CategoryImportViewModel import)
        {
            await _categoryImportValidator.EnsureValidAsync(import);

            var names = import.Rows.Select(r => r.Type!.Trim()).ToList();
            ThrowIfAny(await CategoryImportErrors(names));

            var now = DateTime.UtcNow;

            var categories = names
                .Select(n => new Category
                {
                    Type = n.ToLower(),
                    Created_By = _auditLog.CurrentUserId,
                    Created_At = now,
                    IsActive = true,
                })
                .ToList();

            await using var transaction = await _category.BeginTransaction();

            await _category.SaveMany(categories);

            foreach (var category in categories)
            {
                _auditLog.Log(
                    AuditModuleEnum.Category,
                    AuditActionEnum.Create,
                    $"Imported category '{category.Type}'",
                    category.Id,
                    now
                );
            }
            await _category.SaveChanges();

            await transaction.CommitAsync();

            return categories.Count;
        }

        // Runs only the database rules, without saving, so the import dialog can flag them as soon as a file is picked.
        // Field rules (blank name etc.) are left to the client here; ImportCategories still runs them.
        public async Task CheckCategoryImport(CategoryImportViewModel import)
        {
            EnsureRowCount(import.Rows.Count);

            ThrowIfAny(await CategoryImportErrors(import.Rows.Select(r => r.Type?.Trim() ?? "").ToList()));
        }

        // Name already used, or repeated in the file. Keys are "rows.{i}.type".
        private async Task<Dictionary<string, string[]>> CategoryImportErrors(List<string> names)
        {
            var existing = await _category.GetCategoryIdsByNames(names.Where(n => n != ""));

            var errors = new Dictionary<string, string[]>();
            AddNameErrors(errors, names, "type", existing.ContainsKey, "A category with this name already exists.");

            return errors;
        }

        #endregion

        #region Product

        public async Task<ProductPageViewModel> GetProducts(
            int page,
            int pageSize,
            string? name,
            int categoryPresent,
            int filter
        )
        {
            PageQueryValidator.Ensure(page, pageSize);

            var products = await _product.GetAllProductWithoutTracking(page, pageSize, name, categoryPresent, filter);

            var count = await _product.ProductTotalCountWithoutTracking(name, categoryPresent);

            var userIds = products
                .SelectMany(p => new[] { p.Created_By, p.Updated_By })
                .Where(id => id.HasValue)
                .Select(id => id!.Value);
            var names = await _userAccount.GetFullNamesByIds(userIds);

            var product = products.Select(p => new ProductViewModel
            {
                Id = p.Id,
                CategoryId = p.CategoryId,
                Name = p.Name,
                Price = p.Price,
                UnitId = p.UnitId,
                UnitName = p.Unit?.Name,
                CategoryName = p.Category?.Type,
                Created_At = DateConverter.ConvertToPH(p.Created_At),
                CreatedByName = p.Created_By is int c ? names.GetValueOrDefault(c) : null,
                Updated_At = DateConverter.ConvertToPH(p.Updated_At),
                UpdatedByName = p.Updated_By is int u ? names.GetValueOrDefault(u) : null,
            });

            var final = new ProductPageViewModel
            {
                Products = product,
                PageCount = (int)Math.Ceiling(count / (double)pageSize),
                Rows = count,
            };

            return final;
        }

        public async Task InsertProduct(ProductPostViewModel product)
        {
            await _productPostValidator.EnsureValidAsync(product);

            if (await _product.CheckExistingProduct(product.Name))
            {
                throw new ValidationFailed("name", "A product with this name already exists.");
            }

            await EnsureUnitExists(product.UnitId);

            var now = DateTime.UtcNow;

            var pr = new Product
            {
                CategoryId = product.CategoryId == 0 ? null : product.CategoryId,
                Name = product.Name.ToLower(),
                Price = product.Price,
                UnitId = product.UnitId,
                IsActive = true,
                Created_By = _auditLog.CurrentUserId,
                Created_At = now,
            };

            await _product.Save(pr);

            _auditLog.Log(
                AuditModuleEnum.Product,
                AuditActionEnum.Create,
                $"Added product '{pr.Name}' (₱{pr.Price:N2})",
                pr.Id,
                now
            );
            await _product.SaveChanges();
        }

        public async Task DeleteProduct(int id)
        {
            var product = await _product.GetProductByIdWithTracking(id);

            if (product == null)
            {
                throw new NotFound($"Product not found");
            }

            var now = DateTime.UtcNow;

            product.IsActive = false;
            product.Deleted_By = _auditLog.CurrentUserId;
            product.Deleted_At = now;

            _auditLog.Log(
                AuditModuleEnum.Product,
                AuditActionEnum.Delete,
                $"Deleted product '{product.Name}'",
                product.Id,
                now
            );

            await _product.SaveChanges();

            return;
        }

        public async Task UpdateProduct(ProductUpdateViewModel product)
        {
            await _productUpdateValidator.EnsureValidAsync(product);

            var pr = await _product.GetProductByIdWithTracking(product.Id);

            if (pr == null)
            {
                throw new NotFound("Product not found");
            }

            if (await _product.CheckExistingProduct(product.Name, product.Id))
            {
                throw new ValidationFailed("name", "A product with this name already exists.");
            }

            await EnsureUnitExists(product.UnitId);

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

            if (pr.UnitId != product.UnitId)
            {
                changes.Add("unit changed");
            }

            pr.Name = newName;
            pr.Price = product.Price;
            pr.CategoryId = newCategoryId;
            pr.UnitId = product.UnitId;
            pr.Updated_By = _auditLog.CurrentUserId;
            pr.Updated_At = now;

            var message =
                changes.Count > 0
                    ? $"Updated product '{oldName}': {string.Join(", ", changes)}"
                    : $"Updated product '{oldName}' (no changes)";

            _auditLog.Log(AuditModuleEnum.Product, AuditActionEnum.Update, message, pr.Id, now);

            await _product.SaveChanges();

            return;
        }

        // All-or-nothing: every row is checked first, then products and their opening stock are saved in one transaction.
        public async Task<int> ImportProducts(ProductImportViewModel import)
        {
            await _productImportValidator.EnsureValidAsync(import);

            var rows = import.Rows;
            var names = rows.Select(r => r.Name!.Trim()).ToList();
            var (errors, categoryIds, unitIds) = await ProductImportErrors(rows);
            ThrowIfAny(errors);

            var now = DateTime.UtcNow;

            var products = rows.Select(
                    (r, i) =>
                        new Product
                        {
                            CategoryId = string.IsNullOrWhiteSpace(r.Category)
                                ? null
                                : categoryIds[r.Category.Trim().ToLower()],
                            Name = names[i].ToLower(),
                            Price = r.Price,
                            UnitId = string.IsNullOrWhiteSpace(r.Unit)
                                ? Unit.PieceId
                                : unitIds[r.Unit.Trim().ToLower()],
                            IsActive = true,
                            Created_By = _auditLog.CurrentUserId,
                            Created_At = now,
                        }
                )
                .ToList();

            await using var transaction = await _product.BeginTransaction();

            await _product.SaveMany(products);

            foreach (var pr in products)
            {
                _auditLog.Log(
                    AuditModuleEnum.Product,
                    AuditActionEnum.Create,
                    $"Imported product '{pr.Name}' (₱{pr.Price:N2})",
                    pr.Id,
                    now
                );
            }
            await _product.SaveChanges();

            for (var i = 0; i < rows.Count; i++)
            {
                if (rows[i].Quantity is int quantity && rows[i].ReorderPoint is int reorderPoint)
                {
                    await _inventoryService.CreateOpeningStock(products[i], quantity, reorderPoint, now);
                }
            }

            await transaction.CommitAsync();

            return products.Count;
        }

        // Runs only the database rules, without saving, so the import dialog can flag them as soon as a file is picked.
        // Field rules (price, stock columns) are left to the client here; ImportProducts still runs them.
        public async Task CheckProductImport(ProductImportViewModel import)
        {
            EnsureRowCount(import.Rows.Count);

            var (errors, _, _) = await ProductImportErrors(import.Rows);
            ThrowIfAny(errors);
        }

        // Name already used or repeated in the file, and category or unit names that don't exist.
        // Also returns the matched category and unit ids (keyed by lowercase name) for the insert.
        private async Task<(
            Dictionary<string, string[]> errors,
            Dictionary<string, int> categoryIds,
            Dictionary<string, int> unitIds
        )> ProductImportErrors(List<ProductImportRow> rows)
        {
            var names = rows.Select(r => r.Name?.Trim() ?? "").ToList();
            var existing = await _product.GetExistingProductNames(names.Where(n => n != ""));
            var categoryIds = await _category.GetCategoryIdsByNames(
                rows.Where(r => !string.IsNullOrWhiteSpace(r.Category)).Select(r => r.Category!.Trim())
            );
            var unitIds = await _unit.GetUnitIdsByNames(
                rows.Where(r => !string.IsNullOrWhiteSpace(r.Unit)).Select(r => r.Unit!.Trim())
            );

            var errors = new Dictionary<string, string[]>();
            AddNameErrors(errors, names, "name", existing.Contains, "A product with this name already exists.");

            for (var i = 0; i < rows.Count; i++)
            {
                var category = rows[i].Category?.Trim();

                if (!string.IsNullOrEmpty(category) && !categoryIds.ContainsKey(category.ToLower()))
                {
                    errors[$"rows.{i}.category"] =
                    [
                        $"Category '{category}' doesn't exist. Add it on the Categories page first, or leave the cell blank.",
                    ];
                }

                var unit = rows[i].Unit?.Trim();

                if (!string.IsNullOrEmpty(unit) && !unitIds.ContainsKey(unit.ToLower()))
                {
                    errors[$"rows.{i}.unit"] =
                    [
                        $"Unit '{unit}' doesn't exist. Add it on the Units page first, or leave the cell blank for pc.",
                    ];
                }
            }

            return (errors, categoryIds, unitIds);
        }

        #endregion

        private async Task EnsureUnitExists(int unitId)
        {
            if (!await _unit.UnitExists(unitId))
            {
                throw new ValidationFailed("unitId", "Unit not found");
            }
        }

        private static void ThrowIfAny(Dictionary<string, string[]> errors)
        {
            if (errors.Count > 0)
            {
                throw new ValidationFailed(errors);
            }
        }

        private static void EnsureRowCount(int count)
        {
            if (count == 0)
            {
                throw new ValidationFailed("rows", ImportLimits.EmptyMessage);
            }

            if (count > ImportLimits.MaxRows)
            {
                throw new ValidationFailed("rows", ImportLimits.TooManyMessage);
            }
        }

        // Adds a "rows.{i}.{field}" error for each name that already exists or repeats an earlier row (case-insensitive).
        private static void AddNameErrors(
            Dictionary<string, string[]> errors,
            IReadOnlyList<string> names,
            string field,
            Func<string, bool> exists,
            string existsMessage
        )
        {
            var seen = new HashSet<string>();

            for (var i = 0; i < names.Count; i++)
            {
                var lower = names[i].ToLower();

                // Blank names are a field error the validator (or the client) reports.
                if (lower == "")
                {
                    continue;
                }

                if (exists(lower))
                {
                    errors[$"rows.{i}.{field}"] = [existsMessage];
                }
                else if (!seen.Add(lower))
                {
                    errors[$"rows.{i}.{field}"] = ["This name is already used by another row in the file."];
                }
            }
        }
    }
}
