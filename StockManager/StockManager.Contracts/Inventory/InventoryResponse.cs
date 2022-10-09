using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockManager.Contracts.Inventory
{
    public record InventoryResponse(
          Guid Id,
           Guid BranchId,
             Guid ProductId,
        DateTime OrderDate,
         decimal OrderPrice,
         int Quantity
       
    );
}
