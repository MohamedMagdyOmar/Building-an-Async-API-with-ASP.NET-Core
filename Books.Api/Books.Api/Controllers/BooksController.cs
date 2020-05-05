using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Books.Api.Filters;
using Books.Api.Models;
using Books.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Books.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BooksController : ControllerBase
    {
        private IBooksRepository _booksRepository;
        private IMapper _mapper;

        public BooksController(IBooksRepository booksRepository, IMapper mapper)
        {
            _booksRepository = booksRepository ?? throw new ArgumentException(nameof(booksRepository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        [HttpGet]
        [BooksResultFilter]
        public async Task<IActionResult> GetBooks()
        {
            var books = await _booksRepository.GetBooksAsync();
            return Ok(books);
        }

        [HttpGet]
        [BookResultFilter]
        [Route("{id}", Name ="GetBook")]      
        public async Task<IActionResult> GetBook(Guid id)
        {
            var book = await _booksRepository.GetBookAsync(id);
            if (book == null) return NotFound("Book is not found");

            return Ok(book);
        }

        [HttpPost]
        public async Task<IActionResult> CreateBook([FromBody]BookForCreation book)
        {
            try
            {
                var bookEntity = _mapper.Map<Entities.Book>(book);
                _booksRepository.AddBook(bookEntity);
                await _booksRepository.SaveChangesAsync();

                return CreatedAtRoute("GetBook",
               new { id = bookEntity.Id },
               bookEntity);
            }
            catch(Exception e)
            {
                return NotFound(e.Message);
            }  
        }
    }
}