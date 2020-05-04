using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;

namespace Books.Api.Filters
{
    // we will use this one whenever we need to transform "Book Entity" before sending it back to the consumer.
    public class BookResultFilterAttribute : ResultFilterAttribute
    {
        // "next" parameter allows us to call the next filter on the pipeline
        public override async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
        {
            // result property has the results sent back from the action.
            // note in "BookController" we return "Book Entity" with the "Ok".
            var resultFromAction = context.Result as ObjectResult;

            // here we are dealing with result with NO Value, like "NotFound", so nothing to map
            if(resultFromAction?.Value == null || resultFromAction.StatusCode < 200 || resultFromAction.StatusCode >= 300)
            {
                await next();
                return;
            }

            var _mapper = context.HttpContext.RequestServices.GetService<IMapper>();

            resultFromAction.Value = _mapper.Map<Models.Book>(resultFromAction.Value);


            // call next filter on the pipeline
            await next();
        }
    }
}
