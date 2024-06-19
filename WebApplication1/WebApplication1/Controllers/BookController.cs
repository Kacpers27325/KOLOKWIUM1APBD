using Microsoft.AspNetCore.Mvc;
using WebApplication1.DTOs;
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

    [HttpGet("{id}/authors")]
    public async Task<IActionResult> GetAuthors(int id)
    {
        if (!await _bookRepository.DoesBookExistAsync(id))
        {
            return NotFound();
        }

        var (title, authors) = await _bookRepository.ShowAuthorsAsync(id);
        var response = new 
        {
            Id = id,
            Title = title,
            Authors = authors.Select(a => new AuthorDto 
            {
                FirstName = a.firstName,
                LastName = a.lastName
            })
        };

        return Ok(response);
    }

    [HttpPost]
    public async Task<IActionResult> AddBook([FromBody] BookDto bookDto)
    {
        var bookId = await _bookRepository.AddBookAsync(bookDto);
        var response = new 
        {
            Id = bookId,
            Title = bookDto.Title,
            Authors = bookDto.Authors
        };
        return CreatedAtAction(nameof(GetAuthors), new { id = bookId }, response);
    }
}