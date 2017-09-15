using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using Microsoft.SqlServer.Server;

namespace BusterWood.Mapper
{
    /// <summary>An instance of a <see cref="SqlTableType"/> that contains some records</summary>
    public class SqlTable : IEnumerable<SqlDataRecord>
    {
        /// <summary>The name of the SQL Server table type</summary>
        public string TypeName { get; }

        /// <summary>The encoded <see cref="SqlDataRecord"/></summary>
        public IEnumerable<SqlDataRecord> Records { get; }

        public SqlTable(string typeName, IEnumerable<SqlDataRecord> records)
        {
            Contract.Requires(typeName != null);
            // note: empty records is not allowed and will cause a SQL exception, null must be passed to SQL to avoid the exception
            TypeName = typeName;
            Records = records;
        }

        /// <summary>Returns an enumerator that iterates through the collection.</summary>
        public IEnumerator<SqlDataRecord> GetEnumerator() => Records.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

}