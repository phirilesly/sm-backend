using ErrorOr;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using StockManager.Contracts.Common;
using StockManager.Contracts.User;
using StockManager.Services;
using System.Security.Claims;

namespace StockManager.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IStockManagerService _authService;

        public AuthController(IStockManagerService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<ActionResult<ServiceResponse<int>>> Register(UserRegister request)
        {
            var response = await _authService.Register(
                new User
                {
                    Email = request.Email
                },
                request.Password);

            if (!response.Success)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }

        [HttpPost("login")]
        public async Task<ActionResult<ServiceResponse<string>>> Login(UserLogin request)
        {
            var response = await _authService.Login(request.Email, request.Password);
            if (!response.Success)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }

        [HttpPost("change-password"), Authorize]
        public async Task<ActionResult<ServiceResponse<bool>>> ChangePassword([FromBody] string newPassword)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var response = await _authService.ChangePassword(int.Parse(userId), newPassword);

            if (!response.Success)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }

        [HttpGet("search")]
        public async Task<IActionResult> GetUsers(string? email,string? name,Guid? Id)
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
            if (!string.IsNullOrEmpty(email))
            {
                searchParameters.Add(new SearchParameter { Name = "EMAIL", Value = email});
            }
            ErrorOr<List<User>> getBranchResult = await  _authService.GetUsers(searchParameters);

            return getBranchResult.Match(
                Branches => Ok((Branches)),
                errors => Problem(errors));
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
