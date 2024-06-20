namespace WebApplication1.DTOs;

public class BookAuthorsDto
{
    public int Id { get; set; }
    public string Title { get; set; }
    public List<AuthorDto> Authors { get; set; }
}