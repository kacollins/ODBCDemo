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

        string connString = config.GetConnectionString("Pagila") ?? "DSN=PagilaDSN;";

        try
        {
            using var connection = new OdbcConnection(connString);
            connection.Open();
            Console.WriteLine("Connected to Pagila via ODBC!");

            GetActors(connection);

            InsertActor(connection, "Kimberly", "Collins");
            GetActors(connection, "Collins");

            UpdateActor(connection, "Kimberly", "Collins", "NewLastName");
            GetActors(connection, "NewLastName");

            //DeleteActor(connection, "Kimberly", "NewLastName");
            //GetActors(connection, "NewLastName");
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

    private static void GetActors(OdbcConnection connection)
    {
        string sql = "SELECT actor_id, first_name, last_name FROM actor LIMIT 5";

        using var command = new OdbcCommand(sql, connection);
        using var reader = command.ExecuteReader();

        Console.WriteLine("Actors:");

        while (reader.Read())
        {
            int id = reader.GetInt32(reader.GetOrdinal("actor_id"));
            string first = reader.GetString(reader.GetOrdinal("first_name"));
            string last = reader.GetString(reader.GetOrdinal("last_name"));
            Console.WriteLine($"{id}: {first} {last}");
        }

        Console.WriteLine();
    }

    private static void GetActors(OdbcConnection connection, string lastName)
    {
        string sql = "SELECT actor_id, first_name, last_name FROM actor WHERE last_name = ?";

        using var command = new OdbcCommand(sql, connection);
        command.Parameters.AddWithValue("@last", lastName);

        using var reader = command.ExecuteReader();

        Console.WriteLine($"Actors with last name {lastName}:");

        while (reader.Read())
        {
            int id = reader.GetInt32(reader.GetOrdinal("actor_id"));
            string first = reader.GetString(reader.GetOrdinal("first_name"));
            string last = reader.GetString(reader.GetOrdinal("last_name"));
            Console.WriteLine($"{id}: {first} {last}");
        }

        Console.WriteLine();
    }

    private static void InsertActor(OdbcConnection connection, string firstName, string lastName)
    {
        Console.WriteLine($"Insert actor: {firstName} {lastName}");
        string sql = "INSERT INTO actor (first_name, last_name) VALUES (?, ?)";

        using var command = new OdbcCommand(sql, connection);
        command.Parameters.AddWithValue("@first", firstName);
        command.Parameters.AddWithValue("@last", lastName);

        int rows = command.ExecuteNonQuery();
        Console.WriteLine($"Inserted {rows} row(s)");
        Console.WriteLine();
    }

    private static void UpdateActor(OdbcConnection connection, string firstName, string oldLastName, string newLastName)
    {
        Console.WriteLine($"Update actor last name: {oldLastName} -> {newLastName}");
        string sql = "UPDATE actor SET last_name = ? WHERE first_name = ? AND last_name = ?";

        using var command = new OdbcCommand(sql, connection);
        command.Parameters.AddWithValue("@newLast", newLastName);
        command.Parameters.AddWithValue("@first", firstName);
        command.Parameters.AddWithValue("@oldLast", oldLastName);

        int rows = command.ExecuteNonQuery();
        Console.WriteLine($"Updated {rows} row(s)");
        Console.WriteLine();
    }

    private static void DeleteActor(OdbcConnection connection, string firstName, string lastName)
    {
        Console.WriteLine($"Delete actor: {firstName} {lastName}");
        string sql = "DELETE FROM actor WHERE first_name = ? AND last_name = ?";

        using var command = new OdbcCommand(sql, connection);
        command.Parameters.AddWithValue("@first", firstName);
        command.Parameters.AddWithValue("@last", lastName);

        int rows = command.ExecuteNonQuery();
        Console.WriteLine($"Deleted {rows} row(s)");
        Console.WriteLine();
    }
}
