using ErrorOr;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using StockManager.Contracts.Common;
using StockManager.Contracts.Purchase;
using StockManager.Models;
using StockManager.Services;

namespace StockManager.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PurchasesController : ControllerBase
    {

        private readonly IStockManagerService _stoctManagerService;

        public PurchasesController(IStockManagerService stockManagerService)
        {
            _stoctManagerService = stockManagerService;
        }

        [HttpPost]
        public IActionResult CreatePurchase(CreatePurchasesRequest request)
        {
            ErrorOr<Purchase> requestToPurchaseResult = Purchase.From(request);

            if (requestToPurchaseResult.IsError)
            {
                return Problem(requestToPurchaseResult.Errors);
            }

            var purchase = requestToPurchaseResult.Value;
            ErrorOr<Created> createPurchaseResult = _stoctManagerService.CreatePurchase(purchase);

            return createPurchaseResult.Match(
                created => CreatedAtGetPurchase(purchase),
                errors => Problem(errors));
        }

        [HttpGet("{id:guid}")]
        public IActionResult GetPurchase(Guid id)
        {
            ErrorOr<Purchase> getPurchaseResult = _stoctManagerService.GetPurchase(id);

            return getPurchaseResult.Match(
                Purchase => Ok(MapPurchaseResponse(Purchase)),
                errors => Problem(errors));
        }

        [HttpGet("search")]
        public async Task<IActionResult> GetPurchases(Guid? Id,Guid? branchId,Guid? productId)
        {
            var searchParameters = new List<SearchParameter>();
            if (Id.HasValue)
            {
                searchParameters.Add(new SearchParameter { Name = "ID", Value = Id.ToString() });
            }
            if (branchId.HasValue)
            {
                searchParameters.Add(new SearchParameter { Name = "BRANCHID", Value = branchId.ToString() });
            }
            if (productId.HasValue)
            {
                searchParameters.Add(new SearchParameter { Name = "PRODUCTID", Value = productId.ToString() });
            }
            ErrorOr<List<Purchase>> getPurchaseResult = await _stoctManagerService.GetPurchases(searchParameters);

            return getPurchaseResult.Match(
                Purchases => Ok((Purchases)),
                errors => Problem(errors));
        }

        [HttpPut("{id:guid}")]
        public IActionResult UpsertPurchase(Guid id, UpsertPurchasesRequest request)
        {
            ErrorOr<Purchase> requestToPurchaseResult = Purchase.From(id, request);

            if (requestToPurchaseResult.IsError)
            {
                return Problem(requestToPurchaseResult.Errors);
            }

            var purchase = requestToPurchaseResult.Value;
            ErrorOr<UpsertedProduct> upsertPurchaseResult = _stoctManagerService.UpsertPurchase(purchase);

            return upsertPurchaseResult.Match(
                upserted => upserted.IsNewlyCreated ? CreatedAtGetPurchase(purchase) : NoContent(),
                errors => Problem(errors));
        }

        [HttpDelete("{id:guid}")]
        public IActionResult DeletePurchase(Guid id)
        {
            ErrorOr<Deleted> deletePurchaseResult = _stoctManagerService.DeletePurchase(id);

            return deletePurchaseResult.Match(
                deleted => NoContent(),
                errors => Problem(errors));
        }

        private static PurchasesResponse MapPurchaseResponse(Purchase purchase)
        {
            return new PurchasesResponse(
                purchase.Id,
                purchase.ProductId,
                purchase.BranchId,
                purchase.SaleDate,
                purchase.Quantity,
                purchase.Price
               );
        }

        private CreatedAtActionResult CreatedAtGetPurchase(Purchase purchase)
        {
            return CreatedAtAction(
               actionName: nameof(GetPurchase),
               routeValues: new { id = purchase.Id },
               value: MapPurchaseResponse(purchase));
        }
        protected IActionResult Problem(List<Error> errors)
        {
            if (errors.All(e => e.Type == ErrorType.Validation))
            {
                var modelStateDictionary = new ModelStateDictionary();

                foreach (var error in errors)
                {
                    modelStateDictionary.AddModelError(error.Code, error.Description);
                }

                return ValidationProblem(modelStateDictionary);
            }

            if (errors.Any(e => e.Type == ErrorType.Unexpected))
            {
                return Problem();
            }

            var firstError = errors[0];

            var statusCode = firstError.Type switch
            {
                ErrorType.NotFound => StatusCodes.Status404NotFound,
                ErrorType.Validation => StatusCodes.Status400BadRequest,
                ErrorType.Conflict => StatusCodes.Status409Conflict,
                _ => StatusCodes.Status500InternalServerError
            };

            return Problem(statusCode: statusCode, title: firstError.Description);
        }

    }
}
