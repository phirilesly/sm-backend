using ErrorOr;
using StockManager.Contracts.Inventory;
using StockManager.Contracts.Purchase;

namespace StockManager.Models
{
    public class Purchase
    {
        public const int MinNameLength = 3;
        public const int MaxNameLength = 50;

        public const int MinDescriptionLength = 50;
        public const int MaxDescriptionLength = 150;

        public Guid Id { get; }
        public Guid BranchId { get; }
        public Guid ProductId { get; }
      public int Quantity { get; }
        public decimal Price { get; }

       



        public Purchase(Guid id,Guid branchId, Guid productId, int quantity, decimal price)
        {
            Id = id;
            BranchId = branchId;
           ProductId = productId;
            Quantity = quantity;
            Price = price;
        }

        public static ErrorOr<Purchase> Create(
            Guid branchId,
            Guid productId,
            int quantity,
            decimal price,
            Guid? id = null)


        {
            List<Error> errors = new();

            //if (name.Length is < MinNameLength or > MaxNameLength)
            //{
            //    errors.Add(Errors.Product.InvalidName);
            //}

            //if (description.Length is < MinDescriptionLength or > MaxDescriptionLength)
            //{
            //    errors.Add(Errors.Product.InvalidDescription);
            //}

            if (errors.Count > 0)
            {
                return errors;
            }

            return new Purchase(
                id ?? Guid.NewGuid(),
                branchId,
                productId,
                quantity,
                price
            


             );
        }

        public static ErrorOr<Purchase> From(CreatePurchasesRequest request)
        {
            return Create(
                request.BranchId,
                request.ProductId,
                request.Quantity,
                request.Price
              );
        }

        public static ErrorOr<Purchase> From(Guid id, UpsertPurchasesRequest request)
        {
            return Create(
                request.BranchId,
              request.ProductId,
                request.Quantity,
                request.Price,
                id);
        }
    }
}
