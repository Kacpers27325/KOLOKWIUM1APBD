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
    public async Task<IActionResult> AddBook(int pk, string title, int idauthor)
    {
        if (!await _bookRepository.DoesBookExist(pk))
        {
            _bookRepository.AddBook(pk, title, idauthor);
            return Ok();
        }
        else
        {
            return NotFound();
        }
    }
    
    
    
}