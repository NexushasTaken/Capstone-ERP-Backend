namespace ERP.Repository.Configuration.Validation.Product
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


    }
}
