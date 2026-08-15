namespace ERP.Repository.Model.Inventories
{
    public class OrderType : BaseModel
    {
        public string? Type { get; set; }

        public ICollection<Orders> Orders { get; set; } = [];
    }
}
