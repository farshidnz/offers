using System.Threading.Tasks;

namespace CashrewardsOffers.Infrastructure.Services
{
    public abstract class EventInitialisationService
    {
        public bool IsTriggered { get; set; }

        public bool IsRunning { get; private set; }

        public async Task CheckForInitialisationRequests()
        {
            if (IsTriggered && !IsRunning)
            {
                IsRunning = true;
                await GenerateInitialEvents();
                IsTriggered = false;
                IsRunning = false;
            }
        }

        protected abstract Task GenerateInitialEvents();
    }
}
