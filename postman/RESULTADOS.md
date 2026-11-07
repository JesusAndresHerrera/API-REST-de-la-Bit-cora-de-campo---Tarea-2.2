# Pruebas de la API en local

El 6 de noviembre de 2026 volvimos a ejecutar la colección completa con Newman 6.2.3. Se realizaron **16 solicitudes y las 42 comprobaciones pasaron**, sin errores de ejecución. La prueba usó una copia aislada de SQLite, que empezó y terminó sin registros ni fotografías. La base de datos de la aplicación se mantuvo aparte.

## Qué se comprobó

| Operación | Resultado correcto | Caso con error |
| --- | --- | --- |
| Listar registros | 200; no incluye la foto completa | 406 al solicitar XML |
| Consultar un registro | 200 con fotografía | 404 después de eliminarlo |
| Crear | 201 con encabezado `Location` | 400 por título, descripción o fotografía inválidos |
| Actualizar | 204; un GET confirma los cambios y la fecha original | 400 por datos inválidos y 404 si no existe |
| Eliminar | 204 sin cuerpo | 404 al repetir la eliminación |

También se revisaron los encabezados: los datos se envían en JSON, los errores incluyen `application/problem+json` y las respuestas 204 llegan sin cuerpo. La colección crea y elimina únicamente su propio registro.

## Evidencia guardada

- [Resultado de esta ejecución en texto](resultados/ejecucion-local-2026-11-06.txt).
- [Detalle de las solicitudes, comprobaciones y encabezados en JSON](resultados/ejecucion-local-2026-11-06.json).
- [Runner de Postman: 42 comprobaciones aprobadas](../docs/imagenes/07_postman_runner.png).
- [Creación con fotografía: 201 Created](../docs/imagenes/05_postman_crear_201.png).
- [Título demasiado corto: 400 y mensaje del campo](../docs/imagenes/06_postman_error_400.png).
- [Swagger con los cinco endpoints](../docs/imagenes/04_swagger.png).

Las capturas se tomaron el 6 de noviembre de 2026 en Swagger y Postman de escritorio. El Runner terminó con 42 comprobaciones aprobadas, cero fallos y cero errores a las 23:02, hora de Honduras. Se renovaron las capturas con letra más grande y guardado nativo en PNG. Las imágenes individuales muestran la creación y el error de validación; el Runner conserva el resultado de la ejecución completa de las 23:02. El registro creado para las nuevas capturas también se eliminó al terminar.

El reporte JSON corresponde a otra ejecución de la misma colección con Newman, entre las 23:01:39 y las 23:01:43. Ambas ejecuciones se hicieron en local, con bases de prueba separadas de la base habitual de la aplicación. El error 500 se comprobó en una prueba HTTP anterior y no se cuenta entre estas 16 solicitudes.

Para repetir la colección en Postman, importa los dos archivos JSON de esta carpeta, selecciona el entorno **Bitácora - Local** y ejecuta las solicitudes en orden, desde `01 POST`, con la API encendida. El entorno usa `http://localhost:5106`. La prueba con Newman empleó un puerto local libre para no interferir con esa instancia.
