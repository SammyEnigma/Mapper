using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics.Contracts;

namespace BusterWood.Mapper
{
    public static partial class Extensions
    {
        /// <summary>Filters a source <param name="reader"/> using a <param name="predicate"/></summary>
        public static DbDataReader Where(this DbDataReader reader, Func<IDataRecord, bool> predicate, CommandBehavior commandBehavior = CommandBehavior.Default)
        {
            Contract.Requires(reader != null);
            Contract.Ensures(Contract.Result<DbDataReader>() != null);
            return new FilteredDbDataReader(reader, predicate, commandBehavior);
        }

        /// <summary>Renames some of the fields in a source <param name="reader"/> using the <param name="newNames"/></summary>
        public static DbDataReader Rename(this DbDataReader reader, IReadOnlyDictionary<int, string> newNames, CommandBehavior commandBehavior = CommandBehavior.Default)
        {
            Contract.Requires(reader != null);
            Contract.Ensures(Contract.Result<DbDataReader>() != null);
            return new RenamingDbDataReader(reader, newNames, commandBehavior);
        }

        /// <summary>Renames some of the fields in a source <param name="reader"/> using the <param name="newNames"/></summary>
        public static DbDataReader Rename(this DbDataReader reader, IReadOnlyDictionary<string, string> newNames, CommandBehavior commandBehavior = CommandBehavior.Default)
        {
            Contract.Requires(reader != null);
            Contract.Ensures(Contract.Result<DbDataReader>() != null);
            return new RenamingDbDataReader(reader, newNames, commandBehavior);
        }

        //TODO: Project to so the data reader contains a subset of the original fields
        
        //TODO: (natural) Join that combines two data readers using the common columns
    }
}
