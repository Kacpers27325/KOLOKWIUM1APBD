using Microsoft.Data.SqlClient;
using WebApplication1.DTOs;
using WebApplication1.Models;

namespace WebApplication1.Repositories;

public class BookRepository : IBookRepository
{
    private readonly string _connectionString;

    public BookRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("Default");
    }

    public async Task<BookAuthorsDto> GetBookAuthorsAsync(int idBook)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();

            var title = await GetBookTitleAsync(connection, idBook);
            if (title == null) return new BookAuthorsDto { Id = -1 };

            var authors = await GetAuthorsForBookAsync(connection, idBook);
            return new BookAuthorsDto
            {
                Id = idBook,
                Title = title,
                Authors = authors.Select(a => new AuthorDto
                {
                    FirstName = a.firstName,
                    LastName = a.lastName
                }).ToList()
            };
        }
    }

    public async Task<BookAuthorsDto> AddBookAsync(BookDto bookDto)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        var bookId = await InsertBookAsync(connection, bookDto.Title);
        await AddAuthorsToBookAsync(connection, bookId, bookDto.Authors);
        if (DoesBookWithAuthorsExistAsync(connection, bookDto)==null)
        {
            return new BookAuthorsDto { Id = -1 };
        }

        return new BookAuthorsDto
        {
            Id = bookId,
            Title = bookDto.Title,
            Authors = bookDto.Authors
        };
    }

    public async Task<bool> DoesBookExistAsync(int id)
    {
        using var connection = new SqlConnection(_connectionString);
        using var command = new SqlCommand("SELECT 1 FROM Books WHERE PK = @ID", connection);
        command.Parameters.AddWithValue("@ID", id);
        await connection.OpenAsync();
        var result = await command.ExecuteScalarAsync();
        return result != null;
    }

    private async Task<string> GetBookTitleAsync(SqlConnection connection, int idBook)
    {
        using (var command = new SqlCommand("SELECT title FROM Books WHERE PK = @idBook", connection))
        {
            command.Parameters.AddWithValue("@idBook", idBook);
            var result = await command.ExecuteScalarAsync();
            return result?.ToString();
        }
    }

    private async Task<List<Author>> GetAuthorsForBookAsync(SqlConnection connection, int idBook)
    {
        var authors = new List<Author>();

        using (var command =
               new SqlCommand(
                   "SELECT * FROM Authors JOIN books_authors ON authors.PK = books_authors.FK_author WHERE books_authors.FK_book = @idBook",
                   connection))
        {
            command.Parameters.AddWithValue("@idBook", idBook);

            using (var reader = await command.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                    authors.Add(new Author
                    {
                        id = reader.GetInt32(reader.GetOrdinal("PK")),
                        firstName = reader.GetString(reader.GetOrdinal("first_name")),
                        lastName = reader.GetString(reader.GetOrdinal("last_name"))
                    });
            }
        }

        return authors;
    }

    private async Task<int> InsertBookAsync(SqlConnection connection, string title)
    {
        using var command = new SqlCommand("INSERT INTO Books (title) VALUES (@title); SELECT SCOPE_IDENTITY();",
            connection);
        command.Parameters.AddWithValue("@title", title);
        var result = await command.ExecuteScalarAsync();
        return Convert.ToInt32(result);
    }

    private async Task AddAuthorsToBookAsync(SqlConnection connection, int bookId, List<AuthorDto> authors)
    {
        foreach (var author in authors)
        {
            var authorId = await GetOrCreateAuthorAsync(connection, author);
            await LinkAuthorToBookAsync(connection, bookId, authorId);
        }
    }

    private async Task<int> GetOrCreateAuthorAsync(SqlConnection connection, AuthorDto author)
    {
        var authorId = await GetAuthorIdAsync(connection, author);
        if (authorId == null) authorId = await AddAuthorAsync(connection, author);

        return authorId.Value;
    }

    private async Task<int?> GetAuthorIdAsync(SqlConnection connection, AuthorDto author)
    {
        using var command =
            new SqlCommand("SELECT PK FROM Authors WHERE first_name = @firstName AND last_name = @lastName",
                connection);
        command.Parameters.AddWithValue("@firstName", author.FirstName);
        command.Parameters.AddWithValue("@lastName", author.LastName);
        var result = await command.ExecuteScalarAsync();
        return result == null ? null : Convert.ToInt32(result);
    }

    private async Task<int> AddAuthorAsync(SqlConnection connection, AuthorDto author)
    {
        using var command =
            new SqlCommand(
                "INSERT INTO Authors (first_name, last_name) VALUES (@firstName, @lastName); SELECT SCOPE_IDENTITY();",
                connection);
        command.Parameters.AddWithValue("@firstName", author.FirstName);
        command.Parameters.AddWithValue("@lastName", author.LastName);
        var result = await command.ExecuteScalarAsync();
        return Convert.ToInt32(result);
    }

    private async Task LinkAuthorToBookAsync(SqlConnection connection, int bookId, int authorId)
    {
        using var command =
            new SqlCommand("INSERT INTO books_authors (FK_book, FK_author) VALUES (@bookId, @authorId);", connection);
        command.Parameters.AddWithValue("@bookId", bookId);
        command.Parameters.AddWithValue("@authorId", authorId);
        await command.ExecuteNonQueryAsync();
    }
    
    private async Task<bool> DoesBookWithAuthorsExistAsync(SqlConnection connection, BookDto bookDto)
    {
        using var command = new SqlCommand(
            @"SELECT COUNT(*) 
              FROM Books b
              JOIN books_authors ba ON b.PK = ba.FK_book
              JOIN Authors a ON ba.FK_author = a.PK
              WHERE b.title = @Title
              AND a.first_name = @FirstName
              AND a.last_name = @LastName", connection);

        command.Parameters.AddWithValue("@Title", bookDto.Title);
        command.Parameters.AddWithValue("@FirstName", bookDto.Authors[0].FirstName);
        command.Parameters.AddWithValue("@LastName", bookDto.Authors[0].LastName);

        var result = (int)await command.ExecuteScalarAsync();
        return result > 0;
    }
}