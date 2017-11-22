using System;
using System.Collections;
using System.Data;
using System.Data.Common;
using System.Diagnostics.Contracts;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace BusterWood.Mapper
{
    /// <summary>Used to filter data before it is mapped to an object</summary>
    public class FilteredDbDataReader : DelegatingDbDataReader
    {
        readonly Func<IDataRecord, bool> predicate;

        /// <summary>Filters a source <param name="reader"/> using a <param name="predicate"/></summary>
        public FilteredDbDataReader(DbDataReader reader, Func<IDataRecord, bool> predicate, CommandBehavior commandBehavior) : base(reader, commandBehavior)
        {
            Contract.Requires(reader != null);
            Contract.Requires(predicate != null);
            this.predicate = predicate;
        }

        public override bool Read()
        {
            for(;;)
            {
                bool got = reader.Read();
                if (!got)
                    return false;
                if (predicate(this))
                    return true;
            }
        }

        public async override Task<bool> ReadAsync(CancellationToken cancellationToken)
        {
            for (;;)
            {
                bool got = await reader.ReadAsync(cancellationToken);
                if (!got)
                    return false;
                if (predicate(this))
                    return true;
            }
        }
    }

}