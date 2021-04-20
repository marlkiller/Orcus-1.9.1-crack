using System;
using System.Collections.Generic;

namespace Orcus.Shared.Csv
{
    /// <summary>
    ///     Class to hold csv data. Taken from http://www.codeproject.com/Articles/86973/C-CSV-Reader-and-Writer
    /// </summary>
    [Serializable]
    public sealed class CsvFile
    {
        /// <summary>
        ///     Gets the file headers
        /// </summary>
        public readonly List<string> Headers = new List<string>();

        /// <summary>
        ///     Gets the records in the file
        /// </summary>
        public readonly CsvRecords Records = new CsvRecords();

        /// <summary>
        ///     Gets the header count
        /// </summary>
        public int HeaderCount => Headers.Count;

        /// <summary>
        ///     Gets the record count
        /// </summary>
        public int RecordCount => Records.Count;

        /// <summary>
        ///     Gets a record at the specified index
        /// </summary>
        /// <param name="recordIndex">Record index</param>
        /// <returns>CsvRecord</returns>
        public CsvRecord this[int recordIndex]
        {
            get
            {
                if (recordIndex > Records.Count - 1)
                    throw new IndexOutOfRangeException($"There is no record at index {recordIndex}.");

                return Records[recordIndex];
            }
        }

        /// <summary>
        ///     Gets the field value at the specified record and field index
        /// </summary>
        /// <param name="recordIndex">Record index</param>
        /// <param name="fieldIndex">Field index</param>
        /// <returns></returns>
        public string this[int recordIndex, int fieldIndex]
        {
            get
            {
                if (recordIndex > Records.Count - 1)
                    throw new IndexOutOfRangeException($"There is no record at index {recordIndex}.");

                var record = Records[recordIndex];
                if (fieldIndex > record.Fields.Count - 1)
                    throw new IndexOutOfRangeException(
                        $"There is no field at index {fieldIndex} in record {recordIndex}.");

                return record.Fields[fieldIndex];
            }
            set
            {
                if (recordIndex > Records.Count - 1)
                    throw new IndexOutOfRangeException($"There is no record at index {recordIndex}.");

                var record = Records[recordIndex];

                if (fieldIndex > record.Fields.Count - 1)
                    throw new IndexOutOfRangeException($"There is no field at index {fieldIndex}.");

                record.Fields[fieldIndex] = value;
            }
        }

        /// <summary>
        ///     Gets the field value at the specified record index for the supplied field name
        /// </summary>
        /// <param name="recordIndex">Record index</param>
        /// <param name="fieldName">Field name</param>
        /// <returns></returns>
        public string this[int recordIndex, string fieldName]
        {
            get
            {
                if (recordIndex > Records.Count - 1)
                    throw new IndexOutOfRangeException($"There is no record at index {recordIndex}.");

                var record = Records[recordIndex];

                var fieldIndex = -1;

                for (var i = 0; i < Headers.Count; i++)
                {
                    if (string.CompareOrdinal(Headers[i], fieldName) != 0)
                        continue;

                    fieldIndex = i;
                    break;
                }

                if (fieldIndex == -1)
                    throw new ArgumentException($"There is no field header with the name '{fieldName}'");

                if (fieldIndex > record.Fields.Count - 1)
                    throw new IndexOutOfRangeException(
                        $"There is no field at index {fieldIndex} in record {recordIndex}.");

                return record.Fields[fieldIndex];
            }
            set
            {
                if (recordIndex > Records.Count - 1)
                    throw new IndexOutOfRangeException($"There is no record at index {recordIndex}.");

                var record = Records[recordIndex];

                var fieldIndex = -1;

                for (var i = 0; i < Headers.Count; i++)
                {
                    if (string.CompareOrdinal(Headers[i], fieldName) != 0)
                        continue;

                    fieldIndex = i;
                    break;
                }

                if (fieldIndex == -1)
                    throw new ArgumentException($"There is no field header with the name '{fieldName}'");

                if (fieldIndex > record.Fields.Count - 1)
                    throw new IndexOutOfRangeException(
                        $"There is no field at index {fieldIndex} in record {recordIndex}.");

                record.Fields[fieldIndex] = value;
            }
        }

        /// <summary>
        ///     Class for a collection of CsvRecord objects
        /// </summary>
        [Serializable]
        public sealed class CsvRecords : List<CsvRecord>
        {
        }

        /// <summary>
        ///     Csv record class
        /// </summary>
        [Serializable]
        public sealed class CsvRecord
        {
            /// <summary>
            ///     Gets the Fields in the record
            /// </summary>
            public readonly List<string> Fields = new List<string>();

            /// <summary>
            ///     Gets the number of fields in the record
            /// </summary>
            public int FieldCount => Fields.Count;
        }
    }
}