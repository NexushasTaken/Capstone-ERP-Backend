namespace ERP.Repository.ViewModel.Inventories
{

    public enum InventoryFilter
    {
        ATOZ = 1,
        ZTOA = 2,
        QHIGH = 3,
        QLOW = 4,
        RPOINT = 5,
        WHOUSE = 6
    }

    public class InventoryViewModel
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string? Name { get; set; }
        public int Quantity { get; set; }
        public int ReorderPoint { get; set; }
        public int WarehouseId { get; set; }
        public string? WarehouseName { get; set; }
        public string? Status { get; set; }
        public DateTime DateArrived { get; set; }
    }

    public class InventoryPageViewModel
    {
        public IEnumerable<InventoryViewModel> inventories { get; set; } = [];
        public int PageCount { get; set; }
        public int Rows { get; set; }

    }

    public class InventoryPostViewModel
    {
        public string? Name { get; set; }
        public int Quantity { get; set; }
        public int ProductId { get; set; }
        public int WarehouseId { get; set; }
        public string? DateArrived { get; set; }
        public int ReorderPoint { get; set; }
    }

    public class InventoryUpdateViewModel
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public int ProductId { get; set; }
        public int WarehouseId { get; set; }
        public int ReorderPoint { get; set; }
    }
}
