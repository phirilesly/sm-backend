using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockManager.Contracts.Purchase
{
    public record PurchasesResponse(
        Guid Id,
        Guid ProductId,
         Guid BranchId,
        int Quantity,
        decimal Price
    );
   
}
