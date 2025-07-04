using CaptainPayment.Core.Models;

namespace CaptainPayment.Core.Interfaces;

public interface ISetupIntentService
{
    Task<SetupIntentResult> CreateSetupIntentAsync(CreateSetupIntentRequest request, CancellationToken cancellationToken = default);

    Task<SetupIntentResult> GetSetupIntentAsync(string setupIntentId, CancellationToken cancellationToken = default);

    Task<SetupIntentResult> ConfirmSetupIntentAsync(string setupIntentId, ConfirmSetupIntentRequest request, CancellationToken cancellationToken = default);
}