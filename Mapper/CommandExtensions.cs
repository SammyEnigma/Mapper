using System.Diagnostics.Contracts;
using System.Data.Common;

namespace BusterWood.Mapper
{
    public static partial class Extensions
    {
        static readonly CommandParameterMapper _cmdMapper = new CommandParameterMapper();
        
        /// <summary>Adds parameters to the <paramref name="cmd"/>, one parameter per property of <paramref name="parameters"/></summary>
        public static DbCommand AddParameters(this DbCommand cmd, object parameters)
        {
            Contract.Requires(cmd != null);
            Contract.Requires(parameters != null);
            Contract.Ensures(Contract.Result<DbCommand>() == cmd);
            return _cmdMapper.AddParameters(cmd, parameters);
        }
    }

}