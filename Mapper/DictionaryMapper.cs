using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using static System.StringComparison;

namespace BusterWood.Mapper
{
    //public static partial class Extensions
    //{
    //    public static T Read<T>(this IReadOnlyDictionary<string, object> source) where T : new()
    //    { }
    //}
    delegate T MappingMethod<T>(IReadOnlyDictionary<string, object> dic, MappingResult<ObjectThing, Thing> mapping, T input);

    class DictionaryMapper
    {
        static readonly MostlyReadDictionary<Type, Delegate> MapMethods = new MostlyReadDictionary<Type, Delegate>();

        public static T Read<T>(IReadOnlyDictionary<string, object> source) where T : new()
        {
            var writeTo = Types.WriteablePublicThings(typeof(T));
            var readFrom = source.Keys.Select(k => new ObjectThing(k)).ToList();
            var comparisons = new Func<ObjectThing, Thing, bool>[]
            {
                NameMatches,
                WithId,
                WithoutId,
                (from, to) => WithoutPrefix(from, to, typeof(T).Name)
            };
            MappingResult<ObjectThing, Thing> mapping = GenericMapping.Create(readFrom, writeTo, comparisons);

            var mapper = (MappingMethod<T>)MapMethods.GetOrAdd(typeof(T), t => CreateMapDelegate(t));

            return mapper(source, mapping, default(T));
        }


        /// <summary>Fast copying code that generates a method that does the copy from one type to another</summary>
        static internal Delegate CreateMapDelegate(Type outType)
        {
            Contract.Requires(outType != null);
            Contract.Ensures(Contract.Result<Delegate>() != null);

            LambdaExpression lambdaExpression = CreateMappingLambda(outType);
            return lambdaExpression.Compile();
        }

        static LambdaExpression CreateMappingLambda(Type outType)
        {
            Contract.Requires(outType != null);
            Contract.Ensures(Contract.Result<LambdaExpression>() != null);

            if (outType.IsClass && outType.GetConstructor(Type.EmptyTypes) == null)
                throw new ArgumentException("Output type must have a parameterless constructor");

            var mappingType = typeof(MappingResult<ObjectThing, Thing>);
            var mapping = Expression.Parameter(mappingType, "mapping");

            var dicType = typeof(IReadOnlyDictionary<string, object>);
            ParameterExpression dic = Expression.Parameter(dicType, "dic");

            var to = Expression.Parameter(outType, "to");
            var result = Expression.Parameter(outType, "result");

            var listEnumType = typeof(List<Mapping<ObjectThing, Thing>>.Enumerator);
            var en = Expression.Variable(listEnumType, "en");

            var lines = new List<Expression>();
            if (outType.IsClass)
            {
                // result = to ?? new outType
                lines.Add(Expression.Assign(result, Expression.Coalesce(to, Expression.New(outType.GetConstructor(Type.EmptyTypes)))));
            }

            LabelTarget exitLoop = Expression.Label();
            lines.Add(Expression.Assign(en, 
                Expression.Call(Expression.PropertyOrField(mapping, "Mapped"), typeof(List<Mapping<ObjectThing, Thing>>).GetMethod("GetEnumerator"))));

            var value = Expression.Parameter(typeof(object), "value");
            lines.Add(
                Expression.Loop(
                    Expression.IfThenElse(
                        Expression.Call(en, listEnumType.GetMethod("MoveNext")),
                        Expression.Block(new [] { value }, // enumerator moved next
                            // get the value of this mapping from the dictionary
                            Expression.Assign(value, Expression.Property(dic, dicType.GetProperty("Item"), Expression.Property(Expression.PropertyOrField(Expression.Property(en, "Current"), "From"), "Name"))),
                            Expression.Switch( 
                                Expression.Property(Expression.PropertyOrField(Expression.Property(en, "Current"), "To"), "Name"),
                                //Expression.Throw(Expression.New(typeof(InvalidOperationException).GetConstructor(new[] { typeof(string) }), Expression.Property(Expression.PropertyOrField(Expression.Property(en, "Current"), "To"), "Name"))), 
                                Expression.Constant(null),
                                AssignmentCases(result, value)
                            ),
                            Expression.Constant(null)
                        ),
                        Expression.Break(exitLoop) // else MoveNext returned FALSE, exit the loop
                    ),
                    exitLoop
                )
            );

            lines.Add(result); // the return value

            var variables = new[] { result, en };
            var body = Expression.Block(variables, lines);
            var delegateType = typeof(MappingMethod<>).MakeGenericType(new[] { outType });
            LambdaExpression lambdaExpression = Expression.Lambda(delegateType, body, new[] { dic, mapping, to });
            return lambdaExpression;
        }

        private static SwitchCase[] AssignmentCases(ParameterExpression result, ParameterExpression value)
        {
            var writable = Types.WriteablePublicFieldsAndProperties(result.Type).ToList();
            var cases = new SwitchCase[writable.Count];
            for (int i = 0; i < cases.Length; i++)
            {
                cases[i] = AssignmentCase(result, value, writable[i]);
            }
            return cases;
        }

        private static SwitchCase AssignmentCase(ParameterExpression result, ParameterExpression value, MemberInfo resultMember)
        {
            var isNull = Expression.ReferenceEqual(Expression.Constant(null, typeof(object)), value);
            var defaultT = Expression.Default(resultMember.PropertyOrFieldType());
            var convert = ConvertValue(value, resultMember.PropertyOrFieldType());
            return Expression.SwitchCase(
                Expression.Block(
                    Expression.Assign(
                        Expression.PropertyOrField(result, resultMember.Name), // result.Property = ....
                        Expression.Condition(isNull, defaultT, convert)
                    ),
                    Expression.Constant(null)
                ),
                Expression.Constant(resultMember.Name) // switch on result member name
            );
        }

        private static Expression ConvertValue(ParameterExpression value, Type toType)
        {
            return Expression.Convert(value, toType); //TODOO other conversions here
        }

        static bool NameMatches<TFrom, TTo>(TFrom from, TTo to)
    where TFrom : Thing
    where TTo : Thing
        {
            return string.Equals(from.ComparisonName, to.ComparisonName, OrdinalIgnoreCase);
        }

        static bool WithId<TFrom, TTo>(TFrom from, TTo to)
            where TFrom : Thing
            where TTo : Thing
        {
            if (from.ComparisonName.EndsWith("ID", OrdinalIgnoreCase))
                return false;

            return string.Equals(from.ComparisonName + "ID", to.ComparisonName, OrdinalIgnoreCase);
        }

        static bool WithoutId<TFrom, TTo>(TFrom from, TTo to)
            where TFrom : Thing
            where TTo : Thing
        {
            if (!from.ComparisonName.EndsWith("ID", OrdinalIgnoreCase))
                return false;

            return string.Equals(from.ComparisonName.Substring(0, from.ComparisonName.Length - 2), to.ComparisonName, OrdinalIgnoreCase);
        }

        static bool WithoutPrefix<TFrom, TTo>(TFrom from, TTo to, string removablePrefix)
    where TFrom : Thing
    where TTo : Thing
        {
            if (!from.ComparisonName.StartsWith(removablePrefix, OrdinalIgnoreCase))
                return false;

            return string.Equals(from.ComparisonName.Substring(removablePrefix.Length), to.ComparisonName, OrdinalIgnoreCase);
        }

    }

}
