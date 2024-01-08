using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Savvyio;
using Savvyio.Extensions;
using Wish.JournalApplication.Commands;
using Wish.JournalApplication.Inputs;
using Wish.JournalApplication.Projections;
using Wish.JournalApplication.Queries;
using Wish.JournalApplication.Views;

namespace Wish.JournalApi.Controllers.V1
{
    [ApiController]
    [Route("[controller]")]
    public class JournalsController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<JournalsController> _logger;

        public JournalsController(IMediator mediator, ILogger<JournalsController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        public async Task<IActionResult> Post([FromBody] JournalInputModel input)
        {
            var command = new CreateJournal(new OwnerId(HttpContext.User.Claims.OwnerIdOrDefault()))
            {
                Description = new Description(input.Description),
                Title = new Title(input.Title)
            };

            await _mediator.CommitAsync(new CreateStatus(command.OwnerId, command.GetCorrelationId())
            {
                Action = StatusAction.Create,
                Created = DateTime.UtcNow,
                Message = "Creating a new journal.",
                Endpoint = Request.GetDisplayUrl()
            }).ConfigureAwait(false);

            await _mediator.CommitAsync(command).ConfigureAwait(false);

            _logger.LogInformation("{nameOf} was issued: {command}", nameof(CreateJournal), command);
            
            Response.Headers.RetryAfter = new RetryConditionHeaderValue(TimeSpan.FromSeconds(5)).ToString();
            return AcceptedAtAction("Get", "Status", new { correlationId = command.GetCorrelationId() });
        }

        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        public async Task<IActionResult> Put([FromRoute] string id, [FromBody] JournalInputModel model)
        {
            var command = new UpdateJournal(new OwnerId(HttpContext.User.Claims.OwnerIdOrDefault()), new JournalId(id))
            {
                Description = new Description(model.Description),
                Title = new Title(model.Title)
            };

            await _mediator.CommitAsync(new CreateStatus(command.OwnerId, command.GetCorrelationId())
            {
                Action = StatusAction.Update,
                Created = DateTime.UtcNow,
                Message = "Updating existing journal.",
                Endpoint = Request.GetDisplayUrl()
            }).ConfigureAwait(false);

            await _mediator.CommitAsync(command).ConfigureAwait(false);
            
            _logger.LogInformation("{nameOf} was issued: {command}", nameof(UpdateJournal), command);

            Response.Headers.RetryAfter = new RetryConditionHeaderValue(TimeSpan.FromSeconds(5)).ToString();
            return AcceptedAtAction("Get", "Status", new { correlationId = command.GetCorrelationId() });
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        public async Task<IActionResult> Delete([FromRoute] string id)
        {
            var command = new DeleteJournal(new OwnerId(HttpContext.User.Claims.OwnerIdOrDefault()), new JournalId(id));

            await _mediator.CommitAsync(new CreateStatus(command.OwnerId, command.GetCorrelationId())
            {
                Action = StatusAction.Delete,
                Created = DateTime.UtcNow,
                Message = "Deleting an existing journal.",
                Endpoint = Request.GetDisplayUrl()
            }).ConfigureAwait(false);

            await _mediator.CommitAsync(command).ConfigureAwait(false);

            _logger.LogWarning("{nameOf} was issued: {command}", nameof(DeleteJournal), command);

            Response.Headers.RetryAfter = new RetryConditionHeaderValue(TimeSpan.FromSeconds(5)).ToString();
            return AcceptedAtAction("Get", "Status", new { correlationId = command.GetCorrelationId() });
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<JournalCollectionViewModel>>> List([FromQuery] int limit = int.MaxValue)
        {
            return Ok(await _mediator.QueryAsync(new ListJournal(HttpContext.User.Claims.OwnerIdOrDefault(), limit)));
        }
        
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<JournalViewModel>> Get([FromRoute] string id)
        {
            return Ok(await _mediator.QueryAsync(new GetJournal(HttpContext.User.Claims.OwnerIdOrDefault(), id)));
        }

        [HttpPost("{id}/entries")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        public async Task<IActionResult> PostEntry([FromRoute] string id, [FromBody] JournalEntryCreateInputModel model)
        {
            var command = new CreateJournalEntry(new OwnerId(HttpContext.User.Claims.OwnerIdOrDefault()), new JournalId(id))
            {
                Timestamp = model.Timestamp.HasValue ? new Timestamp(model.Timestamp.Value) : null,
                Coordinates = new Coordinates(model.Latitude, model.Longitude),
                Notes = new Notes(model.Notes)
            };

            await _mediator.CommitAsync(new CreateStatus(command.OwnerId, command.GetCorrelationId())
            {
                Action = StatusAction.Create,
				Created = DateTime.UtcNow,
                Message = "Creating a new entry for a journal.",
                Endpoint = Request.GetDisplayUrl()
            }).ConfigureAwait(false);

            await _mediator.CommitAsync(command).ConfigureAwait(false);

            _logger.LogInformation("{nameOf} was issued: {command}", nameof(CreateJournalEntry), command);
            
            Response.Headers.RetryAfter = new RetryConditionHeaderValue(TimeSpan.FromSeconds(5)).ToString();
            return AcceptedAtAction("Get", "Status", new { correlationId = command.GetCorrelationId() });
        }

        [HttpGet("{id}/entries")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<JournalEntryCollectionViewModel>>> ListEntries([FromRoute] string id, [FromQuery] int limit = int.MaxValue)
        {
            return Ok(await _mediator.QueryAsync(new ListJournalEntries(HttpContext.User.Claims.OwnerIdOrDefault(), id, limit)));
        }

        [HttpGet("{id}/entries/{entryId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<JournalEntryViewModel>> GetEntry([FromRoute] string id, [FromRoute] string entryId)
        {
            return Ok(await _mediator.QueryAsync(new GetJournalEntry(HttpContext.User.Claims.OwnerIdOrDefault(), id, entryId)));
        }

        [HttpPut("{id}/entries/{entryId}")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        public async Task<IActionResult> PutEntry([FromRoute] string id, [FromRoute] string entryId, [FromBody] JournalEntryUpdateInputModel model)
        {
            var command = new UpdateJournalEntry(new OwnerId(HttpContext.User.Claims.OwnerIdOrDefault()), new JournalId(id), new JournalEntryId(entryId))
            {
                Notes = new Notes(model.Notes)
            };

            await _mediator.CommitAsync(new CreateStatus(command.OwnerId, command.GetCorrelationId())
            {
                Action = StatusAction.Update,
                Created = DateTime.UtcNow,
                Message = "Updating an existing entry for a journal.",
                Endpoint = Request.GetDisplayUrl()
            }).ConfigureAwait(false);

            await _mediator.CommitAsync(command).ConfigureAwait(false);

            _logger.LogInformation("{nameOf} was issued: {command}", nameof(UpdateJournalEntry), command);
            
            Response.Headers.RetryAfter = new RetryConditionHeaderValue(TimeSpan.FromSeconds(5)).ToString();
            return AcceptedAtAction("Get", "Status", new { correlationId = command.GetCorrelationId() });
        }

        [HttpDelete("{id}/entries/{entryId}")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        public async Task<IActionResult> DeleteEntry([FromRoute] string id, [FromRoute] string entryId)
        {
            var command = new DeleteJournalEntry(new OwnerId(HttpContext.User.Claims.OwnerIdOrDefault()), new JournalId(id), new JournalEntryId(entryId));

            await _mediator.CommitAsync(new CreateStatus(command.OwnerId, command.GetCorrelationId())
            {
                Action = StatusAction.Delete,
                Created = DateTime.UtcNow,
                Message = "Deleting an existing journal.",
                Endpoint = Request.GetDisplayUrl()
            }).ConfigureAwait(false);

            await _mediator.CommitAsync(command).ConfigureAwait(false);
            
            _logger.LogWarning("{nameOf} was issued: {command}", nameof(DeleteJournalEntry), command);

            Response.Headers.RetryAfter = new RetryConditionHeaderValue(TimeSpan.FromSeconds(5)).ToString();
            return AcceptedAtAction("Get", "Status", new { correlationId = command.GetCorrelationId() });
        }
    }
}
