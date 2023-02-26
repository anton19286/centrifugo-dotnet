using System.Threading.Tasks;

namespace Centrifugo.Sample.TokenProvider
{
    public interface ICentrifugoTokenProvider
    {
        Task<string> ConnectionTokenAsync(
            string clientId,
            string? clientProvidedInfo = null
        );
        public Task<string> SubscriptionTokenAsync(
            string clientId,
            string channel
        );
    }
}