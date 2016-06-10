using System;
using System.Diagnostics.Contracts;
using System.Text;

namespace Mapper
{
    public static partial class CodeGen
    {
        public static string AssertAreEqual(Type type)
        {
            Contract.Requires(type != null);
            var sb = new StringBuilder();
            foreach (var item in Types.ReadablePublicFieldsAndProperties(type))
            {
                sb.Append("Assert.AreEqual(expected.").Append(item.Name).Append(", actual.").Append(item.Name).Append(", \"").Append(item.Name).Append("\");").AppendLine();
            }
            return sb.ToString();
        }

    }
}