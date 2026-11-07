using Bitacora.Domain;

namespace Bitacora.Application;

public interface IRegistroRepository
{
    // Lista los registros sin cargar sus fotografias.
    Task<IReadOnlyList<Registro>> ListarAsync(
        CancellationToken cancellationToken = default);

    Task<Registro?> ObtenerPorIdAsync(
        int id, CancellationToken cancellationToken = default);

    Task<Registro> CrearAsync(
        Registro registro, CancellationToken cancellationToken = default);

    // Devuelve false si el registro no existe.
    Task<bool> ActualizarAsync(
        Registro registro, CancellationToken cancellationToken = default);

    // Devuelve false si el registro no existe.
    Task<bool> EliminarAsync(
        int id, CancellationToken cancellationToken = default);
}
