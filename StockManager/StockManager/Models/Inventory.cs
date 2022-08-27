using ErrorOr;
using StockManager.Contracts.Inventory;

namespace StockManager.Models
{
    public class Inventory
    {
        

        public Guid Id { get; }
        public Guid BranchId { get; }
        public DateTime OrderDate { get; }
        public decimal TotalPrice { get; }
        public List<OrderItem> OrderItems { get; }
       

     

        public Inventory(Guid id,Guid branchId, DateTime orderDate, decimal totalPrice, List<OrderItem> orderItems)
        {
            Id = id;
            BranchId = branchId;
            OrderDate = orderDate;
            TotalPrice = totalPrice;
            OrderItems = orderItems;
           
        }

        public static ErrorOr<Inventory> Create(
            Guid branchId,
            DateTime orderDate,
            decimal totalPrice,
          List<OrderItem> orderItems,
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
                orderDate,
                totalPrice,
                orderItems);
        }

        public static ErrorOr<Inventory> From(CreateInventoryRequest request)
        {
            return Create(
                request.BranchId,
                request.OrderDate,
                request.TotalPrice,
                request.OrderItems
               );
        }

        public static ErrorOr<Inventory> From(Guid id, UpsertInventoryRequest request)
        {
            return Create(
                 request.BranchId,
                request.OrderDate,
                request.TotalPrice,
                request.OrderItems,
                id);
        }
    }
}
