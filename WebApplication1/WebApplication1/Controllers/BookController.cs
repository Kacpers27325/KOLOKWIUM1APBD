using Microsoft.AspNetCore.Mvc;
using WebApplication1.Repositories;

namespace WebApplication1.Controllers;


[Route("api/[controller]")]
[ApiController]
public class BookController : ControllerBase
{
    private readonly IBookRepository _bookRepository;

    public BookController(IBookRepository bookRepository)
    {
        _bookRepository = bookRepository;
    }
    
    [HttpGet]
    public IActionResult GetAuthors(int id)
    {
        var authors = _bookRepository.ShowAuthors(id);
        return Ok(authors);
    }
    [HttpPost]
    public IActionResult AddBook(int id, string title, int idauthor)
    {
        _bookRepository.AddBook(id, title,idauthor);
        return Created("/api/animals", null);
    }
}