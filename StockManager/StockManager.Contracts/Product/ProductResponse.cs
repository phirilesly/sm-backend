using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockManager.Contracts.Product
{
    public record ProductResponse
    (
        Guid Id,
        string Name,
        string Description,
        decimal Price,
        string Barcode,
        string Category,
        string Brand,
        string Supplier
    );
}
