using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Books.Api.Filters
{
    public class BooksResultFilterAttribute : ResultFilterAttribute
    {
        // "next" parameter allows us to call the next filter on the pipeline
        public override async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
        {
            // result property has the results sent back from the action.
            // note in "BookController" we return "Book Entity" with the "Ok".
            var resultFromAction = context.Result as ObjectResult;

            // here we are dealing with result with NO Value, like "NotFound", so nothing to map
            if (resultFromAction?.Value == null || resultFromAction.StatusCode < 200 || resultFromAction.StatusCode >= 300)
            {
                await next();
                return;
            }

            var _mapper = context.HttpContext.RequestServices.GetService<IMapper>();

            // this is to handle "GetBooks" Action, in this case it will return IEnumerable
            resultFromAction.Value = _mapper.Map<IEnumerable<Models.Book>>(resultFromAction.Value);

            // call next filter on the pipeline
            await next();
        }
    }
}