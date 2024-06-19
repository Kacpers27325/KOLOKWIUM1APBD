using Microsoft.Data.SqlClient;
using WebApplication1.DTOs;
using WebApplication1.Models;

namespace WebApplication1.Repositories;
public class BookRepository : IBookRepository
{
    private readonly IConfiguration _configuration;
    public BookRepository(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task<(string Title, List<Author> Authors)> ShowAuthorsAsync(int idBook)
    {
        using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("Default")))
        {
            await connection.OpenAsync();

            string title = "";
            using (SqlCommand command = new SqlCommand("SELECT title FROM Books WHERE PK = @idBook", connection))
            {
                command.Parameters.AddWithValue("@idBook", idBook);
                var result = await command.ExecuteScalarAsync();
                if (result != null)
                {
                    title = result.ToString();
                }
            }

            using (SqlCommand command = new SqlCommand())
            {
                command.Connection = connection;
                command.CommandText = "SELECT * FROM Authors JOIN books_authors ON authors.PK = books_authors.FK_author WHERE books_authors.FK_book = @idBook";
                command.Parameters.AddWithValue("@idBook", idBook);
                var authors = new List<Author>();
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        authors.Add(new Author()
                        {
                            id = reader.GetInt32(reader.GetOrdinal("PK")),
                            firstName = reader.GetString(reader.GetOrdinal("first_name")),
                            lastName = reader.GetString(reader.GetOrdinal("last_name"))
                        });
                    }
                }
                return (title, authors);
            }
        }
    }

    public async Task<int> AddBookAsync(BookDto bookDto)
    {
        using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("Default"));
        await connection.OpenAsync();
        using SqlCommand command = new SqlCommand();
        command.Connection = connection;

        command.CommandText = "INSERT INTO Books (title) VALUES (@title); SELECT SCOPE_IDENTITY();";
        command.Parameters.AddWithValue("@title", bookDto.Title);

        var bookId = Convert.ToInt32(await command.ExecuteScalarAsync());

        foreach (var author in bookDto.Authors)
        {
            var authorId = await GetAuthorIdAsync(connection, author);
            if (authorId == null)
            {
                authorId = await AddAuthorAsync(connection, author);
            }

            using SqlCommand bookAuthorCommand = new SqlCommand();
            bookAuthorCommand.Connection = connection;
            bookAuthorCommand.CommandText = "INSERT INTO books_authors (FK_book, FK_author) VALUES (@bookId, @authorId);";
            bookAuthorCommand.Parameters.AddWithValue("@bookId", bookId);
            bookAuthorCommand.Parameters.AddWithValue("@authorId", authorId);
            await bookAuthorCommand.ExecuteNonQueryAsync();
        }

        return bookId;
    }

    private async Task<int?> GetAuthorIdAsync(SqlConnection connection, AuthorDto author)
    {
        using SqlCommand command = new SqlCommand();
        command.Connection = connection;
        command.CommandText = "SELECT PK FROM Authors WHERE first_name = @firstName AND last_name = @lastName";
        command.Parameters.AddWithValue("@firstName", author.FirstName);
        command.Parameters.AddWithValue("@lastName", author.LastName);
        var result = await command.ExecuteScalarAsync();
        return result == null ? (int?)null : Convert.ToInt32(result);
    }

    private async Task<int> AddAuthorAsync(SqlConnection connection, AuthorDto author)
    {
        using SqlCommand command = new SqlCommand();
        command.Connection = connection;
        command.CommandText = "INSERT INTO Authors (first_name, last_name) VALUES (@firstName, @lastName); SELECT SCOPE_IDENTITY();";
        command.Parameters.AddWithValue("@firstName", author.FirstName);
        command.Parameters.AddWithValue("@lastName", author.LastName);
        var result = await command.ExecuteScalarAsync();
        return Convert.ToInt32(result);
    }

    public async Task<bool> DoesBookExistAsync(int id)
    {
        var query = "SELECT 1 FROM Books WHERE PK = @ID";
        using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("Default"));
        using SqlCommand command = new SqlCommand();

        command.Connection = connection;
        command.CommandText = query;
        command.Parameters.AddWithValue("@ID", id);

        await connection.OpenAsync();
        var res = await command.ExecuteScalarAsync();
        return res != null;
    }
}