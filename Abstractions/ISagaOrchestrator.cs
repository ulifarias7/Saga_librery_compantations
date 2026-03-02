namespace Librery.Saga.Compensations.Abstractions
{
    public interface ISagaOrchestrator
    {
        void AddCompensation(string stepName, Func<Task> action);
        Task RollbackAsync();
    }
}
