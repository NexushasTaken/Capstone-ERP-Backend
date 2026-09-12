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

    public enum SalesOverView
    {
        ThreeMonths = 3,
        SixMonths = 6,
        NineMonths = 9,
        OneYear = 12
    }
}
