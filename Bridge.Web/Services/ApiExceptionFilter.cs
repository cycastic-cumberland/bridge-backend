using Bridge.Domain.Exceptions;
using Bridge.Web.Dto;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Bridge.Web.Services;

public class ApiExceptionFilter : IAsyncExceptionFilter
{
    public Task OnExceptionAsync(ExceptionContext context)
    {
        var exception = context.Exception;
        ExceptionDto result;
        switch (exception)
        {
            case HttpStatusException e:
            {
                result = new()
                {
                    Title = e.Message,
                    Status = e.StatusCode,
                    Path = context.HttpContext.Request.Path,
                    Data = e.CustomData
                };
                break;
            }
            default:
            {
                result = new()
                {
                    Title = exception.Message,
                    Status = 500,
                    Path = context.HttpContext.Request.Path,
                    Data = exception.Data
                };
                break;
            }
        }

        context.Result = new JsonResult(result)
        {
            StatusCode = result.Status
        };
        return Task.CompletedTask;
    }
}