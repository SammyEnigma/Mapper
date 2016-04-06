using System;
using System.Data;

namespace Mapper.UnitTests
{
    public class StubDataReader : IDataReader
    {
        public string[] Names;
        public Type[] Types;
        public object[] Values;

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public string GetName(int i)
        {
            return Names[i];
        }

        public string GetDataTypeName(int i)
        {
            throw new NotImplementedException();
        }

        public Type GetFieldType(int i)
        {
            return Types[i];
        }

        public object GetValue(int i)
        {
            return Values[i];
        }

        public int GetValues(object[] values)
        {
            throw new NotImplementedException();
        }

        public int GetOrdinal(string name)
        {
            throw new NotImplementedException();
        }

        public bool GetBoolean(int i)
        {
            throw new NotImplementedException();
        }

        public byte GetByte(int i)
        {
            throw new NotImplementedException();
        }

        public long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length)
        {
            throw new NotImplementedException();
        }

        public char GetChar(int i)
        {
            throw new NotImplementedException();
        }

        public long GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length)
        {
            throw new NotImplementedException();
        }

        public Guid GetGuid(int i)
        {
            throw new NotImplementedException();
        }

        public short GetInt16(int i)
        {
            return (short) Values[i];
        }

        public int GetInt32(int i)
        {
            return (int) Values[i];
        }

        public long GetInt64(int i)
        {
            return (long) Values[i];
        }

        public float GetFloat(int i)
        {
            return (float)Values[i];
        }

        public double GetDouble(int i)
        {
            return (double)Values[i];
        }

        public string GetString(int i)
        {
            return (string)Values[i];
        }

        public decimal GetDecimal(int i)
        {
            return (decimal)Values[i];
        }

        public DateTime GetDateTime(int i)
        {
            throw new NotImplementedException();
        }

        public IDataReader GetData(int i)
        {
            throw new NotImplementedException();
        }

        public bool IsDBNull(int i)
        {
            return Values[i] is DBNull;
        }

        public int FieldCount => Names.Length;

        object IDataRecord.this[int i]
        {
            get { throw new NotImplementedException(); }
        }

        object IDataRecord.this[string name]
        {
            get { throw new NotImplementedException(); }
        }

        public void Close()
        {
            throw new NotImplementedException();
        }

        public DataTable GetSchemaTable()
        {
            throw new NotImplementedException();
        }

        public bool NextResult()
        {
            throw new NotImplementedException();
        }

        public bool Read()
        {
            throw new NotImplementedException();
        }

        public int Depth { get; set;  }
        public bool IsClosed { get; set; }
        public int RecordsAffected { get; set; }
    }
}