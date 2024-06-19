namespace WebApplication1.DTOs;

public class BookDto
{
    public string Title { get; set; }
    public List<AuthorDto> Authors { get; set; }
}