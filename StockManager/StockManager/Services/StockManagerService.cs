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

        public int GetUserId() => int.Parse(_httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier));

        public string GetUserEmail() => _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.Name);

        public async Task<ServiceResponse<string>> Login(string email, string password)
        {
            FilterDefinition<User> filter = Builders<User>.Filter.Eq(c => c.Email, email);
            var response = new ServiceResponse<string>();
            var user = (await _context.Users.FindAsync(filter)).FirstOrDefault();
            if (user == null)
            {
                response.Success = false;
                response.Message = "User not found.";
            }
            else if (!VerifyPasswordHash(password, user.PasswordHash, user.PasswordSalt))
            {
                response.Success = false;
                response.Message = "Wrong password.";
            }
            else
            {
                response.Data = CreateToken(user);
            }

            return response;
        }

        public async Task<ServiceResponse<Guid>> Register(User user, string password)
        {
            if (await UserExists(user.Email))
            {
                return new ServiceResponse<Guid>
                {
                    Success = false,
                    Message = "User already exists."
                };
            }

            CreatePasswordHash(password, out byte[] passwordHash, out byte[] passwordSalt);

            user.PasswordHash = passwordHash;
            user.PasswordSalt = passwordSalt;

            await _context.Users.InsertOneAsync(user);


            return new ServiceResponse<Guid> { Data = user.Id, Message = "Registration successful!" };
        }

        public async Task<bool> UserExists(string email)
        {
            FilterDefinition<User> filter = Builders<User>.Filter.Eq(c => c.Email, email);
            var user = (await _context.Users.FindAsync(filter)).FirstOrDefault();
            if (user != null)
            {
                return true;
            }
            return false;
        }

        private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac
                    .ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }
        }

        private bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512(passwordSalt))
            {
                var computedHash =
                    hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                return computedHash.SequenceEqual(passwordHash);
            }
        }

        private string CreateToken(User user)
        {
            List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Email),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8
                .GetBytes(_configuration.GetSection("AppSettings:Token").Value));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var token = new JwtSecurityToken(
                    claims: claims,
                    expires: DateTime.Now.AddDays(1),
                    signingCredentials: creds);

            var jwt = new JwtSecurityTokenHandler().WriteToken(token);

            return jwt;
        }

        public async Task<ServiceResponse<bool>> ChangePassword(int userId, string newPassword)
        {
            FilterDefinition<User> filter = Builders<User>.Filter.Eq(c => c.UserData.UserId, userId);

            var user = (await _context.Users.FindAsync(filter)).FirstOrDefault();
            if (user == null)
            {
                return new ServiceResponse<bool>
                {
                    Success = false,
                    Message = "User not found."
                };
            }

            CreatePasswordHash(newPassword, out byte[] passwordHash, out byte[] passwordSalt);

            user.PasswordHash = passwordHash;
            user.PasswordSalt = passwordSalt;

            await _context.Users.ReplaceOneAsync(filter, user);

            return new ServiceResponse<bool> { Data = true, Message = "Password has been changed." };
        }

        public async Task<User> GetUserByEmail(string email)
        {
            FilterDefinition<User> filter = Builders<User>.Filter.Eq(c => c.Email, email);
            return (await _context.Users.FindAsync(filter)).FirstOrDefault();
        }

        public async Task<ErrorOr<List<User>>> GetUsers(List<SearchParameter> searchParameters)
        {


            FilterDefinition<User> filter = Builders<User>.Filter.Ne("isDeleted", true);
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
                            filter = Builders<User>.Filter.Eq(c => c.Id, Guid.Parse(parameter.Value));
                        }
                        else
                        {
                            filter = Builders<User>.Filter.Eq(c => c.Id, Guid.Parse(parameter.Value)) & filter;

                        }
                        break;
                    case SearchOptions.NAME:
                        if (filter == null)
                        {
                            filter = Builders<User>.Filter.Eq(c => c.UserData.FirstName, (parameter.Value));
                        }
                        else
                        {
                            filter = Builders<User>.Filter.Eq(c => c.UserData.FirstName, (parameter.Value)) & filter;

                        }
                        break;
                    case SearchOptions.EMAIL:
                        if (filter == null)
                        {
                            filter = Builders<User>.Filter.Eq(c => c.Email, (parameter.Value));
                        }
                        else
                        {
                            filter = Builders<User>.Filter.Eq(c => c.Email, (parameter.Value)) & filter;

                        }
                        break;

                }

            }
            if (filter == null) throw new ArgumentException("Invalid search parameters specified");
            List<User> result = await _context.Users.Find((FilterDefinition<User>)filter).ToListAsync();

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
