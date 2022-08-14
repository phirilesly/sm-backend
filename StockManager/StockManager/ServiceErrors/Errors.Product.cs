using ErrorOr;

namespace StockManager.ServiceErrors
{
    public static class Errors{

          public static class Product
    {
        public static Error InvalidName => Error.Validation(
            code: "Product.InvalidName",
            description: $"Product name must be at least {Models.Product.MinNameLength}" +
                $" characters long and at most {Models.Product.MaxNameLength} characters long.");

        public static Error InvalidDescription => Error.Validation(
            code: "Product.InvalidDescription",
            description: $"Product description must be at least {Models.Product.MinDescriptionLength}" +
                $" characters long and at most {Models.Product.MaxDescriptionLength} characters long.");

        public static Error NotFound => Error.NotFound(
            code: "Product.NotFound",
            description: "Product not found");
    }

    }
}