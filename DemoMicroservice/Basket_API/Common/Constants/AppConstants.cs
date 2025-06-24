namespace Basket_API.Common.Constants
{
    public static class AppConstants
    {
        public static class Validation
        {
            public const string RequiredUserId = "UserId is required.";
            public const string RequiredProductId = "ProductId is required.";
            public const string InvalidQuantity = "Quantity must be greater than 0.";
            public const string BasketNotFound = "Basket {0} not found.";
        }
    }
} 