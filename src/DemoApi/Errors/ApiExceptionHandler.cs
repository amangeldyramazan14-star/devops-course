using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace DemoApi.Errors;

// Единая точка обработки исключений: код ответа + ProblemDetails, 500 — в лог с ошибкой
public sealed class ApiExceptionHandler(
    IProblemDetailsService problemDetails,
    ILogger<ApiExceptionHandler> log) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken ct)
    {
        var (status, title) = exception switch
        {
            NotFoundException => (StatusCodes.Status404NotFound, "Не найдено"),
            ConflictException => (StatusCodes.Status409Conflict, "Конфликт"),
            _ => (StatusCodes.Status500InternalServerError, "Внутренняя ошибка сервера"),
        };

        if (status == StatusCodes.Status500InternalServerError)
        {
            log.LogError(exception, "Необработанное исключение при запросе {Method} {Path}",
                context.Request.Method, context.Request.Path);
        }

        context.Response.StatusCode = status;
        return await problemDetails.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = context,
            Exception = exception,
            ProblemDetails = new ProblemDetails
            {
                Status = status,
                Title = title,
                Detail = status < 500 ? exception.Message : null,
            },
        });
    }
}
