﻿using Books.Api.Contexts;
using Books.Api.Entities;
using Books.Api.ExternalModels;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Books.Api.Services
{
    public class BooksRepository : IBooksRepository, IDisposable
    {
        private BooksContext _context;
        private IHttpClientFactory _httpClientFactory;
        private ILogger<BooksRepository> _logger;

        // manages and sends notifications to the individual cancellation tokens.
        // it implements disposable, so we have to dispose it when the repository is disposed
        private CancellationTokenSource _cancellationTokenSource;

        public BooksRepository(BooksContext context, IHttpClientFactory httpClientFactory, ILogger<BooksRepository> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
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
            // we are using this logger to make sure that this method runs on different thread than the method "GetBookPages"
            _logger.LogInformation($"ThreadId when entering GetBookAsync: {System.Threading.Thread.CurrentThread.ManagedThreadId}");

            // when we call the "CalculateBookPages" method, we will blocked by this method for 5 sec waiting to be finished.
            // so we need this part of code to be offloaded to be run on different thread
            var bookPages = await GetBookPages();

            return await _context.Books.Include(b => b.Author).FirstOrDefaultAsync(b => b.Id == id);
        }

        public Task<int> GetBookPages()
        {
            // this long running task will run on a new thread
            return Task.Run(() =>
            {
                _logger.LogInformation($"ThreadId when entering GetBookPages: {System.Threading.Thread.CurrentThread.ManagedThreadId}");
                var pageCalculator = new Books.Legacy.ComplicatedPageCalculator();
                return pageCalculator.CalculateBookPages();
            });
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
                if(_cancellationTokenSource != null)
                {
                    _cancellationTokenSource.Dispose();
                    _cancellationTokenSource = null;
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

        public void AddBook(Book bookToAdd)
        {
            if(bookToAdd == null)
            {
                throw new ArgumentException(nameof(bookToAdd));
            }

            // note that we did not use async in this method (look at the note of the implementation of "AddAsync"), because it is used in special case only as mentioned in the comment
            // so it keeps tracking of the entity only, and the data is not yet persisting it to the database, it is only added to "BooksDbSet", and so it is not IO operation,
            // so there is no need to use async, and await, and no need to use "AddAsync", we will use just "Add".
             _context.Books.Add(bookToAdd);
        }

        public async Task<bool> SaveChangesAsync()
        {
            // return true if one or more entites were changed
            return (await _context.SaveChangesAsync() > 0);
        }

        public async Task<IEnumerable<Entities.Book>> GetBooksAsync(IEnumerable<Guid> bookIds)
        {
            return await _context.Books.Where(b => bookIds.Contains(b.Id)).Include(b => b.Author).ToListAsync();
        }

        public async Task<BookCover> GetBookCoverAsync(string coverId)
        {
            // when we call api from another api, we open HttpClient Instance once and reuse it across requests
            // so normally we are writing: var client = new HttpClient();
            // but in Core 2.1, new helper class has been introduced -> HttpClientFactory
            // using this factory handles creating and disposing of httpClient instances and it controls the 
            // reuse, creation, and disposing of HTTP Handlers used by those HTTPClient

            var httpClient = _httpClientFactory.CreateClient();

            // pass through a dummy name
            // this a network call, so we are using await
            var response = await httpClient.GetAsync($"http://localhost:52644/api/bookcovers/{coverId}");

            if(response.IsSuccessStatusCode)
            {
                // deserialize to BookCover
                // note we are using ReadAsStringAsync, because for GetAsync to finish, and the response to start arriving
                // in other words the response is not necessarily fully transferred yet, nor it is completely buffered, so it is still IO Operation
                return JsonConvert.DeserializeObject<BookCover>(await response.Content.ReadAsStringAsync());
            }

            return null;
        }

        public async Task<IEnumerable<BookCover>> GetBookCoversAsync(Guid bookId)
        {
            var httpClient = _httpClientFactory.CreateClient();
            var bookCovers = new List<BookCover>();
            _cancellationTokenSource = new CancellationTokenSource();

            // this is the token that we need to pass to each task that we want to listen to cancellation
            
            // create list of fake bookCovers
            var bookCoverUrls = new[]
            {
                $"http://localhost:52644/api/bookcovers/{bookId}-dummycover1",
                $"http://localhost:52644/api/bookcovers/{bookId}-dummycover2?returnFault=true",
                $"http://localhost:52644/api/bookcovers/{bookId}-dummycover3",
                $"http://localhost:52644/api/bookcovers/{bookId}-dummycover4",
                $"http://localhost:52644/api/bookcovers/{bookId}-dummycover5"
            };

            //foreach(var bookCoverUrl in bookCoverUrls)
            //{
            //    var response = await httpClient.GetAsync(bookCoverUrl);

            //    if(response.IsSuccessStatusCode)
            //    {
            //        bookCovers.Add(JsonConvert.DeserializeObject<BookCover>(await response.Content.ReadAsStringAsync()));
            //    }
            //}

            //we do not want to start downloading yet, that task will start when the query is evaluated, so we are using LINQ deferred execution
            var downloadBookCoverTaskQuery = from bookCoverUrl in bookCoverUrls select DownloadBookCoverAsync(httpClient, bookCoverUrl, _cancellationTokenSource.Token);

            // start the tasks
            var downloadBookCoverTasks = downloadBookCoverTaskQuery.ToList();

            // when still need to do something when all covers have been downloaded, we need to return them.
            // using below line, task that is not completed untill every task in the collection has been completed, as each task in the collection
            // has a potential result of one "BookCover"

            try
            {
                return await Task.WhenAll(downloadBookCoverTasks);
            }
            catch (OperationCanceledException operationCanceledException)
            {

                _logger.LogInformation($"{operationCanceledException.Message}");
                foreach(var task in downloadBookCoverTasks)
                {
                    _logger.LogInformation($"Task {task.Id} has status {task.Status}");
                }

                return new List<BookCover>();
            }
            catch(Exception exception)
            {
                // to handle any other exception that we are not expecting
                _logger.LogError($"{exception.Message}");
                throw;
            }
            
        }

        private async Task<BookCover> DownloadBookCoverAsync(HttpClient httpClient, string bookCoverUrl, CancellationToken cancellationToken)
        {
            var response = await httpClient.GetAsync(bookCoverUrl, cancellationToken);

            if(response.IsSuccessStatusCode)
            {
                var bookCover = JsonConvert.DeserializeObject<BookCover>(await response.Content.ReadAsStringAsync());

                return bookCover;
            }

            // we need when we API call fails(response not successfull), we need all tasks to recieve notification of this once a cancellation token has been called
            // here we are just requesting cancellation, we also need letting the listener know that the cancellation was requested
            // so we need to cancel the download so we will update line 176 to include cancellationToken
            _cancellationTokenSource.Cancel();

            return null;
        }
    }
}
