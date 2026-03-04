using Librery.Saga.Compensations.Abstractions;
using Microsoft.Extensions.Logging;

namespace Librery.Saga.Compensations.Core
{
    /// <summary>
    /// Implementación en memoria del orquestador Saga.
    /// Utiliza una estructura Stack para ejecutar compensaciones
    /// en orden inverso al registro.
    /// </summary>
    public class SagaOrchestrator : ISagaOrchestrator
    {
        private readonly Stack<(string Name, Func<Task> Action)> _compensations = new();
        private readonly ILogger<SagaOrchestrator>? _logger;
        private bool _rollbackExecuted = false;

        public SagaOrchestrator(ILogger<SagaOrchestrator>? logger = null)
        {
            _logger = logger;
        }

        /// <summary>
        /// Registra una acción de compensación que será ejecutada
        /// si se invoca <see cref="RollbackAsync"/>.
        /// </summary>
        /// <param name="stepName">
        /// Nombre descriptivo del paso asociado a la compensación.
        /// Se utiliza principalmente para logging y trazabilidad.
        /// </param>
        /// <param name="action">
        /// Acción asincrónica que deshace el paso previamente ejecutado.
        /// </param>
        /// <remarks>
        /// Las compensaciones se almacenan internamente en una estructura LIFO (Stack),
        /// por lo que serán ejecutadas en orden inverso al registro.
        /// </remarks>
        public void AddCompensation(string stepName, Func<Task> action)
        {
            _compensations.Push((stepName, action));
        }

        /// <summary>
        /// Ejecuta las compensaciones registradas en orden inverso.
        /// Si una compensación falla, el error se registra y el proceso continúa.
        /// </summary>
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

        /// <summary>
        /// Ejecuta una operación asincrónica y, si se completa correctamente,
        /// registra su compensación correspondiente para un posible rollback futuro.
        /// </summary>
        /// <typeparam name="T">
        /// Tipo del resultado devuelto por la operación.
        /// </typeparam>
        /// <param name="stepName">
        /// Nombre descriptivo del paso. Se utiliza para identificar la compensación
        /// en logs y trazabilidad.
        /// </param>
        /// <param name="action">
        /// Operación principal que se desea ejecutar.
        /// </param>
        /// <param name="compensation">
        /// Acción de compensación que se ejecutará durante el rollback.
        /// Recibe como parámetro el resultado exitoso de la operación.
        /// </param>
        /// <returns>
        /// El resultado producido por la operación principal.
        /// </returns>
        /// <exception cref="Exception">
        /// Propaga cualquier excepción lanzada por la operación principal.
        /// En caso de fallo, la compensación NO se registra.
        /// </exception>
        /// <remarks>
        /// La compensación solo se registra si la operación finaliza exitosamente.
        /// Si la operación falla, la excepción se propaga y la responsabilidad
        /// de invocar <see cref="RollbackAsync"/> recae en el consumidor.
        /// </remarks>
        public async Task<T> ExecuteStepAsync<T>(string stepName, Func<Task<T>> action, Func<T, Task> compensation)
        {
            var result = await action();

            AddCompensation(stepName, () => compensation(result));

            return result;
        }
    }
}
