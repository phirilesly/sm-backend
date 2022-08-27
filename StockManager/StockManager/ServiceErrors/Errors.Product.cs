using ErrorOr;

namespace StockManager.ServiceErrors
{
    public static class Errors
    {

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

        public static class Branch
        {
            public static Error InvalidName => Error.Validation(
               code: "Branch.InvalidName",
               description: $"Branch name must be at least {Models.Branch.MinNameLength}" +
                   $" characters long and at most {Models.Branch.MaxNameLength} characters long.");

            public static Error InvalidPhone => Error.Validation(
                code: "Phone.InvalidPhone",
                description: $"Phone number must be at least {Models.Branch.MinPhoneLength}" +
                    $" characters long and at most {Models.Branch.MaxPhoneLength} characters long.");

            public static Error NotFound => Error.NotFound(
                code: "Branch.NotFound",
                description: "Branch not found");
        }

        public static class Purchase
        {
        

            public static Error NotFound => Error.NotFound(
                code: "Purchase.NotFound",
                description: "Sale not found");
        }

        public static class Inventory
        {


            public static Error NotFound => Error.NotFound(
                code: "Inventory.NotFound",
                description: "Inventory not found");
        }

        public static class User
        {


            public static Error NotFound => Error.NotFound(
                code: "User.NotFound",
                description: "User not found");
        }

    }
}
