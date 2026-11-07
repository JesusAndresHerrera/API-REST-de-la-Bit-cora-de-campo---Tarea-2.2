using Bitacora.Application;
using Microsoft.AspNetCore.Mvc;

namespace Bitacora.Api.Controllers;

[ApiController]
[Route("api/registros")]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status406NotAcceptable)]
public class RegistrosController : ControllerBase
{
    private readonly RegistroService _service;

    public RegistrosController(RegistroService service)
    {
        _service = service;
    }

    [HttpGet]
    [EndpointSummary("Listar los registros sin la fotografía completa")]
    [ProducesResponseType(typeof(IReadOnlyList<RegistroListadoDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<RegistroListadoDto>>> Listar(
        CancellationToken cancellationToken)
    {
        return Ok(await _service.ListarAsync(cancellationToken));
    }

    [HttpGet("{id:int}")]
    [EndpointSummary("Consultar un registro con su fotografía")]
    [ProducesResponseType(typeof(RegistroDetalleDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RegistroDetalleDto>> ObtenerPorId(
        int id, CancellationToken cancellationToken)
    {
        var registro = await _service.ObtenerPorIdAsync(id, cancellationToken);
        return registro is null ? RegistroNoEncontrado(id) : Ok(registro);
    }

    [HttpPost]
    [Consumes("application/json")]
    [EndpointSummary("Crear un registro y devolver su dirección en Location")]
    [ProducesResponseType(typeof(RegistroDetalleDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status415UnsupportedMediaType)]
    public async Task<ActionResult<RegistroDetalleDto>> Crear(
        [FromBody] GuardarRegistroDto entrada, CancellationToken cancellationToken)
    {
        var registro = await _service.CrearAsync(entrada, cancellationToken);
        return CreatedAtAction(nameof(ObtenerPorId), new { id = registro.Id }, registro);
    }

    [HttpPut("{id:int}")]
    [Consumes("application/json")]
    [EndpointSummary("Reemplazar título, descripción y fotografía de un registro")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status415UnsupportedMediaType)]
    public async Task<IActionResult> Actualizar(
        int id, [FromBody] GuardarRegistroDto entrada, CancellationToken cancellationToken)
    {
        var actualizado = await _service.ActualizarAsync(id, entrada, cancellationToken);
        return actualizado ? NoContent() : RegistroNoEncontrado(id);
    }

    [HttpDelete("{id:int}")]
    [EndpointSummary("Eliminar un registro y su fotografía")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Eliminar(int id, CancellationToken cancellationToken)
    {
        var eliminado = await _service.EliminarAsync(id, cancellationToken);
        return eliminado ? NoContent() : RegistroNoEncontrado(id);
    }

    private ObjectResult RegistroNoEncontrado(int id)
    {
        return Problem(
            statusCode: StatusCodes.Status404NotFound,
            title: "Registro no encontrado.",
            detail: $"No existe un registro con el identificador {id}.",
            instance: HttpContext.Request.Path);
    }
}
