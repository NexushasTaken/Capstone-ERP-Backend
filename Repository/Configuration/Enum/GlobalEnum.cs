namespace ERP.Repository.Configuration.Enum
{
    public enum GlobalEnum { }

    public enum OrderStatusEnum
    {
        Completed = 1,
        Processing = 2,
        Shipped = 3,
        Cancelled = 4,
    }

    public enum OrderTypeEnum
    {
        Walkin = 1,
        Delivery = 2,
    }

    public enum InventoryStatusEnum
    {
        Available = 1,
        LowStock = 2,
        Critical = 3,
    }

    public enum InventoryLabelEnum
    {
        Purchase = 1,
        Return = 2,
        Restock = 3,
        Damage = 4,
    }

    public enum UserRoleEnum
    {
        Owner = 1,
        Secretary = 2,
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
        ReturnItemDamage = 8,
    }

    public enum AuditModuleEnum
    {
        Category = 1,
        Product = 2,
        Driver = 3,
        Order = 4,
        Inventory = 5,
        Warehouse = 6,
        Account = 7,
    }

    // How a product's demand forecast was made (ForecastResult.Method)
    public enum ForecastMethodEnum
    {
        // SSA with a 52-week window: learns the yearly season (116+ weeks of history)
        YearlySsa = 1,

        // 2 was an 8-week-window SSA; removed because it lost to the average in testing

        // Average of the last 4 weeks: under 116 weeks of history, not enough for the AI
        Average = 3,

        // Average of the last 4 weeks: SSA couldn't model this history (e.g. all zeros)
        AverageFallback = 4,
    }
}
