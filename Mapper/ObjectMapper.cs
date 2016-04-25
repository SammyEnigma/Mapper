using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Mapper
{
    public static class ObjectMapper
    {
        static readonly MostlyReadDictionary<TypePair, Delegate> MapMethods = new MostlyReadDictionary<TypePair, Delegate>();

        static readonly Subject<string> _trace = new Subject<string>();

        public static IObservable<string> Trace => _trace;

        internal static Func<TIn, TOut> GetOrAddMapping<TIn, TOut>()
        {
            return (Func<TIn, TOut>)MapMethods.GetOrAdd(new TypePair(typeof(TIn), typeof(TOut)), _ => CreateMapDelegate<TIn, TOut>());
        }

        /// <summary>
        /// Fast copying code that generates a method that does the copy from one type to another
        /// </summary>
        static internal Delegate CreateMapDelegate<TIn, TOut>()
        {
            Contract.Requires(typeof(TOut).GetConstructor(Type.EmptyTypes) != null);
            Contract.Ensures(Contract.Result<Delegate>() != null);

            var input = Expression.Parameter(typeof(TIn), "input");
            var result = Expression.Parameter(typeof(TOut), "result");

            var lines = new List<Expression> { Expression.Assign(result, Expression.New(typeof(TOut).GetConstructor(Type.EmptyTypes))) };

            var outByName = Types.WritablePropertiesAndFields<TOut>();
            var inByName = Types.ReadablePropertiesAndFields<TIn>();

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
                    {
                        _trace.OnNext($"Don't know how to map {inPF.DeclaringType}.{inPF.Name} to type {typeof(TOut)}");
                        continue;
                    }
                }
                lines.Add(Expression.Assign(Expression.PropertyOrField(result, outPF.Name), value));
            }
            lines.Add(result); // the return value
            var block = Expression.Block(new[] { result }, lines);
            return Expression.Lambda<Func<TIn, TOut>>(block, input).Compile();
        }

        static MemberInfo FindOutPropertyOrField(IDictionary<string, MemberInfo> outByName, MemberInfo inPF)
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

        public bool Equals(TypePair other) => In == other.In && Out == other.Out;

        public override bool Equals(object obj) => obj is TypePair && Equals((TypePair)obj);

        public override int GetHashCode()
        {
            unchecked
            {
                return (In.GetHashCode() * 397) ^ Out.GetHashCode();
            }
        }
    }
}
