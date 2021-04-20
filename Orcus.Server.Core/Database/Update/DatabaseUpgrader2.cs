using Mono.Data.Sqlite;
using NLog;

namespace Orcus.Server.Core.Database.Update
{
    internal class DatabaseUpgrader2
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public static SqliteConnection UpgradeDatabase(string fileName, SqliteConnection sqliteConnection)
        {
            Logger.Info("Removing obsolete tables...");
            using (
                var command =
                    new SqliteCommand(
                        "DROP TABLE `DynamicCommand`",
                        sqliteConnection))
                command.ExecuteNonQuery();

            Logger.Info("Creating new tables...");
            using (
                var command =
                    new SqliteCommand(
                        "CREATE TABLE `DynamicCommand` (Id INTEGER PRIMARY KEY, Status INTEGER, DynamicCommand TEXT, ParameterDataId TEXT UNIQUE, PluginId INTEGER, Timestamp DATETIME)",                        sqliteConnection))
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
                        "ALTER TABLE `Client` ADD MacAddress NONE",
                        sqliteConnection))
                command.ExecuteNonQuery();

            Logger.Info("Upgrade finished");
            return sqliteConnection;
        }
    }
}