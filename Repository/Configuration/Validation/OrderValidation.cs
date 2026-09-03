using ERP.Repository.Configuration.Exception_Extender;
using ERP.Repository.ViewModel.Orders;

namespace ERP.Repository.Configuration.Validation
{
    public class OrderValidation
    {
        public static void AddNewOrderValidation(List<OrderPostViewModel> orders)
        {
            foreach (var ord in orders)
            {
                if (ord.ProductId <= 0)
                {
                    throw new BadRequest("Product is required");
                }
                if (ord.OrderTypeId <= 0)
                {
                    throw new BadRequest("Order Type is required");
                }
                if (ord.Quantity <= 0)
                {
                    throw new BadRequest("Order Quantity is required");
                }
            }
        }


        public static void UpdateOrderStatusValidation(OrderStatusPostViewModel status)
        {
            if (status.OrderId <= 0)
            {
                throw new BadRequest("Order is required");
            }
            if(status.OrderStatusId <= 0)
            {
                throw new BadRequest("Order Status is required");
            }
        }
    }
}
