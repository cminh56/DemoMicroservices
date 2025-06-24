namespace Order_API.Common.Constants
{
    public static class AppConstants
    {
        public static class Validation
        {
            public const string RequiredCustomerName = "Customer name is required.";
            public const string InvalidOrderDetail = "Order must have at least one order detail.";
            public const string OrderNotFound = "Order with id {0} not found.";
        }
    }
} 