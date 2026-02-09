using System.Data.Odbc;
using Microsoft.Extensions.Configuration;

class Program
{
    static void Main()
    {
        var config = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json")
                .Build();

        string connString = config.GetConnectionString("Pagila");

        //string connString = "DSN=PagilaDSN;";

        try
        {
            using var connection = new OdbcConnection(connString);
            connection.Open();
            Console.WriteLine("Connected to Pagila via ODBC!");

            string sql = "SELECT actor_id, first_name, last_name FROM actor LIMIT 5";

            using var command = new OdbcCommand(sql, connection);
            using var reader = command.ExecuteReader();

            Console.WriteLine("\nActors:");

            while (reader.Read())
            {
                int id = reader.GetInt32(reader.GetOrdinal("actor_id"));
                string first = reader.GetString(reader.GetOrdinal("first_name"));
                string last = reader.GetString(reader.GetOrdinal("last_name"));
                Console.WriteLine($"{id}: {first} {last}");
            }
        }
        catch (OdbcException ex)
        {
            Console.WriteLine("ODBC Error: " + ex.Message);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error: " + ex.Message);
        }
    }
}
