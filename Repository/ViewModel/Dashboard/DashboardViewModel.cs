namespace ERP.Repository.ViewModel.Dashboard
{

   public class MonthsDataViewModel
    {
        public decimal Data { get; set; }
    }

    public class DashboardViewModel
    {
        public IEnumerable<MonthsDataViewModel> Data { get; set; } = [];
        public decimal TotalSales { get; set; }
        public double GrowthPercentage { get; set; }
    }
}
