using System;
using System.Collections.Generic;

namespace BusterWood.Mapper
{
    static class Names
    {
        public static List<string> Candidates(string name, Type type, string canRemovePrefix = null)
        {
            var names = new List<string>(2) { name };

            // special handling of xxxId to xxx for primitive types (int, long, etc)
            if (type.IsPrimitiveOrEnum() || type.IsNullablePrimitiveOrEnum())
            {
                if (name.EndsWith("Id", StringComparison.OrdinalIgnoreCase))
                {
                    var left = name.Substring(0, name.Length - 2);
                    if (!string.IsNullOrWhiteSpace(left) && !names.Contains(left))
                        names.Add(left);
                }
                else
                {
                    names.Add(name + "Id");
                }
            }

            // see if we need to replace underscores
            int max = names.Count;
            for (int i = 0; i < max; i++)
            {
                if (names[i].IndexOf('_') >= 0)
                {
                    var left = names[i].Replace("_", "");
                    if (!string.IsNullOrWhiteSpace(left) && !names.Contains(left))
                        names.Add(left);
                }
            }

            // remove (optional) prefix, e.g. remove ORDER from ORDER_ID
            if (!string.IsNullOrEmpty(canRemovePrefix))
            {
                max = names.Count;
                for (int i = 0; i < max; i++)
                {
                    if (names[i].StartsWith(canRemovePrefix, StringComparison.OrdinalIgnoreCase))
                    {
                        var left = names[i].Substring(canRemovePrefix.Length);
                        if (!string.IsNullOrWhiteSpace(left) && !names.Contains(left))
                            names.Add(left);
                    }
                }
            }
            return names;
        }
    }
}