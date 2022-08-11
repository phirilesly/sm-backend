namespace StockManager.Contracts.Product;


    public record UpsertProductRequest
    (
         string Name,
        string Description,
         string Barcode,
      string Category,
         string SubCategory,
         string Brand,
        string Supplier);
