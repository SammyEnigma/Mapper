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
            var mappings = CreateMemberToColumnMap(cols, parameter.GetType());

            var sb = new StringBuilder(100);
            sb.Append("EXEC ").Append(procName).Append(" ");
            foreach (var map in mappings)
            {
                sb.AppendLine().Append("    @").Append(map.ToName).Append(" = @").Append(map.FromName).Append(",");
            }
            sb.Length -= 1; // remove last comma
            return sb.ToString();
        }

        static Column ProcColToColumn(ProcColumn pc, int ordinal) => new Column(ordinal, pc.Name.TrimStart('@'), Types.TypeFromSqlTypeName(pc.Type));

        static List<Mapping> CreateMemberToColumnMap(IReadOnlyCollection<Column> columns, Type type)
        {
            Contract.Requires(type != null);
            Contract.Requires(columns != null);

            var result = new List<Mapping>();
            var membersByName = Types.ReadablePublicFieldsAndProperties(type).ToDictionary(mi => mi.Name, StringComparer.OrdinalIgnoreCase);
            foreach (var col in columns)
            {
                var member = Names.CandidateNames(col.Name, col.Type).Where(membersByName.ContainsKey).Select(name => membersByName[name]).FirstOrDefault();
                if (member != null)
                    result.Add(new Mapping { FromName = member.Name, FromType = member.PropertyOrFieldType(), ToName = col.Name, ToType = col.Type });
                else
                {
                    result.Add(new Mapping { FromName = "?", FromType = null, ToName = col.Name, ToType = col.Type });
                    _trace.OnNext($"Cannot find a member for the stored proc parameter {col.Name}");
                }
            }
            return result;
        }
    }

    class ProcColumn
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public int MaxLength { get; set; }
    }

    class Mapping
    {
        public string FromName { get; set; }
        public Type FromType { get; set; }

        public string ToName { get; set; }
        public Type ToType { get; set; }
    }
}
