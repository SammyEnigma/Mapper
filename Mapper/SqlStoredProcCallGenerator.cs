using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;

namespace Mapper
{
    public class SqlStoredProcCallGenerator
    {
        static readonly Subject<string> _trace = new Subject<string>();

        public static IObservable<string> Trace => _trace;

        internal static string Generate(DbConnection connection, string procName, object parameter)
        {
            const string sql = @"select p.name, t.name as [type], p.max_length
from sys.all_parameters p 
join sys.systypes t on t.xtype = p.system_type_id
where p.[object_id] = OBJECT_ID(@procName, 'P') 
order by p.parameter_id";

            var procCols = connection.QueryList<ProcColumn>(sql, new { procName });
            Console.WriteLine($"proc has {procCols.Count} parameters");
            var cols = new List<Column>();
            int i = 0;
            foreach (var pc in procCols)
            {
                cols.Add(ProcColToColumn(pc, i++));
            }
            var mappings = Mapping.CreateUsingSource(cols, Types.WriteablePublicThings(parameter.GetType()));

            var sb = new StringBuilder(100);
            sb.Append("EXEC ").Append(procName).Append(" ");
            foreach (var map in mappings)
            {
                sb.AppendLine().Append("    @").Append(map.To.Name).Append(" = @").Append(map.From.Name).Append(",");
            }
            sb.Length -= 1; // remove last comma
            return sb.ToString();
        }

        static Column ProcColToColumn(ProcColumn pc, int ordinal) => new Column(ordinal, pc.Name.TrimStart('@'), Types.TypeFromSqlTypeName(pc.Type));
    }

    class ProcColumn
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public int MaxLength { get; set; }
    }

}
