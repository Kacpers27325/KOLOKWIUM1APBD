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
    
    public IEnumerable<Author> ShowAuthors(int idBook)
    {
        using (SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("Default")))
        {
            connection.Open();
            using (SqlCommand command = new SqlCommand())
            {
                command.Connection = connection;
                command.CommandText = "SELECT * FROM Authors JOIN books_authors ON authors.PK = books_authors.FK_author WHERE books_authors.FK_book = @idBook";
                command.Parameters.AddWithValue("@idBook", idBook);
                var authors = new List<Author>();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        authors.Add(new Author()
                        {
                            id = reader.GetInt32(reader.GetOrdinal("PK")),
                            firstName = reader.GetString(reader.GetOrdinal("first_name")),
                            lastName = reader.GetString(reader.GetOrdinal("last_name"))
                        });
                    }
                }
                return authors;
            }
        }
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
        command2.Parameters.AddWithValue("@books_PK", pk); //tu powinienem wyciągnąć id book z klucza który nadała baza za pomocą nazwy i wszystko pójdzie
        command2.Parameters.AddWithValue("@authors_PK", authorId);
        command2.ExecuteNonQuery();
    }
    
    public async Task<bool> DoesBookExist(int id)
    {
        var query = "SELECT 1 from Books where PK=@ID";
        using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("Default"));
        using SqlCommand command = new SqlCommand();

        command.Connection = connection;
        command.CommandText = query;
        command.Parameters.AddWithValue("@ID", id);

        await connection.OpenAsync();

        var res = await command.ExecuteScalarAsync();
        
        return res is not null;
    }
    
    
}