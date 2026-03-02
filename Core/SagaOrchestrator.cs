using Librery.Saga.Compensations.Abstractions;
using Microsoft.Extensions.Logging;

namespace Librery.Saga.Compensations.Core
{
    public class SagaOrchestrator : ISagaOrchestrator
    {
        private readonly Stack<(string Name, Func<Task> Action)> _compensations = new();
        private readonly ILogger<SagaOrchestrator>? _logger;

        public SagaOrchestrator(ILogger<SagaOrchestrator>? logger = null)
        {
            _logger = logger;
        }

        public void AddCompensation(string stepName, Func<Task> action)
        {
            _compensations.Push((stepName, action));
        }

        public async Task RollbackAsync()
        {
            while (_compensations.Count > 0)
            {
                var (name, action) = _compensations.Pop();

                try
                {
                    _logger?.LogWarning("Compensando paso: {Step}", name);
                    await action();
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, "Error compensando paso: {Step}", name);
                }
            }
        }
    }
}
