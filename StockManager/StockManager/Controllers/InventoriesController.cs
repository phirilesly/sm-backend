using ErrorOr;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using StockManager.Contracts.Common;
using StockManager.Contracts.Inventory;
using StockManager.Models;
using StockManager.Services;

namespace StockManager.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InventoriesController : ControllerBase
    {
        private readonly IStockManagerService _stoctManagerService;

        public InventoriesController(IStockManagerService stockManagerService)
        {
            _stoctManagerService = stockManagerService;
        }

        [HttpPost]
        public IActionResult CreateProduct(CreateInventoryRequest request)
        {
            ErrorOr<Inventory> requestToInventoryResult = Inventory.From(request);

            if (requestToInventoryResult.IsError)
            {
                return Problem(requestToInventoryResult.Errors);
            }

            var inventory = requestToInventoryResult.Value;
            ErrorOr<Created> createInventoryResult = _stoctManagerService.CreateInventory(inventory);

            return createInventoryResult.Match(
                created => CreatedAtGetInventory(inventory),
                errors => Problem(errors));
        }

        [HttpGet("{id:guid}")]
        public IActionResult GetInventory(Guid id)
        {
            ErrorOr<Inventory> getInventoryResult = _stoctManagerService.GetInventory(id);

            return getInventoryResult.Match(
                Inventory => Ok(MapInventoryResponse(Inventory)),
                errors => Problem(errors));
        }

        [HttpGet("search")]
        public async Task<IActionResult> GetInventories(Guid? Id,Guid? productId,Guid? branchId)
        {
            var searchParameters = new List<SearchParameter>();
            if (Id.HasValue)
            {
                searchParameters.Add(new SearchParameter { Name = "ID", Value = Id.ToString() });
            }
            if (productId.HasValue)
            {
                searchParameters.Add(new SearchParameter { Name = "PRODUCTID", Value = productId.ToString() });
            }
            if (branchId.HasValue)
            {
                searchParameters.Add(new SearchParameter { Name = "BRANCHID", Value = branchId.ToString() });
            }

            ErrorOr<List<Inventory>> getInventoryResult = await _stoctManagerService.GetInventories(searchParameters);

            return getInventoryResult.Match(
                Inventories => Ok((Inventories)),
                errors => Problem(errors));
        }


        [HttpPut("{id:guid}")]
        public IActionResult UpsertInventory(Guid id, UpsertInventoryRequest request)
        {
            ErrorOr<Inventory> requestToInventoryResult = Inventory.From(id, request);

            if (requestToInventoryResult.IsError)
            {
                return Problem(requestToInventoryResult.Errors);
            }

            var inventory = requestToInventoryResult.Value;
            ErrorOr<UpsertedProduct> upsertInventoryResult = _stoctManagerService.UpsertInventory(inventory);

            return upsertInventoryResult.Match(
                upserted => upserted.IsNewlyCreated ? CreatedAtGetInventory(inventory) : NoContent(),
                errors => Problem(errors));
        }

        [HttpDelete("{id:guid}")]
        public IActionResult DeleteInventory(Guid id)
        {
            ErrorOr<Deleted> deleteInventoryResult = _stoctManagerService.DeleteInventory(id);

            return deleteInventoryResult.Match(
                deleted => NoContent(),
                errors => Problem(errors));
        }

        private static InventoryResponse MapInventoryResponse(Inventory inventory)
        {
            return new InventoryResponse(
                inventory.BranchId,
                inventory.Id,
                inventory.OrderDate,
                inventory.TotalPrice,
                inventory.OrderItems
              );
        }

        private CreatedAtActionResult CreatedAtGetInventory(Inventory inventory)
        {
            return CreatedAtAction(
               actionName: nameof(GetInventory),
               routeValues: new { id = inventory.Id },
               value: MapInventoryResponse(inventory));
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
