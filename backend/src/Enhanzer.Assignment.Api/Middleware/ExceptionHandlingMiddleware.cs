using Enhanzer.Assignment.Application.Common;
using Microsoft.AspNetCore.Mvc;

namespace Enhanzer.Assignment.Api.Middleware;

public sealed class ExceptionHandlingMiddleware(
    RequestDelegate next,
    ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (ValidationException ex)
        {
            await WriteProblemAsync(
                context,
                new ValidationProblemDetails(ex.Errors)
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = "Request validation failed."
                });
        }
        catch (AuthenticationFailedException)
        {
            await WriteProblemAsync(
                context,
                new ProblemDetails
                {
                    Status = StatusCodes.Status401Unauthorized,
                    Title = "Authentication failed.",
                    Detail = "The email or password was not accepted."
                });
        }
        catch (ExternalServiceException ex)
        {
            logger.LogWarning(ex, "External authentication service failure.");
            await WriteProblemAsync(
                context,
                new ProblemDetails
                {
                    Status = StatusCodes.Status502BadGateway,
                    Title = "Authentication service unavailable.",
                    Detail = "The external authentication service could not be used right now."
                });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled API error.");
            await WriteProblemAsync(
                context,
                new ProblemDetails
                {
                    Status = StatusCodes.Status500InternalServerError,
                    Title = "Unexpected server error.",
                    Detail = "An unexpected error occurred."
                });
        }
    }

    private static async Task WriteProblemAsync(HttpContext context, ProblemDetails problem)
    {
        context.Response.StatusCode = problem.Status ?? StatusCodes.Status500InternalServerError;
        context.Response.ContentType = "application/problem+json";
        await context.Response.WriteAsJsonAsync(problem);
    }
}
