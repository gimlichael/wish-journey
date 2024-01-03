using System;
using System.Threading.Tasks;
using Savvyio;
using Savvyio.Domain;
using Savvyio.Handlers;

namespace Wish.Events
{
    internal class JournalEventHandler : DomainEventHandler
    {
        private readonly IOwnerRepository _ownerRepository;

        public JournalEventHandler(IOwnerRepository ownerRepository)
        {
            _ownerRepository = ownerRepository;
        }

        protected override void RegisterDelegates(IFireForgetRegistry<IDomainEvent> handlers)
        {
            handlers.RegisterAsync<JournalOwnerChanged>(OnOwnerChangedAsync);
        }

        private async Task OnOwnerChangedAsync(JournalOwnerChanged @event)
        {
            var journal = await _ownerRepository.FindAllAsync(owner => owner.EmailAddress.Equals(@event.EmailAddress, StringComparison.OrdinalIgnoreCase)).SingleOrDefaultAsync().ConfigureAwait(false);
            if (journal != null) { throw new DomainException($"Email address '{@event.EmailAddress}' is already registered."); }
        }
    }
}
