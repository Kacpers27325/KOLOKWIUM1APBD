using Microsoft.Data.SqlClient;
using WebApplication1.Models;

namespace WebApplication1.Repositories;

public class BookRepoitory : IBookRepository
{
    private static int idies = 10;
    private readonly IConfiguration _configuration;
    public BookRepoitory(IConfiguration configuration)
    {
        _configuration = configuration;
    }
    
    public IEnumerable <Author> ShowAuthors(int idBook)
    {
        using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("Default"));
        connection.Open();
        using SqlCommand command = new SqlCommand();
        command.Connection = connection;
        command.CommandText = "SELECT * FROM Authors JOIN books_authors ON authors.PK = books_authors.FK_author WHERE books_authors.FK_book = @idBook";
        command.Parameters.AddWithValue("@idBook", idBook);
        var reader = command.ExecuteReader();
        var authors = new List<Author>();
        while (reader.Read())
        {
            authors.Add(new Author()
            {
                id = reader.GetInt32(reader.GetOrdinal("PK")),
                firstName = reader.GetString(reader.GetOrdinal("first_na")),
                lastName = reader.GetString(reader.GetOrdinal("last_na"))
            });
        }
        return authors;
    }
    
    
    
    public void AddBook(int pk, string title, int authorId)
    {
        using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("Default"));
        connection.Open();
        using SqlCommand command = new SqlCommand();
        command.Connection = connection;

        command.CommandText = "INSERT INTO Books ( title) values ( @title);";
        
        command.Parameters.AddWithValue("@PK", pk);
        command.Parameters.AddWithValue("@title", title);
        command.ExecuteNonQuery();
        
        using SqlCommand command2 = new SqlCommand();
        command2.Connection = connection;
        command2.CommandText = "INSERT INTO books_authors (FK_book, FK_author) values (@books_pk, @authors_PK);";
        command2.Parameters.AddWithValue("@books_PK", pk);
        command2.Parameters.AddWithValue("@authors_PK", authorId);
        command2.ExecuteNonQuery();
    }
    
}