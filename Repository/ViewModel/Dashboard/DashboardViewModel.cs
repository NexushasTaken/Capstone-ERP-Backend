namespace ERP.Repository.ViewModel.Dashboard
{
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
