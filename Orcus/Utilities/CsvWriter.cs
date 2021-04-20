using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Orcus.Shared.Csv;

namespace Orcus.Utilities
{
    /// <summary>
    ///     Class to write data to a csv file. Taken from http://www.codeproject.com/Articles/86973/C-CSV-Reader-and-Writer
    /// </summary>
    public sealed class CsvWriter : IDisposable
    {
        private StreamWriter _streamWriter;

        /// <summary>
        ///     Gets or sets whether carriage returns and line feeds should be removed from
        ///     field values, the default is true
        /// </summary>
        public bool ReplaceCarriageReturnsAndLineFeedsFromFieldValues { get; set; } = true;

        /// <summary>
        ///     Gets or sets what the carriage return and line feed replacement characters should be
        /// </summary>
        public string CarriageReturnAndLineFeedReplacement { get; set; } = ",";

        /// <summary>
        ///     Disposes of all unmanaged resources
        /// </summary>
        public void Dispose()
        {
            _streamWriter?.Dispose();
        }

        /// <summary>
        ///     Writes csv content to a file
        /// </summary>
        /// <param name="csvFile">CsvFile</param>
        /// <param name="filePath">File path</param>
        public void WriteCsv(CsvFile csvFile, string filePath)
        {
            WriteCsv(csvFile, filePath, null);
        }

        /// <summary>
        ///     Writes csv content to a file
        /// </summary>
        /// <param name="csvFile">CsvFile</param>
        /// <param name="filePath">File path</param>
        /// <param name="encoding">Encoding</param>
        public void WriteCsv(CsvFile csvFile, string filePath, Encoding encoding)
        {
            if (File.Exists(filePath))
                File.Delete(filePath);

            using (var writer = new StreamWriter(filePath, false, encoding ?? Encoding.Default))
            {
                WriteToStream(csvFile, writer);
                writer.Close();
            }
        }

        /// <summary>
        ///     Writes csv content to a stream
        /// </summary>
        /// <param name="csvFile">CsvFile</param>
        /// <param name="stream">Stream</param>
        public void WriteCsv(CsvFile csvFile, Stream stream)
        {
            WriteCsv(csvFile, stream, null);
        }

        /// <summary>
        ///     Writes csv content to a stream
        /// </summary>
        /// <param name="csvFile">CsvFile</param>
        /// <param name="stream">Stream</param>
        /// <param name="encoding">Encoding</param>
        public void WriteCsv(CsvFile csvFile, Stream stream, Encoding encoding)
        {
            stream.Position = 0;
            _streamWriter = new StreamWriter(stream, encoding ?? Encoding.Default);
            WriteToStream(csvFile, _streamWriter);
            _streamWriter.Flush();
            stream.Position = 0;
        }

        /// <summary>
        ///     Writes the Csv File
        /// </summary>
        /// <param name="csvFile">CsvFile</param>
        /// <param name="writer">TextWriter</param>
        public void WriteToStream(CsvFile csvFile, TextWriter writer)
        {
            if (csvFile.Headers.Count > 0)
                WriteRecord(csvFile.Headers, writer);

            foreach (var csvRecord in csvFile.Records)
            {
                WriteRecord(csvRecord.Fields, writer);
            }
        }

        /// <summary>
        ///     Writes the record to the underlying stream
        /// </summary>
        /// <param name="fields">Fields</param>
        /// <param name="writer">TextWriter</param>
        private void WriteRecord(IList<string> fields, TextWriter writer)
        {
            for (var i = 0; i < fields.Count; i++)
            {
                var quotesRequired = fields[i].Contains(",");
                var escapeQuotes = fields[i].Contains("\"");
                var fieldValue = escapeQuotes ? fields[i].Replace("\"", "\"\"") : fields[i];

                if (ReplaceCarriageReturnsAndLineFeedsFromFieldValues &&
                    (fieldValue.Contains("\r") || fieldValue.Contains("\n")))
                {
                    quotesRequired = true;
                    fieldValue = fieldValue.Replace("\r\n", CarriageReturnAndLineFeedReplacement);
                    fieldValue = fieldValue.Replace("\r", CarriageReturnAndLineFeedReplacement);
                    fieldValue = fieldValue.Replace("\n", CarriageReturnAndLineFeedReplacement);
                }

                writer.Write("{0}{1}{0}{2}", quotesRequired || escapeQuotes ? "\"" : string.Empty, fieldValue,
                    i < fields.Count - 1 ? "," : string.Empty);
            }

            writer.WriteLine();
        }
    }
}