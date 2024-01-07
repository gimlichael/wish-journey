using Cuemon.Extensions;
using Cuemon.Extensions.Collections.Generic;
using Savvyio.Commands;
using Savvyio.Commands.Messaging;
using Savvyio.Extensions.DependencyInjection.Messaging;
using Savvyio.Handlers;
using Wish.JournalApplication.Commands;

namespace Wish.JournalEventSvc.Handlers
{
	public class StatusCommandHandler : CommandHandler
	{
		private readonly IPointToPointChannel<ICommand, StatusCommandHandler> _commandQueue;

		public StatusCommandHandler(IPointToPointChannel<ICommand, StatusCommandHandler> commandQueue)
		{
			_commandQueue = commandQueue;
		}

		protected override void RegisterDelegates(IFireForgetRegistry<ICommand> handlers)
		{
			handlers.RegisterAsync<CreateStatus>(command => _commandQueue.SendAsync(command.ToMessage($"urn:status:id:{command.CorrelationId:N}".ToUri(), "journaleventsvc.status.create").Yield()));
			handlers.RegisterAsync<UpdateStatus>(command => _commandQueue.SendAsync(command.ToMessage($"urn:status:id:{command.CorrelationId:N}".ToUri(), "journaleventsvc.status.update").Yield()));
		}
	}
}
