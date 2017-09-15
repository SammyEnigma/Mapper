using Microsoft.SqlServer.Server;
using System.Diagnostics.Contracts;

namespace BusterWood.Mapper
{
    public struct SqlTableType
    {
        /// <summary>The name of the SQL Table Type, which may include the schema.</summary>
        public string Name { get; }

        /// <summary>The columns of the SQL table type</summary>
        public SqlMetaData[] Columns { get; }

        public SqlTableType(string tableTypeName, params SqlMetaData[] columns)
        {
            Contract.Requires(tableTypeName != null);
            Contract.Requires(columns != null);
            Contract.Requires(columns.Length > 0);
            Name = tableTypeName;
            Columns = columns;
        }
    }
    
}
