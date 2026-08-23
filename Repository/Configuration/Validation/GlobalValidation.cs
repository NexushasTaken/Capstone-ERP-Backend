namespace ERP.Repository.Configuration.Validation
{
    public class GlobalValidation
    {
        public static void PageValidation(int page, int pageSize)
        {
            if(page <= 0)
            {
                throw new ArgumentException("Page is required");
            }

            if(pageSize <= 0)
            {
                throw new ArgumentException("Page Size is required");
            }
        }
    }
}
