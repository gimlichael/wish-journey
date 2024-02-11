using System;
using System.Threading.Tasks;
using Cuemon.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Savvyio.Extensions;
using Wish.JournalApplication.Projections;
using Wish.JournalApplication.Queries;
using Wish.JournalApplication.Views;

namespace Wish.JournalApi.Controllers.V1
{
	[Authorize]
    [ApiController]
    [Route("[controller]")]
    public class StatusController : ControllerBase
    {
	    private readonly IMediator _mediator;
	    private readonly ILogger<StatusController> _logger;

        public StatusController(IMediator mediator, ILogger<StatusController> logger)
        {
	        _mediator = mediator;
	        _logger = logger;
        }

        [HttpGet("{correlationId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status303SeeOther)]
        public async Task<ActionResult<StatusViewModel>> Get(string correlationId)
        {
	        var projection = await _mediator.QueryAsync(new GetStatus(HttpContext.User.Claims.OwnerIdOrDefault(), correlationId)).ConfigureAwait(false);
            if (projection == null) { return NotFound(); }
            switch (projection.Result)
            {
                case StatusResult.Completed:
                    switch (projection.Action)
                    {
                        case StatusAction.Delete:
                            return NoContent();
                        default:
                            return new SeeOtherResult(new Uri(string.Concat(projection.Endpoint, projection.EndpointRouteValue)));
                    }
                    
                default:
	                projection.Endpoint = null;
                    projection.EndpointRouteValue = null;
					return Ok(projection);
            }
        }
    }
}
