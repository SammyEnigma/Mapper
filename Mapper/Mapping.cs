using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;

namespace Mapper
{
    /// <remarks>This is marked as public so users can observe <see cref="Trace"/> for things that have not been mapped</remarks>
    public struct Mapping
    {
        internal static readonly Subject<string> _trace = new Subject<string>();
        public static IObservable<string> Trace => _trace;

        internal Thing From { get; }
        internal Thing To { get; }

        internal Mapping(Thing from, Thing to)
        {
            Contract.Requires(to != null);
            Contract.Requires(from != null);
            To = to;
            From = from;
        }

        /// <summary>
        /// Creates the mapping between <paramref name="sourceMappings"/> and <paramref name="destinationMappings"/> using the DESTINATION to generate candidate names for the mapping
        /// </summary>
        internal static List<Mapping> CreateUsingDestination(Type source, Type destination, string removablePrefix = null) 
            => CreateUsingDestination(Types.WriteablePublicThings(source), Types.WriteablePublicThings(destination), removablePrefix);

        /// <summary>
        /// Creates the mapping between <paramref name="sourceMappings"/> and <paramref name="destinationMappings"/> using the DESTINATION to generate candidate names for the mapping
        /// </summary>
        internal static List<Mapping> CreateUsingDestination(Type source, IReadOnlyCollection<Thing> destinationMappings, string removablePrefix = null) 
            => CreateUsingDestination(Types.WriteablePublicThings(source), destinationMappings, removablePrefix);

        /// <summary>
        /// Creates the mapping between <paramref name="sourceMappings"/> and <paramref name="destinationMappings"/> using the DESTINATION to generate candidate names for the mapping
        /// </summary>
        internal static List<Mapping> CreateUsingDestination(IReadOnlyCollection<Thing> sourceMappings, Type destination, string removablePrefix = null) 
            => CreateUsingDestination(sourceMappings, Types.WriteablePublicThings(destination), removablePrefix);

        /// <summary>
        /// Creates the mapping between <paramref name="sourceMappings"/> and <paramref name="destinationMappings"/> using the DESTINATION to generate candidate names for the mapping
        /// </summary>
        internal static List<Mapping> CreateUsingDestination(IReadOnlyCollection<Thing> sourceMappings, IReadOnlyCollection<Thing> destinationMappings, string removablePrefix = null)
        {
            Contract.Requires(sourceMappings != null);
            Contract.Requires(sourceMappings.Count > 0);
            Contract.Requires(destinationMappings != null);
            Contract.Requires(destinationMappings.Count > 0);

            var result = new List<Mapping>();
            var sourceByName = sourceMappings.ToDictionary(m => m.Name, StringComparer.OrdinalIgnoreCase);
            foreach (Thing dest in destinationMappings)
            {
                var source = Names.Candidates(dest.Name, dest.Type, removablePrefix)
                    .Where(name => sourceByName.ContainsKey(name))
                    .Select(name => sourceByName[name])
                    .Where(src => Types.AreInSomeSenseCompatible(src.Type, dest.Type))
                    .FirstOrDefault();

                if (source != null)
                {
                    result.Add(new Mapping(source, dest));
                    sourceByName.Remove(source.Name); // don't map the same thing twice
                }
                else
                {
                    var type = string.IsNullOrWhiteSpace(removablePrefix) ? string.Empty : removablePrefix + ": ";
                    _trace.OnNext($"{type}Cannot find a mapping for target {dest.Name} or type is not compatible");
                }
            }
            return result;
        }

        /// <summary>
        /// Creates the mapping between <paramref name="source"/> and <paramref name="destination"/> using the SOURCE to generate candidate names for the mapping
        /// </summary>
        internal static List<Mapping> CreateUsingSource(Type source, Type destination, string removablePrefix = null) 
            => CreateUsingSource(Types.WriteablePublicThings(source), Types.WriteablePublicThings(destination), removablePrefix);

        /// <summary>
        /// Creates the mapping between <paramref name="sourceMappings"/> and <paramref name="destination"/> using the SOURCE to generate candidate names for the mapping
        /// </summary>
        internal static List<Mapping> CreateUsingSource(IReadOnlyCollection<Thing> sourceMappings, Type destination, string removablePrefix = null) 
            => CreateUsingSource(sourceMappings, Types.WriteablePublicThings(destination), removablePrefix);

        /// <summary>
        /// Creates the mapping between <paramref name="sourceMappings"/> and <paramref name="destinationMappings"/> using the SOURCE to generate candidate names for the mapping
        /// </summary>
        internal static List<Mapping> CreateUsingSource(IReadOnlyCollection<Thing> sourceMappings, IReadOnlyCollection<Thing> destinationMappings, string removablePrefix = null)
        {
            Contract.Requires(sourceMappings != null);
            Contract.Requires(sourceMappings.Count > 0);
            Contract.Requires(destinationMappings != null);
            Contract.Requires(destinationMappings.Count > 0);

            var result = new List<Mapping>();
            var destByName = destinationMappings.ToDictionary(m => m.Name, StringComparer.OrdinalIgnoreCase);
            foreach (Thing source in sourceMappings)
            {
                var dest = Names.Candidates(source.Name, source.Type, removablePrefix)
                    .Where(name => destByName.ContainsKey(name))
                    .Select(name => destByName[name])
                    .Where(d => Types.AreInSomeSenseCompatible(source.Type, d.Type))
                    .FirstOrDefault();

                if (dest != null)
                {
                    result.Add(new Mapping(source, dest));
                    destByName.Remove(source.Name); // don't map the same thing twice
                }
                else
                {
                    var type = string.IsNullOrWhiteSpace(removablePrefix) ? string.Empty : removablePrefix + ": ";
                    _trace.OnNext($"{type}Cannot find a mapping for target {source.Name} or type is not compatible");
                }
            }
            return result;
        }

    }

    /// <summary>The thing being mapped, i.e. a <see cref="Field"/> or <see cref="Property"/> or <see cref="Parameter"/> or <see cref="Column"/></summary>
    /// <remarks>Not a great name but I can't think of a better one!</remarks>
    interface Thing
    {
        string Name { get; }
        Type Type { get; }
    }

    struct Field : Thing
    {
        public FieldInfo Wrapped { get; }
        public string Name => Wrapped.Name;
        public Type Type => Wrapped.FieldType;

        public Field(FieldInfo field)
        {
            Contract.Requires(field != null);
            Wrapped = field;
        }
    }

    struct Property : Thing
    {
        public PropertyInfo Wrapped { get; }
        public string Name => Wrapped.Name;
        public Type Type => Wrapped.PropertyType;

        public Property(PropertyInfo prop)
        {
            Contract.Requires(prop != null);
            Wrapped = prop;
        }
    }

    struct Parameter : Thing
    {
        public ParameterInfo Wrapped { get; }
        public string Name => Wrapped.Name;
        public Type Type => Wrapped.ParameterType;

        public Parameter(ParameterInfo parameter)
        {
            Contract.Requires(parameter != null);
            Wrapped = parameter;
        }
    }

    /// <remarks>
    /// This needs to override equality as it is used by <see cref="DataReaderMapper.MetaData.Equals(DataReaderMapper.MetaData)"/>
    /// </remarks>
    struct Column : Thing, IEquatable<Column>
    {
        readonly string name;
        readonly Type type;

        public string Name => name;
        public Type Type => type;
        public int Ordinal { get; }

        public Column(int ordinal, string name, Type type)
        {
            Ordinal = ordinal;
            this.type = type;
            this.name = name;
        }

        public override bool Equals(object obj) => obj is Column && Equals((Column)obj);

        public bool Equals(Column other)
        {
            return string.Equals(name, other.name, StringComparison.OrdinalIgnoreCase) && Equals(type, other.type);
        }

        public override int GetHashCode()
        {
            unchecked { return name.GetHashCode() * type.GetHashCode();  }
        }
    }
}
