using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockManager.Contracts.Product
{
    public record UpsertProductRequest
      (
           string Name,
          string Description,
           string Barcode,
        string Category,
           string SubCategory,
           string Brand,
          string Supplier);
}
