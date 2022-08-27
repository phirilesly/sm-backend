using ErrorOr;
using StockManager.Contracts.Common;
using StockManager.Contracts.User;
using StockManager.Models;

namespace StockManager.Services
{
    public interface IStockManagerService
    {
        ErrorOr<Created> CreateProduct(Product product);
        ErrorOr<Product> GetProduct(Guid id);
        Task<ErrorOr<List<Product>>> GetProducts(List<SearchParameter> searchParameters);
        
        ErrorOr<UpsertedProduct> UpsertProduct(Product product);
        ErrorOr<Deleted> DeleteProduct(Guid id);


        ErrorOr<Created> CreateBranch(Branch branch);
        ErrorOr<Branch> GetBranch(Guid id);
        Task<ErrorOr<List<Branch>>> GetBranches(List<SearchParameter> searchParameters);
        
        ErrorOr<UpsertedProduct> UpsertBranch(Branch branch);
        ErrorOr<Deleted> DeleteBranch(Guid id);


        ErrorOr<Created> CreateInventory(Inventory inventory);
        ErrorOr<Inventory> GetInventory(Guid id);
        Task<ErrorOr<List<Inventory>>> GetInventories(List<SearchParameter> searchParameters);
        
        ErrorOr<UpsertedProduct> UpsertInventory(Inventory inventory);
        ErrorOr<Deleted> DeleteInventory(Guid id);

        ErrorOr<Created> CreatePurchase(Purchase purchases);
        ErrorOr<Purchase> GetPurchase(Guid id);
        Task<ErrorOr<List<Purchase>>> GetPurchases(List<SearchParameter> searchParameters);
     
        ErrorOr<UpsertedProduct> UpsertPurchase(Purchase  purchases);
        ErrorOr<Deleted> DeletePurchase(Guid id);


        Task<ServiceResponse<Guid>> Register(User user, string password);
        Task<bool> UserExists(string email);
        Task<ServiceResponse<string>> Login(string email, string password);
        Task<ServiceResponse<bool>> ChangePassword(int userId, string newPassword);
        int GetUserId();
        string GetUserEmail();
        Task<User> GetUserByEmail(string email);

        Task<ErrorOr<List<User>>> GetUsers(List<SearchParameter> searchParameters);

       





    }
}
