using Books.Api.Contexts;
using Books.Api.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Books.Api.Services
{
    public class BooksRepository : IBooksRepository, IDisposable
    {
        private BooksContext _context;

        public BooksRepository(BooksContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        } 


        public async Task<IEnumerable<Book>> GetBooksAsync()
        {
            // simulate Long IO Operation
            await _context.Database.ExecuteSqlCommandAsync("WAITFOR DELAY '00:00:02';");

            // we added Include to include Author with the books
            return await _context.Books.Include(b => b.Author).ToListAsync();
        }

        public async Task<Book> GetBookAsync(Guid id)
        {
            
            return await _context.Books.Include(b => b.Author).FirstOrDefaultAsync(b => b.Id == id);
        }

        // when the clr disposes the repository, we want to check if the context is not null, and we want to call dispose on that context, that insures that it get dispose
        public void Dispose()
        {
            Dispose(true);
            // this makes sure that CLR does not call finalizer for our repository
            // in other words we are telling GC that this repository has already been cleaned up
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if(disposing)
            {
                if(_context != null)
                {
                    _context.Dispose();
                    _context = null;
                }
            }
        }

        public IEnumerable<Book> GetBooks()
        {
            // simulate Long IO Operation
            _context.Database.ExecuteSqlCommand("WAITFOR DELAY '00:00:02';");
            return _context.Books.Include(b => b.Author).ToList();
        }

        public Book GetBook(Guid id)
        {
            throw new NotImplementedException();
        }
    }
}
