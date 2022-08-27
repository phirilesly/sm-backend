using ErrorOr;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using StockManager.Contracts.Common;
using StockManager.Contracts.Product;
using StockManager.Models;
using StockManager.Services;

namespace StockManager.Controllers
{
    public class ProductsController : ApiController
    {
        private readonly IStockManagerService _stoctManagerService;

        public ProductsController(IStockManagerService stockManagerService)
        {
            _stoctManagerService = stockManagerService;
        }

        [HttpPost]
        public IActionResult CreateProduct(CreateProductRequest request)
        {
            ErrorOr<Product> requestToProductResult = Product.From(request);

            if (requestToProductResult.IsError)
            {
                return Problem(requestToProductResult.Errors);
            }

            var product = requestToProductResult.Value;
            ErrorOr<Created> createProductResult = _stoctManagerService.CreateProduct(product);

            return createProductResult.Match(
                created => CreatedAtGetProduct(product),
                errors => Problem(errors));
        }

        [HttpGet("{id:guid}")]
        public IActionResult GetProduct(Guid id)
        {
            ErrorOr<Product> getProductResult = _stoctManagerService.GetProduct(id);

            return getProductResult.Match(
                Product => Ok(MapProductResponse(Product)),
                errors => Problem(errors));
        }

        [HttpGet("search")]
        public async Task<IActionResult> GetProducts(string? name,string? brand,string? category,Guid? Id)
        {
            var searchParameters = new List<SearchParameter>();
            if (Id.HasValue)
            {
                searchParameters.Add(new SearchParameter { Name = "ID", Value = Id.ToString() });
            }
            if (!string.IsNullOrEmpty(name))
            {
                searchParameters.Add(new SearchParameter { Name = "NAME", Value = name });
            }
            if (!string.IsNullOrEmpty(brand))
            {
                searchParameters.Add(new SearchParameter { Name = "BRAND", Value = brand });
            }
            if (!string.IsNullOrEmpty(category))
            {
                searchParameters.Add(new SearchParameter { Name = "CATEGORY", Value = category });
            }
            ErrorOr<List<Product>> getProductResult = await _stoctManagerService.GetProducts(searchParameters);

            return getProductResult.Match(
                Products => Ok((Products)),
                errors => Problem(errors));
        }

        [HttpPut("{id:guid}")]
        public IActionResult UpsertProduct(Guid id, UpsertProductRequest request)
        {
            ErrorOr<Product> requestToProductResult = Product.From(id, request);

            if (requestToProductResult.IsError)
            {
                return Problem(requestToProductResult.Errors);
            }

            var product = requestToProductResult.Value;
            ErrorOr<UpsertedProduct> upsertProductResult = _stoctManagerService.UpsertProduct(product);

            return upsertProductResult.Match(
                upserted => upserted.IsNewlyCreated ? CreatedAtGetProduct(product) : NoContent(),
                errors => Problem(errors));
        }

        [HttpDelete("{id:guid}")]
        public IActionResult DeleteProduct(Guid id)
        {
            ErrorOr<Deleted> deleteProductResult = _stoctManagerService.DeleteProduct(id);

            return deleteProductResult.Match(
                deleted => NoContent(),
                errors => Problem(errors));
        }

        private static ProductResponse MapProductResponse(Product product)
        {
            return new ProductResponse(
                product.Id,
                product.Name,
                product.Description,
                product.Barcode,
                product.Category,
                product.SubCategory,
                product.Brand,
                product.Supplier);
        }

        private CreatedAtActionResult CreatedAtGetProduct(Product product)
        {
            return CreatedAtAction(
               actionName: nameof(GetProduct),
               routeValues: new { id = product.Id },
               value: MapProductResponse(product));
        }

    }
}
