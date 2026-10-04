using Application.Promotions.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PromotionsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public PromotionsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        // GET: /api/promotions
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var promotions = await _mediator.Send(new GetPromotionsQuery());
            return Ok(promotions);
        }
    }
}
