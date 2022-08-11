namespace StockManager.Contracts.Product;


    public record ProductResponse
    (
        Guid Id,
        string Name,
        string Description,
        string Barcode,
        string Category,
        string SubCategory,
        string Brand,
        string Supplier
    );
   
