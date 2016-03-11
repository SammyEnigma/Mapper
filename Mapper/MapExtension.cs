using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Mapper
{
    public static class MapExtension
    {
        private static readonly MostlyReadDictionary<TypePair, Delegate> MapMethods = new MostlyReadDictionary<TypePair, Delegate>();

        /// <summary>Create an output object and copies all properties and fields where the property name and types match</summary>
        public static TOut Map<TIn, TOut>(this TIn input)
        {
            Contract.Requires(input != null);
            Contract.Ensures(Contract.Result<TOut>() != null);
            var map = (Func<TIn, TOut>)MapMethods.GetOrAdd(new TypePair(typeof(TIn), typeof(TOut)), _ => CreateMapDelegate<TIn, TOut>());
            return map(input);
        }

        /// <summary>Create an output object and copies all properties and fields</summary>
        public static T Clone<T>(this T input)
        {
            Contract.Requires(input != null);
            Contract.Ensures(Contract.Result<T>() != null);
            var map = (Func<T, T>)MapMethods.GetOrAdd(new TypePair(typeof(T), typeof(T)), _ => CreateMapDelegate<T, T>());
            return map(input);
        }


        /// <summary>creates copies of all input objects, copying all properties and fields with matching names and compatible types</summary>
        public static IEnumerable<TOut> Map<TIn, TOut>(this IEnumerable<TIn> input) {
            Contract.Requires(input != null);
            Contract.Ensures(Contract.Result<IEnumerable<TOut>>() != null);
            var map = (Func<TIn, TOut>)MapMethods.GetOrAdd(new TypePair(typeof(TIn), typeof(TOut)), _ => CreateMapDelegate<TIn, TOut>());
            return input.Select(map);
        }

        /// <summary>creates copies of all input objects, copying all properties and fields with matching names and compatible types</summary>
        public static IEnumerable<TOut> Map<TIn, TOut>(this IEnumerable<TIn> input, Action<TIn, TOut> extraAction) {
            Contract.Requires(input != null);
            Contract.Requires(extraAction != null);
            Contract.Ensures(Contract.Result<IEnumerable<TOut>>() != null);
            var map = (Func<TIn, TOut>)MapMethods.GetOrAdd(new TypePair(typeof(TIn), typeof(TOut)), _ => CreateMapDelegate<TIn, TOut>());
            foreach (var item in input)
            {
                var copy = map(item);
                extraAction(item, copy);
                yield return copy;
            }
        }

        /// <summary>
        /// Fast copying code that generates a method that does the copy from one type to another
        /// </summary>
        private static Delegate CreateMapDelegate<TIn, TOut>()
        {
            Contract.Requires(typeof(TOut).GetConstructor(Type.EmptyTypes) != null);
            Contract.Ensures(Contract.Result<Delegate>() != null);

            var input = Expression.Parameter(typeof(TIn), "input");
            var result = Expression.Parameter(typeof(TOut), "result");

            var lines = new List<Expression> { Expression.Assign(result, Expression.New(typeof(TOut).GetConstructor(Type.EmptyTypes))) };

            var outByName = WritablePropertiesAndFields<TOut>();
            var inByName = ReadablePropertiesAndFields<TIn>();

            foreach (var inPF in inByName.Select(pair => pair.Value))
            {
                var outPF = FindOutPropertyOrField(outByName, inPF);
                if (outPF == null) continue;
                
                var outType = Types.PropertyOrFieldType(outPF);
                var inType = Types.PropertyOrFieldType(inPF);
                Expression value = Expression.PropertyOrField(input, inPF.Name);
                if (inType != outType)
                {
                    // type if not the same, can it be assigned?
                    if (Types.CanBeCast(inType, outType))
                    {
                        value = Expression.Convert(value, outType);
                    }
                    else if (Types.IsNullable(inType) && inType.GetGenericArguments()[0] == outType)
                    {
                        value = Expression.Call(value, inType.GetMethod("GetValueOrDefault", Type.EmptyTypes));
                    }
                    else if (Types.IsNullable(inType) && Types.CanBeCast(inType.GetGenericArguments()[0], outType))
                    {
                        value = Expression.Convert(Expression.Call(value, inType.GetMethod("GetValueOrDefault", Type.EmptyTypes)), outType);
                    }
                    else 
                        continue;                    
                }
                lines.Add(Expression.Assign(Expression.PropertyOrField(result, outPF.Name), value));
            }
            lines.Add(result); // the return value
            var block = Expression.Block(new[] { result }, lines);
            return Expression.Lambda<Func<TIn, TOut>>(block, input).Compile();
        }

        private static MemberInfo FindOutPropertyOrField(IDictionary<string, MemberInfo> outByName, MemberInfo inPF)
        {
            Contract.Requires(outByName != null);
            Contract.Requires(inPF != null);
            foreach (var name in Names.CandidateNames(inPF.Name, Types.PropertyOrFieldType(inPF)))
            {
                MemberInfo outPF;
                if (outByName.TryGetValue(name, out outPF)) // names match
                    return outPF;
            }
            return null;
        }

        private static Dictionary<string, MemberInfo> WritablePropertiesAndFields<T>()
        {
            var map = typeof (T).GetProperties(BindingFlags.Instance | BindingFlags.Public).Where(p => p.CanWrite)
                .Cast<MemberInfo>()
                .ToDictionary(p => p.Name, p => p, StringComparer.OrdinalIgnoreCase);
            foreach (var field in typeof (T).GetFields(BindingFlags.Instance | BindingFlags.Public).Where(f => !f.IsInitOnly))
            {
                map[field.Name] = field;
            }
            return map;
        }

        private static Dictionary<string, MemberInfo> ReadablePropertiesAndFields<T>()
        {
            var map = typeof (T).GetProperties(BindingFlags.Instance | BindingFlags.Public).Where(p => p.CanRead)
                .Cast<MemberInfo>()
                .ToDictionary(p => p.Name, p => p, StringComparer.OrdinalIgnoreCase);
            foreach (var field in typeof (T).GetFields(BindingFlags.Instance | BindingFlags.Public))
            {
                map[field.Name] = field;
            }
            return map;
        }

        struct TypePair : IEquatable<TypePair>
        {
            public readonly Type In;
            public readonly Type Out;

            public TypePair(Type @in, Type @out)
            {
                In = @in;
                Out = @out;
            }

            public bool Equals(TypePair other)
            {
                return In == other.In && Out == other.Out;
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                return obj is TypePair && Equals((TypePair) obj);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return ((In?.GetHashCode() ?? 0)*397) ^ (Out?.GetHashCode() ?? 0);
                }
            }            
        }
    }
}