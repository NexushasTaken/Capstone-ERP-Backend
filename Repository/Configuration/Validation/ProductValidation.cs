using ERP.Repository.Configuration.Exception_Extender;
using ERP.Repository.ViewModel.Products;

namespace ERP.Repository.Configuration.Validation
{
    public class ProductValidation
    {
        public static void CategoryInsertValidation(string categoryName)
        {
            if (string.IsNullOrWhiteSpace(categoryName))
            {
                throw new ArgumentNullException("Category Name is required");
            }
        }

        public static void CategoryDeleteValidation(int id)
        {
            if(id == 0 || id < 0)
            {
                throw new ArgumentException("Category ID is required");
            }
        }

        public static void CategoryUpdateValidation(CategoryUpdateViewModel category)
        {
            if(category.Id <= 0)
            {
                throw new BadRequest("Category is required");
            }
            if (string.IsNullOrWhiteSpace(category.Type))
            {
                throw new BadRequest("Category Type is required");
            }
        }

        public static void ProductInsertValidation(ProductPostViewModel product)
        {
           
            if (string.IsNullOrWhiteSpace(product.Name))
            {
                throw new BadRequest("Product Name is required");
            }
            if (product.Price <= 0)
            {
                throw new BadRequest("Product price must be greater than 0");
            }
        }
        public static void ProductUpdateValidation(ProductUpdateViewModel product)
        {

            var pr = new ProductPostViewModel
            {
                CategoryId = product.CategoryId,
                Price = product.Price,
                Name = product.Name,
            };

            ProductInsertValidation(pr);

            if (product.Id <= 0)
            {
                throw new BadRequest("Product is required");
            }
        }

    }
}
