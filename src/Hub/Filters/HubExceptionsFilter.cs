using Microsoft.AspNetCore.SignalR;
using Spike.Application.Exceptions;
using Spike.Hub.Errors;
using ILogger = Serilog.ILogger;

namespace Spike.Hub.Filters;

public class HubExceptionsFilter : IHubFilter
{
    private ILogger _logger;

    public HubExceptionsFilter(ILogger logger)
    {
        _logger = logger;
    }

    public async ValueTask<object?> InvokeMethodAsync(HubInvocationContext invocationContext, Func<HubInvocationContext, ValueTask<object?>> next)
    {
        ErrorType errorType;
        string? errorMessage;

        try
        {
            return await next(invocationContext);
        }
        catch (ValidationException ex)
        {
            errorType = ErrorType.Validation;
            errorMessage = ex.Message;
        }
        catch (NotFoundException ex)
        {
            errorType = ErrorType.NotFound;
            errorMessage = ex.Message;
        }
        catch (DomainException ex)
        {
            errorType = ErrorType.Domain;
            errorMessage = ex.Message;
        }
        catch (Exception ex)
        {
            errorType = ErrorType.Unhandled;
            errorMessage = "An unexpected error occurred.";

            _logger.Error(ex, "Unhandled error in hub method {Method}", invocationContext.HubMethodName);
        }

        await invocationContext.Hub.Clients.Caller.SendAsync("Error", new Error(errorType, errorMessage));

        return null;
    }
}