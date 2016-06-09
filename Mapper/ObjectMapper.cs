using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;

namespace Mapper
{
    public static class ObjectMapper
    {
        static readonly MostlyReadDictionary<TypePair, Delegate> MapMethods = new MostlyReadDictionary<TypePair, Delegate>();

        internal static Func<TIn, TOut> GetOrAddMapping<TIn, TOut>()
        {
            return (Func<TIn, TOut>)MapMethods.GetOrAdd(new TypePair(typeof(TIn), typeof(TOut)), _ => CreateMapDelegate(typeof(TIn), typeof(TOut)));
        }

        /// <summary>Fast copying code that generates a method that does the copy from one type to another</summary>
        static internal Delegate CreateMapDelegate(Type inType, Type outType)
        {
            Contract.Requires(outType != null);
            Contract.Requires(inType != null);
            Contract.Ensures(Contract.Result<Delegate>() != null);

            List<Mapping> mapping = Mapping.CreateUsingSource(inType, outType);
            LambdaExpression lambdaExpression = CreateMappingLambda(inType, outType, mapping);
            return lambdaExpression.Compile();
        }

        static LambdaExpression CreateMappingLambda(Type inType, Type outType, List<Mapping> mapping)
        {
            Contract.Requires(mapping != null);
            Contract.Requires(outType != null);
            Contract.Requires(inType != null);
            Contract.Ensures(Contract.Result<LambdaExpression>() != null);

            if (outType.IsClass && outType.GetConstructor(Type.EmptyTypes) == null)
                throw new ArgumentException("Output type must have a parameterless constructor");

            var input = Expression.Parameter(inType, "input");
            var result = Expression.Parameter(outType, "result");

            var ctor = outType.IsClass ? (Expression)Expression.New(outType.GetConstructor(Type.EmptyTypes)) : Expression.Default(outType);
            var lines = new List<Expression> { Expression.Assign(result, ctor) };

            foreach (Mapping map in mapping)
            {
                Expression readValue = ReadValue(input, map);
                lines.Add(Expression.Assign(Expression.PropertyOrField(result, map.To.Name), readValue));
            }
            lines.Add(result); // the return value

            var delegateType = typeof(Func<,>).MakeGenericType(new[] { inType, outType });
            var body = Expression.Block(new[] { result }, lines);
            LambdaExpression lambdaExpression = Expression.Lambda(delegateType, body, new[] { input });
            return lambdaExpression;
        }

        static Expression ReadValue(ParameterExpression input, Mapping map)
        {
            Expression value = Expression.PropertyOrField(input, map.From.Name);
            var fromType = map.From.Type;
            var toType = map.To.Type;
            if (fromType == toType)
            {
                return value;
            }
            if (Types.CanBeCast(fromType, toType))
            {
                return Expression.Convert(value, toType);
            }
            if (Types.IsNullable(fromType))
            {
                var nullableArgType = fromType.GetGenericArguments()[0];
                if (nullableArgType == toType)
                {
                    return Expression.Call(value, fromType.GetMethod("GetValueOrDefault", Type.EmptyTypes));
                }
                if (Types.IsNullable(toType) && Types.CanBeCast(nullableArgType, toType))
                {
                    // nullable<> to nullable<> conversion must handle null to null as a special case
                    return Expression.Condition(
                        Expression.PropertyOrField(value, "HasValue"),
                        Expression.Convert(Expression.Call(value, fromType.GetMethod("GetValueOrDefault", Type.EmptyTypes)), toType),
                        Expression.Default(toType));
                }
                if (Types.CanBeCast(nullableArgType, toType))
                {
                    return Expression.Convert(Expression.Call(value, fromType.GetMethod("GetValueOrDefault", Type.EmptyTypes)), toType);
                }
            }
            throw new InvalidOperationException("Should never get here has types compatibility has been checked");
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
