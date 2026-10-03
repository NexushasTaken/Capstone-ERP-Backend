namespace ERP.Repository.Configuration.Enum
{
    public enum GlobalEnum
    {
       
    }

    public enum OrderStatusEnum
    {
        Completed = 1,
        Processing = 2,
        Shipped = 3,
        Cancelled = 4
    }

    public enum DriverFilterEnum
    {
        ATOZ = 1,
        ZTOA = 2,
    }

    public enum AuditActionEnum
    {
        Create = 1,
        Update = 2,
        Delete = 3,
        StatusChange = 4,
        IncreaseStock = 5,
        ReturnStock = 6,
        CurrentItemDamage = 7,
        ReturnItemDamage = 8
    }

    public enum AuditModuleEnum
    {
        Category = 1,
        Product = 2,
        Driver = 3,
        Order = 4,
        Inventory = 5,
        Warehouse = 6,
        Account = 7
    }
}
