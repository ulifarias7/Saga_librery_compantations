namespace Librery.Saga.Compensations.Abstractions
{
    /// <summary>
    /// Orquestador de operaciones bajo el patrón Saga.
    /// Permite registrar compensaciones para ejecutar en caso de fallo.
    /// </summary>
    public interface ISagaOrchestrator
    {
        /// <summary>
        /// Registra una acción de compensación que será ejecutada
        /// en orden inverso si se invoca RollbackAsync.
        /// </summary>
        /// <param name="stepName">Nombre descriptivo del paso.</param>
        /// <param name="action">Acción asincrónica que deshace el paso.</param>
        void AddCompensation(string stepName, Func<Task> action);

        /// <summary>
        /// Ejecuta todas las compensaciones registradas
        /// en orden inverso (LIFO).
        /// </summary>
        Task RollbackAsync();

        /// <summary>
        /// Ejecuta una operación y, si resulta exitosa,
        /// registra su compensación correspondiente.
        /// </summary>
        /// <typeparam name="T">Tipo de resultado de la operación.</typeparam>
        /// <param name="stepName">Nombre descriptivo del paso.</param>
        /// <param name="action">Operación principal a ejecutar.</param>
        /// <param name="compensation">
        /// Acción de compensación que recibirá el resultado exitoso.
        /// </param>
        /// <returns>Resultado de la operación.</returns>
        Task<T> ExecuteStepAsync<T>(string stepName,Func<Task<T>> action,Func<T, Task> compensation);
    }
}
