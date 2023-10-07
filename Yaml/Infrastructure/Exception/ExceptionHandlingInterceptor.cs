using k8s.Autorest;

namespace Yaml.Infrastructure.Exception;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

public class ExceptionHandlingInterceptor
{
    private readonly RequestDelegate _delegate;
    
    public ExceptionHandlingInterceptor(RequestDelegate next)
    {
        _delegate = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _delegate(context);
        }
        catch (NotFoundException e)
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsync(e.Message);
        }
        catch (HttpOperationException e)
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsync(e.Message);
        }
        catch (System.Exception ex)
        { context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsync(ex.Message);
        }
    }
}