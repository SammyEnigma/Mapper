using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Mapper
{
    struct Column : IEquatable<Column>
    {
        public int Ordinal { get; }
        public string Name { get; }
        public Type Type { get; }

        public Column(int ordinal, string name, Type type)
        {
            Ordinal = ordinal;
            Name = name;
            Type = type;
        }

        public override bool Equals(object obj) => obj is Column && Equals((Column)obj);
        public bool Equals(Column other) => Ordinal == other.Ordinal && Name == other.Name && Type == other.Type;
        public override int GetHashCode() => Ordinal;
        public static bool operator ==(Column left, Column right) => left.Equals(right);
        public static bool operator !=(Column left, Column right) => !left.Equals(right);
    }

    static class ColumnExtensions
    {
        public static Func<string, Column> MatchFunc(this IReadOnlyCollection<Column> columns)
        {
            Contract.Requires(columns != null);
            var nameToColumns = columns
                .SelectMany(col => Names.CandidateNames(col.Name, col.Type), (col, name) => new { col, name })
                .OrderByDescending(x => x.col.Name.Length) // make sure columns with the longest names match first
                .ToLookup(x => x.name, x => x.col, StringComparer.OrdinalIgnoreCase);
            return name => nameToColumns[name].FirstOrDefault();
        }
    }
}
