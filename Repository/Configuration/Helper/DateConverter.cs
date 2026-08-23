namespace ERP.Repository.Configuration.Helper
{
    public class DateConverter
    {
        public static DateTime? ConvertToPH(DateTime? date)
        {
            if (date == null)
            {
                return null;
            }

            TimeZoneInfo phTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Singapore Standard Time");

            DateTime phTime = TimeZoneInfo.ConvertTimeFromUtc((DateTime)date, phTimeZone);

            return phTime;
        }
    }
}
