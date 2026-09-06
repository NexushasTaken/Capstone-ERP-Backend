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
        public int? WarehouseId { get; set; }
        public string? WarehouseName { get; set; }
        public string? Status { get; set; }
        public DateTime DateArrived { get; set; }
        public DateTime? Created_At { get; set; }
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
        public int InventoryLabelId { get; set; }
    }

    public class InventoryRestockViewModel
    {
        public int Id { get; set; }
        public int Quantity { get; set; }
        public int RestockType { get; set; }
    }

    public class InventoryUpdateViewModel
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public int ProductId { get; set; }
        public int WarehouseId { get; set; }
        public int ReorderPoint { get; set; }
    }

    public class InventoryDamageViewModel
    {
        public int Quantity { get; set; }
        public string? Reason { get; set; }
        public DateTime? Created_At { get; set; }
    }

    public class InventoryDamagePostViewModel : InventoryDamageViewModel
    {
        public int Id { get; set; }
        public int DamagedType { get; set; }
    }

    public class InventoryTransactionViewModle
    {
        public int Quantity { get; set; }
        public string? Label { get; set; }
        public DateTime? Created_At { get; set; }
    }

    public class InventoryTransactionPostViewModel
    {
        public int Id { get; set; }
        public int Quantity { get; set; }
        public int Label { get; set; }
    }

    public class InventoryWareHousePostViewModel
    {
        public string? Name { get; set; }
        public string? Address { get; set; }
        public int Capicity { get; set; }
    }

    public class InventoryWareHouseUpdateViewModel : InventoryWareHousePostViewModel
    {
        public int Id { get; set; }
    }

    public class InventoryWareHouseViewModel : InventoryWareHouseUpdateViewModel
    {
        public int Stocks { get; set; }
    }

    public class InventoryLabelViewModel
    {
        public int Id { get; set; }
        public string? Type { get; set; }
    }

    public class InventoryMovementVelocityViewModel
    {
        public int InventoryId { get; set; }
        public string? Name { get; set; }
        public string? Warehouse { get; set; }
        public string? Classification { get; set; }
        public double VelocityMetric { get; set; }
    }

    public class InventoryMovementVelocityPageViewModel
    {
        public IEnumerable<InventoryMovementVelocityViewModel> inventories { get; set; } = [];
        public int PageCount { get; set; }
        public int Rows { get; set; }
    }
}
