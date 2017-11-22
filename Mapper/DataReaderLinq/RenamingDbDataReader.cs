using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics.Contracts;

namespace BusterWood.Mapper
{
    /// <summary>Renames some of the fields in a source <param name="reader"/> using the <param name="newNames"/></summary>
    public class RenamingDbDataReader : DelegatingDbDataReader
    {
        readonly string[] namesByOrdinal;
        readonly Dictionary<string, int> ordinalsByName;

        /// <summary>Renames some of the fields in a source <param name="reader"/> using the <param name="newNames"/></summary>
        public RenamingDbDataReader(DbDataReader reader, IReadOnlyDictionary<int, string> newNames, CommandBehavior commandBehavior) : base (reader, commandBehavior)
        {
            Contract.Requires(reader != null);
            Contract.Requires(newNames != null);
            namesByOrdinal = new string[reader.FieldCount];
            ordinalsByName = new Dictionary<string, int>(newNames.Count);
            foreach (var pair in newNames)
            {
                namesByOrdinal[pair.Key] = pair.Value;
                ordinalsByName[pair.Value] = pair.Key;
            }
        }

        /// <summary>Renames some of the fields in a source <param name="reader"/> using the <param name="newNames"/></summary>
        public RenamingDbDataReader(DbDataReader reader, IReadOnlyDictionary<string, string> newNames, CommandBehavior commandBehavior) : base(reader, commandBehavior)
        {
            Contract.Requires(reader != null);
            Contract.Requires(newNames != null);
            namesByOrdinal = new string[reader.FieldCount];
            ordinalsByName = new Dictionary<string, int>(newNames.Count);
            foreach (var pair in newNames)
            {
                var i = reader.GetOrdinal(pair.Key);
                namesByOrdinal[i] = pair.Value;
                ordinalsByName[pair.Value] = i;
            }
        }

        public override string GetName(int i) => namesByOrdinal[i] ?? base.GetName(i);

        public override int GetOrdinal(string name)
        {
            int i;
            return ordinalsByName.TryGetValue(name, out i) ? i : base.GetOrdinal(name);
        }

    }

}