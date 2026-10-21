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

    // Senior, PWD and solo parent have fixed rates from Philippine law: RA 9994, RA 10754, RA 11861.
    public enum DiscountTypeEnum
    {
        None = 1,
        Senior = 2,
        Pwd = 3,
        SoloParent = 4,
        Custom = 5,
    }

    // What came back on a sale return
    public enum ReturnConditionEnum
    {
        Good = 1,
        Damaged = 2,
    }

    public enum InventoryStatusEnum
    {
        Available = 1,
        LowStock = 2,
        Critical = 3,
    }

    // Stock ledger labels. Ids are kept because the forecast reads demand by id (Sale and Return).
    public enum InventoryLabelEnum
    {
        // A completed customer order
        Sale = 1,
        Return = 2,

        // Any StockAdjustment, including the opening stock
        Adjustment = 3,

        // 4 was "damage"; damage is now an adjustment with the Damaged reason

        // Stock received from a supplier purchase; never counts as demand
        Received = 5,
    }

    // Why stock was adjusted (StockAdjustment.ReasonId). The reason sets the direction.
    public enum AdjustmentReasonEnum
    {
        // - quantity
        Damaged = 1,

        // - quantity
        Lost = 2,

        // + quantity
        Found = 3,

        // The counted quantity replaces the stock; the difference is saved
        CountCorrection = 4,

        // + quantity, written when a stock record is created; never chosen by the user
        OpeningStock = 5,
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
        Adjust = 7,
        ReturnItemDamage = 8,
    }

    public enum AuditModuleEnum
    {
        Category = 1,
        Product = 2,
        Driver = 3,
        Order = 4,
        Inventory = 5,
        Account = 7,
    }

    // How a product's demand forecast was made (ForecastResult.Method)
    public enum ForecastMethodEnum
    {
        // SSA with a 52-week window: finds the yearly season (116+ weeks of history)
        YearlySsa = 1,

        // 2 was an 8-week-window SSA; removed because it lost to the average in testing

        // Average of the last 4 weeks: under 116 weeks of history, not enough for SSA
        Average = 3,

        // Average of the last 4 weeks: SSA couldn't model this history (e.g. all zeros)
        AverageFallback = 4,
    }
}
