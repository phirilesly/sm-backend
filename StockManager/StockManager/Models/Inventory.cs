using ErrorOr;
using StockManager.Contracts.Inventory;

namespace StockManager.Models
{
    public class Inventory
    {
        

        public Guid Id { get; }
        public Guid BranchId { get; }
        public Guid ProductId { get; }
        public DateTime OrderDate { get; }
        public decimal OrderPrice { get; }
        public int Quantity { get; }
       

     

        public Inventory(Guid id,Guid branchId,Guid productId, DateTime orderDate, decimal orderPrice, int quantity)
        {
            Id = id;
            BranchId = branchId;
            ProductId = productId;
            OrderDate = orderDate;
          OrderPrice = orderPrice;
           Quantity = quantity;
           
        }

        public static ErrorOr<Inventory> Create(
            Guid branchId,
            Guid productId,
            DateTime orderDate,
            decimal orderPrice,
         int quantity,
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

            return new Inventory(
                id ?? Guid.NewGuid(),
                branchId,
                productId,
                orderDate,
              orderPrice,
                quantity);
        }

        public static ErrorOr<Inventory> From(CreateInventoryRequest request)
        {
            return Create(
                request.BranchId,
                request.ProductId,
                request.OrderDate,
                request.OrderPrice,
                request.Quantity
               );
        }

        public static ErrorOr<Inventory> From(Guid id, UpsertInventoryRequest request)
        {
            return Create(
                 request.BranchId,
                 request.ProductId,
                request.OrderDate,
                request.OrderPrice,
                request.Quantity,
                id);
        }
    }
}
