using CsvHelper;
using Microsoft.Extensions.Configuration;
using System.Data.Odbc;
using System.Globalization;

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

            string lastName = "Collins";
            GetActors(connection, lastName);
            GetFilmsByActorLastName(connection, lastName);

            GetSalesByFilmCategory(connection);

            GetLastDayOfCurrentMonth(connection);

            PrintActorReport(connection);

            WriteActorReportCsv(connection, "actors.csv");

            var rows = ReadActorReportCsv("actors.csv");

            foreach (var row in rows)
            {
                Console.WriteLine($"{row.ActorId}: {row.FirstName} {row.LastName} ({row.FilmCount} films)");
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

    private static void GetFilmsByActorLastName(OdbcConnection connection, string lastName)
    {
        string sql = @"SELECT a.first_name, a.last_name, f.title
                        FROM actor a
                        JOIN film_actor fa ON a.actor_id = fa.actor_id
                        JOIN film f ON fa.film_id = f.film_id
                        WHERE last_name = ?"; //example of joins

        using var command = new OdbcCommand(sql, connection);
        command.Parameters.AddWithValue("@last", lastName);

        using var reader = command.ExecuteReader();

        Console.WriteLine($"Films for actors with last name {lastName}:");

        while (reader.Read())
        {
            string first = reader.GetString(reader.GetOrdinal("first_name"));
            string last = reader.GetString(reader.GetOrdinal("last_name"));
            string title = reader.GetString(reader.GetOrdinal("title"));
            Console.WriteLine($"{first} {last} was in {title}");
        }

        Console.WriteLine();
    }

    private static void GetSalesByFilmCategory(OdbcConnection connection)
    {
        string sql = @"SELECT category, total_sales
	                    FROM public.sales_by_film_category"; //example of a view

        using var command = new OdbcCommand(sql, connection);

        using var reader = command.ExecuteReader();

        Console.WriteLine($"Sales by Film Category:");

        while (reader.Read())
        {
            string category = reader.GetString(reader.GetOrdinal("category"));
            decimal total_sales = reader.GetDecimal(reader.GetOrdinal("total_sales"));
            Console.WriteLine($"{category} {total_sales:C2}");
        }

        Console.WriteLine();
    }

    private static void GetLastDayOfCurrentMonth(OdbcConnection connection)
    {
        string sql = @"SELECT public.last_day(?)"; //example of a function

        using var command = new OdbcCommand(sql, connection);
        command.Parameters.AddWithValue("@timestamp", DateTime.Now.ToString());

        using var reader = command.ExecuteReader();

        if (reader.Read())
        {
            var date = reader.GetDate(0);
            Console.WriteLine($"Last Day of Current Month: {date:d}");
        }

        Console.WriteLine();
    }

    private static void PrintActorReport(OdbcConnection connection)
    {
        int pageSize = 20;
        int totalActors = GetFilmActorCount(connection);
        int totalPages = (int)Math.Ceiling(totalActors / (double)pageSize);

        for (int i = 1; i <= totalPages; i++)
        {
            PrintActorReportPage(connection, i, pageSize);
        }
    }

    private static int GetFilmActorCount(OdbcConnection connection)
    {
        int count = 0;

        string sql = "SELECT COUNT(DISTINCT actor_id) FROM film_actor;";

        using var command = new OdbcCommand(sql, connection);
        using var reader = command.ExecuteReader();

        if (reader.Read())
        {
            count = reader.GetInt32(0);
        }

        return count;
    }

    static void PrintActorReportPage(OdbcConnection connection, int pageNumber, int pageSize)
    {
        int offset = (pageNumber - 1) * pageSize;

        string sql = @"SELECT
                        a.actor_id,
                        a.first_name,
                        a.last_name,
                        COUNT(fa.film_id) AS film_count
                    FROM actor a
                    JOIN film_actor fa ON a.actor_id = fa.actor_id
                    GROUP BY a.actor_id, a.first_name, a.last_name
                    ORDER BY a.last_name, a.first_name
                    LIMIT ?
                    OFFSET ?;";

        using var command = new OdbcCommand(sql, connection);
        command.Parameters.AddWithValue("@PageSize", pageSize);
        command.Parameters.AddWithValue("@Offset", offset);

        using var reader = command.ExecuteReader();

        Console.WriteLine($"--- Page {pageNumber} ---");

        while (reader.Read())
        {
            string first = reader.GetString(reader.GetOrdinal("first_name"));
            string last = reader.GetString(reader.GetOrdinal("last_name"));
            int films = reader.GetInt32(reader.GetOrdinal("film_count"));

            Console.WriteLine($"{last}, {first} ({films} films)");
        }
    }

    static void WriteActorReportCsv(OdbcConnection connection, string filePath)
    {
        string sql = @"SELECT
                        a.actor_id,
                        a.first_name,
                        a.last_name,
                        COUNT(fa.film_id) AS film_count
                    FROM actor a
                    JOIN film_actor fa ON a.actor_id = fa.actor_id
                    GROUP BY a.actor_id, a.first_name, a.last_name
                    ORDER BY a.last_name, a.first_name;";

        using var command = new OdbcCommand(sql, connection);
        using var reader = command.ExecuteReader();
        using var writer = new StreamWriter(filePath);

        writer.WriteLine("ActorId,FirstName,LastName,FilmCount");

        while (reader.Read())
        {
            writer.WriteLine(
                $"{reader["actor_id"]}," +
                $"{reader["first_name"]}," +
                $"{reader["last_name"]}," +
                $"{reader["film_count"]}");
        }
    }

    static List<ActorReportRow> ReadActorReportCsv(string filePath)
    {
        using var reader = new StreamReader(filePath);
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

        var results = csv.GetRecords<ActorReportRow>().ToList();

        return results;
    }
}
