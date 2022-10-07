using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace BlazorBase.Files.Models;

public class IgnoreBaseFileControllerRoutes
{
    private readonly RequestDelegate next;

    public IgnoreBaseFileControllerRoutes(RequestDelegate next)
    {
        this.next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        if (context.Request.Path.HasValue && context.Request.Path.Value.Contains("/api/BaseFile"))
        {
            context.Response.StatusCode = 404;
            return;
        }

        await next.Invoke(context);
    }
}
