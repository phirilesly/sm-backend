using Microsoft.AspNetCore.Mvc;


namespace Stockmanager.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly IStockManagerService _stockManagerService;

        public ProductsController(IStockManagerService stockManagerService)
        {
            _stockManagerService = stockManagerService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductResponse>>> Get()
        {
            var products = await _stockManagerService.GetProducts();
            return Ok(products);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ProductResponse>> Get(Guid id)
        {
            var product = await _stockManagerService.GetProduct(id);
            return Ok(product);
        }

        [HttpPost]
        public async Task<ActionResult<ProductResponse>> Post(CreateProductRequest request)
        {
            var product = await _stockManagerService.CreateProduct(request);
            return Ok(product);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ProductResponse>> Put(Guid id, UpsertProductRequest request)
        {
            var product = await _stockManagerService.UpsertProduct(id, request);
            return Ok(product);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<ProductResponse>> Delete(Guid id)
        {
            var product = await _stockManagerService.DeleteProduct(id);
            return Ok(product);
        }
    }
}