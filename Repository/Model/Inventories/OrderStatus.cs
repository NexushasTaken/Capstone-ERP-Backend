namespace ERP.Repository.Model.Inventories
{
    public class OrderStatus : BaseModel
    {
        public string? Status { get; set; }

        public ICollection<Orders> Orders { get; set; } = [];
    }
}
