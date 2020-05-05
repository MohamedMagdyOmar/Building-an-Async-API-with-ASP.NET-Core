using AutoMapper;
using Books.Api.Filters;
using Books.Api.Models;
using Books.Api.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Books.Api.Controllers
{
    [Route("api/[Controller]")]
    [ApiController]
    // since we are using them in every action, we move it to at the controller level not the action level
    [BooksResultFilter]
    public class BooksCollectionController : ControllerBase
    {
        private IBooksRepository _booksRepository;
        private IMapper _mapper;

        public BooksCollectionController(IBooksRepository booksRepository, IMapper mapper)
        {
            _booksRepository = booksRepository?? throw new ArgumentException(nameof(booksRepository));
            _mapper = mapper?? throw new ArgumentException(nameof(mapper));
        }

        [HttpPost]
        public async Task<IActionResult> CreateBookCollection([FromBody] IEnumerable<BookForCreation> bookCollection)
        {
            var bookEntities = _mapper.Map<IEnumerable<Entities.Book>>(bookCollection);
            
            foreach(var book in bookEntities)
            {
                _booksRepository.AddBook(book);
            }

           await _booksRepository.SaveChangesAsync();

            // now we need to get book Ids for the inserted books, using below action we have just created
            var booksToReturn = await _booksRepository.GetBooksAsync(bookEntities.Select(b => b.Id).ToList());

            var bookIds = string.Join(",", booksToReturn.Select(a => a.Id));

            // bookIds -> to create a resource URI
            return CreatedAtRoute("GetBookCollection", new { bookIds }, booksToReturn);
        }

        // we need to retrieve list of books giving the bookId, so we need our URL to be in the following format:
        // api/bookCollections/(id1, id2)
        // but the model binding used in MVC, can not parse this URL, so we need to create our model binding to parse this URL
        // "No Binding For array that is part of the URI", so we need custom model binder to bind the Ids from the URI to the bookIds Enumerable
        [HttpGet("({bookIds})", Name = "GetBookCollection")]
        // for mapping from entities to model
        [BooksResultFilter]

        // we use the new binder to bind ids from the uri to our IEnumerable of GUID
        public async Task<IActionResult> GetBookCollection([ModelBinder(BinderType = typeof(ArrayModelBinder))]IEnumerable<Guid> bookIds)
        {
            var bookEntities = await _booksRepository.GetBooksAsync(bookIds);
            if(bookIds.Count() != bookEntities.Count())
            {
                return NotFound();
            }

            return Ok(bookEntities);

            // we also need to refer to this route from the bulk insert action, so we give it a name "GetBookCollection"
        }
    }
}
