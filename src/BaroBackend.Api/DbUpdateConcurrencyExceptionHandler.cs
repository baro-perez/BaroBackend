using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BaroBackend.Api;

public class DbUpdateConcurrencyExceptionHandler(IProblemDetailsService problemDetailsService) : 
    ExceptionHandler<DbUpdateConcurrencyException>(problemDetailsService)
{
    public override ProblemDetails CreateDetails(Exception exception)
    {
        return new ProblemDetails
        {
            Status = StatusCodes.Status409Conflict,
            Title = nameof(DbUpdateConcurrencyException),
            Detail = exception.Message
        };
    }
}