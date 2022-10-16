using ErrorOr;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using StockManager.Contracts.Common;
using StockManager.Contracts.User;
using StockManager.Data;
using StockManager.Models;
using StockManager.ServiceErrors;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;

namespace StockManager.Services
{
    public class StockManagerService : IStockManagerService
    {
        private readonly MongoContext _context;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public StockManagerService(MongoContext context,
            IConfiguration configuration,
            IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
        }

        public string GetMyName()
        {
            var result = string.Empty;
            if (_httpContextAccessor.HttpContext != null)
            {
                result = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.Name);
            }
            return result;
        }



        public ErrorOr<Created> CreateBranch(Branch branch)
        {
            _context.Branches.InsertOneAsync(branch);
            return Result.Created;
        }

        public ErrorOr<Created> CreateInventory(Inventory inventory)
        {
            _context.Inventories.InsertOneAsync(inventory);
            return Result.Created;
        }

        public ErrorOr<Created> CreateProduct(Product product)
        {
             _context.Products.InsertOneAsync(product);

            return Result.Created;
        }

        public ErrorOr<Created> CreatePurchase(Purchase purchases)
        {
            _context.Purchases.InsertOneAsync(purchases);
            return Result.Created;
        }

        public ErrorOr<Deleted> DeleteBranch(Guid id)
        {
            FilterDefinition<Branch> filter = Builders<Branch>.Filter.Eq(c => c.Id, id);

             _context.Branches.DeleteOneAsync(filter);
            return Result.Deleted;
        }

        public ErrorOr<Deleted> DeleteInventory(Guid id)
        {
            FilterDefinition<Inventory> filter = Builders<Inventory>.Filter.Eq(c => c.Id, id);

            _context.Inventories.DeleteOneAsync(filter);
            return Result.Deleted;
        }

        public ErrorOr<Deleted> DeleteProduct(Guid id)
        {
            FilterDefinition<Product> filter = Builders<Product>.Filter.Eq(c => c.Id, id);

            _context.Products.DeleteOneAsync(filter);
            return Result.Deleted;
        }

        public ErrorOr<Deleted> DeletePurchase(Guid id)
        {
            FilterDefinition<Purchase> filter = Builders<Purchase>.Filter.Eq(c => c.Id, id);

            _context.Purchases.DeleteOneAsync(filter);
            return Result.Deleted;
        }

        public ErrorOr<Branch> GetBranch(Guid id)
        {
            FilterDefinition<Branch> filter = Builders<Branch>.Filter.Eq(c => c.Id, id);

            var branch = _context.Branches.FindAsync(filter).Result.FirstOrDefault();
            if (branch != null)
            {
                return branch;
            }

            return Errors.Branch.NotFound;
        }

        public async Task<ErrorOr<List<Branch>>> GetBranches(List<SearchParameter> searchParameters)
        {

            FilterDefinition<Branch> filter = Builders<Branch>.Filter.Ne("isDeleted", true);
            foreach (var parameter in searchParameters.Where(
                    parameter => !string.IsNullOrEmpty(parameter.Name) && !string.IsNullOrEmpty(parameter.Value)))
            {
                var validParameter = Enum.TryParse(parameter.Name.ToUpper(), out SearchOptions option);
                if (!validParameter)
                {
                    continue;
                }
                switch (option)
                {
                    case SearchOptions.ID:
                        if (filter == null)
                        {
                            filter = Builders<Branch>.Filter.Eq(c => c.Id, Guid.Parse(parameter.Value));
                        }
                        else
                        {
                            filter = Builders<Branch>.Filter.Eq(c => c.Id, Guid.Parse(parameter.Value)) & filter;

                        }
                        break;
                    case SearchOptions.NAME:
                        if (filter == null)
                        {
                            filter = Builders<Branch>.Filter.Eq(c => c.Name, (parameter.Value));
                        }
                        else
                        {
                            filter = Builders<Branch>.Filter.Eq(c => c.Name, (parameter.Value)) & filter;

                        }
                        break;

                }

            }
            if (filter == null) throw new ArgumentException("Invalid search parameters specified");
            List<Branch> result = await _context.Branches.Find((FilterDefinition<Branch>)filter).ToListAsync();

            return result;
        }

        public async Task<ErrorOr<List<Inventory>>> GetInventories(List<SearchParameter> searchParameters)
        {
            FilterDefinition<Inventory> filter = Builders<Inventory>.Filter.Ne("isDeleted", true);
            foreach (var parameter in searchParameters.Where(
                    parameter => !string.IsNullOrEmpty(parameter.Name) && !string.IsNullOrEmpty(parameter.Value)))
            {
                var validParameter = Enum.TryParse(parameter.Name.ToUpper(), out SearchOptions option);
                if (!validParameter)
                {
                    continue;
                }
                switch (option)
                {
                    case SearchOptions.ID:
                        if (filter == null)
                        {
                            filter = Builders<Inventory>.Filter.Eq(c => c.Id, Guid.Parse(parameter.Value));
                        }
                        else
                        {
                            filter = Builders<Inventory>.Filter.Eq(c => c.Id, Guid.Parse(parameter.Value)) & filter;

                        }
                        break;
                    case SearchOptions.BRANCHID:
                        if (filter == null)
                        {
                            filter = Builders<Inventory>.Filter.Eq(c => c.BranchId, Guid.Parse(parameter.Value));
                        }
                        else
                        {
                            filter = Builders<Inventory>.Filter.Eq(c => c.BranchId, Guid.Parse(parameter.Value)) & filter;

                        }
                        break;
                  
                }

            }
            if (filter == null) throw new ArgumentException("Invalid search parameters specified");
            List<Inventory> result = await _context.Inventories.Find((FilterDefinition<Inventory>)filter).ToListAsync();

            return result;
        }

        public ErrorOr<Inventory> GetInventory(Guid id)
        {
            FilterDefinition<Inventory> filter = Builders<Inventory>.Filter.Eq(c => c.Id, id);

            var inventory = _context.Inventories.FindAsync(filter).Result.FirstOrDefault();
            if (inventory != null)
            {
                return inventory;
            }

            return Errors.Inventory.NotFound;
        }

        public ErrorOr<Product> GetProduct(Guid id)
        {
            FilterDefinition<Product> filter = Builders<Product>.Filter.Eq(c => c.Id, id);

            var product = _context.Products.FindAsync(filter).Result.FirstOrDefault();
            if (product != null)
            {
                return product;
            }

            return Errors.Product.NotFound;
        }

        public async Task<ErrorOr<List<Product>>> GetProducts(List<SearchParameter> searchParameters)
        {

            FilterDefinition<Product> filter = Builders<Product>.Filter.Ne("isDeleted", true);
            foreach (var parameter in searchParameters.Where(
                    parameter => !string.IsNullOrEmpty(parameter.Name) && !string.IsNullOrEmpty(parameter.Value)))
            {
                var validParameter = Enum.TryParse(parameter.Name.ToUpper(), out SearchOptions option);
                if (!validParameter)
                {
                    continue;
                }
                switch (option)
                {
                    case SearchOptions.ID:
                        if (filter == null)
                        {
                            filter = Builders<Product>.Filter.Eq(c => c.Id, Guid.Parse(parameter.Value));
                        }
                        else
                        {
                            filter = Builders<Product>.Filter.Eq(c => c.Id, Guid.Parse(parameter.Value)) & filter;

                        }
                        break;
                    case SearchOptions.NAME:
                        if (filter == null)
                        {
                            filter = Builders<Product>.Filter.Eq(c => c.Name, (parameter.Value));
                        }
                        else
                        {
                            filter = Builders<Product>.Filter.Eq(c => c.Name, (parameter.Value)) & filter;

                        }
                        break;
                    case SearchOptions.BRAND:
                        if (filter == null)
                        {
                            filter = Builders<Product>.Filter.Eq(c => c.Brand, (parameter.Value));
                        }
                        else
                        {
                            filter = Builders<Product>.Filter.Eq(c => c.Brand, (parameter.Value)) & filter;

                        }
                        break;

                }

            }
            if (filter == null) throw new ArgumentException("Invalid search parameters specified");
            List<Product> result = await _context.Products.Find((FilterDefinition<Product>)filter).ToListAsync();

            return result;
        }

        public ErrorOr<Purchase> GetPurchase(Guid id)
        {
            FilterDefinition<Purchase> filter = Builders<Purchase>.Filter.Eq(c => c.Id, id);

            var purchases = _context.Purchases.FindAsync(filter).Result.FirstOrDefault();
            if (purchases != null)
            {
                return purchases;
            }

            return Errors.Purchase.NotFound;
        }

        public async Task<ErrorOr<List<Purchase>>> GetPurchases(List<SearchParameter> searchParameters)
        {

            FilterDefinition<Purchase> filter = Builders<Purchase>.Filter.Ne("isDeleted", true);
            foreach (var parameter in searchParameters.Where(
                    parameter => !string.IsNullOrEmpty(parameter.Name) && !string.IsNullOrEmpty(parameter.Value)))
            {
                var validParameter = Enum.TryParse(parameter.Name.ToUpper(), out SearchOptions option);
                if (!validParameter)
                {
                    continue;
                }
                switch (option)
                {
                    case SearchOptions.ID:
                        if (filter == null)
                        {
                            filter = Builders<Purchase>.Filter.Eq(c => c.Id, Guid.Parse(parameter.Value));
                        }
                        else
                        {
                            filter = Builders<Purchase>.Filter.Eq(c => c.Id, Guid.Parse(parameter.Value)) & filter;

                        }
                        break;
                 

                }

            }
            if (filter == null) throw new ArgumentException("Invalid search parameters specified");
            List<Purchase> result = await _context.Purchases.Find((FilterDefinition<Purchase>)filter).ToListAsync();

            return result;
        }

        public ErrorOr<UpsertedProduct> UpsertBranch(Branch branch)
        {
            FilterDefinition<Branch> filter = Builders<Branch>.Filter.Eq(c => c.Id, branch.Id);

            var result = _context.Branches.FindAsync(filter).Result.FirstOrDefault();
            if (result != null)
            {
                _context.Branches.ReplaceOneAsync(filter, branch);
                return new UpsertedProduct(true);
            }
            return Errors.Branch.NotFound;
        }

        public ErrorOr<UpsertedProduct> UpsertInventory(Inventory inventory)
        {
            FilterDefinition<Inventory> filter = Builders<Inventory>.Filter.Eq(c => c.Id, inventory.Id);

            var result = _context.Inventories.FindAsync(filter).Result.FirstOrDefault();
            if (result != null)
            {
                _context.Inventories.ReplaceOneAsync(filter, inventory);
                return new UpsertedProduct(true);
            }
            return Errors.Inventory.NotFound;
        }

        public ErrorOr<UpsertedProduct> UpsertProduct(Product product)
        {
            FilterDefinition<Product> filter = Builders<Product>.Filter.Eq(c => c.Id, product.Id);

            var result = _context.Products.FindAsync(filter).Result.FirstOrDefault();
            if (result != null)
            {
                _context.Products.ReplaceOneAsync(filter, product);
                return new UpsertedProduct(true);
            }
            return Errors.Product.NotFound;
        }

        public ErrorOr<UpsertedProduct> UpsertPurchase(Purchase purchases)
        {
            FilterDefinition<Purchase> filter = Builders<Purchase>.Filter.Eq(c => c.Id, purchases.Id);

            var result = _context.Purchases.FindAsync(filter).Result.FirstOrDefault();
            if (result != null)
            {
                _context.Purchases.ReplaceOneAsync(filter, purchases);
                return new UpsertedProduct(true);
            }
            return Errors.Purchase.NotFound;
        }
    }
}
