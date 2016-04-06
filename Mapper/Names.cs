using System;
using System.Collections.Generic;

namespace Mapper
{
    internal static class Names
    {
        public static List<string> CandidateNames(string name, Type type)
        {
            var names = new List<string>(2) { name };

            // special handling of xxxId to xxx for primitive types (int, long, etc)
            if (type.IsPrimitiveOrEnum() || (Types.IsNullable(type) && type.NullableOf().IsPrimitiveOrEnum()))
            {
                if (name.EndsWith("Id", StringComparison.OrdinalIgnoreCase))
                    names.Add(name.Substring(0, name.Length - 2));
                else
                    names.Add(name + "Id");
            }

            // see if we need to replace underscores
            int max = names.Count;
            for (int i = 0; i < max; i++)
            {
                if (names[i].IndexOf('_') >= 0)
                    names.Add(names[i].Replace("_", ""));
            }
            return names;
        }
    }
}