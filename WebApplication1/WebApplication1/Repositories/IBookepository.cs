using WebApplication1.DTOs;

namespace WebApplication1.Repositories;

public interface IBookRepository
{
    Task<BookAuthorsDto> GetBookAuthorsAsync(int idBook);
    Task<BookAuthorsDto> AddBookAsync(BookDto bookDto);
    Task<bool> DoesBookExistAsync(int id);
}