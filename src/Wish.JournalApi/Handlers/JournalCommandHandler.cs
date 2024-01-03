using Cuemon.Extensions;
using Savvyio.Commands;
using Savvyio.Commands.Messaging;
using Savvyio.Extensions.DependencyInjection.Messaging;
using Savvyio.Handlers;
using Savvyio.Messaging;
using Wish.JournalApplication.Commands;

namespace Wish.JournalApi.Handlers
{
	public class JournalCommandHandler : CommandHandler
	{
		private readonly IPointToPointChannel<ICommand> _commandQueue;

		public JournalCommandHandler(IPointToPointChannel<ICommand, JournalCommandHandler> commandQueue)
		{
			_commandQueue = commandQueue;
		}

		protected override void RegisterDelegates(IFireForgetRegistry<ICommand> handlers)
		{
			handlers.RegisterAsync<CreateJournal>(command => _commandQueue.SendAsync(command.ToMessage("urn:journals".ToUri(), "journalapi.journals.create-command")));
			handlers.RegisterAsync<UpdateJournal>(command => _commandQueue.SendAsync(command.ToMessage($"urn:journals:id:{command.JournalId}".ToUri(), "journalapi.journals.update-command")));
			handlers.RegisterAsync<DeleteJournal>(command => _commandQueue.SendAsync(command.ToMessage($"urn:journals:id:{command.JournalId}".ToUri(), "journalapi.journals.delete-command")));
			handlers.RegisterAsync<CreateJournalEntry>(command => _commandQueue.SendAsync(command.ToMessage($"urn:journals:id:{command.JournalId}:entries".ToUri(), "journalapi.journals.entries.create-command")));
			handlers.RegisterAsync<UpdateJournalEntry>(command => _commandQueue.SendAsync(command.ToMessage($"urn:journals:id:{command.JournalId}:entries:id:{command.EntryId}".ToUri(), "journalapi.journals.entries.update-command")));
			handlers.RegisterAsync<DeleteJournalEntry>(command => _commandQueue.SendAsync(command.ToMessage($"urn:journals:id:{command.JournalId}:entries:id:{command.EntryId}".ToUri(), "journalapi.journals.entries.delete-command")));
		}
	}
}
