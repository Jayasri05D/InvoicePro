
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace InvoicePro.Filters;

public class ActionLoggingFilter : IAsyncActionFilter
{
    private readonly ILogger<ActionLoggingFilter> _logger;

    public ActionLoggingFilter(ILogger<ActionLoggingFilter> logger)
    {
        _logger = logger;
    }

    public async Task OnActionExecutionAsync(
        ActionExecutingContext context,
        ActionExecutionDelegate next)
    {
        var controller = context.RouteData.Values["controller"];
        var action = context.RouteData.Values["action"];

        _logger.LogInformation(
            "Starting MVC action {Controller}.{Action}",
            controller,
            action);

        var result = await next();

        _logger.LogInformation(
            "Completed MVC action {Controller}.{Action} with status {StatusCode}",
            controller,
            action,
            result.HttpContext.Response.StatusCode);
    }
}
