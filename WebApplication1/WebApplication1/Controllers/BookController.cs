using Microsoft.AspNetCore.Mvc;
using WebApplication1.DTOs;
using WebApplication1.Repositories;

namespace WebApplication1.Controllers;

[Route("api/books")]
[ApiController]
public class BookController : ControllerBase
{
    private readonly IBookRepository _bookRepository;

    public BookController(IBookRepository bookRepository)
    {
        _bookRepository = bookRepository;
    }

    [HttpGet("{id}/authors")]
    public async Task<IActionResult> GetAuthors(int id)
    {
        var bookAuthors = await _bookRepository.GetBookAuthorsAsync(id);
        if (bookAuthors.Id == -1) return NotFound("Book doesn't exist");
        return Ok(bookAuthors);
    }

    [HttpPost]
    public async Task<IActionResult> AddBook([FromBody] BookDto bookDto)
    {
        var newBook = await _bookRepository.AddBookAsync(bookDto);
        if (newBook.Id == -1)
        {
            return Conflict("AAAA");
        }
        return CreatedAtAction(nameof(GetAuthors), new { id = newBook.Id }, newBook);
    }
}