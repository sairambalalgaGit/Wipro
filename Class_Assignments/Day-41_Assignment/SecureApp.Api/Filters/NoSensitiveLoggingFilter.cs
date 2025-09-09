using Microsoft.AspNetCore.Mvc.Filters;

namespace SecureApp.Api.Filters;

// Prevent sensitive values from being logged accidentally by model binders etc.
public class NoSensitiveLoggingFilter : IActionFilter
{
    public void OnActionExecuting(ActionExecutingContext context)
    {
        foreach (var key in context.ActionArguments.Keys.ToList())
        {
            if (key.Contains("password", StringComparison.OrdinalIgnoreCase) ||
                key.Contains("card", StringComparison.OrdinalIgnoreCase) ||
                key.Contains("ssn", StringComparison.OrdinalIgnoreCase))
            {
                context.ActionArguments[key] = "***REDACTED***";
            }
        }
    }
    public void OnActionExecuted(ActionExecutedContext context) { }
}
