using ErrorOr;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using StockManager.Contracts.Branch;
using StockManager.Contracts.Common;
using StockManager.Models;
using StockManager.Services;

namespace StockManager.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BranchesController : ControllerBase
    {
        private readonly IStockManagerService _stoctManagerService;

        public BranchesController(IStockManagerService stockManagerService)
        {
            _stoctManagerService = stockManagerService;
        }

        [HttpPost]
        public IActionResult CreateBranch(CreateBranchRequest request)
        {
            ErrorOr<Branch> requestToBranchResult = Branch.From(request);

            if (requestToBranchResult.IsError)
            {
                return Problem(requestToBranchResult.Errors);
            }

            var branch = requestToBranchResult.Value;
            ErrorOr<Created> createBranchResult = _stoctManagerService.CreateBranch(branch);

            return createBranchResult.Match(
                created => CreatedAtGetBranch(branch),
                errors => Problem(errors));
        }

        [HttpGet("{id:guid}")]
        public IActionResult GetBranch(Guid id)
        {
            ErrorOr<Branch> getBranchResult = _stoctManagerService.GetBranch(id);

            return getBranchResult.Match(
                Branch => Ok(MapBranchResponse(Branch)),
                errors => Problem(errors));
        }

        [HttpGet("search")]
        public async Task<IActionResult> GetBranch(Guid? Id,string? name)
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
         
            ErrorOr<List<Branch>> getBranchResult = await  _stoctManagerService.GetBranches(searchParameters);

            return getBranchResult.Match(
                Branches => Ok((Branches)),
                errors => Problem(errors));
        }

        [HttpPut("{id:guid}")]
        public IActionResult UpsertBranch(Guid id, UpsertBranchRequest request)
        {
            ErrorOr<Branch> requestToBranchResult = Branch.From(id, request);

            if (requestToBranchResult.IsError)
            {
                return Problem(requestToBranchResult.Errors);
            }

            var branch = requestToBranchResult.Value;
            ErrorOr<UpsertedProduct> upsertBranchResult = _stoctManagerService.UpsertBranch(branch);

            return upsertBranchResult.Match(
                upserted => upserted.IsNewlyCreated ? CreatedAtGetBranch(branch) : NoContent(),
                errors => Problem(errors));
        }

        [HttpDelete("{id:guid}")]
        public IActionResult DeleteBranch(Guid id)
        {
            ErrorOr<Deleted> deleteBranchResult = _stoctManagerService.DeleteBranch(id);

            return deleteBranchResult.Match(
                deleted => NoContent(),
                errors => Problem(errors));
        }

        private static BranchResponse MapBranchResponse(Branch branch)
        {
            return new BranchResponse(
                branch.Id,
                branch.Name,
                branch.Town,
                branch.Phone,
                branch.Address
                );
        }



        private CreatedAtActionResult CreatedAtGetBranch(Branch branch)
        {
            return CreatedAtAction(
               actionName: nameof(GetBranch),
               routeValues: new { id = branch.Id },
               value: MapBranchResponse(branch));
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
