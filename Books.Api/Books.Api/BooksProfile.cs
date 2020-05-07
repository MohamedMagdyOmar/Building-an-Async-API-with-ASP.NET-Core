using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Books.Api
{
    public class BooksProfile : Profile
    {
        public BooksProfile()
        {
            CreateMap<Entities.Book, Models.Book>()
                .ForMember(dest => dest.Author, opt => opt.MapFrom(src =>
                $"{src.Author.FirstName} {src.Author.LastName}"));

            CreateMap<Models.BookForCreation, Entities.Book>();

            // we need to combine 2 objects "Book" and "BookCover" into one object, so we are going to use automapper, but in 2 steps

            // mapping from Book -> BookWithCovers
            CreateMap<Entities.Book, Models.BookWithCovers>().
                ForMember(dest => dest.Author, opt => opt.MapFrom(src => $"{src.Author.FirstName}{src.Author.LastName}"));

            CreateMap<IEnumerable<ExternalModels.BookCover>, Models.BookWithCovers>().
                ForMember(dest => dest.BookCovers, opt => opt.MapFrom(src => src));
        }
    }
}
