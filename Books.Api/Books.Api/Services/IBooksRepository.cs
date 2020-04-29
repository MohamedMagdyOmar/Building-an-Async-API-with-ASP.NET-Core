﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Books.Api.Services
{
    public interface IBooksRepository
    {
        // these for Sync Method
        IEnumerable<Entities.Book> GetBooks();
        Entities.Book GetBook(Guid id);

        // same as above but for async
        Task<IEnumerable<Entities.Book>> GetBooksAsync();
        Task<Entities.Book> GetBookAsync(Guid id);
    }
}
