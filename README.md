# ODBCDemo

This project shows how to use [ODBC](https://learn.microsoft.com/en-us/sql/odbc/microsoft-open-database-connectivity-odbc) to connect a C# console app to the [Pagila](https://github.com/devrimgunduz/pagila) database.

* Fork this repo on GitHub.
* Go to Visual Studio to clone your forked repo.
	* `Git` menu -> `Clone Repository`
 	* Under `Browse Repositories`, click the `GitHub` button.
  * Select your `ODBCDemo` repo and click `Clone`.

* Go to Solution Explorer in Visual Studio
	* Right-click `ODBCDemo`
		* Select `Manage Nuget Packages`
		* Browse -> Search for these packages and install each one:
			* `Microsoft.Extensions.Configuration`
			* `Microsoft.Extensions.Configuration.Json`
			* `System.Data.Odbc`
	* Right-click `ODBCDemo`
		* Add -> New Item
		* Name the file `appsettings.json`
* Paste this code in appsettings.json:
```
{
  "ConnectionStrings": {
    "Pagila": "Driver={PostgreSQL Unicode};Server=YOUR_SERVER_NAME_GOES_HERE;Port=5432;Database=pagila;Uid=YOUR_USERNAME_GOES_HERE;Pwd=YOUR_PASSWORD_GOES_HERE;"
  }
}
```
* Click F5 or the play button to run the code.
