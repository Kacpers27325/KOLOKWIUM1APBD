using WebApplication1.DTOs;
using WebApplication1.Models;

namespace WebApplication1.Repositories;

public interface IBookRepository
{
    Task<(string Title, List<Author> Authors)> ShowAuthorsAsync(int idBook);
    Task<int> AddBookAsync(BookDto bookDto);
    Task<bool> DoesBookExistAsync(int id);
}