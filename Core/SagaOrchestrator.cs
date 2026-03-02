using Librery.Saga.Compensations.Abstractions;
using Microsoft.Extensions.Logging;

namespace Librery.Saga.Compensations.Core
{
    public class SagaOrchestrator : ISagaOrchestrator
    {
        private readonly Stack<(string Name, Func<Task> Action)> _compensations = new();
        private readonly ILogger<SagaOrchestrator>? _logger;
        private bool _rollbackExecuted = false;

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
            if (_rollbackExecuted)
                return;

            _rollbackExecuted = true;

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

        public async Task<T> ExecuteStepAsync<T>(string stepName, Func<Task<T>> action, Func<T, Task> compensation)
        {
            var result = await action();

            AddCompensation(stepName, () => compensation(result));

            return result;
        }
    }
}
