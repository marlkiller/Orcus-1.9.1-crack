using System;
using System.Globalization;
using System.IO;
using System.Net;
using System.Threading;
using ICSharpCode.SharpZipLib.Zip;
using Mono.Data.Sqlite;
using NLog;
using Orcus.Server.Core.Config;
using Orcus.Server.Core.UI;
using Orcus.Server.Core.Utilities;
using Orcus.Shared.Connection;
#if DEBUG
using System.Diagnostics;

#endif

namespace Orcus.Server.Core.GeoIp
{
	public class Ip2LocationService : IDisposable
	{
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private const string DatabaseProduct = "DB11LITE";
		private readonly object _databaseLock = new object();
		private string _currentIpAddress;
		private DateTime _currentIpAddressTimestamp;
		private SqliteConnection _sqliteConnection;
		private bool _updateRequired;

		public bool IsStarted { get; private set; }

		public void Dispose()
		{
			_sqliteConnection?.Dispose();
			_sqliteConnection = null;
			IsStarted = false;
		}

		private string GetCurrentIpAddress()
		{
			if (_currentIpAddress == null ||
			    (DateTime.Now - _currentIpAddressTimestamp).TotalHours >
			    int.Parse(GlobalConfig.Current.IniFile.GetKeyValue("GEOIP_LOCATION", "RefreshServerIpPeriod")))
			{
				_currentIpAddress = new WebClient().DownloadString("https://api.ipify.org/");
				_currentIpAddressTimestamp = DateTime.Now;
			}

			return _currentIpAddress;
		}

		public void Stop()
		{
            Logger.Debug("Stop IP2Location service");
			_sqliteConnection?.Dispose();
			_sqliteConnection = null;
			IsStarted = false;
		}

		public void Start(string emailAddress, string password)
		{
			if (_sqliteConnection != null && !_updateRequired)
				return;

            Logger.Debug("Start IP2Location service");

            var directory = new DirectoryInfo(GlobalConfig.Current.IniFile.GetKeyValue("GEOIP_LOCATION", "Directory"));
			var databaseFile = new FileInfo(Path.Combine(directory.FullName, "geoipDatabase.sqlite"));
			if (!CheckDatabase(directory, databaseFile, out _sqliteConnection))
			{
				if (emailAddress == null || password == null)
				{
					_updateRequired = true;
					IsStarted = false;
					return;
				}

				try
				{
					DownloadDatabase(directory, emailAddress, password);
				}
				catch (Exception ex)
				{
					Logger.Error(ex, "An error occurred when trying to download the geo location database");
					return;
				}
				_sqliteConnection = new SqliteConnection($"Data Source={databaseFile.FullName};Version=3;");
				_sqliteConnection.Open();
			}

			Logger.Info("Geo-IP database loaded");
			IsStarted = true;
		}

		private long IpToLong(string ipAddress)
		{
			long longIp = 0;
			var parts = ipAddress.Split('.');
			for (int i = 0; i < parts.Length - 1; i++)
			{
				longIp += (long) (int.Parse(parts[i])%256*Math.Pow(256, 3 - i));
			}

			return longIp;
		}

		public LocationInfo GetLocationInfo(string ipAddress)
		{
			return InternalGetLocationInfo(IpToLong(ipAddress), false);
		}

		public LocationInfo GetLocationInfo(long ipAddress)
		{
			return InternalGetLocationInfo(ipAddress, false);
		}

		private LocationInfo InternalGetLocationInfo(long ipAddress, bool usingServerIpAddress)
		{
			if (_sqliteConnection == null)
				throw new InvalidOperationException(nameof(_sqliteConnection));

			lock (_databaseLock)
			{
#if DEBUG
				var sw = Stopwatch.StartNew();
#endif
				using (
					var command =
						new SqliteCommand(
							$"SELECT Country, CountryName, Region, City, Latitude, Longitude, ZipCode, Timezone FROM `IpLocation` WHERE {ipAddress} BETWEEN IpFrom AND IpTo LIMIT 1",
							_sqliteConnection))
				using (var reader = command.ExecuteReader())
				{
					if (!reader.Read())
						return null;

					if (reader.GetString(0) == "-")
					{
						if (
							bool.Parse(GlobalConfig.Current.IniFile.GetKeyValue("GEOIP_LOCATION",
								"UseServerIpAddressIfOutOfRange")) && !usingServerIpAddress)
							return InternalGetLocationInfo(IpToLong(GetCurrentIpAddress()), true);
						return null;
					}

#if DEBUG
					Debug.Print("IP database call: " + sw.ElapsedMilliseconds);
#endif
					return new LocationInfo
					{
						Country = reader.GetString(0),
						CountryName = reader.GetString(1),
						Region = reader.GetString(2),
						City = reader.GetString(3),
						Latitude = reader.GetFloat(4),
						Longitude = reader.GetFloat(5),
						ZipCode = reader.GetString(6),
						Timezone = int.Parse(reader.GetString(7).Split(':')[0])
					};
				}
			}
		}

		private bool CheckDatabase(DirectoryInfo directory, FileInfo databaseFile, out SqliteConnection sqliteConnection)
		{
			sqliteConnection = null;

			if (!directory.Exists || !databaseFile.Exists)
				return false;

			try
			{
				sqliteConnection = new SqliteConnection($"Data Source={databaseFile.FullName};Version=3;");
				sqliteConnection.Open();

				using (
					var command = new SqliteCommand("SELECT Value FROM `Properties` WHERE Name='CreationDate'",
						sqliteConnection))
				using (var reader = command.ExecuteReader())
				{
					if (!reader.Read())
					{
                        command.Dispose();
                        reader.Dispose();
                        GC.Collect();
                        GC.WaitForPendingFinalizers();
                        sqliteConnection.Dispose();
						return false;
					}

					var creationDate = DateTime.Parse(reader.GetString(0), CultureInfo.InvariantCulture);
					var now = DateTime.Now;
					var lastUpdateDate = new DateTime(now.Year, now.Month,
						1 + int.Parse(GlobalConfig.Current.IniFile.GetKeyValue("GEOIP_LOCATION", "PatchDay")));
					if (lastUpdateDate > creationDate && DateTime.Now > lastUpdateDate)
                    {
                        command.Dispose();
                        reader.Dispose();
                        GC.Collect();
                        GC.WaitForPendingFinalizers();
						sqliteConnection.Close();
						return false;
					}

					return true;
				}
			}
			catch (Exception)
			{
                sqliteConnection?.Close();
                GC.Collect();
                GC.WaitForPendingFinalizers();
                return false;
			}
		}

		public void DownloadDatabase(DirectoryInfo directory, string emailAddress, string password)
		{
			Logger.Info("Geo-IP database not found, preparing download...");
			if (directory.Exists)
				directory.Delete(true);

            Logger.Debug("Create directory");
            directory.Create();

			FileInfo csvFile = null;

			if (File.Exists("IP2LOCATION-LITE-DB11.CSV"))
			{
				csvFile = new FileInfo("IP2LOCATION-LITE-DB11.CSV");
				Logger.Info("Local CSV file found");
			}
			else
			{
				var zipFile = new FileInfo(Path.Combine(directory.FullName, "temp.zip"));
				var progressInfo =
					new ProgressBarInfo(
						$"{DateTime.Now:dd-MM-yyyy HH:mm:ss.ffff}\t[PROGRESS] Download IP2Location database");
				UiManager.UiImplementation.ShowProgressBar(progressInfo);
				try
				{
					using (var webClient = new WebClient {Proxy = null})
					{
						var url =
							$"http://www.ip2location.com/download?login={Uri.EscapeDataString(emailAddress)}&password={Uri.EscapeDataString(password)}&productcode={DatabaseProduct}";
						webClient.DownloadProgressChanged +=
							(sender, args) =>
								progressInfo.ReportProgress(args.ProgressPercentage/100d);
						using (var autoResetEvent = new AutoResetEvent(false))
						{
							webClient.DownloadFileCompleted += (sender, args) => autoResetEvent.Set();
							webClient.DownloadFileAsync(new Uri(url), zipFile.FullName);
							autoResetEvent.WaitOne();
						}
					}
				}
				finally
				{
					progressInfo.Close();
				}

				if (zipFile.Length < 10*1024) //noob tactic, checking the file size
				{
					var errorText = File.ReadAllText(zipFile.FullName);
					directory.Delete(true);
					throw new InvalidOperationException(errorText);
				}

				Logger.Info("Download finished successfully");

				using (var fileStream = new FileStream(zipFile.FullName, FileMode.Open, FileAccess.Read))
				{
					var zf = new ZipFile(fileStream);
					foreach (ZipEntry zipEntry in zf)
					{
						if (zipEntry.IsFile && zipEntry.Name.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
						{
							var zipProgressInfo =
								new ProgressBarInfo($"{DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss.ffff")}\t[PROGRESS] Extracting archive");
							UiManager.UiImplementation.ShowProgressBar(zipProgressInfo);
							try
							{
								using (var csvFileStream = new FileStream("IP2LOCATION-LITE-DB11.CSV", FileMode.CreateNew, FileAccess.Write))
								using (var inputStream = zf.GetInputStream(zipEntry))
								{
									byte[] buffer = new byte[16*1024];
									int read;
									while ((read = inputStream.Read(buffer, 0, buffer.Length)) > 0)
									{
										csvFileStream.Write(buffer, 0, read);
										zipProgressInfo.ReportProgress(csvFileStream.Position/
										                               (double) zipEntry.Size);
									}
								}
							}
							finally
							{
								zipProgressInfo.Close();
							}
							csvFile = new FileInfo("IP2LOCATION-LITE-DB11.CSV");
							break;
						}
					}
				}

				if (csvFile == null)
					throw new FileNotFoundException("Could not find CSV file in zip archive");

				zipFile.Delete();
			}

			Logger.Info("Building database");
			var databaseFile = new FileInfo(Path.Combine(directory.FullName, "geoipDatabase.sqlite"));
			SqliteConnection.CreateFile(databaseFile.FullName);
			using (var sqliteConnection = new SqliteConnection($"Data Source={databaseFile.FullName};Version=3;"))
			{
				sqliteConnection.Open();
				using (
					var command =
						new SqliteCommand(
							"CREATE TABLE `IpLocation` (IpFrom INTEGER UNIQUE, IpTo INTEGER PRIMARY KEY, Country TEXT, CountryName TEXT, Region TEXT, City TEXT, Latitude REAL, Longitude REAL, ZipCode TEXT, Timezone TEXT)",
							sqliteConnection))
					command.ExecuteNonQuery();

				using (
					var command =
						new SqliteCommand(
							"CREATE TABLE `Properties` (Name TEXT, Value TEXT)",
							sqliteConnection))
					command.ExecuteNonQuery();

				using (
					var command =
						new SqliteCommand(
							$"INSERT INTO `Properties` (Name, Value) VALUES ('CreationDate', '{DateTime.Now.ToString(CultureInfo.InvariantCulture)}')",
							sqliteConnection))
					command.ExecuteNonQuery();

				using (var csvReader = new CsvReader(csvFile.FullName))
				using (var transaction = sqliteConnection.BeginTransaction())
				{
					using (var command = sqliteConnection.CreateCommand())
					{
						command.Transaction = transaction;
						while (csvReader.ReadNextRecord())
						{
							command.CommandText =
								$"INSERT INTO `IpLocation` (IpFrom, IpTo, Country, CountryName, Region, City, Latitude, Longitude, ZipCode, Timezone) VALUES ({csvReader.Fields[0]}, {csvReader.Fields[1]}, '{csvReader.Fields[2]}', @countryName, @region, @city, {csvReader.Fields[6]}, {csvReader.Fields[7]}, '{csvReader.Fields[8]}', '{csvReader.Fields[9]}')";
							command.Parameters.AddWithValue("@countryName", csvReader.Fields[3]);
							command.Parameters.AddWithValue("@region", csvReader.Fields[4]);
							command.Parameters.AddWithValue("@city", csvReader.Fields[5]);
							command.ExecuteNonQuery();
						}
						transaction.Commit();
					}
				}

				Logger.Info("Database created");
			}
		}
	}
}