using WebApplication1.Models;

namespace WebApplication1.Repositories;

public interface IBookRepository
{
    IEnumerable<Author> ShowAuthors(int idBook);
    void AddBook(int id, string title, int authorId);
    
    Task<bool> DoesBookExist(int id);
}