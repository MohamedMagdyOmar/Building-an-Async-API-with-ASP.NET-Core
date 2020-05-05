using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Books.Api.Models
{
    public class BookForCreation
    {
        // note that we did not use the class "Book", But we created another one for "Post", and keep the other class for "Get" Request
        public Guid AuthorId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
    }
}
