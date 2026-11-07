using Bitacora.Domain;

namespace Bitacora.Application;

public class RegistroService
{
    private readonly IRegistroRepository _repository;

    public RegistroService(IRegistroRepository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyList<RegistroListadoDto>> ListarAsync(
        CancellationToken cancellationToken = default)
    {
        var registros = await _repository.ListarAsync(cancellationToken);
        return registros.Select(registro => new RegistroListadoDto
        {
            Id = registro.Id,
            Titulo = registro.Titulo,
            Descripcion = registro.Descripcion,
            FechaCreacion = registro.FechaCreacion
        }).ToList();
    }

    public async Task<RegistroDetalleDto?> ObtenerPorIdAsync(
        int id, CancellationToken cancellationToken = default)
    {
        var registro = await _repository.ObtenerPorIdAsync(id, cancellationToken);
        return registro is null ? null : MapearDetalle(registro);
    }

    public async Task<RegistroDetalleDto> CrearAsync(
        GuardarRegistroDto entrada, CancellationToken cancellationToken = default)
    {
        ValidarEntrada(entrada);
        var registro = CrearEntidad(entrada);
        registro.FechaCreacion = DateTime.UtcNow;
        var creado = await _repository.CrearAsync(registro, cancellationToken);
        return MapearDetalle(creado);
    }

    public Task<bool> ActualizarAsync(
        int id, GuardarRegistroDto entrada, CancellationToken cancellationToken = default)
    {
        ValidarEntrada(entrada);
        var registro = CrearEntidad(entrada);
        registro.Id = id;
        return _repository.ActualizarAsync(registro, cancellationToken);
    }

    public Task<bool> EliminarAsync(
        int id, CancellationToken cancellationToken = default)
    {
        return _repository.EliminarAsync(id, cancellationToken);
    }

    private static void ValidarEntrada(GuardarRegistroDto? entrada)
    {
        if (entrada is null)
        {
            throw new RegistroValidacionException(new Dictionary<string, string[]>
            {
                ["Solicitud"] = new[] { "Debes enviar los datos del registro." }
            });
        }

        var errores = RegistroReglas.Validar(
            entrada.Titulo, entrada.Descripcion, entrada.FotoBase64);
        if (errores.Count > 0)
        {
            throw new RegistroValidacionException(errores);
        }
    }

    private static Registro CrearEntidad(GuardarRegistroDto entrada)
    {
        var foto = entrada.FotoBase64;
        return new Registro
        {
            Titulo = entrada.Titulo.Trim(),
            Descripcion = entrada.Descripcion.Trim(),
            Fotografia = foto is null ? null : new Fotografia
            {
                FotoBase64 = Convert.ToBase64String(Convert.FromBase64String(foto))
            }
        };
    }

    private static RegistroDetalleDto MapearDetalle(Registro registro)
    {
        return new RegistroDetalleDto
        {
            Id = registro.Id,
            Titulo = registro.Titulo,
            Descripcion = registro.Descripcion,
            FechaCreacion = registro.FechaCreacion,
            FotoBase64 = registro.Fotografia?.FotoBase64
        };
    }
}

public class RegistroValidacionException : Exception
{
    public RegistroValidacionException(IReadOnlyDictionary<string, string[]> errores)
        : base("Los datos del registro no son válidos.")
    {
        Errores = errores;
    }

    public IReadOnlyDictionary<string, string[]> Errores { get; }
}
