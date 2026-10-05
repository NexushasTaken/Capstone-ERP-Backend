namespace ERP.Repository.ViewModel.Dashboard
{

   public class MonthsDataViewModel
    {
        public string? Month { get; set; }
        public decimal Data { get; set; }
    }

    public class DashboardViewModel
    {
        public IEnumerable<MonthsDataViewModel> Data { get; set; } = [];
        public decimal TotalSales { get; set; }
        public decimal GrowthPercentage { get; set; }
        public string? GrowthErrorMessage { get; set; }
    }

    public class InventoryStatusTotalViewModel
    {
        public string? Status { get; set; }
        public int Total { get; set; }
    }

    public class InventoryOverViewModel
    {
        public int TotalStock { get; set; }
        public int Risk { get; set; }
        public IEnumerable<InventoryStatusTotalViewModel> InventoryStatus { get; set; } = [];
    }
}
