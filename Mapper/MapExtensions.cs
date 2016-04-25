using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Mapper
{
    public static class MapExtensions
    {
        /// <summary>Create an output object using the parameter-less constructor and setting public fields and properties</summary>
        public static T Clone<T>(this T input) => Map<T, T>(input);

        /// <summary>Create shallow copies of the <paramref name="input"/> objects using the parameter-less constructor and setting public fields and properties</summary>
        public static IEnumerable<T> CloneSome<T>(this IEnumerable<T> input) => MapSome<T, T>(input);

        /// <summary>Create shallow copies of the <paramref name="input"/> objects using the parameter-less constructor and setting public fields and properties</summary>
        /// <remarks><paramref name="extraAction"/> can be used to set additional values on each cloned objects</remarks>
        public static IEnumerable<T> CloneSome<T>(this IEnumerable<T> input, Action<T, T> extraAction) => MapSome(input, extraAction);

        /// <summary>Create shallow copies of the <paramref name="input"/> objects using the parameter-less constructor and setting public fields and properties</summary>
        /// <remarks><paramref name="extraAction"/> can be used to set additional values on each cloned objects</remarks>
        public static IEnumerable<T> CloneSome<T>(this IEnumerable<T> input, Action<T, T, int> extraAction) => MapSome(input, extraAction);

        /// <summary>Create an output object and copies all properties and fields where the property name and types match</summary>
        public static TOut Map<TIn, TOut>(this TIn input)
        {
            Contract.Requires(input != null);
            Contract.Ensures(Contract.Result<TOut>() != null);
            Func<TIn, TOut> map = ObjectMapper.GetOrAddMapping<TIn, TOut>();
            return map(input);
        }

        /// <summary>creates copies of all input objects, copying all properties and fields with matching names and compatible types</summary>
        public static IEnumerable<TOut> MapSome<TIn, TOut>(this IEnumerable<TIn> input) {
            Contract.Requires(input != null);
            Contract.Ensures(Contract.Result<IEnumerable<TOut>>() != null);
            Func<TIn, TOut> map = ObjectMapper.GetOrAddMapping<TIn, TOut>();
            return input.Select(map);
        }

        /// <summary>creates copies of all input objects, copying all properties and fields with matching names and compatible types</summary>
        /// <remarks><paramref name="extraAction"/> can be used to set additional values on each mapped objects</remarks>
        public static IEnumerable<TOut> MapSome<TIn, TOut>(this IEnumerable<TIn> input, Action<TIn, TOut> extraAction) {
            Contract.Requires(input != null);
            Contract.Requires(extraAction != null);
            Contract.Ensures(Contract.Result<IEnumerable<TOut>>() != null);
            Func<TIn, TOut> map = ObjectMapper.GetOrAddMapping<TIn, TOut>();
            foreach (var item in input)
            {
                var copy = map(item);
                extraAction(item, copy);
                yield return copy;
            }
        }

        /// <summary>creates copies of all input objects, copying all properties and fields with matching names and compatible types</summary>
        /// <remarks><paramref name="extraAction"/> can be used to set additional values on each mapped objects, passing the index (sequence number) of the item being mapped</remarks>
        public static IEnumerable<TOut> MapSome<TIn, TOut>(this IEnumerable<TIn> input, Action<TIn, TOut, int> extraAction)
        {
            Contract.Requires(input != null);
            Contract.Requires(extraAction != null);
            Contract.Ensures(Contract.Result<IEnumerable<TOut>>() != null);
            Func<TIn, TOut> map = ObjectMapper.GetOrAddMapping<TIn, TOut>();
            int i = 0;
            foreach (var item in input)
            {
                var copy = map(item);
                extraAction(item, copy, i);
                yield return copy;
                i++;
            }
        }

    }
}