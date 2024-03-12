using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace BlazorBase.Files.Models;

public class IgnoreBaseFileControllerRoutes(RequestDelegate next)
{
    private readonly RequestDelegate next = next;

    public Task Invoke(HttpContext context)
    {
        if (context.Request.Path.HasValue && context.Request.Path.Value.Contains("/api/BaseFile"))
        {
            context.Response.StatusCode = 404;
            return Task.CompletedTask;
        }

        return next.Invoke(context);
    }
}
