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

    public class InventoryViewModel
    {
        public string? Name { get; set; }
        public string? Warehouse { get; set; }
        public string? Status { get; set; }
    }

    public class InventoryPageViewModel
    {
        public ICollection<InventoryViewModel> Inventory { get; set; } = [];
        public int PageCount { get; set; }
        public int Rows { get; set; }
    }

    public class InventoryOverViewModel
    {
        public int TotalWareHouseCapacity { get; set; }
        public int Risk { get; set; }
        public ICollection<InventoryStatusTotalViewModel> InventoryStatus { get; set; } = [];
        public InventoryPageViewModel Inventory { get; set; }
    }
}
