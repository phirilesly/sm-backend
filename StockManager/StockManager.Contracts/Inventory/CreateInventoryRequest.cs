using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockManager.Contracts.Inventory
{
    public record CreateInventoryRequest
    (
          Guid Id,
          Guid BranchId,
        DateTime OrderDate,
         decimal TotalPrice,
         List<OrderItem> OrderItems 
    );
}
