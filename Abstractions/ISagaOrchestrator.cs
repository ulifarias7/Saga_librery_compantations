namespace Librery.Saga.Compensations.Abstractions
{
    public interface ISagaOrchestrator
    {
        void AddCompensation(string stepName, Func<Task> action);
        Task RollbackAsync();
        Task<T> ExecuteStepAsync<T>(string stepName,Func<Task<T>> action,Func<T, Task> compensation);
    }
}
