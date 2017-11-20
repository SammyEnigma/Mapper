using System;
using System.Collections;
using System.Data;
using System.Data.Common;
using System.Diagnostics.Contracts;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace BusterWood.Mapper
{
    /// <summary>Used to filter data before it is mapped to an object</summary>
    public class FilteredDbDataReader : DbDataReader
    {
        private Func<IDataRecord, bool> predicate;
        private DbDataReader reader;

        public FilteredDbDataReader(DbDataReader reader, Func<IDataRecord, bool> predicate)
        {
            Contract.Requires(reader != null);
            Contract.Requires(predicate != null);
            this.reader = reader;
            this.predicate = predicate;
        }

        public override object this[string name] => reader[name];

        public override object this[int i] => reader[i];

        public override int Depth => reader.Depth;

        public override int FieldCount => reader.FieldCount;

        public override bool IsClosed => reader.IsClosed;

        public override int RecordsAffected => reader.RecordsAffected;

        public override bool HasRows => reader.HasRows;

        public override void Close()
        {
            reader.Close();
        }

        public override bool GetBoolean(int i) => reader.GetBoolean(i);

        public override byte GetByte(int i) => reader.GetByte(i);

        public override long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length) => reader.GetBytes(i, fieldOffset, buffer, bufferoffset, length);

        public override char GetChar(int i) => reader.GetChar(i);

        public override long GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length) => reader.GetChars(i, fieldoffset, buffer, bufferoffset, length);

        public override string GetDataTypeName(int i) => reader.GetDataTypeName(i);

        public override DateTime GetDateTime(int i) => reader.GetDateTime(i);

        public override decimal GetDecimal(int i) => reader.GetDecimal(i);

        public override double GetDouble(int i) => reader.GetDouble(i);

        public override Type GetFieldType(int i) => reader.GetFieldType(i);

        public override float GetFloat(int i) => reader.GetFloat(i);

        public override Guid GetGuid(int i) => reader.GetGuid(i);

        public override short GetInt16(int i) => reader.GetInt16(i);

        public override int GetInt32(int i) => reader.GetInt32(i);

        public override long GetInt64(int i) => reader.GetInt64(i);

        public override string GetName(int i) => reader.GetName(i);

        public override int GetOrdinal(string name) => reader.GetOrdinal(name);

        public override DataTable GetSchemaTable() => reader.GetSchemaTable();

        public override string GetString(int i) => reader.GetString(i);

        public override object GetValue(int i) => reader.GetValue(i);

        public override Task<T> GetFieldValueAsync<T>(int ordinal, CancellationToken cancellationToken) => reader.GetFieldValueAsync<T>(ordinal, cancellationToken);

        public override int GetValues(object[] values) => reader.GetValues(values);

        public override bool IsDBNull(int i) => reader.IsDBNull(i);

        public override Task<bool> IsDBNullAsync(int ordinal, CancellationToken cancellationToken) => reader.IsDBNullAsync(ordinal, cancellationToken);

        public override bool NextResult() => reader.NextResult();

        public override Task<bool> NextResultAsync(CancellationToken cancellationToken) => reader.NextResultAsync(cancellationToken);

        public override bool Read()
        {
            for(;;)
            {
                bool got = reader.Read();
                if (!got)
                    return false;
                if (predicate(this))
                    return true;
            }
        }

        public async override Task<bool> ReadAsync(CancellationToken cancellationToken)
        {
            for (;;)
            {
                bool got = await reader.ReadAsync(cancellationToken);
                if (!got)
                    return false;
                if (predicate(this))
                    return true;
            }
        }

        public override Type GetProviderSpecificFieldType(int ordinal) => reader.GetProviderSpecificFieldType(ordinal);

        public override object GetProviderSpecificValue(int ordinal) => reader.GetProviderSpecificValue(ordinal);

        public override int GetProviderSpecificValues(object[] values) => reader.GetProviderSpecificValues(values);

        public override TextReader GetTextReader(int ordinal)=>  reader.GetTextReader(ordinal);

        public override IEnumerator GetEnumerator() => reader.GetEnumerator();
    }
}