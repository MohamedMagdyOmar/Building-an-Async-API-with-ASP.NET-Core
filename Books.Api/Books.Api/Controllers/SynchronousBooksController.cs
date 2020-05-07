using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Books.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Books.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SynchronousBooksController : ControllerBase
    {
        private IBooksRepository _booksRepository;

        public SynchronousBooksController(IBooksRepository booksRepository)
        {
            _booksRepository = booksRepository ?? throw new ArgumentException(nameof(booksRepository));
        }

        [HttpGet]
        public IActionResult GetBooks()
        {
            // var bookEntities = _booksRepository.GetBooks();
            // assume we do not have the previous "GetBooks:=" method, and we have only the async version as below:

            // here GetBooksAsync returns a task, which we do not need, so we can use .Result
            // .Result make sure that the "bookEntities" returns after the task has been completed, instead of just returning a task.
            // so it will block the thread, and return the result of GetBooksAsync
            var bookEntities = _booksRepository.GetBooksAsync().Result;
            return Ok(bookEntities);
        }
    }
}