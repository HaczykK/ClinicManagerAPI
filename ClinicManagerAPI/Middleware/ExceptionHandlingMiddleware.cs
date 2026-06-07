using System.Net;
using System.Text.Json;

namespace ClinicManagerAPI.Middleware;

/// <summary>
/// Globalny middleware do obsługi nieobsłużonych wyjątków.
/// Loguje błąd przez ILogger i zwraca ustandaryzowaną odpowiedź JSON z kodem 500.
/// </summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Nieobsłużony wyjątek podczas przetwarzania żądania {Method} {Path}",
                context.Request.Method, context.Request.Path);

            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

        var response = new
        {
            StatusCode = context.Response.StatusCode,
            Message = "Wystąpił wewnętrzny błąd serwera. Spróbuj ponownie później.",
            Detail = context.RequestServices.GetService<IHostEnvironment>()?.IsDevelopment() == true
                ? exception.Message
                : null
        };

        var jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        return context.Response.WriteAsync(JsonSerializer.Serialize(response, jsonOptions));
    }
}
