using Savvyio.Queries;
using System;
using Wish.JournalApplication.Views;

namespace Wish.JournalApplication.Queries
{
	public record GetStatus : Query<StatusViewModel>
	{
		public GetStatus(string ownerId, string correlationId)
		{
			OwnerId = Guid.Parse(ownerId);
			CorrelationId = Guid.Parse(correlationId);
		}

		public Guid OwnerId { get; }

		public Guid CorrelationId { get; }
	}
}
