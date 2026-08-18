namespace ERP.Repository.ViewModel.Inventories
{
    public class InventoryViewModel
    {

    }

    public class InventoryPostViewModel
    {
        public string? Name { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public int StatusId { get; set; }
        public int WarehouseId { get; set; }
        public string? DateArrived { get; set; }
        public int ReorderPoint { get; set; }
    }
}
