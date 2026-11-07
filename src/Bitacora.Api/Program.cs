using Bitacora.Api;
using Bitacora.Application;
using Bitacora.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var databaseDirectory = Path.Combine(builder.Environment.ContentRootPath, "App_Data");
Directory.CreateDirectory(databaseDirectory);

var connectionSettings = new SqliteConnectionStringBuilder(
    builder.Configuration.GetConnectionString("Bitacora") ?? string.Empty)
{
    ForeignKeys = true
};
if (string.IsNullOrWhiteSpace(connectionSettings.DataSource))
{
    connectionSettings.DataSource = Path.Combine(databaseDirectory, "bitacora.db");
}

// Add services to the container.
builder.Services.AddDbContext<BitacoraDbContext>(options =>
    options.UseSqlite(connectionSettings.ToString()));
builder.Services.AddScoped<IRegistroRepository, RegistroRepository>();
builder.Services.AddScoped<RegistroService>();

builder.Services.AddControllers(options =>
{
    options.ReturnHttpNotAcceptable = true;
    options.RespectBrowserAcceptHeader = true;
}).ConfigureApiBehaviorOptions(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var problem = new ValidationProblemDetails(context.ModelState)
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "Revisa los datos del registro.",
            Instance = context.HttpContext.Request.Path
        };
        problem.Extensions["traceId"] = context.HttpContext.TraceIdentifier;
        return new BadRequestObjectResult(problem)
        {
            ContentTypes = { "application/problem+json" }
        };
    };
});
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails(options =>
{
    options.CustomizeProblemDetails = context =>
        context.ProblemDetails.Extensions.TryAdd("traceId", context.HttpContext.TraceIdentifier);
});
builder.Services.AddOpenApi();

var app = builder.Build();

app.UseExceptionHandler();
app.Use(async (context, next) =>
{
    await next(context);

    // MVC puede devolver 406 con Content-Length: 0; también necesita un mensaje.
    if (context.Response.StatusCode < 400 || context.Response.HasStarted ||
        context.Response.ContentLength is > 0)
    {
        return;
    }

    var status = context.Response.StatusCode;
    var title = status switch
    {
        404 => "No se encontró la dirección solicitada.",
        405 => "El método HTTP no está permitido en esta dirección.",
        406 => "La API devuelve datos en formato JSON. Usa Accept: application/json.",
        415 => "Envía los datos con Content-Type: application/json.",
        _ => "No se pudo completar la solicitud."
    };
    context.Response.ContentLength = null;
    var problem = new ProblemDetails
    {
        Status = status,
        Title = title,
        Instance = context.Request.Path
    };
    problem.Extensions["traceId"] = context.TraceIdentifier;
    // Escribir JSON directamente evita volver al formateador XML rechazado.
    await Results.Json(problem, contentType: "application/problem+json",
        statusCode: status).ExecuteAsync(context);
});

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "Bitácora de campo v1");
        options.DocumentTitle = "Bitácora de campo - API";
    });
    app.MapGet("/", () => Results.Redirect("/swagger")).ExcludeFromDescription();
}
else
{
    app.UseHttpsRedirection();
}

app.UseAuthorization();

app.MapControllers();

app.Run();
