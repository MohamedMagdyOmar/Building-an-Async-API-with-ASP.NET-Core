using AutoMapper;
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
    public class BooksCollectionController : ControllerBase
    {
        private IBooksRepository _booksRepository;
        private IMapper _mapper;

        public BooksCollectionController(IBooksRepository booksRepository, IMapper mapper)
        {
            _booksRepository = booksRepository?? throw new ArgumentException(nameof(booksRepository));
            _mapper = mapper?? throw new ArgumentException(nameof(mapper));
        }

        public async Task<IActionResult> CreateBookCollection([FromBody] IEnumerable<BookForCreation> bookCollection)
        {
            var bookEntities = _mapper.Map<IEnumerable<Entities.Book>>(bookCollection);
            
            foreach(var book in bookEntities)
            {
                _booksRepository.AddBook(book);
            }

           await _booksRepository.SaveChangesAsync();

            return Ok();
        }
    }
}
