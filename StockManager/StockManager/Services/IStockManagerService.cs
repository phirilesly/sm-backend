using ErrorOr;
using StockManager.Models;

namespace StockManager.Services
{
    public interface IStockManagerService
    {
        ErrorOr<Created> CreateProduct(Product product);
        ErrorOr<Product> GetProduct(Guid id);
        ErrorOr<UpsertedProduct> UpsertProduct(Product product);
        ErrorOr<Deleted> DeleteProduct(Guid id);

    }
}
